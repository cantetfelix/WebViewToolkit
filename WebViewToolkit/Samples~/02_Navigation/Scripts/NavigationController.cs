// ============================================================================
// WebViewToolkit - Navigation Sample
// ============================================================================
// This sample demonstrates URL navigation, history management, and responding
// to navigation events. It shows how to build a simple browser with back/
// forward buttons and an address bar.
// ============================================================================

using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Samples.Navigation
{
    /// <summary>
    /// Controller for the Navigation sample.
    /// Demonstrates URL navigation, history management, and navigation events.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/Navigation Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class NavigationController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the navigation UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("The USS file for navigation UI styling")]
        [SerializeField] private StyleSheet _uss;

        // UI Element References
        private UIDocument _uiDocument;
        private WebViewElement _webViewElement;
        private TextField _addressBar;
        private Button _backButton;
        private Button _forwardButton;
        private Button _refreshButton;
        private Button _goButton;
        private Label _statusText;

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
                Debug.LogError("[Navigation] UIDocument or UXML not assigned!");
                return;
            }

            BuildUI();
            BindEvents();
        }

        private void OnDestroy()
        {
            UnbindEvents();
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

            // Query UI elements
            _webViewElement = root.Q<WebViewElement>("webview");
            _addressBar = root.Q<TextField>("address-bar");
            _backButton = root.Q<Button>("btn-back");
            _forwardButton = root.Q<Button>("btn-forward");
            _refreshButton = root.Q<Button>("btn-refresh");
            _goButton = root.Q<Button>("btn-go");
            _statusText = root.Q<Label>("status-text");

            // Validate
            if (_webViewElement == null)
            {
                Debug.LogError("[Navigation] WebViewElement not found in UXML!");
            }
        }

        private void BindEvents()
        {
            if (_webViewElement == null) return;

            // WebView events
            _webViewElement.NavigationCompleted += OnNavigationCompleted;

            // Button clicks
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;

            if (_forwardButton != null)
                _forwardButton.clicked += OnForwardClicked;

            if (_refreshButton != null)
                _refreshButton.clicked += OnRefreshClicked;

            if (_goButton != null)
                _goButton.clicked += OnGoClicked;

            // Address bar - Navigate on Enter key
            if (_addressBar != null)
            {
                _addressBar.RegisterCallback<KeyDownEvent>(OnAddressBarKeyDown);

                // Set initial URL
                _addressBar.value = _webViewElement.InitialUrl;
            }

            // Initial button state update (after WebView is ready)
            // We'll update it when navigation completes
        }

        private void UnbindEvents()
        {
            if (_webViewElement != null)
            {
                _webViewElement.NavigationCompleted -= OnNavigationCompleted;
            }

            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;

            if (_forwardButton != null)
                _forwardButton.clicked -= OnForwardClicked;

            if (_refreshButton != null)
                _refreshButton.clicked -= OnRefreshClicked;

            if (_goButton != null)
                _goButton.clicked -= OnGoClicked;

            if (_addressBar != null)
            {
                _addressBar.UnregisterCallback<KeyDownEvent>(OnAddressBarKeyDown);
            }
        }

        // ====================================================================
        // Navigation Actions
        // ====================================================================

        private void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                SetStatus("Invalid URL", true);
                return;
            }

            // Add https:// if no protocol specified
            if (!url.StartsWith("http://") &&
                !url.StartsWith("https://") &&
                !url.StartsWith("file://"))
            {
                url = "https://" + url;
            }

            SetStatus($"Loading: {url}...", false);
            _webViewElement?.Navigate(url);
        }

        private void GoBack()
        {
            if (_webViewElement?.WebView == null) return;

            if (_webViewElement.WebView.CanGoBack())
            {
                SetStatus("Going back...", false);
                _webViewElement.WebView.GoBack();
            }
        }

        private void GoForward()
        {
            if (_webViewElement?.WebView == null) return;

            if (_webViewElement.WebView.CanGoForward())
            {
                SetStatus("Going forward...", false);
                _webViewElement.WebView.GoForward();
            }
        }

        private void Refresh()
        {
            if (_webViewElement?.CurrentUrl == null) return;

            SetStatus("Refreshing...", false);
            _webViewElement.Navigate(_webViewElement.CurrentUrl);
        }

        // ====================================================================
        // Event Handlers
        // ====================================================================

        private void OnBackClicked()
        {
            GoBack();
        }

        private void OnForwardClicked()
        {
            GoForward();
        }

        private void OnRefreshClicked()
        {
            Refresh();
        }

        private void OnGoClicked()
        {
            Navigate(_addressBar.value);
        }

        private void OnAddressBarKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                Navigate(_addressBar.value);
                _addressBar.Blur(); // Remove focus from address bar
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Called when navigation completes (success or failure)
        /// </summary>
        private void OnNavigationCompleted(string url, bool isSuccess)
        {
            if (isSuccess)
            {
                // Update address bar with the actual URL
                if (_addressBar != null)
                {
                    _addressBar.value = url;
                }

                SetStatus("Ready", false);
            }
            else
            {
                SetStatus("Navigation failed", true);
            }

            // Update button states based on history
            UpdateNavigationButtons();
        }

        // ====================================================================
        // UI Updates
        // ====================================================================

        /// <summary>
        /// Updates the enable/disable state of navigation buttons based on
        /// whether we can go back or forward in history.
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (_webViewElement?.WebView == null) return;

            // Update back button
            if (_backButton != null)
            {
                bool canGoBack = _webViewElement.WebView.CanGoBack();
                _backButton.SetEnabled(canGoBack);

                // Visual feedback - change opacity when disabled
                _backButton.style.opacity = canGoBack ? 1f : 0.4f;
            }

            // Update forward button
            if (_forwardButton != null)
            {
                bool canGoForward = _webViewElement.WebView.CanGoForward();
                _forwardButton.SetEnabled(canGoForward);

                // Visual feedback - change opacity when disabled
                _forwardButton.style.opacity = canGoForward ? 1f : 0.4f;
            }
        }

        /// <summary>
        /// Updates the status bar text
        /// </summary>
        private void SetStatus(string message, bool isError)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.style.color = isError
                    ? new Color(1f, 0.4f, 0.4f)
                    : new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }
}
