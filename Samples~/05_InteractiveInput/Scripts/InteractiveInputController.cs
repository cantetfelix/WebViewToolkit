// ============================================================================
// WebViewToolkit - Interactive Input Sample
// ============================================================================
// This sample demonstrates how mouse and keyboard input works with WebView.
// Shows how WebViewElement automatically forwards input events to the web content
// and how to handle form submissions and user interactions.
// ============================================================================

using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Samples.InteractiveInput
{
    /// <summary>
    /// Controller for the Interactive Input sample.
    /// Demonstrates automatic input forwarding and form interaction.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/Interactive Input Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class InteractiveInputController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the interactive input UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("The USS file for interactive input UI styling")]
        [SerializeField] private StyleSheet _uss;

        // UI Element References
        private UIDocument _uiDocument;
        private WebViewElement _webViewElement;

        // Status labels
        private Label _mouseXLabel;
        private Label _mouseYLabel;
        private Label _mouseNormalizedLabel;
        private Label _clickPositionLabel;
        private Label _clickTimeLabel;
        private Label _scrollDeltaLabel;
        private Label _scrollTotalLabel;
        private Label _formCountLabel;
        private Label _lastFormDataLabel;

        // Tracking data
        private int _formSubmissionCount = 0;
        private int _scrollTotal = 0;

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
                Debug.LogError("[InteractiveInput] UIDocument or UXML not assigned!");
                return;
            }

            BuildUI();

            // Check if WebView is already attached
            if (_webViewElement != null && _webViewElement.panel != null)
            {
                // Already attached, load content with delay to ensure WebView is fully initialized
                _webViewElement.schedule.Execute(() => LoadInteractiveForm()).ExecuteLater(100);
            }
            else if (_webViewElement != null)
            {
                // Not attached yet, wait for attachment
                _webViewElement.RegisterCallback<AttachToPanelEvent>(OnWebViewAttached);
            }
        }

        private void OnWebViewAttached(AttachToPanelEvent evt)
        {
            // Unregister to avoid multiple calls
            _webViewElement.UnregisterCallback<AttachToPanelEvent>(OnWebViewAttached);

            // Now the WebView is ready, load the initial view
            Debug.Log("[InteractiveInput] WebView attached to panel, loading interactive form");
            _webViewElement.schedule.Execute(() => LoadInteractiveForm()).ExecuteLater(100);
        }

        private void OnDestroy()
        {
            if (_webViewElement != null)
            {
                _webViewElement.MessageReceived -= OnMessageReceived;
            }
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

            // Query status labels
            _mouseXLabel = root.Q<Label>("lbl-mouse-x");
            _mouseYLabel = root.Q<Label>("lbl-mouse-y");
            _mouseNormalizedLabel = root.Q<Label>("lbl-mouse-normalized");
            _clickPositionLabel = root.Q<Label>("lbl-click-position");
            _clickTimeLabel = root.Q<Label>("lbl-click-time");
            _scrollDeltaLabel = root.Q<Label>("lbl-scroll-delta");
            _scrollTotalLabel = root.Q<Label>("lbl-scroll-total");
            _formCountLabel = root.Q<Label>("lbl-form-count");
            _lastFormDataLabel = root.Q<Label>("lbl-last-form-data");

            // Validate
            if (_webViewElement == null)
            {
                Debug.LogError("[InteractiveInput] WebViewElement not found in UXML!");
            }
            else
            {
                Debug.Log("[InteractiveInput] WebViewElement found successfully");

                // Subscribe to message events for form submissions
                _webViewElement.MessageReceived += OnMessageReceived;
            }
        }

        // ====================================================================
        // WebView Content
        // ====================================================================

        /// <summary>
        /// Loads the interactive form HTML
        /// </summary>
        private void LoadInteractiveForm()
        {
            Debug.Log("[InteractiveInput] Loading interactive form");

            if (_webViewElement == null)
            {
                Debug.LogError("[InteractiveInput] Cannot load form - WebViewElement is null!");
                return;
            }

            string html = GenerateInteractiveFormHTML();
            _webViewElement.NavigateToString(html);

            Debug.Log("[InteractiveInput] Interactive form loaded");
        }

        /// <summary>
        /// Generates HTML for the interactive form
        /// </summary>
        private string GenerateInteractiveFormHTML()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Interactive Form</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        html {
            scroll-behavior: smooth;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #1e1e2e 0%, #2d2d44 100%);
            color: #e0e0e0;
            padding: 40px 20px;
            min-height: 120vh; /* Reduced height for better scroll control */
        }

        .container {
            max-width: 600px;
            margin: 0 auto;
        }

        .header {
            text-align: center;
            margin-bottom: 40px;
            padding-bottom: 20px;
            border-bottom: 2px solid #444;
        }

        h1 {
            font-size: 32px;
            color: #60a5fa;
            margin-bottom: 10px;
        }

        .subtitle {
            color: #999;
            font-size: 14px;
        }

        .form-card {
            background: #2a2a3e;
            border-radius: 12px;
            padding: 30px;
            border: 1px solid #3a3a4e;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
        }

        .form-group {
            margin-bottom: 24px;
        }

        label {
            display: block;
            font-size: 14px;
            font-weight: 600;
            color: #bbb;
            margin-bottom: 8px;
        }

        input[type='text'],
        input[type='email'],
        input[type='number'],
        textarea,
        select {
            width: 100%;
            padding: 12px;
            background: #1e1e2e;
            border: 2px solid #444;
            border-radius: 6px;
            color: #e0e0e0;
            font-size: 14px;
            transition: all 0.3s ease;
        }

        input:focus,
        textarea:focus,
        select:focus {
            outline: none;
            border-color: #60a5fa;
            box-shadow: 0 0 0 3px rgba(96, 165, 250, 0.1);
        }

        input:hover,
        textarea:hover,
        select:hover {
            border-color: #555;
        }

        textarea {
            min-height: 100px;
            resize: vertical;
        }

        .checkbox-group {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .checkbox-item {
            display: flex;
            align-items: center;
            cursor: pointer;
            padding: 12px;
            background: #1e1e2e;
            border-radius: 6px;
            border: 2px solid #444;
            transition: all 0.3s ease;
        }

        .checkbox-item:hover {
            border-color: #60a5fa;
            background: #252535;
        }

        .checkbox-item input[type='checkbox'] {
            width: 20px;
            height: 20px;
            margin-right: 12px;
            cursor: pointer;
        }

        .submit-button {
            width: 100%;
            padding: 16px;
            background: linear-gradient(135deg, #3b82f6 0%, #60a5fa 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 6px rgba(59, 130, 246, 0.3);
        }

        .submit-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(59, 130, 246, 0.4);
        }

        .submit-button:active {
            transform: translateY(0);
            box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
        }

        .mouse-follower {
            position: fixed;
            width: 20px;
            height: 20px;
            border: 2px solid #60a5fa;
            border-radius: 50%;
            pointer-events: none;
            transform: translate(-50%, -50%);
            transition: opacity 0.15s ease;
            z-index: 9999;
            opacity: 0;
        }

        .click-ripple {
            position: fixed;
            border: 2px solid #60a5fa;
            border-radius: 50%;
            pointer-events: none;
            transform: translate(-50%, -50%);
            animation: ripple 0.6s ease-out;
            z-index: 9998;
        }

        @keyframes ripple {
            0% {
                width: 20px;
                height: 20px;
                opacity: 1;
            }
            100% {
                width: 100px;
                height: 100px;
                opacity: 0;
            }
        }

        .scroll-indicator {
            position: fixed;
            top: 50%;
            right: 20px;
            transform: translateY(-50%);
            background: #2a2a3e;
            padding: 12px;
            border-radius: 8px;
            border: 1px solid #444;
            font-size: 12px;
            color: #999;
            min-width: 100px;
        }

        .footer {
            margin-top: 60px;
            text-align: center;
            padding: 40px 20px;
            background: #2a2a3e;
            border-radius: 12px;
            border: 1px solid #3a3a4e;
        }

        .footer h2 {
            color: #60a5fa;
            margin-bottom: 16px;
        }

        .footer p {
            color: #999;
            line-height: 1.6;
        }
    </style>
</head>
<body>
    <div class='mouse-follower' id='mouseFollower'></div>
    <div class='scroll-indicator' id='scrollIndicator'>
        Scroll: 0px
    </div>

    <div class='container'>
        <div class='header'>
            <h1>📝 Interactive Form</h1>
            <div class='subtitle'>Move your mouse, click, and interact with the form</div>
        </div>

        <div class='form-card'>
            <form id='userForm'>
                <div class='form-group'>
                    <label for='name'>Name</label>
                    <input type='text' id='name' name='name' placeholder='Enter your name' required>
                </div>

                <div class='form-group'>
                    <label for='email'>Email</label>
                    <input type='email' id='email' name='email' placeholder='Enter your email' required>
                </div>

                <div class='form-group'>
                    <label for='age'>Age</label>
                    <input type='number' id='age' name='age' placeholder='Enter your age' min='1' max='120'>
                </div>

                <div class='form-group'>
                    <label for='country'>Country</label>
                    <select id='country' name='country'>
                        <option value=''>Select a country</option>
                        <option value='USA'>United States</option>
                        <option value='UK'>United Kingdom</option>
                        <option value='Canada'>Canada</option>
                        <option value='Australia'>Australia</option>
                        <option value='Germany'>Germany</option>
                        <option value='France'>France</option>
                        <option value='Japan'>Japan</option>
                        <option value='Other'>Other</option>
                    </select>
                </div>

                <div class='form-group'>
                    <label>Interests</label>
                    <div class='checkbox-group'>
                        <label class='checkbox-item'>
                            <input type='checkbox' name='interests' value='Gaming'> Gaming
                        </label>
                        <label class='checkbox-item'>
                            <input type='checkbox' name='interests' value='Programming'> Programming
                        </label>
                        <label class='checkbox-item'>
                            <input type='checkbox' name='interests' value='Design'> Design
                        </label>
                        <label class='checkbox-item'>
                            <input type='checkbox' name='interests' value='Music'> Music
                        </label>
                    </div>
                </div>

                <div class='form-group'>
                    <label for='message'>Message</label>
                    <textarea id='message' name='message' placeholder='Tell us about yourself...'></textarea>
                </div>

                <button type='submit' class='submit-button'>Submit Form</button>
            </form>
        </div>

        <div class='footer'>
            <h2>🖱️ Input Features</h2>
            <p>
                <strong>Mouse Tracking:</strong> Your mouse position is automatically forwarded to the web content.<br>
                <strong>Click Detection:</strong> All clicks are registered and can trigger web interactions.<br>
                <strong>Scroll Support:</strong> Use your scroll wheel to scroll this page.<br>
                <strong>Form Interaction:</strong> Type, select, and submit data back to Unity.
            </p>
        </div>
    </div>

    <script>
        // Throttle function to limit event frequency
        function throttle(func, delay) {
            let lastCall = 0;
            return function(...args) {
                const now = Date.now();
                if (now - lastCall >= delay) {
                    lastCall = now;
                    func(...args);
                }
            };
        }

        // Mouse follower visual effect
        const mouseFollower = document.getElementById('mouseFollower');
        let mouseTimeout;

        // Throttled mouse move handler (60 FPS max = ~16ms)
        const throttledMouseMove = throttle((e) => {
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify({
                    type: 'mouseMove',
                    x: e.clientX,
                    y: e.clientY,
                    normalizedX: e.clientX / window.innerWidth,
                    normalizedY: e.clientY / window.innerHeight
                }));
            }
        }, 16);

        document.addEventListener('mousemove', (e) => {
            // Visual updates (not throttled for smooth animation)
            mouseFollower.style.left = e.clientX + 'px';
            mouseFollower.style.top = e.clientY + 'px';
            mouseFollower.style.opacity = '0.5';

            // Send mouse position to Unity (throttled)
            throttledMouseMove(e);

            clearTimeout(mouseTimeout);
            mouseTimeout = setTimeout(() => {
                mouseFollower.style.opacity = '0';
            }, 2000);
        });

        // Click ripple effect
        document.addEventListener('click', (e) => {
            const ripple = document.createElement('div');
            ripple.className = 'click-ripple';
            ripple.style.left = e.clientX + 'px';
            ripple.style.top = e.clientY + 'px';
            document.body.appendChild(ripple);

            setTimeout(() => ripple.remove(), 600);

            // Send click data to Unity
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify({
                    type: 'click',
                    x: e.clientX,
                    y: e.clientY,
                    normalizedX: e.clientX / window.innerWidth,
                    normalizedY: e.clientY / window.innerHeight,
                    time: new Date().toLocaleTimeString()
                }));
            }
        });

        // Scroll tracking
        const scrollIndicator = document.getElementById('scrollIndicator');

        // Throttled scroll handler (update every 100ms for better control)
        const throttledScroll = throttle((scrollY, scrollX) => {
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify({
                    type: 'scroll',
                    scrollY: scrollY,
                    scrollX: scrollX
                }));
            }
        }, 100);

        document.addEventListener('scroll', () => {
            const scrollY = window.scrollY;
            const scrollX = window.scrollX;

            // Visual update (not throttled)
            scrollIndicator.textContent = `Scroll: ${scrollY}px`;

            // Send scroll data to Unity (throttled)
            throttledScroll(scrollY, scrollX);
        });

        // Form submission
        document.getElementById('userForm').addEventListener('submit', (e) => {
            e.preventDefault();

            const formData = new FormData(e.target);

            // Build flat message object with type and all form fields
            const message = {
                type: 'formSubmit',
                name: formData.get('name') || '',
                email: formData.get('email') || '',
                age: parseInt(formData.get('age')) || 0,
                country: formData.get('country') || '',
                interests: formData.getAll('interests'),
                message: formData.get('message') || ''
            };

            // Send form data to Unity with flat structure
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify(message));
            }

            // Visual feedback
            const button = e.target.querySelector('.submit-button');
            button.textContent = '✓ Submitted!';
            button.style.background = 'linear-gradient(135deg, #10b981 0%, #34d399 100%)';

            setTimeout(() => {
                button.textContent = 'Submit Form';
                button.style.background = 'linear-gradient(135deg, #3b82f6 0%, #60a5fa 100%)';
            }, 2000);
        });

        console.log('Interactive form ready! Mouse, click, scroll, and form events are being tracked.');
    </script>
