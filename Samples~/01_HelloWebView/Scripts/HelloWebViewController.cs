// ============================================================================
// WebViewToolkit - Hello WebView Sample
// ============================================================================
// This is the simplest possible WebView setup. It demonstrates the absolute
// minimum code needed to display a web page in Unity using UIToolkit.
// ============================================================================

using UnityEngine;
using UnityEngine.UIElements;

namespace WebViewToolkit.Samples.HelloWebView
{
    /// <summary>
    /// Minimal controller that loads the Hello WebView UI.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/Hello WebView Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class HelloWebViewController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the WebView UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("Optional USS file for custom styling")]
        [SerializeField] private StyleSheet _uss;

        private UIDocument _uiDocument;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            if (_uiDocument == null || _uxml == null)
            {
                Debug.LogError("[HelloWebView] UIDocument or UXML not assigned!");
                return;
            }

            // Load the UI from UXML
            var root = _uiDocument.rootVisualElement;
            root.Clear();
            _uxml.CloneTree(root);

            // Apply custom styles if provided
            if (_uss != null)
            {
                root.styleSheets.Add(_uss);
            }

            // That's it! The WebViewElement in the UXML will automatically:
            // 1. Create the WebView when attached to the panel
            // 2. Load the initial URL (https://www.google.com)
            // 3. Handle resizing when the window size changes
            // 4. Forward mouse input to the web content
            //
            // No additional code needed for basic functionality!
        }
    }
}
