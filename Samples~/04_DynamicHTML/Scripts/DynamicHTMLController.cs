// ============================================================================
// WebViewToolkit - Dynamic HTML Sample
// ============================================================================
// This sample demonstrates generating and loading HTML content dynamically
// from C# data structures. Perfect for creating game UIs like inventories,
// quest logs, and settings menus using web technologies.
// ============================================================================

using UnityEngine;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Samples.DynamicHTML
{
    /// <summary>
    /// Controller for the Dynamic HTML sample.
    /// Demonstrates NavigateToString() with dynamically generated HTML content.
    /// </summary>
    [AddComponentMenu("WebView Toolkit/Samples/Dynamic HTML Controller")]
    [RequireComponent(typeof(UIDocument))]
    public class DynamicHTMLController : MonoBehaviour
    {
        [Header("UI Assets")]
        [Tooltip("The UXML file containing the dynamic HTML UI layout")]
        [SerializeField] private VisualTreeAsset _uxml;

        [Tooltip("The USS file for dynamic HTML UI styling")]
        [SerializeField] private StyleSheet _uss;

        // UI Element References
        private UIDocument _uiDocument;
        private WebViewElement _webViewElement;

        // View buttons
        private Button _viewInventoryButton;
        private Button _viewQuestLogButton;
        private Button _viewSettingsButton;

        // Action buttons (Inventory)
        private Button _addItemButton;
        private Button _removeItemButton;
        private Button _clearInventoryButton;

        // Action buttons (Quest Log)
        private Button _addQuestButton;
        private Button _completeQuestButton;
        private Button _clearQuestsButton;

        // Action buttons (Settings)
        private Slider _volumeSlider;
        private DropdownField _difficultyDropdown;
        private Toggle _fullscreenToggle;

        // Info labels
        private Label _currentViewLabel;
        private Label _itemCountLabel;

        // Data
        private GameData _gameData;
        private string _currentView = "inventory";

        // ====================================================================
        // Unity Lifecycle
        // ====================================================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _gameData = new GameData();
        }

        private void Start()
        {
            if (_uiDocument == null || _uxml == null)
            {
                Debug.LogError("[DynamicHTML] UIDocument or UXML not assigned!");
                return;
            }

            BuildUI();
            BindEvents();

            // Check if WebView is already attached (which it should be after BuildUI)
            if (_webViewElement != null && _webViewElement.panel != null)
            {
                // Already attached, load content with longer delay to ensure WebView is fully initialized
                _webViewElement.schedule.Execute(() => ShowInventoryView()).ExecuteLater(100);
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
            Debug.Log("[DynamicHTML] WebView attached to panel, loading initial view");
            _webViewElement.schedule.Execute(() => ShowInventoryView()).ExecuteLater(100);
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

            // View buttons
            _viewInventoryButton = root.Q<Button>("btn-inventory");
            _viewQuestLogButton = root.Q<Button>("btn-quest-log");
            _viewSettingsButton = root.Q<Button>("btn-settings");

            // Inventory action buttons
            _addItemButton = root.Q<Button>("btn-add-item");
            _removeItemButton = root.Q<Button>("btn-remove-item");
            _clearInventoryButton = root.Q<Button>("btn-clear-inventory");

            // Quest log action buttons
            _addQuestButton = root.Q<Button>("btn-add-quest");
            _completeQuestButton = root.Q<Button>("btn-complete-quest");
            _clearQuestsButton = root.Q<Button>("btn-clear-quests");

            // Settings controls
            _volumeSlider = root.Q<Slider>("slider-volume");
            _difficultyDropdown = root.Q<DropdownField>("dropdown-difficulty");
            _fullscreenToggle = root.Q<Toggle>("toggle-fullscreen");

            // Info labels
            _currentViewLabel = root.Q<Label>("lbl-current-view");
            _itemCountLabel = root.Q<Label>("lbl-item-count");

            // Initialize difficulty dropdown
            if (_difficultyDropdown != null)
            {
                _difficultyDropdown.choices = new System.Collections.Generic.List<string> { "Easy", "Normal", "Hard", "Nightmare" };
                _difficultyDropdown.value = _gameData.Settings.difficulty;
            }

            // Initialize volume slider
            if (_volumeSlider != null)
            {
                _volumeSlider.value = _gameData.Settings.volume * 100f;
            }

            // Initialize fullscreen toggle
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.value = _gameData.Settings.fullscreen;
            }

            // Validate
            if (_webViewElement == null)
            {
                Debug.LogError("[DynamicHTML] WebViewElement not found in UXML!");
            }
            else
            {
                Debug.Log("[DynamicHTML] WebViewElement found successfully");
            }

            // Validate buttons
            if (_viewInventoryButton == null) Debug.LogWarning("[DynamicHTML] btn-inventory not found");
            if (_viewQuestLogButton == null) Debug.LogWarning("[DynamicHTML] btn-quest-log not found");
            if (_viewSettingsButton == null) Debug.LogWarning("[DynamicHTML] btn-settings not found");
        }

        private void BindEvents()
        {
            // View buttons
            if (_viewInventoryButton != null)
            {
                _viewInventoryButton.clicked += ShowInventoryView;
                Debug.Log("[DynamicHTML] Bound Inventory button");
            }

            if (_viewQuestLogButton != null)
            {
                _viewQuestLogButton.clicked += ShowQuestLogView;
                Debug.Log("[DynamicHTML] Bound Quest Log button");
            }

            if (_viewSettingsButton != null)
            {
                _viewSettingsButton.clicked += ShowSettingsView;
                Debug.Log("[DynamicHTML] Bound Settings button");
            }

            // Inventory action buttons
            if (_addItemButton != null)
                _addItemButton.clicked += OnAddItem;

            if (_removeItemButton != null)
                _removeItemButton.clicked += OnRemoveItem;

            if (_clearInventoryButton != null)
                _clearInventoryButton.clicked += OnClearInventory;

            // Quest log action buttons
            if (_addQuestButton != null)
                _addQuestButton.clicked += OnAddQuest;

            if (_completeQuestButton != null)
                _completeQuestButton.clicked += OnCompleteQuest;

            if (_clearQuestsButton != null)
                _clearQuestsButton.clicked += OnClearQuests;

            // Settings controls
            if (_volumeSlider != null)
                _volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);

            if (_difficultyDropdown != null)
                _difficultyDropdown.RegisterValueChangedCallback(OnDifficultyChanged);

            if (_fullscreenToggle != null)
                _fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
        }

        private void UnbindEvents()
        {
            // View buttons
            if (_viewInventoryButton != null)
                _viewInventoryButton.clicked -= ShowInventoryView;

            if (_viewQuestLogButton != null)
                _viewQuestLogButton.clicked -= ShowQuestLogView;

            if (_viewSettingsButton != null)
                _viewSettingsButton.clicked -= ShowSettingsView;

            // Inventory action buttons
            if (_addItemButton != null)
                _addItemButton.clicked -= OnAddItem;

            if (_removeItemButton != null)
                _removeItemButton.clicked -= OnRemoveItem;

            if (_clearInventoryButton != null)
                _clearInventoryButton.clicked -= OnClearInventory;

            // Quest log action buttons
            if (_addQuestButton != null)
                _addQuestButton.clicked -= OnAddQuest;

            if (_completeQuestButton != null)
                _completeQuestButton.clicked -= OnCompleteQuest;

            if (_clearQuestsButton != null)
                _clearQuestsButton.clicked -= OnClearQuests;

            // Settings controls
            if (_volumeSlider != null)
                _volumeSlider.UnregisterValueChangedCallback(OnVolumeChanged);

            if (_difficultyDropdown != null)
                _difficultyDropdown.UnregisterValueChangedCallback(OnDifficultyChanged);

            if (_fullscreenToggle != null)
                _fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);
        }

        // ====================================================================
        // View Management
        // ====================================================================

        /// <summary>
        /// Shows the inventory view with current items
        /// </summary>
        private void ShowInventoryView()
        {
            Debug.Log("[DynamicHTML] ShowInventoryView() called");
            _currentView = "inventory";
            UpdateInfoLabels();

            if (_webViewElement == null)
            {
                Debug.LogError("[DynamicHTML] Cannot show inventory - WebViewElement is null!");
                return;
            }

            string html = HTMLTemplates.GenerateInventoryHTML(_gameData.Inventory);
            _webViewElement.NavigateToString(html);

            Debug.Log("[DynamicHTML] Loaded Inventory view with " + _gameData.Inventory.Count + " items");
        }

        /// <summary>
        /// Shows the quest log view with current quests
        /// </summary>
        private void ShowQuestLogView()
        {
            Debug.Log("[DynamicHTML] ShowQuestLogView() called");
            _currentView = "questlog";
            UpdateInfoLabels();

            if (_webViewElement == null)
            {
                Debug.LogError("[DynamicHTML] Cannot show quest log - WebViewElement is null!");
                return;
            }

            string html = HTMLTemplates.GenerateQuestLogHTML(_gameData.Quests);
            _webViewElement.NavigateToString(html);

            Debug.Log("[DynamicHTML] Loaded Quest Log view with " + _gameData.Quests.Count + " quests");
        }

        /// <summary>
        /// Shows the settings view with current settings
        /// </summary>
        private void ShowSettingsView()
        {
            Debug.Log("[DynamicHTML] ShowSettingsView() called");
            _currentView = "settings";
            UpdateInfoLabels();

            if (_webViewElement == null)
            {
                Debug.LogError("[DynamicHTML] Cannot show settings - WebViewElement is null!");
                return;
            }

            string html = HTMLTemplates.GenerateSettingsHTML(_gameData.Settings);
            _webViewElement.NavigateToString(html);

            Debug.Log("[DynamicHTML] Loaded Settings view");
        }

        /// <summary>
        /// Updates the info labels with current data
        /// </summary>
        private void UpdateInfoLabels()
        {
            if (_currentViewLabel != null)
            {
                string viewName = _currentView switch
                {
                    "inventory" => "Inventory",
                    "questlog" => "Quest Log",
                    "settings" => "Settings",
                    _ => "Unknown"
                };
                _currentViewLabel.text = $"Current: {viewName}";
            }

            if (_itemCountLabel != null)
            {
                _itemCountLabel.text = $"Items: {_gameData.Inventory.Count}";
            }
        }

        // ====================================================================
        // Inventory Actions
        // ====================================================================

        private void OnAddItem()
        {
            _gameData.AddRandomItem();
            ShowInventoryView(); // Refresh view
            Debug.Log("[DynamicHTML] Added random item to inventory");
        }

        private void OnRemoveItem()
        {
            _gameData.RemoveLastItem();
            ShowInventoryView(); // Refresh view
            Debug.Log("[DynamicHTML] Removed last item from inventory");
        }

        private void OnClearInventory()
        {
            _gameData.Inventory.Clear();
            ShowInventoryView(); // Refresh view
            Debug.Log("[DynamicHTML] Cleared all inventory items");
        }

        // ====================================================================
        // Quest Actions
        // ====================================================================

        private void OnAddQuest()
        {
            // Add a new random quest
            string[] titles = { "Explore the Cavern", "Defeat Bandits", "Deliver Package", "Investigate Ruins" };
            int index = Random.Range(0, titles.Length);

            _gameData.Quests.Add(new Quest(
                titles[index],
                "A new adventure awaits!",
                "Active",
                0,
                Random.Range(5, 15),
                "Mystery Reward"
            ));

            ShowQuestLogView(); // Refresh view
            Debug.Log("[DynamicHTML] Added new quest");
        }

        private void OnCompleteQuest()
        {
            // Complete first active quest
            var activeQuest = _gameData.Quests.Find(q => q.status == "Active");
            if (activeQuest != null)
            {
                activeQuest.status = "Completed";
                activeQuest.progress = activeQuest.maxProgress;
                ShowQuestLogView(); // Refresh view
                Debug.Log($"[DynamicHTML] Completed quest: {activeQuest.title}");
            }
        }

        private void OnClearQuests()
        {
            _gameData.Quests.Clear();
            ShowQuestLogView(); // Refresh view
            Debug.Log("[DynamicHTML] Cleared all quests");
        }

        // ====================================================================
        // Settings Actions
        // ====================================================================

        private void OnVolumeChanged(ChangeEvent<float> evt)
        {
            _gameData.Settings.volume = evt.newValue / 100f;
            ShowSettingsView(); // Refresh view
            Debug.Log($"[DynamicHTML] Volume changed to {_gameData.Settings.volume:P0}");
        }

        private void OnDifficultyChanged(ChangeEvent<string> evt)
        {
            _gameData.Settings.difficulty = evt.newValue;
            ShowSettingsView(); // Refresh view
            Debug.Log($"[DynamicHTML] Difficulty changed to {_gameData.Settings.difficulty}");
        }

        private void OnFullscreenChanged(ChangeEvent<bool> evt)
        {
            _gameData.Settings.fullscreen = evt.newValue;
            ShowSettingsView(); // Refresh view
            Debug.Log($"[DynamicHTML] Fullscreen changed to {_gameData.Settings.fullscreen}");
        }
    }
}
