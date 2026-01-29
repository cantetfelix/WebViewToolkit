#pragma once

// ============================================================================
// WebViewToolkit - WebView Instance Manager
// ============================================================================
// Manages multiple WebView2 instances and their lifecycle.
// Uses CompositionController + GraphicsCapture for GPU-accelerated texture capture.
// ============================================================================

#include "Types.h"
#include "RenderAPI.h"
#include <memory>
#include <unordered_map>
#include <mutex>
#include <atomic>
#include <string>

namespace WebViewToolkit
{
    class WebView; // Forward declaration
    
    // ========================================================================
    // WebView Manager
    // ========================================================================
    // ========================================================================
    // WebView Manager
    // ========================================================================
    class WebViewManager
    {
    public:
        WebViewManager();
        ~WebViewManager();

        // ====================================================================
        // Initialization
        // ====================================================================
        
        Result Initialize(IRenderAPI* renderAPI);
        void Shutdown();
        bool IsInitialized() const { return m_initialized; }
        IRenderAPI* GetRenderAPI() const { return m_renderAPI; }

        // ====================================================================
        // WebView Lifecycle
        // ====================================================================
        
        Result CreateWebView(const WebViewCreateParams& params, WebViewHandle& outHandle);
        Result DestroyWebView(WebViewHandle handle);
        WebView* GetWebView(WebViewHandle handle);
        Result ResizeWebView(WebViewHandle handle, uint32_t width, uint32_t height);

        // ====================================================================
        // Navigation
        // ====================================================================
        
        Result Navigate(WebViewHandle handle, const wchar_t* url);
        Result NavigateToString(WebViewHandle handle, const wchar_t* html);
        Result ExecuteScript(WebViewHandle handle, const wchar_t* script);
        Result GoBack(WebViewHandle handle);
        Result GoForward(WebViewHandle handle);
        bool CanGoBack(WebViewHandle handle);
        bool CanGoForward(WebViewHandle handle);

        // ====================================================================
        // Input
        // ====================================================================
        
        Result SendMouseEvent(WebViewHandle handle, const MouseEventParams& event);
        Result SendKeyEvent(WebViewHandle handle, const KeyEventParams& event);

        // ====================================================================
        // Rendering
        // ====================================================================
        
        void UpdateTexture(WebViewHandle handle);
        void UpdateAllTextures();

        void OnDeviceLost();
        void OnDeviceRestored();

        // ====================================================================
        // Callbacks
        // ====================================================================
        
        void SetLogCallback(LogCallback callback) { m_logCallback = callback; }
        void SetNavigationCallback(NavigationCallback callback) { m_navigationCallback = callback; }
        void SetMessageCallback(MessageCallback callback) { m_messageCallback = callback; }
        void SetDeviceEventCallback(DeviceEventCallback callback) { m_deviceEventCallback = callback; }

        static bool IsShuttingDown();
        static void SignalShuttingDown();

        // Internal access
        void Log(int32_t level, const char* message);
        void LogW(int32_t level, const wchar_t* message);

        // Callback triggers (for WebView instances)
        void InvokeNavigationCallback(WebViewHandle handle, const wchar_t* url, bool isSuccess);
        void InvokeMessageCallback(WebViewHandle handle, const wchar_t* message);

    private:
        WebViewHandle GenerateHandle();

        // Internal initialization helpers
        Result InitializeWinRT();
        
        bool m_initialized = false;
        bool m_shutdownComplete = false;
        IRenderAPI* m_renderAPI = nullptr;

        // WinRT dispatcher queue (required for Compositor)
        // STATIC: Persists across WebViewManager instances to allow re-initialization
        static void* s_dispatcherQueueController;
        static std::atomic<bool> s_isShuttingDown;

        std::mutex m_mutex;
        
        // New: Map of Handles to WebView objects
        std::unordered_map<WebViewHandle, std::unique_ptr<WebView>> m_instances;
        
        WebViewHandle m_nextHandle = 1;

        // Callbacks
        LogCallback m_logCallback = nullptr;
        NavigationCallback m_navigationCallback = nullptr;
        MessageCallback m_messageCallback = nullptr;
        DeviceEventCallback m_deviceEventCallback = nullptr;
    };

} // namespace WebViewToolkit
