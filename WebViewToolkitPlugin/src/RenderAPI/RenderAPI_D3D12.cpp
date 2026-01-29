// ============================================================================
// WebViewToolkit - DirectX 12 Render API Implementation
// ============================================================================

#include "RenderAPI_D3D12.h"

#ifdef WEBVIEW_TOOLKIT_DX12_SUPPORT

// DirectX headers MUST be included before Unity headers
#include <d3d12.h>
#include <d3d11on12.h>
#include <d3d11.h>
#include <dxgi1_2.h>

#include "DebugLog.h"
using WebViewToolkit::DebugLog;

// Unity Plugin API
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsD3D12.h"

#include <stdexcept>

namespace WebViewToolkit
{
    RenderAPI_D3D12::RenderAPI_D3D12() = default;

    RenderAPI_D3D12::~RenderAPI_D3D12()
    {
        ReleaseResources();
    }

    void RenderAPI_D3D12::ProcessDeviceEvent(int eventType, IUnityInterfaces* interfaces)
    {
        switch (eventType)
        {
        case kUnityGfxDeviceEventInitialize:
        case kUnityGfxDeviceEventAfterReset:
        {
            if (interfaces)
            {
                // Use v5 interface for command queue access (v4+ required)
                auto d3d12Interface = interfaces->Get<IUnityGraphicsD3D12v5>();
                if (d3d12Interface)
                {
                    m_d3d12Device = d3d12Interface->GetDevice();
                    m_d3d12CommandQueue = d3d12Interface->GetCommandQueue();

                    if (m_d3d12Device && m_d3d12CommandQueue)
                    {
                        DebugLog::Log("ProcessDeviceEvent: Starting D3D12 initialization...");
                        Result result;

                        DebugLog::Log("ProcessDeviceEvent: Calling InitializeD3D11On12...");
                        result = InitializeD3D11On12();
                        if (result != Result::Success)
                        {
                            DebugLog::Log("ProcessDeviceEvent: ERROR - InitializeD3D11On12 failed with result %d", (int)result);
                            return;
                        }
                        DebugLog::Log("ProcessDeviceEvent: InitializeD3D11On12 succeeded");

                        DebugLog::Log("ProcessDeviceEvent: Calling InitializeCaptureDevice...");
                        result = InitializeCaptureDevice();
                        if (result != Result::Success)
                        {
                            DebugLog::Log("ProcessDeviceEvent: ERROR - InitializeCaptureDevice failed with result %d", (int)result);
                            return;
                        }
                        DebugLog::Log("ProcessDeviceEvent: InitializeCaptureDevice succeeded");

                        DebugLog::Log("ProcessDeviceEvent: Calling InitializeCompositionDevice...");
                        result = InitializeCompositionDevice();
                        if (result != Result::Success)
                        {
                            DebugLog::Log("ProcessDeviceEvent: ERROR - InitializeCompositionDevice failed with result %d", (int)result);
                            return;
                        }
                        DebugLog::Log("ProcessDeviceEvent: InitializeCompositionDevice succeeded");

                        DebugLog::Log("ProcessDeviceEvent: Calling CreateFence...");
                        result = CreateFence();
                        if (result != Result::Success)
                        {
                            DebugLog::Log("ProcessDeviceEvent: ERROR - CreateFence failed with result %d", (int)result);
                            return;
                        }
                        DebugLog::Log("ProcessDeviceEvent: CreateFence succeeded");
                        DebugLog::Log("ProcessDeviceEvent: All D3D12 initialization complete!");
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

    Result RenderAPI_D3D12::InitializeD3D11On12()
    {
        if (!m_d3d12Device || !m_d3d12CommandQueue)
        {
            return Result::ErrorNotInitialized;
        }

        // Create D3D11On12 device wrapping the D3D12 device
        IUnknown* queues[] = { m_d3d12CommandQueue.Get() };

        UINT d3d11DeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef WEBVIEW_TOOLKIT_DEBUG
        d3d11DeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

        // Specify feature levels to ensure compatibility
        D3D_FEATURE_LEVEL featureLevels[] = {
            D3D_FEATURE_LEVEL_11_1,
            D3D_FEATURE_LEVEL_11_0
        };

        D3D_FEATURE_LEVEL featureLevel;
        HRESULT hr = D3D11On12CreateDevice(
            m_d3d12Device.Get(),
            d3d11DeviceFlags,
            featureLevels,                  // Feature levels
            ARRAYSIZE(featureLevels),
            queues, 1,                      // Command queues
            0,                              // Node mask
            &m_d3d11Device,
            &m_d3d11Context,
            &featureLevel                   // Feature level output
        );

        if (FAILED(hr))
        {
            return Result::ErrorDeviceCreationFailed;
        }

        // Get the D3D11On12 interface for wrapping resources
        hr = m_d3d11Device.As(&m_d3d11On12Device);
        if (FAILED(hr))
        {
            return Result::ErrorDeviceCreationFailed;
        }

        return Result::Success;
    }

    Result RenderAPI_D3D12::InitializeCaptureDevice()
    {
        DebugLog::Log("InitializeCaptureDevice: Starting...");

        if (!m_d3d12Device)
        {
            DebugLog::Log("InitializeCaptureDevice: ERROR - m_d3d12Device is null");
            return Result::ErrorNotInitialized;
        }

        // Get the adapter LUID from D3D12 device
        LUID adapterLuid = m_d3d12Device->GetAdapterLuid();
        DebugLog::Log("InitializeCaptureDevice: D3D12 adapter LUID: Low=%u, High=%u", adapterLuid.LowPart, adapterLuid.HighPart);

        // Create DXGI factory to enumerate adapters
        ComPtr<IDXGIFactory1> dxgiFactory;
        HRESULT hr = CreateDXGIFactory1(IID_PPV_ARGS(&dxgiFactory));
        if (FAILED(hr))
        {
            DebugLog::Log("InitializeCaptureDevice: ERROR - CreateDXGIFactory1 failed with HRESULT 0x%08X", hr);
            return Result::ErrorDeviceCreationFailed;
        }
        DebugLog::Log("InitializeCaptureDevice: DXGI factory created successfully");

        // Find the adapter matching Unity's D3D12 device
        ComPtr<IDXGIAdapter1> adapter;
        UINT adapterIndex = 0;
        for (UINT i = 0; ; ++i)
        {
            ComPtr<IDXGIAdapter1> currentAdapter;
            if (FAILED(dxgiFactory->EnumAdapters1(i, &currentAdapter)))
            {
                break;  // No more adapters
            }

            DXGI_ADAPTER_DESC1 desc;
            currentAdapter->GetDesc1(&desc);
            DebugLog::Log("InitializeCaptureDevice: Adapter %d: LUID Low=%u, High=%u, Name=%ls",
                i, desc.AdapterLuid.LowPart, desc.AdapterLuid.HighPart, desc.Description);

            if (desc.AdapterLuid.LowPart == adapterLuid.LowPart &&
                desc.AdapterLuid.HighPart == adapterLuid.HighPart)
            {
                adapter = currentAdapter;
                adapterIndex = i;
                DebugLog::Log("InitializeCaptureDevice: Found matching adapter at index %d", i);
                break;
            }
        }

        // If we couldn't find matching adapter, use default (nullptr)
        D3D_DRIVER_TYPE driverType = adapter ? D3D_DRIVER_TYPE_UNKNOWN : D3D_DRIVER_TYPE_HARDWARE;
        if (!adapter)
        {
            DebugLog::Log("InitializeCaptureDevice: WARNING - Could not find matching adapter, using default (may cause cross-GPU issues)");
        }

        UINT d3d11DeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
#ifdef WEBVIEW_TOOLKIT_DEBUG
        d3d11DeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

        D3D_FEATURE_LEVEL featureLevels[] = {
            D3D_FEATURE_LEVEL_11_1,
            D3D_FEATURE_LEVEL_11_0,
            D3D_FEATURE_LEVEL_10_1,
            D3D_FEATURE_LEVEL_10_0
        };

        DebugLog::Log("InitializeCaptureDevice: Creating D3D11 device (adapter=%p, driverType=%d)...",
            adapter.Get(), driverType);

        D3D_FEATURE_LEVEL featureLevel;
        hr = D3D11CreateDevice(
            adapter.Get(),                  // Use Unity's adapter (or nullptr if not found)
            driverType,                     // UNKNOWN when adapter provided, HARDWARE otherwise
            nullptr,                        // Software rasterizer (not used)
            d3d11DeviceFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,
            &m_captureD3D11Device,
            &featureLevel,
            &m_captureD3D11Context
        );

        if (FAILED(hr))
        {
            DebugLog::Log("InitializeCaptureDevice: ERROR - D3D11CreateDevice failed with HRESULT 0x%08X", hr);
            return Result::ErrorDeviceCreationFailed;
        }

        DebugLog::Log("InitializeCaptureDevice: D3D11 device created successfully (Feature Level: 0x%X)", featureLevel);
        DebugLog::Log("InitializeCaptureDevice: Success!");
        return Result::Success;
    }

    Result RenderAPI_D3D12::InitializeCompositionDevice()
    {
        if (!m_d3d11Device)
        {
            return Result::ErrorNotInitialized;
        }

        // Get the DXGI device from the wrapped D3D11 device
        ComPtr<IDXGIDevice> dxgiDevice;
        HRESULT hr = m_d3d11Device.As(&dxgiDevice);
        if (FAILED(hr))
        {
            return Result::ErrorDeviceCreationFailed;
        }

        // Create DirectComposition device using the wrapped D3D11 device
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

    Result RenderAPI_D3D12::CreateFence()
    {
        if (!m_d3d12Device)
        {
            return Result::ErrorNotInitialized;
        }

        HRESULT hr = m_d3d12Device->CreateFence(
            0,
            D3D12_FENCE_FLAG_NONE,
            IID_PPV_ARGS(&m_fence)
        );

        if (FAILED(hr))
        {
            return Result::ErrorDeviceCreationFailed;
        }

        m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (!m_fenceEvent)
        {
            return Result::ErrorDeviceCreationFailed;
        }

        return Result::Success;
    }

    Result RenderAPI_D3D12::CreateSharedTexture(uint32_t width, uint32_t height, void** outNativePtr)
    {
        if (!m_d3d12Device || !outNativePtr)
        {
            return Result::ErrorNotInitialized;
        }

        // Create D3D12 texture
        D3D12_RESOURCE_DESC desc = {};
        desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        desc.Width = width;
        desc.Height = height;
        desc.DepthOrArraySize = 1;
        desc.MipLevels = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;  // WebView2 uses BGRA
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
        desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;

        D3D12_HEAP_PROPERTIES heapProps = {};
        heapProps.Type = D3D12_HEAP_TYPE_DEFAULT;

        ComPtr<ID3D12Resource> texture;
        HRESULT hr = m_d3d12Device->CreateCommittedResource(
            &heapProps,
            D3D12_HEAP_FLAG_NONE,
            &desc,
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,  // Initial state for Unity
            nullptr,
            IID_PPV_ARGS(&texture)
        );

        if (FAILED(hr))
        {
            return Result::ErrorTextureCreationFailed;
        }

        // Return raw pointer - caller is responsible for AddRef/Release
        *outNativePtr = texture.Detach();
        return Result::Success;
    }

    void RenderAPI_D3D12::DestroySharedTexture(void* nativePtr)
    {
        if (nativePtr)
        {
            // Remove wrapped resource if it exists
            m_wrappedResources.erase(nativePtr);

            auto resource = static_cast<ID3D12Resource*>(nativePtr);
            resource->Release();
        }
    }

    Result RenderAPI_D3D12::ResizeSharedTexture(void* nativePtr, uint32_t newWidth, uint32_t newHeight, void** outNewNativePtr)
    {
        if (!outNewNativePtr)
        {
            return Result::ErrorInvalidHandle;
        }

        // Wait for GPU before destroying old texture
        WaitForGPU();

        // Destroy old texture
        DestroySharedTexture(nativePtr);

        // Create new texture with new dimensions
        return CreateSharedTexture(newWidth, newHeight, outNewNativePtr);
    }

    RenderAPI_D3D12::WrappedResource* RenderAPI_D3D12::GetOrCreateWrappedResource(void* d3d12TexturePtr)
    {
        // Check if Unity resized the texture - need to invalidate cache if dimensions changed
        auto d3d12Resource = static_cast<ID3D12Resource*>(d3d12TexturePtr);
        D3D12_RESOURCE_DESC currentDesc = d3d12Resource->GetDesc();

        auto it = m_wrappedResources.find(d3d12TexturePtr);
        if (it != m_wrappedResources.end())
        {
            // Verify cached resource still has correct dimensions
            ComPtr<ID3D11Texture2D> cachedTexture;
            it->second->d3d11Resource.As(&cachedTexture);
            if (cachedTexture)
            {
                D3D11_TEXTURE2D_DESC cachedDesc;
                cachedTexture->GetDesc(&cachedDesc);

                // If dimensions don't match, Unity resized the texture - invalidate cache
                if (cachedDesc.Width != currentDesc.Width || cachedDesc.Height != currentDesc.Height)
                {
                    DebugLog::Log("GetOrCreateWrappedResource: Unity texture resized (%dx%d -> %dx%d), invalidating cached resource",
                        cachedDesc.Width, cachedDesc.Height, (UINT)currentDesc.Width, (UINT)currentDesc.Height);
                    m_wrappedResources.erase(it);
                }
                else
                {
                    // Dimensions match, use cached resource
                    return it->second.get();
                }
            }
        }

        if (!m_d3d11On12Device)
        {
            return nullptr;
        }
        
        // Wrap the D3D12 resource for use with D3D11
        D3D11_RESOURCE_FLAGS d3d11Flags = {};
        d3d11Flags.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;

        ComPtr<ID3D11Resource> d3d11Resource;
        HRESULT hr = m_d3d11On12Device->CreateWrappedResource(
            d3d12Resource,
            &d3d11Flags,
            D3D12_RESOURCE_STATE_RENDER_TARGET,     // In state (for WebView rendering)
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,  // Out state (for Unity)
            IID_PPV_ARGS(&d3d11Resource)
        );

        if (FAILED(hr))
        {
            return nullptr;
        }

        auto wrapped = std::make_unique<WrappedResource>();
        wrapped->d3d12Resource = d3d12Resource;
        wrapped->d3d11Resource = d3d11Resource;

        auto* result = wrapped.get();
        m_wrappedResources[d3d12TexturePtr] = std::move(wrapped);
        return result;
    }

    void RenderAPI_D3D12::BeginRenderToTexture(void* texturePtr)
    {
        if (!m_d3d11On12Device || !texturePtr)
        {
            return;
        }

        auto* wrapped = GetOrCreateWrappedResource(texturePtr);
        if (!wrapped)
        {
            return;
        }

        // Acquire the wrapped resource for D3D11 use
        // This transitions from PIXEL_SHADER_RESOURCE to RENDER_TARGET
        ID3D11Resource* resources[] = { wrapped->d3d11Resource.Get() };
        m_d3d11On12Device->AcquireWrappedResources(resources, 1);
    }

    void RenderAPI_D3D12::EndRenderToTexture(void* texturePtr)
    {
        if (!m_d3d11On12Device || !texturePtr)
        {
            return;
        }

        auto it = m_wrappedResources.find(texturePtr);
        if (it == m_wrappedResources.end())
        {
            return;
        }

        auto* wrapped = it->second.get();

        // Release the wrapped resource back to D3D12
        // This transitions from RENDER_TARGET to PIXEL_SHADER_RESOURCE
        ID3D11Resource* resources[] = { wrapped->d3d11Resource.Get() };
        m_d3d11On12Device->ReleaseWrappedResources(resources, 1);

        // Flush the D3D11 context to ensure commands are submitted
        m_d3d11Context->Flush();
    }

    void RenderAPI_D3D12::WaitForGPU()
    {
        if (!m_d3d12CommandQueue || !m_fence || !m_fenceEvent)
        {
            return;
        }

        // Signal the fence with the next value
        const uint64_t fenceValueToWait = ++m_fenceValue;
        HRESULT hr = m_d3d12CommandQueue->Signal(m_fence.Get(), fenceValueToWait);
        if (FAILED(hr))
        {
            return;
        }

        // Wait for the fence
        if (m_fence->GetCompletedValue() < fenceValueToWait)
        {
            hr = m_fence->SetEventOnCompletion(fenceValueToWait, m_fenceEvent);
            if (SUCCEEDED(hr))
            {
                WaitForSingleObject(m_fenceEvent, INFINITE);
            }
        }
    }

    void RenderAPI_D3D12::SignalRenderComplete()
    {
        if (m_d3d11Context)
        {
            m_d3d11Context->Flush();
        }
    }

    void RenderAPI_D3D12::CopyCapturedTextureToUnityTexture(void* capturedTexture, void* unityTexturePtr, bool flipY)
    {
        HRESULT hr; // Declare once for entire function

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Start (capturedTexture=%p, unityTexture=%p, flipY=%d)",
            capturedTexture, unityTexturePtr, flipY);

        if (!capturedTexture || !unityTexturePtr)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: ERROR - null pointer");
            return;
        }

        auto srcTexture = static_cast<ID3D11Texture2D*>(capturedTexture);

        // Get source texture description
        D3D11_TEXTURE2D_DESC srcDesc;
        srcTexture->GetDesc(&srcDesc);
        DebugLog::Log("CopyCapturedTextureToUnityTexture: Source texture %dx%d", srcDesc.Width, srcDesc.Height);

        // Unity's texture is a D3D12 resource, need to wrap it
        // GetOrCreateWrappedResource will automatically handle dimension changes and invalidate cache if needed
        // Get the D3D12 resource to check its actual size
        auto* d3d12Resource = static_cast<ID3D12Resource*>(unityTexturePtr);
        D3D12_RESOURCE_DESC d3d12Desc = d3d12Resource->GetDesc();
        UINT d3d12Width = static_cast<UINT>(d3d12Desc.Width);
        UINT d3d12Height = static_cast<UINT>(d3d12Desc.Height);

        // Check if Unity's D3D12 texture size matches the captured size
        // If not, Unity might be in the middle of resizing - skip this frame
        if (srcDesc.Width != d3d12Width || srcDesc.Height != d3d12Height)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: Unity D3D12 texture size mismatch (captured=%dx%d, Unity D3D12=%dx%d), skipping frame",
                srcDesc.Width, srcDesc.Height, d3d12Width, d3d12Height);
            return;
        }

        auto* wrapped = GetOrCreateWrappedResource(unityTexturePtr);
        if (!wrapped)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: ERROR - failed to wrap Unity texture");
            return;
        }

        ComPtr<ID3D11Texture2D> dstTexture;
        hr = wrapped->d3d11Resource.As(&dstTexture);
        if (FAILED(hr) || !dstTexture)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: ERROR - failed to cast wrapped resource to ID3D11Texture2D: 0x%08X", hr);
            return;
        }

        D3D11_TEXTURE2D_DESC dstDesc;
        dstTexture->GetDesc(&dstDesc);
        DebugLog::Log("CopyCapturedTextureToUnityTexture: Destination texture %dx%d", dstDesc.Width, dstDesc.Height);

        // Check if sizes match - if not, skip this frame
        // This happens during resize when we get an old-sized frame for a new-sized Unity texture
        if (srcDesc.Width != dstDesc.Width || srcDesc.Height != dstDesc.Height)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: Size mismatch (src=%dx%d, dst=%dx%d), skipping frame",
                srcDesc.Width, srcDesc.Height, dstDesc.Width, dstDesc.Height);
            return;
        }

