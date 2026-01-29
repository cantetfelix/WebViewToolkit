#pragma once

// ============================================================================
// WebViewToolkit - Type Definitions
// ============================================================================

#include <cstdint>

// DLL Export/Import macros
#ifdef WEBVIEW_TOOLKIT_EXPORTS
    #define WEBVIEW_API __declspec(dllexport)
#else
    #define WEBVIEW_API __declspec(dllimport)
#endif

// C-linkage for Unity interop
#define WEBVIEW_EXTERN extern "C"
#define WEBVIEW_EXPORT WEBVIEW_EXTERN WEBVIEW_API

namespace WebViewToolkit
{
    // ========================================================================
    // Graphics API Enumeration
    // ========================================================================
    enum class GraphicsAPI : int32_t
    {
        Unknown = 0,
        Direct3D11 = 2,     // Matches Unity's GraphicsDeviceType.Direct3D11
        Direct3D12 = 18,    // Matches Unity's GraphicsDeviceType.Direct3D12
    };

    // ========================================================================
    // WebView Instance Handle
    // ========================================================================
    using WebViewHandle = uint32_t;
    constexpr WebViewHandle InvalidWebViewHandle = 0;

    // ========================================================================
    // Result Codes
    // ========================================================================
    enum class Result : int32_t
    {
        Success = 0,
        
        // General errors
        ErrorUnknown = -1,
        ErrorInvalidHandle = -2,
        ErrorNotInitialized = -3,
        ErrorAlreadyInitialized = -4,
        
        // Graphics errors
        ErrorUnsupportedGraphicsAPI = -100,
        ErrorDeviceCreationFailed = -101,
        ErrorTextureCreationFailed = -102,
        ErrorResourceBarrierFailed = -103,
        
        // WebView errors
        ErrorWebViewCreationFailed = -200,
        ErrorCompositionFailed = -201,
        ErrorNavigationFailed = -202,
    };

    // ========================================================================
    // WebView Creation Parameters
    // ========================================================================
    struct WebViewCreateParams
    {
        uint32_t width;
        uint32_t height;
        const wchar_t* userDataFolder;      // Can be nullptr for default
        const wchar_t* initialUrl;          // Can be nullptr for blank
        bool enableDevTools;
    };

    // ========================================================================
    // Input Event Types
    // ========================================================================
    enum class MouseButton : int32_t
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
    };

    enum class MouseEventType : int32_t
    {
        Move = 0,
        Down = 1,
        Up = 2,
        Wheel = 3,
        Leave = 4,
    };

    struct MouseEventParams
    {
        MouseEventType type;
        MouseButton button;
        float x;                // Normalized UV coordinate [0, 1]
        float y;                // Normalized UV coordinate [0, 1]
        float wheelDelta;       // For wheel events
    };

    struct KeyEventParams
    {
        uint32_t virtualKeyCode;
        uint32_t scanCode;
        bool isKeyDown;
        bool isSystemKey;
    };

    // ========================================================================
    // Render Event Callbacks (for GL.IssuePluginEvent)
    // ========================================================================
    enum class RenderEventType : int32_t
    {
        Initialize = 0,
        Shutdown = 1,
        UpdateTexture = 2,
    };

    // ========================================================================
    // Callback Function Types
    // ========================================================================
    using LogCallback = void(*)(int32_t level, const char* message);
    using NavigationCallback = void(*)(WebViewHandle handle, const wchar_t* url, bool isSuccess);
    using MessageCallback = void(*)(WebViewHandle handle, const wchar_t* message);

    enum class DeviceEventType : int32_t
    {
        DeviceLost = 0,
        DeviceRestored = 1
    };
    
    using DeviceEventCallback = void(*)(DeviceEventType eventType);

} // namespace WebViewToolkit