</body>
</html>
";
        }

        // ====================================================================
        // Message Handling
        // ====================================================================

        /// <summary>
        /// Handles messages received from JavaScript
        /// </summary>
        private void OnMessageReceived(string message)
        {
            Debug.Log($"[InteractiveInput] Received message: {message}");

            try
            {
                // First, determine the message type
                var baseMessage = JsonUtility.FromJson<BaseMessage>(message);

                switch (baseMessage.type)
                {
                    case "mouseMove":
                        HandleMouseMove(message);
                        break;

                    case "click":
                        HandleClick(message);
                        break;

                    case "scroll":
                        HandleScroll(message);
                        break;

                    case "formSubmit":
                        HandleFormSubmit(message);
                        break;

                    default:
                        Debug.LogWarning($"[InteractiveInput] Unknown message type: {baseMessage.type}");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InteractiveInput] Error parsing message: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles mouse movement data
        /// </summary>
        private void HandleMouseMove(string message)
        {
            var data = JsonUtility.FromJson<MouseMoveMessage>(message);

            if (_mouseXLabel != null)
                _mouseXLabel.text = $"X: {data.x:F0}px";

            if (_mouseYLabel != null)
                _mouseYLabel.text = $"Y: {data.y:F0}px";

            if (_mouseNormalizedLabel != null)
                _mouseNormalizedLabel.text = $"Normalized: ({data.normalizedX:F3}, {data.normalizedY:F3})";
        }

        /// <summary>
        /// Handles click data
        /// </summary>
        private void HandleClick(string message)
        {
            var data = JsonUtility.FromJson<ClickMessage>(message);

            if (_clickPositionLabel != null)
                _clickPositionLabel.text = $"Position: ({data.x:F0}, {data.y:F0})";

            if (_clickTimeLabel != null)
                _clickTimeLabel.text = $"Time: {data.time}";

            Debug.Log($"[InteractiveInput] Click at ({data.normalizedX:F3}, {data.normalizedY:F3})");
        }

        /// <summary>
        /// Handles scroll data
        /// </summary>
        private void HandleScroll(string message)
        {
            var data = JsonUtility.FromJson<ScrollMessage>(message);

            int delta = data.scrollY - _scrollTotal;
            _scrollTotal = data.scrollY;

            if (_scrollDeltaLabel != null)
                _scrollDeltaLabel.text = $"Delta: {delta}";

            if (_scrollTotalLabel != null)
                _scrollTotalLabel.text = $"Total: {_scrollTotal}";
        }

        /// <summary>
        /// Handles form submission data
        /// </summary>
        private void HandleFormSubmit(string message)
        {
            _formSubmissionCount++;

            if (_formCountLabel != null)
                _formCountLabel.text = $"Total: {_formSubmissionCount}";

            if (_lastFormDataLabel != null)
            {
                // Show truncated version for display
                string truncated = message.Length > 80 ? message.Substring(0, 80) + "..." : message;
                _lastFormDataLabel.text = $"Last Data: {truncated}";
            }

            Debug.Log($"[InteractiveInput] Form submitted #{_formSubmissionCount}: {message}");
        }

        // ====================================================================
        // Data Classes for JSON Deserialization
        // ====================================================================

        [System.Serializable]
        private class BaseMessage
        {
            public string type;
        }

        [System.Serializable]
        private class MouseMoveMessage
        {
            public string type;
            public float x;
            public float y;
            public float normalizedX;
            public float normalizedY;
        }

        [System.Serializable]
        private class ClickMessage
        {
            public string type;
            public float x;
            public float y;
            public float normalizedX;
            public float normalizedY;
            public string time;
        }

        [System.Serializable]
        private class ScrollMessage
        {
            public string type;
            public int scrollY;
            public int scrollX;
        }

        [System.Serializable]
        private class FormSubmitMessage
        {
            public string type;
            public string name;
            public string email;
            public int age;
            public string country;
            public string[] interests;
            public string message;
        }
    }
}
