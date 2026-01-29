#pragma once

// ============================================================================
// WebViewToolkit - Abstract Render API Interface
// ============================================================================
// This interface abstracts DirectX 11 and DirectX 12 implementations,
// allowing WebView2 to render to Unity textures regardless of the graphics API.
// ============================================================================

#include "Types.h"
#include <memory>

// Forward declarations for Unity types
struct IUnityInterfaces;

namespace WebViewToolkit
{
    // Forward declarations
    class WebViewInstance;

    // ========================================================================
    // Abstract Render API Interface
    // ========================================================================
    class IRenderAPI
    {
    public:
        virtual ~IRenderAPI() = default;

        // ====================================================================
        // Lifecycle
        // ====================================================================
        
        /// @brief Process Unity graphics device events (create, destroy, etc.)
        /// @param eventType The type of device event
        /// @param interfaces Unity interfaces for accessing device resources
        virtual void ProcessDeviceEvent(int eventType, IUnityInterfaces* interfaces) = 0;

        /// @brief Check if the render API is properly initialized
        virtual bool IsInitialized() const = 0;

        /// @brief Get the current graphics API type
        virtual GraphicsAPI GetAPIType() const = 0;

        // ====================================================================
        // Texture Management
        // ====================================================================
        
        /// @brief Create a shared texture for WebView rendering
        /// @param width Texture width in pixels
        /// @param height Texture height in pixels
        /// @param outNativePtr [out] Native texture pointer for Unity
        /// @return Result code
        virtual Result CreateSharedTexture(uint32_t width, uint32_t height, void** outNativePtr) = 0;

        /// @brief Destroy a previously created shared texture
        /// @param nativePtr Native texture pointer
        virtual void DestroySharedTexture(void* nativePtr) = 0;

        /// @brief Resize an existing shared texture
        /// @param nativePtr Current native texture pointer
        /// @param newWidth New width in pixels
        /// @param newHeight New height in pixels
        /// @param outNewNativePtr [out] New native texture pointer (may differ from input)
        /// @return Result code
        virtual Result ResizeSharedTexture(void* nativePtr, uint32_t newWidth, uint32_t newHeight, void** outNewNativePtr) = 0;

        // ====================================================================
        // WebView Rendering
        // ====================================================================
        
        /// @brief Begin rendering frame - called before WebView updates
        /// @param texturePtr Native texture pointer
        /// @note For DX12, this handles resource barrier transitions
        virtual void BeginRenderToTexture(void* texturePtr) = 0;

        /// @brief End rendering frame - called after WebView updates
        /// @param texturePtr Native texture pointer
        /// @note For DX12, this restores resource state for Unity
        virtual void EndRenderToTexture(void* texturePtr) = 0;

        /// @brief Get the composition device for WebView2
        /// @return Pointer to IDCompositionDevice (DX11) or wrapped device (DX12)
        virtual void* GetCompositionDevice() const = 0;

        /// @brief Get the D3D11 device (direct for DX11, wrapped via D3D11On12 for DX12)
        /// @return Pointer to ID3D11Device
        virtual void* GetD3D11Device() const = 0;

        /// @brief Get the D3D11 device for Windows Graphics Capture API
        /// @return Pointer to ID3D11Device (standalone device for DX12, same as GetD3D11Device() for DX11)
        /// @note For DX12, returns a separate standalone D3D11 device to work around capture frame callback limitations
        virtual void* GetCaptureD3D11Device() const { return GetD3D11Device(); }

        // ====================================================================
        // Synchronization (DX12 specific, no-op on DX11)
        // ====================================================================
        
        /// @brief Wait for GPU operations to complete
        /// @note Critical for DX12 to prevent flickering
        virtual void WaitForGPU() = 0;

        /// @brief Signal that WebView rendering is complete
        virtual void SignalRenderComplete() = 0;

        // ====================================================================
        // Texture Copying (for Windows Graphics Capture API)
        // ====================================================================

        /// @brief Copy a captured texture to Unity's texture
        /// @param capturedTexture Texture from Windows Graphics Capture API
        /// @param unityTexturePtr Unity's native texture pointer
        /// @param flipY Whether to flip Y coordinates during copy
        /// @note For DX12, handles cross-device copy and texture wrapping
        virtual void CopyCapturedTextureToUnityTexture(void* capturedTexture, void* unityTexturePtr, bool flipY) = 0;

    protected:
        IRenderAPI() = default;
        
        // Non-copyable
        IRenderAPI(const IRenderAPI&) = delete;
        IRenderAPI& operator=(const IRenderAPI&) = delete;
    };

    // ========================================================================
    // Factory Function
    // ========================================================================
    
    /// @brief Create a render API implementation for the specified graphics API
    /// @param api The target graphics API (DX11 or DX12)
    /// @return Unique pointer to the render API implementation, or nullptr on failure
    std::unique_ptr<IRenderAPI> CreateRenderAPI(GraphicsAPI api);

} // namespace WebViewToolkit
