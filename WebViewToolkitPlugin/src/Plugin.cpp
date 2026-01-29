// ============================================================================
// WebViewToolkit - Plugin Core Implementation
// ============================================================================

#include "WebViewToolkit/Plugin.h"
#include "WebViewToolkit/RenderAPI.h"
#include "WebViewToolkit/WebViewManager.h"


// DirectX headers MUST be included before Unity headers
#include <d3d11.h>
#include <d3d12.h>

// Unity Plugin API
#include "IUnityInterface.h"
#include "IUnityGraphics.h"

#include <memory>
#include <mutex>

namespace WebViewToolkit
{
    // ========================================================================
    // Global State
    // ========================================================================
    namespace
    {
        std::unique_ptr<IRenderAPI> g_renderAPI;
        std::unique_ptr<WebViewManager> g_webViewManager;
        IUnityInterfaces* g_unityInterfaces = nullptr;
        GraphicsAPI g_currentAPI = GraphicsAPI::Unknown;
        // Intentional leak to prevent destruction order crashes
        std::mutex* g_mutex = new std::mutex();

        LogCallback g_logCallback = nullptr;
        NavigationCallback g_navigationCallback = nullptr;
        MessageCallback g_messageCallback = nullptr;

        void Log(int32_t level, const char* message)
        {
            if (g_logCallback)
            {
                g_logCallback(level, message);
            }
        }
    }

    // ========================================================================
    // Unity Graphics Event Callback
    // ========================================================================
    static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
    {
        std::lock_guard<std::mutex> lock(*g_mutex);

        if (g_renderAPI)
        {
            g_renderAPI->ProcessDeviceEvent(static_cast<int>(eventType), g_unityInterfaces);
        }

        switch (eventType)
        {
        case kUnityGfxDeviceEventInitialize:
            Log(0, "WebViewToolkit: Graphics device initialized");
            if (g_webViewManager)
            {
                g_webViewManager->OnDeviceRestored();
            }
            break;
 
        case kUnityGfxDeviceEventBeforeReset:
            Log(1, "WebViewToolkit: Graphics device reset starting (BeforeReset)");
            if (g_webViewManager)
            {
                g_webViewManager->OnDeviceLost();
            }
            break;

        case kUnityGfxDeviceEventAfterReset:
             Log(0, "WebViewToolkit: Graphics device reset finished (AfterReset)");
             if (g_webViewManager)
             {
                 g_webViewManager->OnDeviceRestored();
             }
             break;

        case kUnityGfxDeviceEventShutdown:
            Log(0, "WebViewToolkit: Graphics device shutdown");
            break;

        default:
            break;
        }
    }

    // ========================================================================
    // Render Event Callbacks
    // ========================================================================
    static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
    {
        auto eventType = static_cast<RenderEventType>(eventID);

        std::lock_guard<std::mutex> lock(*g_mutex);

        switch (eventType)
        {
        case RenderEventType::UpdateTexture:
            // Check existence, initialization, AND that we're not shutting down
            if (g_webViewManager &&
                !WebViewToolkit::WebViewManager::IsShuttingDown() &&
                g_webViewManager->IsInitialized())
            {
                g_webViewManager->UpdateAllTextures();
            }
            break;

        default:
            break;
        }
    }

    static void UNITY_INTERFACE_API OnRenderEventAndData(int eventID, void* data)
    {
        auto eventType = static_cast<RenderEventType>(eventID);

        std::lock_guard<std::mutex> lock(*g_mutex);

        switch (eventType)
        {
        case RenderEventType::UpdateTexture:
            if (g_webViewManager && data)
            {
                auto handle = static_cast<WebViewHandle>(reinterpret_cast<uintptr_t>(data));
                g_webViewManager->UpdateTexture(handle);
            }
            break;

        default:
            break;
        }
    }

    // ========================================================================
    // Public API Implementation
    // ========================================================================

    Result Initialize(GraphicsAPI api)
    {
        std::lock_guard<std::mutex> lock(*g_mutex);

        if (g_renderAPI)
        {
            // Reset shutdown flag in manager if it exists
            if (g_webViewManager)
            {
                g_webViewManager->Initialize(g_renderAPI.get());
            }
            return Result::ErrorAlreadyInitialized;
        }

        g_currentAPI = api;
        g_renderAPI = CreateRenderAPI(api);

        if (!g_renderAPI)
        {
            Log(2, "WebViewToolkit: Failed to create render API - unsupported graphics API");
            return Result::ErrorUnsupportedGraphicsAPI;
        }

        // Process initialize event if Unity interfaces are available
        if (g_unityInterfaces)
        {
            g_renderAPI->ProcessDeviceEvent(
                static_cast<int>(kUnityGfxDeviceEventInitialize),
                g_unityInterfaces
            );
        }

        // Create WebView manager
        g_webViewManager = std::make_unique<WebViewManager>();
        auto result = g_webViewManager->Initialize(g_renderAPI.get());

        if (result != Result::Success)
        {
            Log(2, "WebViewToolkit: Failed to initialize WebView manager");
            g_webViewManager.reset();
            g_renderAPI.reset();
            return result;
        }

        // Set callbacks
        if (g_navigationCallback) g_webViewManager->SetNavigationCallback(g_navigationCallback);
        if (g_messageCallback) g_webViewManager->SetMessageCallback(g_messageCallback);

        Log(0, "WebViewToolkit: Initialized successfully");
        return Result::Success;
    }

