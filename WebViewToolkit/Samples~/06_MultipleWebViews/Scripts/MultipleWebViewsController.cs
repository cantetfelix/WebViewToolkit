// ============================================================================
// WebViewToolkit - Multiple WebViews Sample
// ============================================================================
// This sample demonstrates managing multiple WebView instances simultaneously.
// Shows proper lifecycle management, independent navigation, and performance
// monitoring when running several WebViews at once.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Samples.MultipleWebViews
{
    /// <summary>
    /// Controller for the Multiple WebViews sample.
    /// Manages 4 independent WebView instances with individual controls.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/Multiple WebViews Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class MultipleWebViewsController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the multiple webviews UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("The USS file for multiple webviews UI styling")]
        [SerializeField] private StyleSheet _uss;

        [Header("Default URLs")]
        [Tooltip("Default URLs for each WebView")]
        [SerializeField] private string _defaultUrl1 = "https://unity.com";
        [SerializeField] private string _defaultUrl2 = "https://docs.unity3d.com";
        [SerializeField] private string _defaultUrl3 = "https://github.com";
        [SerializeField] private string _defaultUrl4 = "https://stackoverflow.com";

        // UI Element References
        private UIDocument _uiDocument;

        // WebView cards (container for each WebView + controls)
        private List<WebViewCard> _webViewCards = new List<WebViewCard>();

        // Stats labels
        private Label _fpsLabel;
        private Label _memoryLabel;
        private Label _activeViewsLabel;

        // Preset buttons
        private Button _presetNewsButton;
        private Button _presetDevButton;
        private Button _presetSocialButton;
        private Button _presetDocsButton;
        private Button _resetAllButton;

        // Performance tracking
        private float _fpsUpdateInterval = 0.5f;
        private float _timeSinceLastFpsUpdate = 0f;
        private int _frameCount = 0;

        // ====================================================================
        // Unity Lifecycle
        // ====================================================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            if (_uiDocument == null || _uxml == null)
            {
                Debug.LogError("[MultipleWebViews] UIDocument or UXML not assigned!");
                return;
            }

            BuildUI();
            BindEvents();
            InitializeWebViews();
        }

        private void Update()
        {
            UpdatePerformanceStats();
        }

        private void OnDestroy()
        {
            UnbindEvents();

            // Clean up all WebView cards
            foreach (var card in _webViewCards)
            {
                card.Dispose();
            }
            _webViewCards.Clear();
        }

        // ====================================================================
        // UI Setup
        // ====================================================================

        private void BuildUI()
        {
            // Load UXML
            var root = _uiDocument.rootVisualElement;
            root.Clear();
            _uxml.CloneTree(root);

            // Apply styles
            if (_uss != null)
            {
                root.styleSheets.Add(_uss);
            }

            // Create WebView cards for all 4 WebViews
            for (int i = 1; i <= 4; i++)
            {
                var card = new WebViewCard(i, root, this);
                _webViewCards.Add(card);
            }

            // Query stats labels
            _fpsLabel = root.Q<Label>("lbl-fps");
            _memoryLabel = root.Q<Label>("lbl-memory");
            _activeViewsLabel = root.Q<Label>("lbl-active-views");

            // Query preset buttons
            _presetNewsButton = root.Q<Button>("btn-preset-news");
            _presetDevButton = root.Q<Button>("btn-preset-dev");
            _presetSocialButton = root.Q<Button>("btn-preset-social");
            _presetDocsButton = root.Q<Button>("btn-preset-docs");
            _resetAllButton = root.Q<Button>("btn-reset-all");

            Debug.Log("[MultipleWebViews] UI built successfully");
        }

        private void BindEvents()
        {
            // Bind all WebView card events
            foreach (var card in _webViewCards)
            {
                card.BindEvents();
            }

            // Bind preset buttons
            if (_presetNewsButton != null)
                _presetNewsButton.clicked += OnPresetNews;

            if (_presetDevButton != null)
                _presetDevButton.clicked += OnPresetDev;

            if (_presetSocialButton != null)
                _presetSocialButton.clicked += OnPresetSocial;

            if (_presetDocsButton != null)
                _presetDocsButton.clicked += OnPresetDocs;

            if (_resetAllButton != null)
                _resetAllButton.clicked += OnResetAll;
        }

        private void UnbindEvents()
        {
            // Unbind all WebView card events
            foreach (var card in _webViewCards)
            {
                card.UnbindEvents();
            }

            // Unbind preset buttons
            if (_presetNewsButton != null)
                _presetNewsButton.clicked -= OnPresetNews;

            if (_presetDevButton != null)
                _presetDevButton.clicked -= OnPresetDev;

            if (_presetSocialButton != null)
                _presetSocialButton.clicked -= OnPresetSocial;

            if (_presetDocsButton != null)
                _presetDocsButton.clicked -= OnPresetDocs;

            if (_resetAllButton != null)
                _resetAllButton.clicked -= OnResetAll;
        }

        // ====================================================================
        // WebView Initialization
        // ====================================================================

        private void InitializeWebViews()
        {
            // Set default URLs
            string[] defaultUrls = { _defaultUrl1, _defaultUrl2, _defaultUrl3, _defaultUrl4 };

            for (int i = 0; i < _webViewCards.Count && i < defaultUrls.Length; i++)
            {
                _webViewCards[i].SetUrl(defaultUrls[i]);
            }

            // Navigate all WebViews with delay
            foreach (var card in _webViewCards)
            {
                card.ScheduleInitialNavigation(100);
            }

            Debug.Log("[MultipleWebViews] Initialized all WebView instances");
        }

        // ====================================================================
        // Preset Actions
        // ====================================================================

        private void OnPresetNews()
        {
            string[] newsUrls = {
                "https://news.ycombinator.com",
                "https://www.reddit.com/r/programming",
                "https://techcrunch.com",
                "https://arstechnica.com"
            };
            ApplyPreset(newsUrls, "News Sites");
        }

        private void OnPresetDev()
        {
            string[] devUrls = {
                "https://github.com",
                "https://stackoverflow.com",
                "https://developer.mozilla.org",
                "https://docs.unity3d.com"
            };
            ApplyPreset(devUrls, "Dev Resources");
        }

        private void OnPresetSocial()
        {
            string[] socialUrls = {
                "https://twitter.com",
                "https://www.reddit.com",
                "https://www.linkedin.com",
                "https://discord.com"
            };
            ApplyPreset(socialUrls, "Social Media");
        }

        private void OnPresetDocs()
        {
            string[] docsUrls = {
                "https://docs.unity3d.com",
                "https://learn.microsoft.com",
                "https://developer.mozilla.org",
                "https://docs.github.com"
            };
            ApplyPreset(docsUrls, "Documentation");
        }

        private void OnResetAll()
        {
            InitializeWebViews();
            Debug.Log("[MultipleWebViews] Reset all WebViews to default URLs");
        }

        private void ApplyPreset(string[] urls, string presetName)
        {
            for (int i = 0; i < _webViewCards.Count && i < urls.Length; i++)
            {
                _webViewCards[i].SetUrl(urls[i]);
                _webViewCards[i].Navigate();
            }
            Debug.Log($"[MultipleWebViews] Applied preset: {presetName}");
        }

        // ====================================================================
        // Performance Monitoring
        // ====================================================================

        private void UpdatePerformanceStats()
        {
            // FPS calculation
            _frameCount++;
            _timeSinceLastFpsUpdate += Time.deltaTime;

            if (_timeSinceLastFpsUpdate >= _fpsUpdateInterval)
            {
                float fps = _frameCount / _timeSinceLastFpsUpdate;
                _frameCount = 0;
                _timeSinceLastFpsUpdate = 0f;

                if (_fpsLabel != null)
                {
                    _fpsLabel.text = $"FPS: {fps:F0}";
                }
            }

            // Memory usage (update less frequently)
            if (Time.frameCount % 60 == 0 && _memoryLabel != null)
            {
                long memoryBytes = System.GC.GetTotalMemory(false);
                float memoryMB = memoryBytes / (1024f * 1024f);
                _memoryLabel.text = $"Memory: {memoryMB:F1} MB";
            }

            // Active views count
            if (Time.frameCount % 30 == 0 && _activeViewsLabel != null)
            {
                int activeCount = 0;
                foreach (var card in _webViewCards)
                {
                    if (card.IsActive)
                        activeCount++;
                }
                _activeViewsLabel.text = $"Active: {activeCount}/{_webViewCards.Count}";
            }
        }

        // ====================================================================
        // Public Methods for WebViewCard
        // ====================================================================

        public void OnWebViewNavigationCompleted(int cardIndex, string url, bool success)
        {
            Debug.Log($"[MultipleWebViews] WebView {cardIndex} completed navigation to: {url} (Success: {success})");
        }
    }

    // ====================================================================
    // WebViewCard Class - Manages Individual WebView Instance
    // ====================================================================

    /// <summary>
    /// Represents a single WebView card with controls
    /// </summary>
    public class WebViewCard
    {
        private int _index;
        private MultipleWebViewsController _controller;

        // UI Elements
        private WebViewElement _webView;
        private TextField _urlField;
        private Button _backButton;
        private Button _forwardButton;
        private Button _refreshButton;
        private Button _goButton;
        private Label _statusLabel;

        private string _currentUrl;
        private bool _isInitialized = false;

        public bool IsActive => _webView != null && _webView.panel != null;

        public WebViewCard(int index, VisualElement root, MultipleWebViewsController controller)
        {
            _index = index;
            _controller = controller;

            // Query UI elements for this card
            _webView = root.Q<WebViewElement>($"webview-{index}");
            _urlField = root.Q<TextField>($"txt-url-{index}");
            _backButton = root.Q<Button>($"btn-back-{index}");
            _forwardButton = root.Q<Button>($"btn-forward-{index}");
            _refreshButton = root.Q<Button>($"btn-refresh-{index}");
            _goButton = root.Q<Button>($"btn-go-{index}");
            _statusLabel = root.Q<Label>($"lbl-status-{index}");

            if (_webView == null)
            {
                Debug.LogError($"[WebViewCard {index}] WebViewElement not found!");
            }
        }

        public void BindEvents()
        {
            if (_backButton != null)
                _backButton.clicked += OnBack;

            if (_forwardButton != null)
                _forwardButton.clicked += OnForward;

            if (_refreshButton != null)
                _refreshButton.clicked += OnRefresh;

            if (_goButton != null)
                _goButton.clicked += Navigate;

            if (_urlField != null)
                _urlField.RegisterCallback<KeyDownEvent>(OnUrlFieldKeyDown);

            if (_webView != null)
            {
                _webView.NavigationCompleted += OnNavigationCompleted;
            }
        }

        public void UnbindEvents()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBack;

            if (_forwardButton != null)
                _forwardButton.clicked -= OnForward;

            if (_refreshButton != null)
                _refreshButton.clicked -= OnRefresh;

            if (_goButton != null)
                _goButton.clicked -= Navigate;

            if (_urlField != null)
                _urlField.UnregisterCallback<KeyDownEvent>(OnUrlFieldKeyDown);

            if (_webView != null)
            {
                _webView.NavigationCompleted -= OnNavigationCompleted;
            }
        }

        public void SetUrl(string url)
        {
            _currentUrl = url;
            if (_urlField != null)
            {
                _urlField.value = url;
            }
        }

        public void ScheduleInitialNavigation(long delayMs)
        {
            if (_webView != null)
            {
                // Check if already attached
                if (_webView.panel != null)
                {
                    _webView.schedule.Execute(() => Navigate()).ExecuteLater(delayMs);
                }
                else
                {
                    _webView.RegisterCallback<AttachToPanelEvent>(evt =>
                    {
                        _webView.schedule.Execute(() => Navigate()).ExecuteLater(delayMs);
                    });
                }
            }
        }

        public void Navigate()
        {
            if (_webView == null || string.IsNullOrEmpty(_currentUrl))
                return;

            UpdateStatus("Navigating...");
            _webView.Navigate(_currentUrl);
        }

        private void OnBack()
        {
            if (_webView != null && _webView.CanGoBack())
            {
                _webView.GoBack();
            }
        }

        private void OnForward()
        {
            if (_webView != null && _webView.CanGoForward())
            {
                _webView.GoForward();
            }
        }

        private void OnRefresh()
        {
            if (_webView != null)
            {
                _webView.Reload();
            }
        }

        private void OnUrlFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                _currentUrl = _urlField.value;
                Navigate();
                evt.StopPropagation();
            }
        }

        private void OnNavigationCompleted(string url, bool isSuccess)
        {
            _currentUrl = url;

            if (_urlField != null)
            {
                _urlField.value = url;
            }

            UpdateStatus(isSuccess ? "Ready" : "Failed");
            UpdateNavigationButtons();
            _controller?.OnWebViewNavigationCompleted(_index, url, isSuccess);
        }

        private void UpdateNavigationButtons()
        {
            if (_webView == null) return;

            if (_backButton != null)
                _backButton.SetEnabled(_webView.CanGoBack());

            if (_forwardButton != null)
                _forwardButton.SetEnabled(_webView.CanGoForward());
        }

        private void UpdateStatus(string status)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = status;
            }
        }

        public void Dispose()
        {
            UnbindEvents();
        }
    }
}