        // Problem: srcTexture is from capture device, dstTexture is from D3D11On12 device
        // We need to copy via CPU or shared texture
        // For now, use CPU copy via staging texture

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Creating staging texture on capture device...");

        // Create staging texture on capture device
        D3D11_TEXTURE2D_DESC stagingDesc = srcDesc;
        stagingDesc.Usage = D3D11_USAGE_STAGING;
        stagingDesc.BindFlags = 0;
        stagingDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
        stagingDesc.MiscFlags = 0;

        ComPtr<ID3D11Texture2D> stagingTexture;
        hr = m_captureD3D11Device->CreateTexture2D(&stagingDesc, nullptr, &stagingTexture);
        if (FAILED(hr))
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: ERROR - failed to create staging texture: 0x%08X", hr);
            return;
        }

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Copying captured texture to staging...");
        // Copy captured texture to staging
        m_captureD3D11Context->CopyResource(stagingTexture.Get(), srcTexture);

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Mapping staging texture...");
        // Map staging texture
        D3D11_MAPPED_SUBRESOURCE mapped;
        hr = m_captureD3D11Context->Map(stagingTexture.Get(), 0, D3D11_MAP_READ, 0, &mapped);
        if (FAILED(hr))
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: ERROR - failed to map staging texture: 0x%08X", hr);
            return;
        }

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Acquiring wrapped resource...");
        // Acquire the wrapped resource for D3D11 use
        ID3D11Resource* resources[] = { wrapped->d3d11Resource.Get() };
        m_d3d11On12Device->AcquireWrappedResources(resources, 1);

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Updating destination texture via CPU...");
        // Update destination texture via UpdateSubresource or Map/Unmap
        UINT minWidth = std::min(srcDesc.Width, dstDesc.Width);
        UINT minHeight = std::min(srcDesc.Height, dstDesc.Height);

        if (flipY)
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: Copying with Y-flip...");
            // Copy row by row with Y-flip
            for (UINT y = 0; y < minHeight; y++)
            {
                // For Y-flip: destination row y gets source row (Height - 1 - y)
                UINT srcY = srcDesc.Height - 1 - y;
                BYTE* srcRow = (BYTE*)mapped.pData + srcY * mapped.RowPitch;

                // Destination box - where we're writing in the destination texture
                D3D11_BOX dstBox;
                dstBox.left = 0;
                dstBox.right = minWidth;
                dstBox.top = y;
                dstBox.bottom = y + 1;
                dstBox.front = 0;
                dstBox.back = 1;

                m_d3d11Context->UpdateSubresource(dstTexture.Get(), 0, &dstBox, srcRow, mapped.RowPitch, 0);
            }
        }
        else
        {
            DebugLog::Log("CopyCapturedTextureToUnityTexture: Copying without flip...");
            m_d3d11Context->UpdateSubresource(dstTexture.Get(), 0, nullptr, mapped.pData, mapped.RowPitch, 0);
        }

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Unmapping staging texture...");
        m_captureD3D11Context->Unmap(stagingTexture.Get(), 0);

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Releasing wrapped resource...");
        // Release the wrapped resource back to D3D12
        m_d3d11On12Device->ReleaseWrappedResources(resources, 1);
        m_d3d11Context->Flush();

        DebugLog::Log("CopyCapturedTextureToUnityTexture: Success!");
    }

    void RenderAPI_D3D12::ReleaseResources()
    {
        // Wait for GPU to finish all work
        WaitForGPU();

        // Clear wrapped resources
        m_wrappedResources.clear();

        // Release fence event
        if (m_fenceEvent)
        {
            CloseHandle(m_fenceEvent);
            m_fenceEvent = nullptr;
        }

        // Release COM objects in order
        m_fence.Reset();
        m_compositionDevice.Reset();
        m_captureD3D11Context.Reset();
        m_captureD3D11Device.Reset();
        m_d3d11On12Device.Reset();
        m_d3d11Context.Reset();
        m_d3d11Device.Reset();
        m_d3d12CommandQueue.Reset();
        m_d3d12Device.Reset();
    }

} // namespace WebViewToolkit

#endif // WEBVIEW_TOOLKIT_DX12_SUPPORT