    void Shutdown()
    {
        std::lock_guard<std::mutex> lock(*g_mutex);

        // If not initialized, nothing to do
        if (!g_webViewManager && !g_renderAPI)
        {
            Log(0, "WebViewToolkit: Plugin shutdown already complete or not initialized, skipping");
            return;
        }

        if (g_webViewManager)
        {
            g_webViewManager->Shutdown();
            // Intentional leak to avoid use-after-free during process destruction
            // The OS will clean up the process memory, preventing crashes from late async callbacks
            g_webViewManager.release();
        }

        if (g_renderAPI)
        {
            // Intentional leak - see above
            g_renderAPI.release();
        }
        
        g_webViewManager = nullptr;
        g_renderAPI = nullptr;

        g_currentAPI = GraphicsAPI::Unknown;
        
        // Clear callbacks
        g_logCallback = nullptr;
        g_navigationCallback = nullptr; 
        g_messageCallback = nullptr;

        Log(0, "WebViewToolkit: Shutdown complete"); 
    }

    bool IsInitialized()
    {
        std::lock_guard<std::mutex> lock(*g_mutex);
        return g_renderAPI != nullptr && g_renderAPI->IsInitialized();
    }

    WebViewManager* GetWebViewManager()
    {
        return g_webViewManager.get();
    }

    IRenderAPI* GetRenderAPI()
    {
        return g_renderAPI.get();
    }

    void SetLogCallback(LogCallback callback)
    {
        g_logCallback = callback;
        if (g_webViewManager)
        {
            g_webViewManager->SetLogCallback(callback);
        }
    }

    void SetNavigationCallback(NavigationCallback callback)
    {
        g_navigationCallback = callback;
        if (g_webViewManager)
        {
            g_webViewManager->SetNavigationCallback(callback);
        }
    }

    void SetMessageCallback(MessageCallback callback)
    {
        g_messageCallback = callback;
        if (g_webViewManager)
        {
            g_webViewManager->SetMessageCallback(callback);
        }
    }

    void SetDeviceEventCallback(DeviceEventCallback callback)
    {
        if (g_webViewManager)
        {
            g_webViewManager->SetDeviceEventCallback(callback);
        }
    }

} // namespace WebViewToolkit

// ============================================================================
// Unity Plugin Load/Unload Entry Points
// ============================================================================

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    WebViewToolkit::g_unityInterfaces = unityInterfaces;

    // =========================================================================
    // CRITICAL: Pin this DLL in memory to prevent unload during async callbacks
    // =========================================================================
    // WebView2 and GraphicsCapture use threadpool threads for IPC callbacks.
    // When Unity closes, it calls UnityPluginUnload and then unloads DLLs.
    // However, pending WebView2 callbacks may still be queued on threadpool threads.
    // If the DLL is unloaded before these callbacks complete, they crash when trying
    // to execute code that no longer exists in memory.
    //
    // Solution: Use GET_MODULE_HANDLE_EX_FLAG_PIN to permanently pin this DLL.
    // The DLL will stay in memory until process termination, ensuring all pending
    // callbacks have valid code to execute.
    HMODULE selfModule = nullptr;
    if (GetModuleHandleExW(
            GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_PIN,
            reinterpret_cast<LPCWSTR>(&UnityPluginLoad),
            &selfModule))
    {
        OutputDebugStringA("[WebViewToolkit] DLL pinned in memory - will not unload until process exit\n");
    }
    else
    {
        OutputDebugStringA("[WebViewToolkit] WARNING: Failed to pin DLL in memory\n");
    }

    auto graphics = unityInterfaces->Get<IUnityGraphics>();
    if (graphics)
    {
        graphics->RegisterDeviceEventCallback(WebViewToolkit::OnGraphicsDeviceEvent);

        // Run initialization if device already exists
        auto deviceType = graphics->GetRenderer();
        if (deviceType != kUnityGfxRendererNull)
        {
            WebViewToolkit::OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
        }
    }
}

// Static flag to indicate we're in shutdown mode
static bool s_inShutdown = false;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    // DO NOTHING - let OS clean up on process exit
    // Following the pattern from UnityGraphicsCapture and Microsoft's guidance:
    // "When the building is being demolished, don't bother sweeping the floor"
    //
    // Attempting cleanup here causes crashes from late async callbacks that:
    // - Try to access freed WinRT apartments after uninit_apartment()
    // - Invoke COM destructors that expect valid apartments
    // - Access freed memory after Release() on COM objects
    //
    // The OS will automatically clean up:
    // - All COM objects and apartments
    // - All WinRT resources
    // - All graphics handles
    // - The WebView2 browser subprocess
    //
    // This is the ONLY safe approach for main thread cleanup.

    OutputDebugStringA("[WebViewToolkit] UnityPluginUnload - no cleanup (OS will handle it)\n");
}

// ============================================================================
// Render Event Function Getters
// ============================================================================

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return WebViewToolkit::OnRenderEvent;
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventAndDataFunc()
{
    return WebViewToolkit::OnRenderEventAndData;
}
