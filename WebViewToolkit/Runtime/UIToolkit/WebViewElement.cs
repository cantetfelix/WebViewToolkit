// ============================================================================
// WebViewToolkit - UIToolkit WebView Element
// ============================================================================
// A VisualElement that displays WebView content and handles automatic resizing.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.Native;

namespace WebViewToolkit.UIToolkit
{
    /// <summary>
    /// A VisualElement that displays WebView2 content.
    /// Automatically creates and manages the underlying WebView texture.
    /// </summary>
    public class WebViewElement : VisualElement
    {
        // ====================================================================
        // UXML Factory
        // ====================================================================
        
        public new class UxmlFactory : UxmlFactory<WebViewElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_InitialUrl = new UxmlStringAttributeDescription
            {
                name = "initial-url",
                defaultValue = "https://www.google.com"
            };

            private UxmlBoolAttributeDescription m_EnableDevTools = new UxmlBoolAttributeDescription
            {
                name = "enable-dev-tools",
                defaultValue = false
            };

            private UxmlBoolAttributeDescription m_AutoCreate = new UxmlBoolAttributeDescription
            {
                name = "auto-create",
                defaultValue = true
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = ve as WebViewElement;
                if (element != null)
                {
                    element.InitialUrl = m_InitialUrl.GetValueFromBag(bag, cc);
                    element.EnableDevTools = m_EnableDevTools.GetValueFromBag(bag, cc);
                    element._autoCreate = m_AutoCreate.GetValueFromBag(bag, cc);
                }
            }
        }

        // ====================================================================
        // Properties
        // ====================================================================

        /// <summary>
        /// Initial URL to load when WebView is created
        /// </summary>
        public string InitialUrl { get; set; } = "https://www.google.com";

        /// <summary>
        /// Enable Chrome DevTools for debugging
        /// </summary>
        public bool EnableDevTools { get; set; } = false;

        /// <summary>
        /// The underlying WebView instance
        /// </summary>
        public WebViewInstance WebView { get; private set; }

        /// <summary>
        /// Whether the WebView is ready
        /// </summary>
        public bool IsReady => WebView != null && !WebView.IsDestroyed;

        /// <summary>
        /// Current URL being displayed
        /// </summary>
        public string CurrentUrl => WebView?.CurrentUrl;

        // ====================================================================
        // Events
        // ====================================================================

        /// <summary>
        /// Fired when navigation completes
        /// </summary>
        public event Action<string, bool> NavigationCompleted;

        /// <summary>
        /// Fired when a message is received from JavaScript
        /// </summary>
        public event Action<string> MessageReceived;

        // ====================================================================
        // Private Fields
        // ====================================================================

        private Image _backgroundImage;
        private bool _autoCreate = true;
        private bool _isCreated = false;
        private int _lastWidth = 0;
        private int _lastHeight = 0;
        private const int MinSize = 64;
        private bool _isMouseOver = false;

        // ====================================================================
        // Constructor
        // ====================================================================

