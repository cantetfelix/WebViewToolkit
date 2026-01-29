#pragma once

// ============================================================================
// WebViewToolkit - Plugin Public Interface
// ============================================================================
// This header defines the C API exposed to Unity for interoperability.
// ============================================================================

#include "Types.h"

WEBVIEW_EXPORT void WebViewToolkit_SetLogCallback(WebViewToolkit::LogCallback callback);
WEBVIEW_EXPORT void WebViewToolkit_SetNavigationCallback(WebViewToolkit::NavigationCallback callback);
WEBVIEW_EXPORT void WebViewToolkit_SetMessageCallback(WebViewToolkit::MessageCallback callback);

// ============================================================================
// Initialization
// ============================================================================

/// @brief Initialize the plugin with the specified graphics API
/// @param graphicsAPI Graphics API type (2 = DX11, 18 = DX12)
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_Initialize(int32_t graphicsAPI);

/// @brief Shutdown the plugin and release all resources
WEBVIEW_EXPORT void WebViewToolkit_Shutdown();

/// @brief Check if the plugin is initialized
/// @return 1 if initialized, 0 otherwise
WEBVIEW_EXPORT int32_t WebViewToolkit_IsInitialized();

// ============================================================================
// WebView Management
// ============================================================================

/// @brief Create a new WebView instance
/// @param width Width in pixels
/// @param height Height in pixels
/// @param userDataFolder User data folder path (can be null)
/// @param initialUrl Initial URL to load (can be null)
/// @param enableDevTools Enable developer tools
/// @param outHandle [out] Handle to the created instance
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_CreateWebView(
    uint32_t width,
    uint32_t height,
    const wchar_t* userDataFolder,
    const wchar_t* initialUrl,
    int32_t enableDevTools,
    uint32_t* outHandle
);

/// @brief Destroy a WebView instance
/// @param handle Instance handle
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_DestroyWebView(uint32_t handle);

/// @brief Get the native texture pointer for a WebView
/// @param handle Instance handle
/// @return Native texture pointer, or nullptr on failure
WEBVIEW_EXPORT void* WebViewToolkit_GetTexturePtr(uint32_t handle);

/// @brief Resize a WebView instance
/// @param handle Instance handle
/// @param width New width in pixels
/// @param height New height in pixels
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_Resize(uint32_t handle, uint32_t width, uint32_t height);

// ============================================================================
// Navigation
// ============================================================================

/// @brief Navigate to a URL
/// @param handle Instance handle
/// @param url URL to navigate to
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_Navigate(uint32_t handle, const wchar_t* url);

/// @brief Navigate to HTML content
/// @param handle Instance handle
/// @param html HTML content string
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_NavigateToString(uint32_t handle, const wchar_t* html);

/// @brief Execute JavaScript in the WebView
/// @param handle Instance handle
/// @param script JavaScript code to execute
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_ExecuteScript(uint32_t handle, const wchar_t* script);

// ============================================================================
// Input
// ============================================================================

/// @brief Send a mouse event to the WebView
/// @param handle Instance handle
/// @param eventType Mouse event type (0=Move, 1=Down, 2=Up, 3=Wheel, 4=Leave)
/// @param button Mouse button (0=None, 1=Left, 2=Right, 3=Middle)
/// @param x Normalized X coordinate [0, 1]
/// @param y Normalized Y coordinate [0, 1]
/// @param wheelDelta Wheel delta for wheel events
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_SendMouseEvent(
    uint32_t handle,
    int32_t eventType,
    int32_t button,
    float x,
    float y,
    float wheelDelta
);

/// @brief Send a keyboard event to the WebView
/// @param handle Instance handle
/// @param virtualKeyCode Virtual key code
/// @param scanCode Scan code
/// @param isKeyDown 1 if key down, 0 if key up
/// @param isSystemKey 1 if system key, 0 otherwise
/// @return Result code
WEBVIEW_EXPORT int32_t WebViewToolkit_SendKeyEvent(
    uint32_t handle,
    uint32_t virtualKeyCode,
    uint32_t scanCode,
    int32_t isKeyDown,
    int32_t isSystemKey
);

// ============================================================================
// Render Events (for GL.IssuePluginEvent)
// ============================================================================

/// @brief Get the render event callback function pointer
/// @return Function pointer for use with GL.IssuePluginEvent
WEBVIEW_EXPORT void* WebViewToolkit_GetRenderEventFunc();

/// @brief Get the render event callback with data support
/// @return Function pointer for use with GL.IssuePluginEventAndData
WEBVIEW_EXPORT void* WebViewToolkit_GetRenderEventAndDataFunc();
