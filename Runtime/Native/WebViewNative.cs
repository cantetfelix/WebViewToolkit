// ============================================================================
// WebViewToolkit - Native P/Invoke Declarations
// ============================================================================

using System;
using System.Runtime.InteropServices;

namespace WebViewToolkit.Native
{
    /// <summary>
    /// Graphics API types matching Unity's GraphicsDeviceType
    /// </summary>
    public enum GraphicsAPI : int
    {
        Unknown = 0,
        Direct3D11 = 2,
        Direct3D12 = 18
    }

    /// <summary>
    /// Result codes from native operations
    /// </summary>
    public enum NativeResult : int
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
    }

    /// <summary>
    /// Mouse event types for input forwarding
    /// </summary>
    public enum MouseEventType : int
    {
        Move = 0,
        Down = 1,
        Up = 2,
        Wheel = 3,
        Leave = 4
    }

    /// <summary>
    /// Mouse button types
    /// </summary>
    public enum MouseButton : int
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3
    }

    /// <summary>
    /// Render event types for GL.IssuePluginEvent
    /// </summary>
    public enum RenderEventType : int
    {
        Initialize = 0,
        Shutdown = 1,
        UpdateTexture = 2
    }

    /// <summary>
    /// Device event types for native callback
    /// </summary>
    public enum DeviceEventType : int
    {
        DeviceLost = 0,
        DeviceRestored = 1
    }

    /// <summary>
    /// Callback delegate for logging
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(int level, [MarshalAs(UnmanagedType.LPStr)] string message);

    /// <summary>
    /// Callback delegate for navigation events
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NavigationCallback(uint handle, [MarshalAs(UnmanagedType.LPWStr)] string url, [MarshalAs(UnmanagedType.Bool)] bool isSuccess);

    /// <summary>
    /// Callback delegate for JavaScript messages
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MessageCallback(uint handle, [MarshalAs(UnmanagedType.LPWStr)] string message);

    /// <summary>
    /// Callback delegate for device events
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DeviceEventCallback(DeviceEventType eventType);

    /// <summary>
    /// Native P/Invoke declarations for WebViewToolkit DLL
    /// </summary>
    public static class WebViewNative
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private const string DllName = "WebViewToolkit";
#else
        private const string DllName = "__Internal";
#endif

        // ====================================================================
        // Callbacks
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_SetLogCallback(LogCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_SetNavigationCallback(NavigationCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_SetMessageCallback(MessageCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_SetDeviceEventCallback(DeviceEventCallback callback);

        // ====================================================================
        // Initialization
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_Initialize(int graphicsAPI);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_Shutdown();

        /// <summary>
        /// Signal that the application is quitting - call BEFORE destroying any WebView instances
        /// to ensure native cleanup is abandoned (preventing async callback crashes)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void WebViewToolkit_SignalApplicationQuit();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_IsInitialized();

        // ====================================================================
        // WebView Management
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_CreateWebView(
            uint width,
            uint height,
            [MarshalAs(UnmanagedType.LPWStr)] string userDataFolder,
            [MarshalAs(UnmanagedType.LPWStr)] string initialUrl,
            int enableDevTools,
            out uint outHandle
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_DestroyWebView(uint handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebViewToolkit_GetTexturePtr(uint handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_Resize(uint handle, uint width, uint height);

        // ====================================================================
        // Navigation
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_Navigate(uint handle, [MarshalAs(UnmanagedType.LPWStr)] string url);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_NavigateToString(uint handle, [MarshalAs(UnmanagedType.LPWStr)] string html);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_ExecuteScript(uint handle, [MarshalAs(UnmanagedType.LPWStr)] string script);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_GoBack(uint handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_GoForward(uint handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_CanGoBack(uint handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_CanGoForward(uint handle);

        // ====================================================================
        // Input
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_SendMouseEvent(
            uint handle,
            int eventType,
            int button,
            float x,
            float y,
            float wheelDelta
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebViewToolkit_SendKeyEvent(
            uint handle,
            uint virtualKeyCode,
            uint scanCode,
            int isKeyDown,
            int isSystemKey
        );

        // ====================================================================
        // Render Events
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebViewToolkit_GetRenderEventFunc();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WebViewToolkit_GetRenderEventAndDataFunc();
    }
}
