#pragma once

// ============================================================================
// WebViewToolkit - DirectX 12 Render API Implementation
// ============================================================================
// Uses D3D11On12 to provide WebView2 compatibility with Unity's DX12 backend.
// ============================================================================

#include "WebViewToolkit/RenderAPI.h"

#ifdef WEBVIEW_TOOLKIT_DX12_SUPPORT

#include <d3d12.h>
#include <d3d11on12.h>
#include <d3d11.h>
#include <dcomp.h>
#include <wrl/client.h>
#include <unordered_map>

using Microsoft::WRL::ComPtr;

namespace WebViewToolkit
{
    class RenderAPI_D3D12 final : public IRenderAPI
    {
    public:
        RenderAPI_D3D12();
        ~RenderAPI_D3D12() override;

        // IRenderAPI implementation
        void ProcessDeviceEvent(int eventType, IUnityInterfaces* interfaces) override;
        bool IsInitialized() const override { return m_d3d12Device != nullptr && m_d3d11Device != nullptr; }
        GraphicsAPI GetAPIType() const override { return GraphicsAPI::Direct3D12; }

        Result CreateSharedTexture(uint32_t width, uint32_t height, void** outNativePtr) override;
        void DestroySharedTexture(void* nativePtr) override;
        Result ResizeSharedTexture(void* nativePtr, uint32_t newWidth, uint32_t newHeight, void** outNewNativePtr) override;

        void BeginRenderToTexture(void* texturePtr) override;
        void EndRenderToTexture(void* texturePtr) override;

        void* GetCompositionDevice() const override { return m_compositionDevice.Get(); }
        void* GetD3D11Device() const override { return m_d3d11Device.Get(); }
        void* GetCaptureD3D11Device() const override { return m_captureD3D11Device.Get(); }

        void WaitForGPU() override;
        void SignalRenderComplete() override;

        void CopyCapturedTextureToUnityTexture(void* capturedTexture, void* unityTexturePtr, bool flipY) override;

    private:
        Result InitializeD3D11On12();
        Result InitializeCaptureDevice();
        Result InitializeCompositionDevice();
        Result CreateFence();
        void ReleaseResources();

        // Track wrapped resources for state transitions
        struct WrappedResource
        {
            ComPtr<ID3D12Resource> d3d12Resource;
            ComPtr<ID3D11Resource> d3d11Resource;
        };

        WrappedResource* GetOrCreateWrappedResource(void* d3d12TexturePtr);

        // D3D12 resources (from Unity)
        ComPtr<ID3D12Device> m_d3d12Device;
        ComPtr<ID3D12CommandQueue> m_d3d12CommandQueue;

        // D3D11On12 wrapper
        ComPtr<ID3D11Device> m_d3d11Device;
        ComPtr<ID3D11DeviceContext> m_d3d11Context;
        ComPtr<ID3D11On12Device> m_d3d11On12Device;

        // Standalone D3D11 device for Windows Graphics Capture API
        ComPtr<ID3D11Device> m_captureD3D11Device;
        ComPtr<ID3D11DeviceContext> m_captureD3D11Context;

        // DirectComposition
        ComPtr<IDCompositionDevice> m_compositionDevice;

        // Synchronization
        ComPtr<ID3D12Fence> m_fence;
        HANDLE m_fenceEvent = nullptr;
        uint64_t m_fenceValue = 0;

        // Resource tracking
        std::unordered_map<void*, std::unique_ptr<WrappedResource>> m_wrappedResources;
    };

} // namespace WebViewToolkit

#endif // WEBVIEW_TOOLKIT_DX12_SUPPORT
