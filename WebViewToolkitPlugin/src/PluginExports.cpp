// ============================================================================
// WebViewToolkit - C API Exports for Unity Interop
// ============================================================================

// DirectX headers MUST be included before Unity headers
#include <d3d11.h>
#include <d3d12.h>

#include "WebViewToolkit/Plugin.h"
#include "WebViewToolkit/RenderAPI.h"
#include "WebViewToolkit/WebViewManager.h"
#include "WebViewToolkit/WebView.h"

// Unity Plugin API
#include "IUnityInterface.h"
#include "IUnityGraphics.h"

// Forward declarations from Plugin.cpp
namespace WebViewToolkit
{
    Result Initialize(GraphicsAPI api);
    void Shutdown();
    bool IsInitialized();
    WebViewManager* GetWebViewManager();
    void SetLogCallback(LogCallback callback);
    void SetNavigationCallback(NavigationCallback callback);
    void SetMessageCallback(MessageCallback callback);
    void SetDeviceEventCallback(DeviceEventCallback callback);
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc();
extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventAndDataFunc();

// ============================================================================
// Callback Setters
// ============================================================================

WEBVIEW_EXPORT void WebViewToolkit_SetLogCallback(WebViewToolkit::LogCallback callback)
{
    WebViewToolkit::SetLogCallback(callback);
}

WEBVIEW_EXPORT void WebViewToolkit_SetNavigationCallback(WebViewToolkit::NavigationCallback callback)
{
    WebViewToolkit::SetNavigationCallback(callback);
}

WEBVIEW_EXPORT void WebViewToolkit_SetMessageCallback(WebViewToolkit::MessageCallback callback)
{
    WebViewToolkit::SetMessageCallback(callback);
}

WEBVIEW_EXPORT void WebViewToolkit_SetDeviceEventCallback(WebViewToolkit::DeviceEventCallback callback)
{
    WebViewToolkit::SetDeviceEventCallback(callback);
}

// ============================================================================
// Initialization
// ============================================================================

WEBVIEW_EXPORT int32_t WebViewToolkit_Initialize(int32_t graphicsAPI)
{
    auto api = static_cast<WebViewToolkit::GraphicsAPI>(graphicsAPI);
    return static_cast<int32_t>(WebViewToolkit::Initialize(api));
}

WEBVIEW_EXPORT void WebViewToolkit_Shutdown()
{
    WebViewToolkit::Shutdown();
}

WEBVIEW_EXPORT void WebViewToolkit_SignalApplicationQuit()
{
    // Set the shutdown flag early, before C# destroys individual instances
    // This ensures ReleaseWebViewInstance does abandoned cleanup (no async callbacks)
    WebViewToolkit::WebViewManager::SignalShuttingDown();
}

WEBVIEW_EXPORT int32_t WebViewToolkit_IsInitialized()
{
    return WebViewToolkit::IsInitialized() ? 1 : 0;
}

// ============================================================================
// WebView Management
// ============================================================================

WEBVIEW_EXPORT int32_t WebViewToolkit_CreateWebView(
    uint32_t width,
    uint32_t height,
    const wchar_t* userDataFolder,
    const wchar_t* initialUrl,
    int32_t enableDevTools,
    uint32_t* outHandle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager || !outHandle)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    WebViewToolkit::WebViewCreateParams params = {};
    params.width = width;
    params.height = height;
    params.userDataFolder = userDataFolder;
    params.initialUrl = initialUrl;
    params.enableDevTools = enableDevTools != 0;

    WebViewToolkit::WebViewHandle handle;
    auto result = manager->CreateWebView(params, handle);
    
    if (result == WebViewToolkit::Result::Success)
    {
        *outHandle = handle;
    }

    return static_cast<int32_t>(result);
}

WEBVIEW_EXPORT int32_t WebViewToolkit_DestroyWebView(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->DestroyWebView(handle));
}

WEBVIEW_EXPORT void* WebViewToolkit_GetTexturePtr(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return nullptr;
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return nullptr;
    }

    auto webView = manager->GetWebView(handle);
    return webView ? webView->GetTexturePtr() : nullptr;
}

WEBVIEW_EXPORT int32_t WebViewToolkit_Resize(uint32_t handle, uint32_t width, uint32_t height)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->ResizeWebView(handle, width, height));
}

// ============================================================================
// Navigation
// ============================================================================

WEBVIEW_EXPORT int32_t WebViewToolkit_Navigate(uint32_t handle, const wchar_t* url)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->Navigate(handle, url));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_NavigateToString(uint32_t handle, const wchar_t* html)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->NavigateToString(handle, html));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_ExecuteScript(uint32_t handle, const wchar_t* script)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->ExecuteScript(handle, script));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_GoBack(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->GoBack(handle));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_GoForward(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    return static_cast<int32_t>(manager->GoForward(handle));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_CanGoBack(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return 0;
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return 0;
    }

    return manager->CanGoBack(handle) ? 1 : 0;
}

WEBVIEW_EXPORT int32_t WebViewToolkit_CanGoForward(uint32_t handle)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return 0;
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return 0;
    }

    return manager->CanGoForward(handle) ? 1 : 0;
}

// ============================================================================
// Input
// ============================================================================

WEBVIEW_EXPORT int32_t WebViewToolkit_SendMouseEvent(
    uint32_t handle,
    int32_t eventType,
    int32_t button,
    float x,
    float y,
    float wheelDelta)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    WebViewToolkit::MouseEventParams params = {};
    params.type = static_cast<WebViewToolkit::MouseEventType>(eventType);
    params.button = static_cast<WebViewToolkit::MouseButton>(button);
    params.x = x;
    params.y = y;
    params.wheelDelta = wheelDelta;

    return static_cast<int32_t>(manager->SendMouseEvent(handle, params));
}

WEBVIEW_EXPORT int32_t WebViewToolkit_SendKeyEvent(
    uint32_t handle,
    uint32_t virtualKeyCode,
    uint32_t scanCode,
    int32_t isKeyDown,
    int32_t isSystemKey)
{
    if (WebViewToolkit::WebViewManager::IsShuttingDown())
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    auto manager = WebViewToolkit::GetWebViewManager();
    if (!manager)
    {
        return static_cast<int32_t>(WebViewToolkit::Result::ErrorNotInitialized);
    }

    WebViewToolkit::KeyEventParams params = {};
    params.virtualKeyCode = virtualKeyCode;
    params.scanCode = scanCode;
    params.isKeyDown = isKeyDown != 0;
    params.isSystemKey = isSystemKey != 0;

    return static_cast<int32_t>(manager->SendKeyEvent(handle, params));
}

// ============================================================================
// Render Events
// ============================================================================

WEBVIEW_EXPORT void* WebViewToolkit_GetRenderEventFunc()
{
    return reinterpret_cast<void*>(GetRenderEventFunc());
}

WEBVIEW_EXPORT void* WebViewToolkit_GetRenderEventAndDataFunc()
{
    return reinterpret_cast<void*>(GetRenderEventAndDataFunc());
}
