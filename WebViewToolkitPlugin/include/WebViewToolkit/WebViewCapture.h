#pragma once

#include "Types.h"
#include "RenderAPI.h"
#include <memory>
#include <mutex>
#include <winrt/Windows.Graphics.Capture.h>
#include <winrt/Windows.Graphics.DirectX.Direct3D11.h>
#include <winrt/Windows.UI.Composition.h>

namespace WebViewToolkit
{
    class WebView;

    /// <summary>
    /// Handles GraphicsCapture and Visual Composition for a WebView instance.
    /// Manages the bridge between WebView2's visual tree and Unity's texture.
    /// </summary>
    class WebViewCapture
    {
    public:
        WebViewCapture(WebView* webView, IRenderAPI* renderAPI);
        ~WebViewCapture();

        Result Initialize();
        void Shutdown();
        void UpdateTexture(void* unityTexturePtr);
        Result Resize(uint32_t width, uint32_t height);

    private:
        void InitializeVisualTree();
        void InitializeGraphicsCapture();

        WebView* m_webView; // Weak ref
        IRenderAPI* m_renderAPI; // Weak ref

        // WinRT Objects (Implementation details hidden in cpp usually, but for internal headers ok)
        // OR use void* to keep compilation fast/clean if we don't include winrt headers here.
        // For cleanup, sticking to void* pattern in headers is safer to avoid polluting global namespace with WinRT.
        
        void* m_compositor = nullptr;
        void* m_rootVisual = nullptr;    // IContainerVisual
        void* m_webViewVisual = nullptr; // IVisual
        void* m_windowTarget = nullptr;  // DesktopWindowTarget
        
        // Capture
        void* m_captureItem = nullptr;
        void* m_framePool = nullptr;
        void* m_session = nullptr;

        // Helpers
        void* m_d3dDevice = nullptr; // WinRT IDirect3DDevice
    };

} // namespace WebViewToolkit
