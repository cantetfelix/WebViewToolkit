// ============================================================================
// WebViewToolkit - DirectX 11 Render API Implementation
// ============================================================================

#include "RenderAPI_D3D11.h"

// DirectX headers MUST be included before Unity headers
#include <d3d11.h>
#include <dxgi.h>

#include "DebugLog.h"
using WebViewToolkit::DebugLog;

// Unity Plugin API
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsD3D11.h"

#include <stdexcept>

namespace WebViewToolkit
{
    RenderAPI_D3D11::RenderAPI_D3D11() = default;

    RenderAPI_D3D11::~RenderAPI_D3D11()
    {
        ReleaseResources();
    }

    void RenderAPI_D3D11::ProcessDeviceEvent(int eventType, IUnityInterfaces* interfaces)
    {
        switch (eventType)
        {
        case kUnityGfxDeviceEventInitialize:
        case kUnityGfxDeviceEventAfterReset:
        {
            if (interfaces)
            {
                auto d3d11Interface = interfaces->Get<IUnityGraphicsD3D11>();
                if (d3d11Interface)
                {
                    m_device = d3d11Interface->GetDevice();
                    if (m_device)
                    {
                        m_device->GetImmediateContext(&m_context);
                        InitializeCompositionDevice();
                    }
                }
            }
            break;
        }

        case kUnityGfxDeviceEventShutdown:
        case kUnityGfxDeviceEventBeforeReset:
        {
            ReleaseResources();
            break;
        }

        default:
            break;
        }
    }

    Result RenderAPI_D3D11::InitializeCompositionDevice()
    {
        if (!m_device)
        {
            return Result::ErrorNotInitialized;
        }

        // Get the DXGI device from D3D11 device
        ComPtr<IDXGIDevice> dxgiDevice;
        HRESULT hr = m_device.As(&dxgiDevice);
        if (FAILED(hr))
        {
            return Result::ErrorDeviceCreationFailed;
        }

        // Create DirectComposition device
        hr = DCompositionCreateDevice(
            dxgiDevice.Get(),
            __uuidof(IDCompositionDevice),
            reinterpret_cast<void**>(m_compositionDevice.GetAddressOf())
        );

        if (FAILED(hr))
        {
            return Result::ErrorCompositionFailed;
        }

        return Result::Success;
    }

    Result RenderAPI_D3D11::CreateSharedTexture(uint32_t width, uint32_t height, void** outNativePtr)
    {
        if (!m_device || !outNativePtr)
        {
            return Result::ErrorNotInitialized;
        }

        D3D11_TEXTURE2D_DESC desc = {};
        desc.Width = width;
        desc.Height = height;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;  // WebView2 uses BGRA
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;
        desc.CPUAccessFlags = 0;
        desc.MiscFlags = D3D11_RESOURCE_MISC_SHARED;  // Shared for composition

        ComPtr<ID3D11Texture2D> texture;
        HRESULT hr = m_device->CreateTexture2D(&desc, nullptr, &texture);
        if (FAILED(hr))
        {
            return Result::ErrorTextureCreationFailed;
        }

        // Return raw pointer - caller is responsible for AddRef/Release
        *outNativePtr = texture.Detach();
        return Result::Success;
    }

    void RenderAPI_D3D11::DestroySharedTexture(void* nativePtr)
    {
        if (nativePtr)
        {
            auto texture = static_cast<ID3D11Texture2D*>(nativePtr);
            texture->Release();
        }
    }

    Result RenderAPI_D3D11::ResizeSharedTexture(void* nativePtr, uint32_t newWidth, uint32_t newHeight, void** outNewNativePtr)
    {
        if (!outNewNativePtr)
        {
            return Result::ErrorInvalidHandle;
        }

        // Destroy old texture
        DestroySharedTexture(nativePtr);

        // Create new texture with new dimensions
        return CreateSharedTexture(newWidth, newHeight, outNewNativePtr);
    }

    void RenderAPI_D3D11::BeginRenderToTexture(void* /*texturePtr*/)
    {
        // DX11: No special handling needed - WebView2 handles composition directly
    }

    void RenderAPI_D3D11::EndRenderToTexture(void* /*texturePtr*/)
    {
        // DX11: No special handling needed
        // Optionally flush to ensure WebView content is visible
        if (m_context)
        {
            m_context->Flush();
        }
    }

    void RenderAPI_D3D11::WaitForGPU()
    {
        // DX11: Simple flush is usually sufficient
        if (m_context)
        {
            m_context->Flush();
        }
    }

    void RenderAPI_D3D11::SignalRenderComplete()
    {
        // DX11: No explicit signaling needed
    }

    void RenderAPI_D3D11::CopyCapturedTextureToUnityTexture(void* capturedTexture, void* unityTexturePtr, bool flipY)
    {
        if (!capturedTexture || !unityTexturePtr || !m_context)
        {
            return;
        }

        auto srcTexture = static_cast<ID3D11Texture2D*>(capturedTexture);
        auto dstTexture = static_cast<ID3D11Texture2D*>(unityTexturePtr);

        D3D11_TEXTURE2D_DESC srcDesc, dstDesc;
        srcTexture->GetDesc(&srcDesc);
        dstTexture->GetDesc(&dstDesc);

        // Validate dimensions match to prevent crashes during resize
        // During resize, old-sized frames may still be in the pool, so skip mismatched frames
        if (srcDesc.Width != dstDesc.Width || srcDesc.Height != dstDesc.Height)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: Size mismatch (src=%dx%d, dst=%dx%d), skipping frame",
                srcDesc.Width, srcDesc.Height, dstDesc.Width, dstDesc.Height);
            return;
        }

        if (flipY)
        {
            // Copy row by row in reverse to flip Y
            for (UINT y = 0; y < std::min(srcDesc.Height, dstDesc.Height); y++)
            {
                D3D11_BOX srcBox;
                srcBox.left = 0;
                srcBox.right = std::min(srcDesc.Width, dstDesc.Width);
                srcBox.top = srcDesc.Height - 1 - y;
                srcBox.bottom = srcDesc.Height - y;
                srcBox.front = 0;
                srcBox.back = 1;

                m_context->CopySubresourceRegion(
                    dstTexture, 0,
                    0, y, 0,
                    srcTexture, 0,
                    &srcBox
                );
            }
        }
        else
        {
            m_context->CopyResource(dstTexture, srcTexture);
        }
    }

    void RenderAPI_D3D11::ReleaseResources()
    {
        m_compositionDevice.Reset();
        m_context.Reset();
        m_device.Reset();
    }

} // namespace WebViewToolkit
