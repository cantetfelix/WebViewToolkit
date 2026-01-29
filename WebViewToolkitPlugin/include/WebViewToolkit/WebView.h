#pragma once

#include "Types.h"
#include <memory>
#include <string>
#include <atomic>
#include <mutex>

namespace WebViewToolkit
{
    class WebViewManager;
    class WebViewCapture;

    // Instance state enumeration
    enum class WebViewState : int32_t
    {
        Uninitialized = 0,
        CreatingEnvironment,
        CreatingController,
        Ready,
        Error,
        Destroyed
    };

    /// <summary>
    /// Represents a single WebView2 instance.
    /// Manages the CoreWebView2 objects and the host window.
    /// </summary>
    class WebView
    {
    public:
        WebView(WebViewHandle handle, const WebViewCreateParams& params, WebViewManager* manager);
        ~WebView();

        // No copying
        WebView(const WebView&) = delete;
        WebView& operator=(const WebView&) = delete;

        Result Initialize();
        void Shutdown();

        // Getters
        WebViewHandle GetHandle() const { return m_handle; }
        uint32_t GetWidth() const { return m_width; }
        uint32_t GetHeight() const { return m_height; }
        WebViewState GetState() const { return m_state; }
        bool IsReady() const { return m_state == WebViewState::Ready; }
        
        // Internal access for friendly classes
        void* GetController() const { return m_controller; }
        void* GetHostWindow() const { return m_hostWindow; }
        void* GetCompositionController() const { return m_compositionController; }
        void* GetEnvironment() const { return m_environment; }

        // Navigation
        Result Navigate(const wchar_t* url);
        Result NavigateToString(const wchar_t* html);
        Result ExecuteScript(const wchar_t* script);
        Result GoBack();
        Result GoForward();
        bool CanGoBack();
        bool CanGoForward();

        // Lifecycle
        Result Resize(uint32_t width, uint32_t height);

        // Input
        Result SendMouseEvent(const MouseEventParams& params);
        Result SendKeyEvent(const KeyEventParams& params);

        void OnDeviceLost();
        void OnDeviceRestored();

    private:
        Result InitializeWebViewEnvironment();
        Result InitializeCompositionController();
        void OnEnvironmentCreated(long result, void* environment);
        void OnCompositionControllerCreated(long result, void* compositionController);
        
        // Host window management
        void* CreateHostWindow(uint32_t width, uint32_t height);
        void DestroyHostWindow();

        WebViewHandle m_handle;
        WebViewManager* m_manager; // Weak ref

        uint32_t m_width;
        uint32_t m_height;
        std::wstring m_userDataFolder;
        std::wstring m_pendingUrl;
        bool m_devToolsEnabled;

        std::atomic<WebViewState> m_state{ WebViewState::Uninitialized };
        
        // Native objects (void* to avoid header pollution)
        void* m_environment = nullptr;           // ICoreWebView2Environment*
        void* m_compositionController = nullptr; // ICoreWebView2CompositionController*
        void* m_controller = nullptr;            // ICoreWebView2Controller*
        void* m_webView = nullptr;               // ICoreWebView2*
        void* m_hostWindow = nullptr;            // HWND

        // Friend access for capture manager which needs deep access to composition visual logic
        friend class WebViewCapture; 
        
        std::unique_ptr<WebViewCapture> m_capture;

    public:
        // Added for Manager delegation
        void UpdateTexture();
        void* GetTexturePtr() const { return m_texturePtr; }

    private:
        void* m_texturePtr = nullptr; // Shared texture
    };

} // namespace WebViewToolkit
