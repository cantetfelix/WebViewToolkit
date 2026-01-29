using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace WebViewToolkit.UIToolkit.Demo
{
    [AddComponentMenu("WebView Toolkit/Demo/WebView Panel Demo")]
    [RequireComponent(typeof(UIDocument))]
    public class WebViewPanelDemo : MonoBehaviour
    {
        [Header("UI Assets")]
        [SerializeField] private VisualTreeAsset _uxml;
        [SerializeField] private StyleSheet _uss;

        [Header("Configuration")]
        [SerializeField] private string _initialUrl = "https://www.google.com";

        // UI References
        private UIDocument _uiDocument;
        private WebViewPanel _webViewPanel;
        
        // Custom Toolbar
        private TextField _addressBar;
        private Button _backBtn;
        private Button _fwdBtn;
        private ScrollView _historyList;
        private Label _statusLabel;
        
        // State
        private readonly List<string> _history = new List<string>();
        private const int MaxHistoryItems = 20;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BuildUI();
        }

        private void OnDisable()
        {
            if (_webViewPanel != null)
            {
                _webViewPanel.NavigationStarted -= OnNavigationStarted;
                _webViewPanel.NavigationCompleted -= OnNavigationCompleted;
                _webViewPanel.UrlChanged -= OnUrlChanged;
            }
        }

        private void BuildUI()
        {
            if (_uiDocument == null) return;
            var root = _uiDocument.rootVisualElement;
            root.Clear();

            // Load UXML if assigned, otherwise try to find it (or user must assign it)
            // Ideally user assigns it in Inspector. We can also fallback to loading from Resources if we placed it there.
            // For now, if _uxml is null, we assume the user will assign it.
            // FIX: Since this is a package, we can't easily rely on Inspector assignment for "Script default".
            // But we can try to Find it if null? Or simply assume user sets it up.
            
            if (_uxml != null)
            {
                _uxml.CloneTree(root);
            }
            else
            {
                // Fallback or Error
                Debug.LogWarning("[WebViewPanelDemo] UXML asset not assigned! Please assign WebViewDemo.uxml in Inspector.");
                // Try Load via AssetDatabase if Editor? No, runtime.
                return;
            }

            if (_uss != null)
            {
                root.styleSheets.Add(_uss);
            }

            // Find Elements
            _webViewPanel = root.Q<WebViewPanel>("webview-panel");
            _addressBar = root.Q<TextField>("address-bar");
            _historyList = root.Q<ScrollView>("history-list");
            _statusLabel = root.Q<Label>("status-label");

            // Bind WebViewPanel
            if (_webViewPanel != null)
            {
                _webViewPanel.InitialUrl = _initialUrl;
                _webViewPanel.NavigationStarted += OnNavigationStarted;
                _webViewPanel.NavigationCompleted += OnNavigationCompleted;
                _webViewPanel.UrlChanged += OnUrlChanged;

                // Bind Nav Buttons
                root.Q<Button>("btn-back")?.RegisterCallback<ClickEvent>(e => _webViewPanel.GoBack());
                root.Q<Button>("btn-fwd")?.RegisterCallback<ClickEvent>(e => _webViewPanel.GoForward());
                root.Q<Button>("btn-reload")?.RegisterCallback<ClickEvent>(e => _webViewPanel.Refresh());
                root.Q<Button>("btn-home")?.RegisterCallback<ClickEvent>(e => _webViewPanel.GoHome());
                root.Q<Button>("btn-devtools")?.RegisterCallback<ClickEvent>(e => _webViewPanel.WebViewElement.EnableDevTools = !_webViewPanel.WebViewElement.EnableDevTools);
                
                // Bind Address Bar
                root.Q<Button>("btn-go")?.RegisterCallback<ClickEvent>(e => Navigate(_addressBar.value));
                _addressBar?.RegisterCallback<KeyDownEvent>(e => 
                {
                    if (e.keyCode == KeyCode.Return) Navigate(_addressBar.value);
                });
            }

            // Bind Sidebar Buttons
            root.Q<Button>("link-google")?.RegisterCallback<ClickEvent>(e => Navigate("https://google.com"));
            root.Q<Button>("link-youtube")?.RegisterCallback<ClickEvent>(e => Navigate("https://youtube.com"));
            root.Q<Button>("link-github")?.RegisterCallback<ClickEvent>(e => Navigate("https://github.com"));
            root.Q<Button>("link-unity")?.RegisterCallback<ClickEvent>(e => Navigate("https://docs.unity3d.com"));

            root.Q<Button>("btn-js")?.RegisterCallback<ClickEvent>(e => 
                _webViewPanel?.ExecuteScript("alert('Hello from Premium Demo!');"));
                
            root.Q<Button>("btn-html")?.RegisterCallback<ClickEvent>(e => 
                _webViewPanel?.NavigateToString("<h1>Native Content</h1><p>Rendered directly from string.</p>"));
                
            // GFX Label
            var gfxLabel = root.Q<Label>("gfx-label");
            if (gfxLabel != null && WebViewManager.Instance != null && WebViewManager.Instance.IsInitialized)
            {
                gfxLabel.text = WebViewManager.Instance.CurrentGraphicsAPI.ToString();
            }
        }

        private void Navigate(string url)
        {
            _webViewPanel?.Navigate(url);
        }

        private void OnNavigationStarted(string url)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = "Loading...";
                _statusLabel.style.color = new Color(0.9f, 0.9f, 0.4f);
            }
        }

        private void OnNavigationCompleted(string url, bool success)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = success ? "Ready" : "Failed";
                _statusLabel.style.color = success ? new Color(0.4f, 0.8f, 0.4f) : Color.red;
            }

            if (success)
            {
                Debug.Log($"[Demo] Navigation Success: {url}. Adding to history.");
                AddToHistory(url);
            }
            else
            {
                Debug.LogWarning($"[Demo] Navigation Failed: {url}");
            }
        }

        private void OnUrlChanged(string url)
        {
            if (_addressBar != null)
            {
                _addressBar.value = url;
            }
        }

        private void AddToHistory(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            
            // Avoid duplicates at top
            if (_history.Count > 0 && _history[0] == url) return;

            _history.Insert(0, url);
            if (_history.Count > MaxHistoryItems) _history.RemoveAt(_history.Count - 1);

            RefreshHistoryUI();
        }

        private void RefreshHistoryUI()
        {
            if (_historyList == null) 
            {
                Debug.LogError("[Demo] History List (ScrollView) is null!");
                return;
            }
            
            _historyList.Clear();
            Debug.Log($"[Demo] Refreshing History UI. Count: {_history.Count}");

            foreach (var url in _history)
            {
                var btn = new Button(() => 
                {
                    Navigate(url);
                });
                btn.text = url;
                btn.AddToClassList("sidebar__btn");
                btn.AddToClassList("sidebar__btn--history");
                _historyList.Add(btn);
            }
        }
    }
}
