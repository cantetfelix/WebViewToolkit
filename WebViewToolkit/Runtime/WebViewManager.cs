// ============================================================================
// WebViewToolkit - WebView Manager (Singleton)
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WebViewToolkit.Native;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WebViewToolkit
{
    /// <summary>
    /// Manages the WebView native plugin lifecycle and provides factory methods.
    /// Works in both Runtime and Editor environments.
    /// </summary>
    public class WebViewManager
    {
        private static WebViewManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static WebViewManager Instance
        {
            get
            {
                if (_isQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new WebViewManager();
                        _instance.Initialize();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Whether the native plugin is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Current graphics API
        /// </summary>
        public GraphicsAPI CurrentGraphicsAPI { get; private set; }

        // Active WebView instances
        private readonly Dictionary<uint, WebViewInstance> _instances = new Dictionary<uint, WebViewInstance>();

        // Native callbacks (must be stored to prevent GC)
        private LogCallback _logCallback;
        private NavigationCallback _navigationCallback;
        private MessageCallback _messageCallback;
        private DeviceEventCallback _deviceEventCallback;

        // Render event function pointer
        private IntPtr _renderEventFunc;

        // Frame throttling
        private int _lastFrameCount = -1;
        private float _lastUpdateTime = 0f;
        private const float UpdateInterval = 0.033f; // ~30 FPS for WebView updates

        // ====================================================================
        // Constructor & Lifecycle
        // ====================================================================

        private WebViewManager()
        {
            // Register global cleanup
            Application.quitting += OnApplicationQuit;
        }

        private void Initialize()
        {
            if (IsInitialized) return;

            // Detect graphics API
            CurrentGraphicsAPI = SystemInfo.graphicsDeviceType switch
            {
                GraphicsDeviceType.Direct3D11 => GraphicsAPI.Direct3D11,
                GraphicsDeviceType.Direct3D12 => GraphicsAPI.Direct3D12,
                _ => GraphicsAPI.Unknown
            };

            if (CurrentGraphicsAPI == GraphicsAPI.Unknown)
            {
                Debug.LogError($"[WebViewManager] Unsupported graphics API: {SystemInfo.graphicsDeviceType}");
                return;
            }

            Debug.Log($"[WebViewManager] Initializing with {CurrentGraphicsAPI}...");

            // Setup callbacks
            _logCallback = OnNativeLog;
            _navigationCallback = OnNativeNavigation;
            _messageCallback = OnNativeMessage;
            _deviceEventCallback = OnNativeDeviceEvent;

            WebViewNative.WebViewToolkit_SetLogCallback(_logCallback);
            WebViewNative.WebViewToolkit_SetNavigationCallback(_navigationCallback);
            WebViewNative.WebViewToolkit_SetMessageCallback(_messageCallback);
            WebViewNative.WebViewToolkit_SetDeviceEventCallback(_deviceEventCallback);

            // Initialize native plugin
            var result = (NativeResult)WebViewNative.WebViewToolkit_Initialize((int)CurrentGraphicsAPI);
            
            if (result != NativeResult.Success)
            {
                if (result == NativeResult.ErrorAlreadyInitialized)
                {
                    Debug.Log("[WebViewManager] Native plugin already initialized (Persistent Mode). Forcing re-initialization to refresh RenderAPI...");
                    
                    // Force shutdown to clear stale state
                    WebViewNative.WebViewToolkit_Shutdown();
                    
                    // Re-initialize to get fresh RenderAPI
                    result = (NativeResult)WebViewNative.WebViewToolkit_Initialize((int)CurrentGraphicsAPI);
                    
                    if (result != NativeResult.Success)
                    {
                        Debug.LogError($"[WebViewManager] Failed to re-initialize native plugin: {result}");
                        return;
                    }
                    
                    Debug.Log("[WebViewManager] Re-initialization successful.");
                }
                else
                {
                    Debug.LogError($"[WebViewManager] Failed to initialize native plugin: {result}");
                    return;
                }
            }

            _renderEventFunc = WebViewNative.WebViewToolkit_GetRenderEventFunc();
            IsInitialized = true;
 
            Debug.Log("[WebViewManager] Initialized successfully");
        }
 
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            // In Editor, this means Play Mode is stopping.
            // We shut down the instance, but allow re-creation for Editor usage.
            Shutdown();
#else
            // In Player, this is the end.
            _isQuitting = true;
            Shutdown();
#endif
        }

#if UNITY_EDITOR
        private void OnEditorQuitting()
        {
            _isQuitting = true;
            Shutdown();
        }
#endif

        /// <summary>
        /// Drive the update loop. Can be called from multiple sources safely.
        /// </summary>
        public void Tick()
        {
            if (!IsInitialized || _renderEventFunc == IntPtr.Zero) return;

            // Frame throttling: Only execute once per frame
            if (Time.frameCount == _lastFrameCount) return;
            _lastFrameCount = Time.frameCount;

            // Time throttling: specific interval
            if (Time.realtimeSinceStartup - _lastUpdateTime < UpdateInterval) return;
            _lastUpdateTime = Time.realtimeSinceStartup;

            // Issue render event to update all WebView textures
            GL.IssuePluginEvent(_renderEventFunc, (int)RenderEventType.UpdateTexture);
        }

        /// <summary>
        /// Full shutdown of the manager and native resources
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized) return;

            Debug.Log("[WebViewManager] Shutting down...");

            // Signal native quit first
            WebViewNative.WebViewToolkit_SignalApplicationQuit();

            // Destroy all instances (C# side)
            var handles = new List<uint>(_instances.Keys);
            foreach (var handle in handles)
            {
                if (_instances.TryGetValue(handle, out var instance))
                {
                    instance.DestroyInternal();
                }
            }
            _instances.Clear();

            // Native Shutdown
            WebViewNative.WebViewToolkit_Shutdown();
            
            IsInitialized = false;
            _renderEventFunc = IntPtr.Zero;
            _instance = null; // Reset singleton
        }

        // ====================================================================
        // WebView Factory
        // ====================================================================

        public WebViewInstance CreateWebView(int width, int height, string initialUrl = null, bool enableDevTools = false)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[WebViewManager] Cannot create WebView - not initialized");
                return null;
            }

            var result = (NativeResult)WebViewNative.WebViewToolkit_CreateWebView(
                (uint)width,
                (uint)height,
                null,
                initialUrl,
                enableDevTools ? 1 : 0,
                out uint handle
            );

            if (result != NativeResult.Success)
            {
                Debug.LogError($"[WebViewManager] Failed to create WebView: {result}");
                return null;
            }

            var instance = new WebViewInstance(handle, width, height);
            _instances[handle] = instance;

            Debug.Log($"[WebViewManager] Created WebView (handle={handle}, size={width}x{height})");
            return instance;
        }

        internal void DestroyWebView(WebViewInstance instance)
        {
            if (instance == null) return;

            if (_instances.Remove(instance.Handle))
            {
                WebViewNative.WebViewToolkit_DestroyWebView(instance.Handle);
                Debug.Log($"[WebViewManager] Destroyed WebView (handle={instance.Handle})");
            }
        }

        // ====================================================================
        // Native Callbacks
        // ====================================================================

        private void OnNativeLog(int level, string message)
        {
            switch (level)
            {
                case 0: Debug.Log($"[WebView Native] {message}"); break;
                case 1: Debug.LogWarning($"[WebView Native] {message}"); break;
                default: Debug.LogError($"[WebView Native] {message}"); break;
            }
        }

        private void OnNativeNavigation(uint handle, string url, bool isSuccess)
        {
            if (_instances.TryGetValue(handle, out var instance))
            {
                instance.OnNavigationCompleted(url, isSuccess);
            }
        }

        private void OnNativeMessage(uint handle, string message)
        {
            if (_instances.TryGetValue(handle, out var instance))
            {
                instance.OnMessageReceived(message);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DeviceEventCallback))]
        private void OnNativeDeviceEvent(DeviceEventType eventType)
        {
            if (eventType == DeviceEventType.DeviceRestored)
            {
                Debug.Log("[WebViewManager] Native device restore detected, refreshing textures...");
                // Dispatch to main thread just in case, though usually this callback runs on Unity thread
                // But textures should be updated.
                foreach (var instance in _instances.Values)
                {
                    instance.RefreshTexture();
                }
            }
        }
    }
}
