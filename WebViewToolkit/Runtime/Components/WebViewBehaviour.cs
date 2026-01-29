// ============================================================================
// WebViewToolkit - WebView MonoBehaviour Component
// ============================================================================

using UnityEngine;
using UnityEngine.Events;
using WebViewToolkit.Native;

namespace WebViewToolkit.Components
{
    /// <summary>
    /// MonoBehaviour component for easily adding a WebView to GameObjects.
    /// Can be used with UI RawImage, 3D meshes, or UIToolkit.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/WebView")]
    public class WebViewBehaviour : MonoBehaviour
    {
        [Header("WebView Settings")]
        [SerializeField]
        [Tooltip("Width of the WebView texture in pixels")]
        private int _width = 1280;

        [SerializeField]
        [Tooltip("Height of the WebView texture in pixels")]
        private int _height = 720;

        [SerializeField]
        [Tooltip("Initial URL to load")]
        private string _initialUrl = "https://www.google.com";

        [SerializeField]
        [Tooltip("Enable Chrome DevTools for debugging")]
        private bool _enableDevTools = false;

        [SerializeField]
        [Tooltip("Automatically create WebView on Start")]
        private bool _createOnStart = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent<string, bool> _onNavigationCompleted;

        [SerializeField]
        private UnityEvent<string> _onMessageReceived;

        /// <summary>
        /// The underlying WebView instance
        /// </summary>
        public WebViewInstance WebView { get; private set; }

        /// <summary>
        /// The texture containing WebView content
        /// </summary>
        public Texture2D Texture => WebView?.Texture;

        /// <summary>
        /// Whether the WebView is ready
        /// </summary>
        public bool IsReady => WebView != null && !WebView.IsDestroyed;

        /// <summary>
        /// Width of the WebView
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    if (IsReady) WebView.Resize(_width, _height);
                }
            }
        }

        /// <summary>
        /// Height of the WebView
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    if (IsReady) WebView.Resize(_width, _height);
                }
            }
        }

        // ====================================================================
        // Unity Lifecycle
        // ====================================================================

        private void Start()
        {
            if (_createOnStart)
            {
                CreateWebView();
            }
        }

        private void OnDestroy()
        {
            DestroyWebView();
        }

        // ====================================================================
        // Public API
        // ====================================================================

        /// <summary>
        /// Create the WebView instance
        /// </summary>
        public void CreateWebView()
        {
            if (WebView != null)
            {
                Debug.LogWarning("[WebViewBehaviour] WebView already created");
                return;
            }

            WebView = WebViewManager.Instance.CreateWebView(_width, _height, _initialUrl, _enableDevTools);

            if (WebView != null)
            {
                WebView.NavigationCompleted += OnNavigationCompleted;
                WebView.MessageReceived += OnMessageReceived;
            }
        }

        /// <summary>
        /// Destroy the WebView instance
        /// </summary>
        public void DestroyWebView()
        {
            if (WebView != null)
            {
                WebView.NavigationCompleted -= OnNavigationCompleted;
                WebView.MessageReceived -= OnMessageReceived;
                WebView.Dispose();
                WebView = null;
            }
        }

        /// <summary>
        /// Navigate to a URL
        /// </summary>
        public void Navigate(string url)
        {
            WebView?.Navigate(url);
        }

        /// <summary>
        /// Navigate to HTML content
        /// </summary>
        public void NavigateToString(string html)
        {
            WebView?.NavigateToString(html);
        }

        /// <summary>
        /// Execute JavaScript
        /// </summary>
        public void ExecuteScript(string script)
        {
            WebView?.ExecuteScript(script);
        }

        /// <summary>
        /// Send a mouse event using normalized coordinates [0-1]
        /// </summary>
        public void SendMouseEvent(MouseEventType eventType, MouseButton button, Vector2 normalizedPosition, float wheelDelta = 0)
        {
            WebView?.SendMouseEvent(eventType, button, normalizedPosition.x, normalizedPosition.y, wheelDelta);
        }

        /// <summary>
        /// Resize the WebView
        /// </summary>
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            WebView?.Resize(width, height);
        }

        // ====================================================================
        // Event Handlers
        // ====================================================================

        private void OnNavigationCompleted(string url, bool isSuccess)
        {
            _onNavigationCompleted?.Invoke(url, isSuccess);
        }

        private void OnMessageReceived(string message)
        {
            _onMessageReceived?.Invoke(message);
        }
    }
}
