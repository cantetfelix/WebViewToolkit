#pragma once

// ============================================================================
// WebViewToolkit - DirectX 11 Render API Implementation
// ============================================================================

#include "WebViewToolkit/RenderAPI.h"

#include <d3d11.h>
#include <dcomp.h>
#include <wrl/client.h>

using Microsoft::WRL::ComPtr;

namespace WebViewToolkit
{
    class RenderAPI_D3D11 final : public IRenderAPI
    {
    public:
        RenderAPI_D3D11();
        ~RenderAPI_D3D11() override;

        // IRenderAPI implementation
        void ProcessDeviceEvent(int eventType, IUnityInterfaces* interfaces) override;
        bool IsInitialized() const override { return m_device != nullptr; }
        GraphicsAPI GetAPIType() const override { return GraphicsAPI::Direct3D11; }

        Result CreateSharedTexture(uint32_t width, uint32_t height, void** outNativePtr) override;
        void DestroySharedTexture(void* nativePtr) override;
        Result ResizeSharedTexture(void* nativePtr, uint32_t newWidth, uint32_t newHeight, void** outNewNativePtr) override;

        void BeginRenderToTexture(void* texturePtr) override;
        void EndRenderToTexture(void* texturePtr) override;

        void* GetCompositionDevice() const override { return m_compositionDevice.Get(); }
        void* GetD3D11Device() const override { return m_device.Get(); }

        void WaitForGPU() override;
        void SignalRenderComplete() override;

        void CopyCapturedTextureToUnityTexture(void* capturedTexture, void* unityTexturePtr, bool flipY) override;

    private:
        Result InitializeCompositionDevice();
        void ReleaseResources();

        ComPtr<ID3D11Device> m_device;
        ComPtr<ID3D11DeviceContext> m_context;
        ComPtr<IDCompositionDevice> m_compositionDevice;
    };

} // namespace WebViewToolkit
