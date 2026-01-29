// ============================================================================
// WebViewToolkit - JavaScript Bridge Sample
// ============================================================================
// This sample demonstrates two-way communication between C# and JavaScript:
// - C# → JavaScript: Using ExecuteScript() to call JavaScript functions
// - JavaScript → C#: Using window.chrome.webview.postMessage() to send data
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Samples.JavaScriptBridge
{
    /// <summary>
    /// Controller for the JavaScript Bridge sample.
    /// Demonstrates two-way communication between Unity C# and web JavaScript.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/JavaScript Bridge Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class JavaScriptBridgeController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the JavaScript Bridge UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("The USS file for JavaScript Bridge UI styling")]
        [SerializeField] private StyleSheet _uss;

        // Game State
        private GameState _gameState = new GameState();

        // UI Element References
        private UIDocument _uiDocument;
        private WebViewElement _webViewElement;

        // Unity Control Panel Elements
        private Label _lblScore;
        private Label _lblHealth;
        private Label _lblLevel;
        private Label _lblLastSent;
        private ScrollView _messageLog;

        private Button _btnAddScore;
        private Button _btnTakeDamage;
        private Button _btnLevelUp;
        private Button _btnReset;
        private Button _btnUpdateWeb;

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
                Debug.LogError("[JavaScriptBridge] UIDocument or UXML not assigned!");
                return;
            }

            BuildUI();
            BindEvents();
            LoadDashboard();
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

            // Labels
            _lblScore = root.Q<Label>("lbl-score");
            _lblHealth = root.Q<Label>("lbl-health");
            _lblLevel = root.Q<Label>("lbl-level");
            _lblLastSent = root.Q<Label>("lbl-last-sent");

            // Message log
            _messageLog = root.Q<ScrollView>("message-log");

            // Buttons
            _btnAddScore = root.Q<Button>("btn-add-score");
            _btnTakeDamage = root.Q<Button>("btn-take-damage");
            _btnLevelUp = root.Q<Button>("btn-level-up");
            _btnReset = root.Q<Button>("btn-reset");
            _btnUpdateWeb = root.Q<Button>("btn-update-web");

            // Validate
            if (_webViewElement == null)
            {
                Debug.LogError("[JavaScriptBridge] WebViewElement not found in UXML!");
            }

            // Initial UI update
            UpdateGameStateUI();
        }

        private void BindEvents()
        {
            if (_webViewElement == null) return;

            // WebView events
            _webViewElement.NavigationCompleted += OnNavigationCompleted;
            _webViewElement.MessageReceived += OnMessageReceived;

            // Button clicks
            if (_btnAddScore != null)
                _btnAddScore.clicked += OnAddScoreClicked;

            if (_btnTakeDamage != null)
                _btnTakeDamage.clicked += OnTakeDamageClicked;

            if (_btnLevelUp != null)
                _btnLevelUp.clicked += OnLevelUpClicked;

            if (_btnReset != null)
                _btnReset.clicked += OnResetClicked;

            if (_btnUpdateWeb != null)
                _btnUpdateWeb.clicked += OnUpdateWebClicked;
        }

        private void UnbindEvents()
        {
            if (_webViewElement != null)
            {
                _webViewElement.NavigationCompleted -= OnNavigationCompleted;
                _webViewElement.MessageReceived -= OnMessageReceived;
            }

            if (_btnAddScore != null)
                _btnAddScore.clicked -= OnAddScoreClicked;

            if (_btnTakeDamage != null)
                _btnTakeDamage.clicked -= OnTakeDamageClicked;

            if (_btnLevelUp != null)
                _btnLevelUp.clicked -= OnLevelUpClicked;

            if (_btnReset != null)
                _btnReset.clicked -= OnResetClicked;

            if (_btnUpdateWeb != null)
                _btnUpdateWeb.clicked -= OnUpdateWebClicked;
        }

        // ====================================================================
        // Game State Management
        // ====================================================================

        private void OnAddScoreClicked()
        {
            _gameState.Score += 10;
            UpdateGameStateUI();
            LogMessage("Added 10 points to score");
        }

        private void OnTakeDamageClicked()
        {
            _gameState.Health -= 10;
            UpdateGameStateUI();
            LogMessage("Took 10 damage");

            if (_gameState.Health <= 0)
            {
                LogMessage("⚠️ Player is dead! Reset game to continue.");
            }
        }

        private void OnLevelUpClicked()
        {
            _gameState.Level += 1;
            UpdateGameStateUI();
            LogMessage($"Leveled up to level {_gameState.Level}!");
        }

        private void OnResetClicked()
        {
            _gameState.Reset();
            UpdateGameStateUI();
            LogMessage("Game reset to initial state");
        }

        /// <summary>
        /// Updates the Unity UI labels to reflect current game state
        /// </summary>
        private void UpdateGameStateUI()
        {
            if (_lblScore != null)
                _lblScore.text = $"Score: {_gameState.Score}";

            if (_lblHealth != null)
                _lblHealth.text = $"Health: {_gameState.Health}";

            if (_lblLevel != null)
                _lblLevel.text = $"Level: {_gameState.Level}";
        }

        // ====================================================================
        // C# → JavaScript Communication
        // ====================================================================

        private void OnUpdateWebClicked()
        {
            SendGameStateToWeb();
        }

        /// <summary>
        /// Sends the current game state to the web dashboard via JavaScript.
        /// This demonstrates C# → JavaScript communication using ExecuteScript().
        /// </summary>
        private void SendGameStateToWeb()
        {
            if (!_webViewElement.IsReady)
            {
                LogMessage("❌ WebView is not ready yet!");
                return;
            }

            // Convert game state to JSON
            string json = _gameState.ToJson();

            // Call JavaScript function with the JSON data
            // The updateGameState() function must exist in the HTML page
            string script = $"if (typeof updateGameState === 'function') {{ updateGameState({json}); }}";

            _webViewElement.ExecuteScript(script);

            // Update status
            if (_lblLastSent != null)
            {
                _lblLastSent.text = $"Last sent: {DateTime.Now:HH:mm:ss} - {_gameState}";
            }

            LogMessage($"📤 Sent to web: {json}");
        }

        // ====================================================================
        // JavaScript → C# Communication
        // ====================================================================

        /// <summary>
        /// Called when a message is received from JavaScript.
        /// This demonstrates JavaScript → C# communication using window.chrome.webview.postMessage().
        /// </summary>
        private void OnMessageReceived(string message)
        {
            LogMessage($"📥 Received from web: {message}");

            // Parse the message as JSON to understand the action
            try
            {
                // Messages from JavaScript should be in format: { "action": "actionName", "data": {...} }
                var messageData = JsonUtility.FromJson<WebMessage>(message);

                switch (messageData.action)
                {
                    case "jump":
                        HandleJumpAction();
                        break;

                    case "shoot":
                        HandleShootAction();
                        break;

                    case "requestUpdate":
                        SendGameStateToWeb();
                        break;

                    default:
                        LogMessage($"⚠️ Unknown action: {messageData.action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Failed to parse message: {ex.Message}");
            }
        }

        private void HandleJumpAction()
        {
            LogMessage("🎮 Player jumped! (triggered from web)");
            // In a real game, you would trigger the jump animation/logic here
        }

        private void HandleShootAction()
        {
            LogMessage("🎮 Player shot! (triggered from web)");
            // In a real game, you would trigger the shoot animation/logic here
        }

        /// <summary>
        /// Simple message structure for parsing JSON messages from JavaScript
        /// </summary>
        [Serializable]
        private class WebMessage
        {
            public string action;
            public string data;
        }

        // ====================================================================
        // WebView Events
        // ====================================================================

        /// <summary>
        /// Called when the WebView finishes loading a page.
        /// We load the HTML dashboard dynamically when ready.
        /// </summary>
        private void OnNavigationCompleted(string url, bool isSuccess)
        {
            if (isSuccess)
            {
                LogMessage("✅ Web dashboard loaded successfully");

                // Automatically send initial game state
                SendGameStateToWeb();
            }
            else
            {
                LogMessage($"❌ Failed to load web dashboard");
            }
        }

        /// <summary>
        /// Loads the interactive HTML dashboard into the WebView
        /// </summary>
        private void LoadDashboard()
        {
            string html = GenerateDashboardHTML();
            _webViewElement.NavigateToString(html);
        }

        // ====================================================================
        // HTML Dashboard Generation
        // ====================================================================

        /// <summary>
        /// Generates the HTML for the interactive game dashboard.
        /// This dashboard receives game state updates from Unity and can send actions back.
        /// </summary>
        private string GenerateDashboardHTML()
        {
            return @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Game Dashboard</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            color: white;
            padding: 20px;
            height: 100vh;
            display: flex;
            flex-direction: column;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
        }

        .header h1 {
            font-size: 2.5em;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }

        .header p {
            font-size: 1.1em;
            opacity: 0.9;
        }

        .stats-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .stat-card {
            background: rgba(255, 255, 255, 0.1);
            backdrop-filter: blur(10px);
            border-radius: 12px;
            padding: 20px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            transition: transform 0.2s, box-shadow 0.2s;
        }

        .stat-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 25px rgba(0,0,0,0.3);
        }

        .stat-label {
            font-size: 0.9em;
            opacity: 0.8;
            margin-bottom: 8px;
        }

        .stat-value {
            font-size: 2.5em;
            font-weight: bold;
        }

        .stat-bar {
            margin-top: 10px;
            height: 8px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 4px;
            overflow: hidden;
        }

        .stat-bar-fill {
            height: 100%;
            background: linear-gradient(90deg, #4CAF50, #8BC34A);
            transition: width 0.3s ease;
            border-radius: 4px;
        }

        .actions-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            margin-bottom: 20px;
        }

        .action-button {
            background: rgba(255, 255, 255, 0.15);
            border: 2px solid rgba(255, 255, 255, 0.3);
            color: white;
            padding: 15px 25px;
            font-size: 1.1em;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s;
            font-weight: bold;
        }

        .action-button:hover {
            background: rgba(255, 255, 255, 0.25);
            transform: scale(1.05);
        }

        .action-button:active {
            transform: scale(0.95);
        }

        .info-box {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            padding: 15px;
            margin-top: 20px;
            border-left: 4px solid #4CAF50;
        }

        .info-box h3 {
            margin-bottom: 10px;
            font-size: 1.2em;
        }

        .info-box code {
            background: rgba(0, 0, 0, 0.3);
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎮 Game Dashboard</h1>
        <p>Real-time game state synchronized with Unity</p>
    </div>

    <div class='stats-container'>
        <div class='stat-card'>
            <div class='stat-label'>SCORE</div>
            <div class='stat-value' id='score'>0</div>
        </div>

        <div class='stat-card'>
            <div class='stat-label'>HEALTH</div>
            <div class='stat-value' id='health'>100</div>
            <div class='stat-bar'>
                <div class='stat-bar-fill' id='health-bar' style='width: 100%'></div>
            </div>
        </div>

        <div class='stat-card'>
            <div class='stat-label'>LEVEL</div>
            <div class='stat-value' id='level'>1</div>
        </div>
    </div>

    <div class='actions-container'>
        <button class='action-button' onclick='sendJump()'>🏃 Jump</button>
        <button class='action-button' onclick='sendShoot()'>🔫 Shoot</button>
        <button class='action-button' onclick='requestUpdate()'>🔄 Refresh</button>
    </div>

    <div class='info-box'>
        <h3>💡 How It Works</h3>
        <p>
            <strong>Unity → Web:</strong> Click 'Update Web Dashboard' in Unity to send game state<br>
            <strong>Web → Unity:</strong> Click buttons above to send actions to Unity<br>
            <strong>API:</strong> Uses <code>window.chrome.webview.postMessage()</code>
        </p>
    </div>

    <script>
        // =================================================================
        // Unity → JavaScript: Receive game state updates
        // =================================================================

        /**
         * This function is called from Unity via ExecuteScript()
         * It receives the game state as a JSON object
         */
        function updateGameState(state) {
            console.log('Received game state from Unity:', state);

            // Update the UI with new values
            document.getElementById('score').textContent = state.score;
            document.getElementById('health').textContent = state.health;
            document.getElementById('level').textContent = state.level;

            // Update health bar (assuming max health is 100)
            const healthBar = document.getElementById('health-bar');
            healthBar.style.width = state.health + '%';

            // Change health bar color based on health level
            if (state.health > 66) {
                healthBar.style.background = 'linear-gradient(90deg, #4CAF50, #8BC34A)';
            } else if (state.health > 33) {
                healthBar.style.background = 'linear-gradient(90deg, #FF9800, #FFC107)';
            } else {
                healthBar.style.background = 'linear-gradient(90deg, #F44336, #E91E63)';
            }
        }

        // =================================================================
        // JavaScript → Unity: Send actions to Unity
        // =================================================================

        /**
         * Sends a message to Unity using the WebView2 postMessage API
         */
        function sendToUnity(action, data = null) {
            const message = {
                action: action,
                data: data
            };

            // This is the WebView2 API for sending messages to C#
            window.chrome.webview.postMessage(JSON.stringify(message));
            console.log('Sent to Unity:', message);
        }

        function sendJump() {
            sendToUnity('jump');
        }

        function sendShoot() {
            sendToUnity('shoot');
        }

        function requestUpdate() {
            sendToUnity('requestUpdate');
        }

        // Initialize with a request for current game state
        window.addEventListener('load', () => {
            console.log('Dashboard loaded, requesting initial game state...');
            requestUpdate();
        });
    </script>
</body>
</html>";
        }

        // ====================================================================
        // Utilities
        // ====================================================================

        /// <summary>
        /// Adds a message to the message log UI
        /// </summary>
        private void LogMessage(string message)
        {
            if (_messageLog == null) return;

            var label = new Label(message);
            label.AddToClassList("message-item");
            _messageLog.Add(label);

            // Keep log size manageable (max 50 messages)
            while (_messageLog.childCount > 50)
            {
                _messageLog.RemoveAt(0);
            }
        }
    }
}