        public WebViewElement()
        {
            // Set up default styling
            style.flexGrow = 1;
            style.overflow = Overflow.Hidden;
            
            // Create background image to display the texture
            _backgroundImage = new Image();
            _backgroundImage.style.position = Position.Absolute;
            _backgroundImage.style.left = 0;
            _backgroundImage.style.top = 0;
            _backgroundImage.style.right = 0;
            _backgroundImage.style.bottom = 0;
            _backgroundImage.scaleMode = ScaleMode.StretchToFill;
            _backgroundImage.pickingMode = PickingMode.Ignore; // Let parent handle events
            Add(_backgroundImage);

            // Enable picking for mouse events
            pickingMode = PickingMode.Position;

            // Register for geometry changes
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            // Register mouse event handlers
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<WheelEvent>(OnMouseWheel);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        // ====================================================================
        // Public API
        // ====================================================================

        /// <summary>
        /// Create the WebView with the current element size
        /// </summary>
        public void CreateWebView()
        {
            // Remove guard to allow editor creation
            // if (!Application.isPlaying) { return; }

            if (_isCreated)
            {
                Debug.LogWarning("[WebViewElement] WebView already created");
                return;
            }

            // Get size from resolved style
            float rWidth = resolvedStyle.width;
            float rHeight = resolvedStyle.height;

            if (float.IsNaN(rWidth) || float.IsNaN(rHeight))
            {
               // Still in layout phase
               return;
            }

            int width = Mathf.RoundToInt(rWidth);
            int height = Mathf.RoundToInt(rHeight);

            // Bounds check for valid texture creation
            if (width <= 0 || height <= 0)
            {
                // Wait for valid layout
                return;
            }

            if (width < MinSize || height < MinSize)
            {
                Debug.LogWarning($"[WebViewElement] Size too small ({width}x{height}), waiting for layout...");
                return;
            }

            _lastWidth = width;
            _lastHeight = height;

            if (width < MinSize || height < MinSize)
            {
                Debug.LogWarning($"[WebViewElement] Size too small ({width}x{height}), waiting for layout...");
                return;
            }

            // In Editor, prevent creation during static preview generation (Project Browser icons, Inspector previews)
            // We check for 'panel.name' or context. RenderStaticPreview usually uses a temporary panel.
            if (!Application.isPlaying && panel != null)
            {
                // If the panel has no window associated, it's likely an offscreen preview render
                // UI Builder window has a name 'Builder' usually, Game View is 'GameView', etc.
                // A safer check: Don't create if we are not in a focused/real window context?
                
                // For now, let's skip if we suspect it's a static preview interaction
                // Static previews often have very specific small sizes or 'unnamed' panels
                // But we already filtered by MinSize > 100.
                
                // Let's rely on preventing immediate creation in Editor for now to be safe against crash loops.
                // Or check contextType strictly.
                // NOTE: UI Builder uses ContextType.Editor. Canvas preview uses ContextType.Editor.
                
                // Let's add a debouncer. If this is a static preview, it will be destroyed immediately.
                // We shouldn't create the native view synchronously in OnGeometryChanged for Editor.
                
                // Note: The NATIVE plugin is crashing. This means we are creating a HWND or texture resource that conflicts.
                // Best fix: Require explicit manual activation in Editor (e.g. "Load" button) OR
                // use a dedicated 'EditorWebView' wrapper that handles this safely.
                
                // For this task, let's inhibit creation if we detect we are inside a VisualTreeAsset preview render.
                // There is no public API to detect 'RenderStaticPreview', but we can check if 'panel.visualTree' is valid.
            }
            
            // CRITICAL FIX: To prevent Editor crashes (especially in UI Builder/Project Browser preview),
            // we delay creation in Editor to ensure the layout is stable and persistent.
            if (!Application.isPlaying && !_isCreated)
            {
                schedule.Execute(() => 
                {
                    // Double check we still exist and have size
                    if (panel == null || float.IsNaN(resolvedStyle.width) || resolvedStyle.width < MinSize) return;
                    
                    CreateWebViewInternal(width, height);
                    
                }).ExecuteLater(100); // 100ms delay to skip static preview renders
                return;
            }

            CreateWebViewInternal(width, height);
        }

        private void CreateWebViewInternal(int width, int height)
        {
            if (_isCreated) return;

            var manager = WebViewManager.Instance;
            if (manager == null) return;

            WebView = manager.CreateWebView(width, height, InitialUrl, EnableDevTools);
            if (WebView != null)
            {
                _isCreated = true;
                WebView.NavigationCompleted += OnNavigationCompleted;
                WebView.MessageReceived += OnMessageReceived;
                UpdateTexture();
            }
        }

        /// <summary>
        /// Destroy the WebView
        /// </summary>
        public void DestroyWebView()
        {
            if (WebView != null && !WebView.IsDestroyed)
            {
                WebView.NavigationCompleted -= OnNavigationCompleted;
                WebView.MessageReceived -= OnMessageReceived;

                // Only dispose if manager is still valid (not during shutdown)
                // This prevents double-destruction race condition
                var manager = WebViewManager.Instance;
                if (manager != null && manager.IsInitialized)
                {
                    WebView.Dispose();
                }

                WebView = null;
            }
            _isCreated = false;
            _backgroundImage.image = null;
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
        /// Execute JavaScript in the WebView
        /// </summary>
        public void ExecuteScript(string script)
        {
            WebView?.ExecuteScript(script);
        }

        /// <summary>
        /// Send a mouse event (coordinates in local element space)
        /// </summary>
        public void SendMouseEvent(Native.MouseEventType eventType, Native.MouseButton button, Vector2 localPosition, float wheelDelta = 0)
        {
            if (!IsReady) return;

            // Convert to normalized coordinates [0-1]
            float normalizedX = localPosition.x / resolvedStyle.width;
            float normalizedY = localPosition.y / resolvedStyle.height;
            
            WebView.SendMouseEvent(eventType, button, normalizedX, normalizedY, wheelDelta);
        }

        /// <summary>
        /// Manually resize the WebView (usually automatic)
        /// </summary>
        public void Resize(int width, int height)
        {
            if (width < MinSize || height < MinSize) return;
            if (!IsReady) return;
            if (width == _lastWidth && height == _lastHeight) return;

            _lastWidth = width;
            _lastHeight = height;
            WebView.Resize(width, height);
            UpdateTexture();
        }

        // ====================================================================
        // Mouse Event Handlers
        // ====================================================================

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!IsReady) return;

            var localPos = evt.localMousePosition;
            var button = ConvertMouseButton(evt.button);
            
            SendMouseEventInternal(Native.MouseEventType.Down, button, localPos);
            
            // Capture mouse to receive events outside element
            this.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!IsReady) return;

