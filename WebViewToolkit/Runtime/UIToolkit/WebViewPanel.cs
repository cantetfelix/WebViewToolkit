// ============================================================================
// WebViewToolkit - Full-Featured WebView Panel
// ============================================================================
// A complete, configurable WebView component with navigation controls,
// address bar, loading indicator, and more.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace WebViewToolkit.UIToolkit
{
    /// <summary>
    /// A full-featured WebView panel with configurable UI components.
    /// Includes navigation controls, address bar, loading indicator, and more.
    /// </summary>
    public class WebViewPanel : VisualElement
    {
        // ====================================================================
        // UXML Factory
        // ====================================================================

        public new class UxmlFactory : UxmlFactory<WebViewPanel, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // WebView settings
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

            // Feature toggles
            private UxmlBoolAttributeDescription m_ShowToolbar = new UxmlBoolAttributeDescription
            {
                name = "show-toolbar",
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_ShowAddressBar = new UxmlBoolAttributeDescription
            {
                name = "show-address-bar",
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_ShowNavigationButtons = new UxmlBoolAttributeDescription
            {
                name = "show-navigation-buttons",
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_ShowRefreshButton = new UxmlBoolAttributeDescription
            {
                name = "show-refresh-button",
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_ShowHomeButton = new UxmlBoolAttributeDescription
            {
                name = "show-home-button",
                defaultValue = false
            };

            private UxmlBoolAttributeDescription m_ShowLoadingIndicator = new UxmlBoolAttributeDescription
            {
                name = "show-loading-indicator",
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_ShowDevToolsButton = new UxmlBoolAttributeDescription
            {
                name = "show-devtools-button",
                defaultValue = false
            };

            private UxmlBoolAttributeDescription m_ShowStatusBar = new UxmlBoolAttributeDescription
            {
                name = "show-status-bar",
                defaultValue = false
            };

            // Styling
            private UxmlStringAttributeDescription m_ToolbarHeight = new UxmlStringAttributeDescription
            {
                name = "toolbar-height",
                defaultValue = "40"
            };

            private UxmlStringAttributeDescription m_HomeUrl = new UxmlStringAttributeDescription
            {
                name = "home-url",
                defaultValue = "https://www.google.com"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var panel = ve as WebViewPanel;
                if (panel != null)
                {
                    panel.InitialUrl = m_InitialUrl.GetValueFromBag(bag, cc);
                    panel.EnableDevTools = m_EnableDevTools.GetValueFromBag(bag, cc);
                    panel.ShowToolbar = m_ShowToolbar.GetValueFromBag(bag, cc);
                    panel.ShowAddressBar = m_ShowAddressBar.GetValueFromBag(bag, cc);
                    panel.ShowNavigationButtons = m_ShowNavigationButtons.GetValueFromBag(bag, cc);
                    panel.ShowRefreshButton = m_ShowRefreshButton.GetValueFromBag(bag, cc);
                    panel.ShowHomeButton = m_ShowHomeButton.GetValueFromBag(bag, cc);
                    panel.ShowLoadingIndicator = m_ShowLoadingIndicator.GetValueFromBag(bag, cc);
                    panel.ShowDevToolsButton = m_ShowDevToolsButton.GetValueFromBag(bag, cc);
                    panel.ShowStatusBar = m_ShowStatusBar.GetValueFromBag(bag, cc);
                    panel.HomeUrl = m_HomeUrl.GetValueFromBag(bag, cc);

                    if (int.TryParse(m_ToolbarHeight.GetValueFromBag(bag, cc), out int height))
                    {
                        panel.ToolbarHeight = height;
                    }
                }
            }
        }

        // ====================================================================
        // USS Class Names
        // ====================================================================

        public static readonly string ussClassName = "webview-panel";
        public static readonly string toolbarUssClassName = "webview-panel__toolbar";
        public static readonly string addressBarUssClassName = "webview-panel__address-bar";
        public static readonly string buttonUssClassName = "webview-panel__button";
        public static readonly string contentUssClassName = "webview-panel__content";
        public static readonly string statusBarUssClassName = "webview-panel__status-bar";
        public static readonly string loadingUssClassName = "webview-panel__loading";

        // ====================================================================
        // Properties - WebView Settings
        // ====================================================================

        private string _initialUrl = "https://www.google.com";
        public string InitialUrl
        {
            get => _initialUrl;
            set
            {
                _initialUrl = value;
                if (_webViewElement != null)
                {
                    _webViewElement.InitialUrl = value;
                }
            }
        }

        private bool _enableDevTools = false;
        public bool EnableDevTools
        {
            get => _enableDevTools;
            set
            {
                _enableDevTools = value;
                if (_webViewElement != null)
                {
                    _webViewElement.EnableDevTools = value;
                }
            }
        }

        private string _homeUrl = "https://www.google.com";
        public string HomeUrl
        {
            get => _homeUrl;
            set => _homeUrl = value;
        }

        // ====================================================================
        // Properties - Feature Toggles
        // ====================================================================

        private bool _showToolbar = true;
        public bool ShowToolbar
        {
            get => _showToolbar;
            set
            {
                _showToolbar = value;
                UpdateToolbarVisibility();
            }
        }

        private bool _showAddressBar = true;
        public bool ShowAddressBar
        {
            get => _showAddressBar;
            set
            {
                _showAddressBar = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showNavigationButtons = true;
        public bool ShowNavigationButtons
        {
            get => _showNavigationButtons;
            set
            {
                _showNavigationButtons = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showRefreshButton = true;
        public bool ShowRefreshButton
        {
            get => _showRefreshButton;
            set
            {
                _showRefreshButton = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showHomeButton = false;
        public bool ShowHomeButton
        {
            get => _showHomeButton;
            set
            {
                _showHomeButton = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showLoadingIndicator = true;
        public bool ShowLoadingIndicator
        {
            get => _showLoadingIndicator;
            set
            {
                _showLoadingIndicator = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showDevToolsButton = false;
        public bool ShowDevToolsButton
        {
            get => _showDevToolsButton;
            set
            {
                _showDevToolsButton = value;
                UpdateToolbarLayout();
            }
        }

        private bool _showStatusBar = false;
        public bool ShowStatusBar
        {
            get => _showStatusBar;
            set
            {
                _showStatusBar = value;
                UpdateStatusBarVisibility();
            }
        }

        private int _toolbarHeight = 40;
        public int ToolbarHeight
        {
            get => _toolbarHeight;
            set
            {
                _toolbarHeight = value;
                if (_toolbar != null)
                {
                    _toolbar.style.height = value;
                }
            }
        }

        // ====================================================================
        // Public Properties
        // ====================================================================

        /// <summary>
        /// The underlying WebViewElement
        /// </summary>
        public WebViewElement WebViewElement => _webViewElement;

        /// <summary>
        /// The underlying WebView instance
        /// </summary>
        public WebViewInstance WebView => _webViewElement?.WebView;

        /// <summary>
        /// Whether the WebView is ready
        /// </summary>
        public bool IsReady => _webViewElement?.IsReady ?? false;

        /// <summary>
        /// Current URL being displayed
        /// </summary>
        public string CurrentUrl => _webViewElement?.CurrentUrl;

        /// <summary>
        /// Whether the page is currently loading
        /// </summary>
        public bool IsLoading { get; private set; }

        // ====================================================================
        // Events
        // ====================================================================

        /// <summary>
        /// Fired when navigation starts
        /// </summary>
        public event Action<string> NavigationStarted;

        /// <summary>
        /// Fired when navigation completes
        /// </summary>
        public event Action<string, bool> NavigationCompleted;

        /// <summary>
        /// Fired when a message is received from JavaScript
        /// </summary>
        public event Action<string> MessageReceived;

        /// <summary>
        /// Fired when the URL in the address bar changes
        /// </summary>
        public event Action<string> UrlChanged;

        // ====================================================================
        // Private Fields - UI Elements
        // ====================================================================

        private VisualElement _toolbar;
        private VisualElement _navigationGroup;
        private Button _backButton;
        private Button _forwardButton;
        private Button _refreshButton;
        private Button _homeButton;
        private TextField _addressBar;
        private Button _goButton;
        private VisualElement _loadingIndicator;
        private Button _devToolsButton;
        private WebViewElement _webViewElement;
        private VisualElement _statusBar;
        private Label _statusLabel;


        // ====================================================================
        // Constructor
        // ====================================================================

        public WebViewPanel()
        {
            AddToClassList(ussClassName);
            style.flexDirection = FlexDirection.Column;
            style.flexGrow = 1;

            BuildUI();
            ApplyDefaultStyles();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        // ====================================================================
        // Public API
        // ====================================================================

        /// <summary>
        /// Navigate to a URL
        /// </summary>
        public void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // Add protocol if missing
            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("file://"))
            {
                url = "https://" + url;
            }

            UpdateAddressBar(url);
            SetLoading(true);
            NavigationStarted?.Invoke(url);

            _webViewElement?.Navigate(url);
        }

        /// <summary>
        /// Navigate to HTML content
        /// </summary>
        public void NavigateToString(string html)
        {
            SetLoading(true);
            _webViewElement?.NavigateToString(html);
        }

        /// <summary>
        /// Execute JavaScript in the WebView
        /// </summary>
        public void ExecuteScript(string script)
        {
            _webViewElement?.ExecuteScript(script);
        }

        /// <summary>
        /// Go back in history
        /// </summary>
        public void GoBack()
        {
            if (_webViewElement?.WebView != null)
            {
                SetLoading(true);
                _webViewElement.WebView.GoBack();
                // Schedule button update for next frame (history state updates async)
                schedule.Execute(UpdateNavigationButtons).ExecuteLater(100);
            }
        }

        /// <summary>
        /// Go forward in history
        /// </summary>
        public void GoForward()
        {
            if (_webViewElement?.WebView != null)
            {
                SetLoading(true);
                _webViewElement.WebView.GoForward();
                // Schedule button update for next frame (history state updates async)
                schedule.Execute(UpdateNavigationButtons).ExecuteLater(100);
            }
        }

        /// <summary>
        /// Refresh the current page
        /// </summary>
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(CurrentUrl))
            {
                SetLoading(true);
                _webViewElement?.Navigate(CurrentUrl);
            }
        }

        /// <summary>
        /// Navigate to home page
        /// </summary>
        public void GoHome()
        {
            Navigate(HomeUrl);
        }

        /// <summary>
        /// Check if can go back
        /// </summary>
        public bool CanGoBack() => _webViewElement?.WebView?.CanGoBack() ?? false;

        /// <summary>
        /// Check if can go forward
        /// </summary>
        public bool CanGoForward() => _webViewElement?.WebView?.CanGoForward() ?? false;

        // ====================================================================
        // UI Building
        // ====================================================================

        private void BuildUI()
        {
            // Toolbar
            _toolbar = new VisualElement();
            _toolbar.name = "toolbar";
            _toolbar.AddToClassList(toolbarUssClassName);
            Add(_toolbar);

            // Navigation buttons group
            _navigationGroup = new VisualElement();
            _navigationGroup.name = "navigation-group";
            _navigationGroup.style.flexDirection = FlexDirection.Row;
            _toolbar.Add(_navigationGroup);

            // Back button
            _backButton = CreateButton("◀", "back-button", GoBack);
            _backButton.tooltip = "Go Back";
            _navigationGroup.Add(_backButton);

            // Forward button
            _forwardButton = CreateButton("▶", "forward-button", GoForward);
            _forwardButton.tooltip = "Go Forward";
            _navigationGroup.Add(_forwardButton);

            // Refresh button
            _refreshButton = CreateButton("↻", "refresh-button", Refresh);
            _refreshButton.tooltip = "Refresh";
            _navigationGroup.Add(_refreshButton);

            // Home button
            _homeButton = CreateButton("⌂", "home-button", GoHome);
            _homeButton.tooltip = "Home";
            _navigationGroup.Add(_homeButton);

            // Address bar container
            var addressContainer = new VisualElement();
            addressContainer.name = "address-container";
            addressContainer.style.flexDirection = FlexDirection.Row;
            addressContainer.style.flexGrow = 1;
            _toolbar.Add(addressContainer);

            // Address bar
            _addressBar = new TextField();
            _addressBar.name = "address-bar";
            _addressBar.AddToClassList(addressBarUssClassName);
            _addressBar.value = _initialUrl;
            _addressBar.RegisterCallback<KeyDownEvent>(OnAddressBarKeyDown);
            _addressBar.RegisterCallback<FocusInEvent>(e => _addressBar.SelectAll());
            addressContainer.Add(_addressBar);

            // Go button
            _goButton = CreateButton("→", "go-button", () => Navigate(_addressBar.value));
            _goButton.tooltip = "Go";
            addressContainer.Add(_goButton);

            // Loading indicator
            _loadingIndicator = new VisualElement();
            _loadingIndicator.name = "loading-indicator";
            _loadingIndicator.AddToClassList(loadingUssClassName);
            _loadingIndicator.style.display = DisplayStyle.None;
            _toolbar.Add(_loadingIndicator);

            // DevTools button
            _devToolsButton = CreateButton("🔧", "devtools-button", OpenDevTools);
            _devToolsButton.tooltip = "Developer Tools";
            _toolbar.Add(_devToolsButton);

            // WebView content area
            _webViewElement = new WebViewElement();
            _webViewElement.name = "webview-content";
            _webViewElement.AddToClassList(contentUssClassName);
            _webViewElement.InitialUrl = _initialUrl;
            _webViewElement.EnableDevTools = _enableDevTools;
            _webViewElement.NavigationCompleted += OnWebViewNavigationCompleted;
            _webViewElement.MessageReceived += OnWebViewMessageReceived;
            Add(_webViewElement);

            // Status bar
            _statusBar = new VisualElement();
            _statusBar.name = "status-bar";
            _statusBar.AddToClassList(statusBarUssClassName);
            
            _statusLabel = new Label();
            _statusLabel.name = "status-label";
            _statusBar.Add(_statusLabel);
            Add(_statusBar);

            // Apply initial visibility
            UpdateToolbarVisibility();
            UpdateToolbarLayout();
            UpdateStatusBarVisibility();
            UpdateNavigationButtons();
        }

        private Button CreateButton(string text, string name, Action onClick)
        {
            var button = new Button(onClick);
            button.name = name;
            button.text = text;
            button.AddToClassList(buttonUssClassName);
            return button;
        }

        private void ApplyDefaultStyles()
        {
            // Panel
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Toolbar
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.height = _toolbarHeight;
            _toolbar.style.paddingLeft = 8;
            _toolbar.style.paddingRight = 8;
            _toolbar.style.paddingTop = 4;
            _toolbar.style.paddingBottom = 4;
            _toolbar.style.alignItems = Align.Center;
            _toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            _toolbar.style.borderBottomWidth = 1;
            _toolbar.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Navigation group
            _navigationGroup.style.marginRight = 8;

            // Buttons
            StyleButton(_backButton);
            StyleButton(_forwardButton);
            StyleButton(_refreshButton);
            StyleButton(_homeButton);
            StyleButton(_goButton);
            StyleButton(_devToolsButton);

            // Address bar
            _addressBar.style.flexGrow = 1;
            _addressBar.style.height = 28;
            _addressBar.style.marginRight = 4;
            _addressBar.style.borderTopLeftRadius = 4;
            _addressBar.style.borderTopRightRadius = 4;
            _addressBar.style.borderBottomLeftRadius = 4;
            _addressBar.style.borderBottomRightRadius = 4;

            // Loading indicator
            _loadingIndicator.style.width = 16;
            _loadingIndicator.style.height = 16;
            _loadingIndicator.style.marginLeft = 8;
            _loadingIndicator.style.marginRight = 8;
            _loadingIndicator.style.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f);
            _loadingIndicator.style.borderTopLeftRadius = 8;
            _loadingIndicator.style.borderTopRightRadius = 8;
            _loadingIndicator.style.borderBottomLeftRadius = 8;
            _loadingIndicator.style.borderBottomRightRadius = 8;

            // WebView
            _webViewElement.style.flexGrow = 1;

            // Status bar
            _statusBar.style.height = 24;
            _statusBar.style.paddingLeft = 8;
            _statusBar.style.paddingRight = 8;
            _statusBar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            _statusBar.style.borderTopWidth = 1;
            _statusBar.style.borderTopColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            _statusBar.style.justifyContent = Justify.Center;

            _statusLabel.style.fontSize = 11;
            _statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        private void StyleButton(Button button)
        {
            button.style.width = 32;
            button.style.height = 28;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;
            button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            button.style.color = Color.white;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
        }

        // ====================================================================
        // UI Updates
        // ====================================================================

        private void UpdateToolbarVisibility()
        {
            if (_toolbar != null)
            {
                _toolbar.style.display = _showToolbar ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateToolbarLayout()
        {
            if (_navigationGroup != null)
            {
                _navigationGroup.style.display = _showNavigationButtons ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_backButton != null)
            {
                _backButton.style.display = _showNavigationButtons ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_forwardButton != null)
            {
                _forwardButton.style.display = _showNavigationButtons ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_refreshButton != null)
            {
                _refreshButton.style.display = _showRefreshButton ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_homeButton != null)
            {
                _homeButton.style.display = _showHomeButton ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_addressBar != null)
            {
                _addressBar.style.display = _showAddressBar ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_goButton != null)
            {
                _goButton.style.display = _showAddressBar ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_loadingIndicator != null && !_showLoadingIndicator)
            {
                _loadingIndicator.style.display = DisplayStyle.None;
            }

            if (_devToolsButton != null)
            {
                _devToolsButton.style.display = _showDevToolsButton ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateStatusBarVisibility()
        {
            if (_statusBar != null)
            {
                _statusBar.style.display = _showStatusBar ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateNavigationButtons()
        {
            // Note: Don't use SetEnabled(false) - it prevents clicks entirely in UIToolkit!
            // Instead, just change visual appearance. The GoBack/GoForward methods handle the actual check.
            if (_backButton != null)
            {
                bool canGoBack = CanGoBack();
                _backButton.style.opacity = canGoBack ? 1f : 0.4f;
                _backButton.style.color = canGoBack ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            if (_forwardButton != null)
            {
                bool canGoForward = CanGoForward();
                _forwardButton.style.opacity = canGoForward ? 1f : 0.4f;
                _forwardButton.style.color = canGoForward ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        private void UpdateAddressBar(string url)
        {
            if (_addressBar != null && !string.IsNullOrEmpty(url))
            {
                _addressBar.value = url;
                UrlChanged?.Invoke(url);
            }
        }

        private void SetLoading(bool loading)
        {
            IsLoading = loading;

            if (_loadingIndicator != null && _showLoadingIndicator)
            {
                _loadingIndicator.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_refreshButton != null)
            {
                _refreshButton.text = loading ? "✕" : "↻";
                _refreshButton.tooltip = loading ? "Stop" : "Refresh";
            }

            if (_statusLabel != null)
            {
                _statusLabel.text = loading ? "Loading..." : "Done";
            }
        }

        private void SetStatus(string status)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = status;
            }
        }

        // ====================================================================
        // Event Handlers
        // ====================================================================

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // WebViewElement handles its own creation
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            // WebViewElement handles its own cleanup
        }

        private void OnAddressBarKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                Navigate(_addressBar.value);
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                // Restore current URL
                if (!string.IsNullOrEmpty(CurrentUrl))
                {
                    _addressBar.value = CurrentUrl;
                }
                _addressBar.Blur();
                evt.StopPropagation();
            }
        }

        private void OnWebViewNavigationCompleted(string url, bool isSuccess)
        {
            SetLoading(false);
            UpdateAddressBar(url);
            UpdateNavigationButtons();

            if (isSuccess)
            {
                SetStatus("Done");
            }
            else
            {
                SetStatus("Navigation failed");
            }

            NavigationCompleted?.Invoke(url, isSuccess);
        }

        private void OnWebViewMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }

        private void OpenDevTools()
        {
            // DevTools opens automatically if EnableDevTools is true
            // We can also try to open it via JavaScript
            ExecuteScript("if(typeof __WEBVIEW_DEVTOOLS__ !== 'undefined') __WEBVIEW_DEVTOOLS__();");
            Debug.Log("[WebViewPanel] DevTools requested. Make sure EnableDevTools is set to true.");
        }
    }
}