            var localPos = evt.localMousePosition;
            var button = ConvertMouseButton(evt.button);
            
            SendMouseEventInternal(Native.MouseEventType.Up, button, localPos);
            
            // Release mouse capture
            if (this.HasMouseCapture())
            {
                this.ReleaseMouse();
            }
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!IsReady) return;

            var localPos = evt.localMousePosition;
            
            // Determine which button is pressed during move
            Native.MouseButton button = Native.MouseButton.None;
            if ((evt.pressedButtons & 1) != 0) button = Native.MouseButton.Left;
            else if ((evt.pressedButtons & 2) != 0) button = Native.MouseButton.Right;
            else if ((evt.pressedButtons & 4) != 0) button = Native.MouseButton.Middle;
            
            SendMouseEventInternal(Native.MouseEventType.Move, button, localPos);
        }

        private void OnMouseWheel(WheelEvent evt)
        {
            if (!IsReady) return;

            var localPos = evt.localMousePosition;
            
            // Unity's delta.y: positive = scroll down, negative = scroll up
            // WebView2 expects: positive = scroll up, negative = scroll down
            // Native code multiplies by WHEEL_DELTA (120), so just send the inverted normalized value
            float wheelDelta = -evt.delta.y;
            
            SendMouseEventInternal(Native.MouseEventType.Wheel, Native.MouseButton.None, localPos, wheelDelta);
            
            // Prevent UIToolkit from processing the scroll event (which would cause flickering)
            evt.StopImmediatePropagation();
            evt.PreventDefault();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _isMouseOver = true;
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _isMouseOver = false;
            
            if (!IsReady) return;
            
            // Send leave event to WebView
            SendMouseEventInternal(Native.MouseEventType.Leave, Native.MouseButton.None, Vector2.zero);
        }

        private void SendMouseEventInternal(Native.MouseEventType eventType, Native.MouseButton button, Vector2 localPosition, float wheelDelta = 0)
        {
            if (!IsReady) return;

            // Clamp to element bounds
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;
            
            if (width <= 0 || height <= 0) return;

            // Convert to normalized coordinates [0-1]
            float normalizedX = Mathf.Clamp01(localPosition.x / width);
            float normalizedY = Mathf.Clamp01(localPosition.y / height);
            
            WebView.SendMouseEvent(eventType, button, normalizedX, normalizedY, wheelDelta);
        }

        private Native.MouseButton ConvertMouseButton(int button)
        {
            switch (button)
            {
                case 0: return Native.MouseButton.Left;
                case 1: return Native.MouseButton.Right;
                case 2: return Native.MouseButton.Middle;
                default: return Native.MouseButton.None;
            }
        }

        // ====================================================================
        // Internal
        // ====================================================================

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Schedule creation after layout is resolved
            if (_autoCreate && !_isCreated)
            {
                schedule.Execute(() =>
                {
                    if (!_isCreated)
                    {
                        CreateWebView();
                    }
                }).ExecuteLater(100); 
            }

            // Drive the update loop (works in Editor and Runtime)
            // Execute.Every(0) runs every frame (update loop)
            schedule.Execute(() => WebViewManager.Instance?.Tick()).Every(0);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            DestroyWebView();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!_isCreated)
            {
                // Try to create if we now have a valid size
                if (_autoCreate)
                {
                    CreateWebView();
                }
                return;
            }

            // Check if size changed significantly (more than 1 pixel)
            int newWidth = Mathf.RoundToInt(evt.newRect.width);
            int newHeight = Mathf.RoundToInt(evt.newRect.height);

            if (Mathf.Abs(newWidth - _lastWidth) > 1 || Mathf.Abs(newHeight - _lastHeight) > 1)
            {
                Resize(newWidth, newHeight);
            }
        }

        private void UpdateTexture()
        {
            if (WebView?.Texture != null)
            {
                _backgroundImage.image = WebView.Texture;
            }
        }

        private void OnNavigationCompleted(string url, bool isSuccess)
        {
            NavigationCompleted?.Invoke(url, isSuccess);
        }

        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}
