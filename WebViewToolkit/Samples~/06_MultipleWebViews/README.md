# Sample 06: Multiple WebViews

## Learning Objectives

- Manage multiple WebView instances simultaneously
- Understand resource implications of running multiple WebViews
- Implement independent navigation for each instance
- Monitor performance with multiple active WebViews
- Learn proper lifecycle management and cleanup
- Organize code with helper classes for scalability
- Build dashboard-style layouts with multiple web panels

## Prerequisites

- Complete Sample 01 (Hello WebView) first
- Complete Sample 02 (Navigation) - understanding of navigation controls
- Basic understanding of performance considerations
- Familiarity with Unity lifecycle management

## What This Sample Demonstrates

This sample shows how to create and manage multiple WebView instances in a single scene. This is useful for:

- **Multi-monitor dashboards**: Display different data sources simultaneously
- **Split-screen browsers**: View multiple websites side-by-side
- **Comparison tools**: Compare different websites or pages
- **Resource monitors**: Track multiple web-based metrics
- **Workflow tools**: Keep reference documentation open while working

You'll learn:
- **Multi-instance Management**: Creating and coordinating multiple WebViews
- **Resource Management**: Understanding memory and performance implications
- **Independent Navigation**: Each WebView has its own navigation state
- **Scalable Architecture**: Using helper classes to organize multi-instance code
- **Performance Monitoring**: Tracking FPS and memory with multiple active WebViews

## Setup Instructions

1. Import this sample from Package Manager (Window > Package Manager > WebView Toolkit > Samples)
2. Open the `MultipleWebViews.unity` scene
3. Press Play in the Unity Editor
4. Each WebView loads a different default URL
5. Use individual navigation controls for each WebView
6. Try preset button groups to load related sites
7. Monitor performance stats in the header

## Scene Layout

The sample uses a **grid layout** with performance monitoring:

- **Header**: Title, subtitle, and real-time performance stats (FPS, Memory, Active Views)
- **WebView Grid (2x2)**: Four independent WebView instances, each with:
  - Title bar with WebView number
  - Navigation controls (Back, Forward, Refresh)
  - Address bar with URL text field and Go button
  - WebView display area
  - Status indicator showing current state
- **Footer**: Quick link preset buttons and reset option

## Code Walkthrough

### Main Controller ([MultipleWebViewsController.cs](Scripts/MultipleWebViewsController.cs))

**Lines 19-47: Class Structure and Fields**
```csharp
[RequireComponent(typeof(UIDocument))]
public class MultipleWebViewsController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset _uxml;
    [SerializeField] private StyleSheet _uss;

    [SerializeField] private string _defaultUrl1 = "https://unity.com";
    [SerializeField] private string _defaultUrl2 = "https://docs.unity3d.com";
    [SerializeField] private string _defaultUrl3 = "https://github.com";
    [SerializeField] private string _defaultUrl4 = "https://stackoverflow.com";

    private List<WebViewCard> _webViewCards = new List<WebViewCard>();
}
```
The controller manages a list of `WebViewCard` objects, each representing one WebView instance with its controls.

**Lines 88-104: Creating WebView Cards**
```csharp
private void BuildUI()
{
    var root = _uiDocument.rootVisualElement;
    root.Clear();
    _uxml.CloneTree(root);

    if (_uss != null)
        root.styleSheets.Add(_uss);

    // Create WebView cards for all 4 WebViews
    for (int i = 1; i <= 4; i++)
    {
        var card = new WebViewCard(i, root, this);
        _webViewCards.Add(card);
    }
}
```
Creates 4 `WebViewCard` instances in a loop, demonstrating scalable multi-instance management.

**Lines 110-117: Lifecycle Management**
```csharp
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
```
Critical: Properly disposes all WebView instances to avoid memory leaks.

**Lines 185-202: Initialization with Default URLs**
```csharp
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
```
Initializes all WebViews with their default URLs using scheduled navigation to avoid overwhelming the system.

**Lines 210-237: Preset URL Groups**
```csharp
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

private void ApplyPreset(string[] urls, string presetName)
{
    for (int i = 0; i < _webViewCards.Count && i < urls.Length; i++)
    {
        _webViewCards[i].SetUrl(urls[i]);
        _webViewCards[i].Navigate();
    }
    Debug.Log($"[MultipleWebViews] Applied preset: {presetName}");
}
```
Demonstrates bulk URL management - applying themed URL groups to all WebViews simultaneously.

**Lines 258-290: Performance Monitoring**
```csharp
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
            _fpsLabel.text = $"FPS: {fps:F0}";
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
        int activeCount = _webViewCards.Count(card => card.IsActive);
        _activeViewsLabel.text = $"Active: {activeCount}/{_webViewCards.Count}";
    }
}
```
Monitors real-time performance metrics to help understand the resource cost of multiple WebViews.

### WebViewCard Helper Class ([MultipleWebViewsController.cs](Scripts/MultipleWebViewsController.cs))

**Lines 315-336: WebViewCard Structure**
```csharp
public class WebViewCard
{
    private int _index;
    private MultipleWebViewsController _controller;

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
}
```
Encapsulates all UI elements and logic for a single WebView, making the code more maintainable.

**Lines 338-356: Initialization**
```csharp
public WebViewCard(int index, VisualElement root, MultipleWebViewsController controller)
{
    _index = index;
    _controller = controller;

    // Query UI elements for this card using indexed names
    _webView = root.Q<WebViewElement>($"webview-{index}");
    _urlField = root.Q<TextField>($"txt-url-{index}");
    _backButton = root.Q<Button>($"btn-back-{index}");
    _forwardButton = root.Q<Button>($"btn-forward-{index}");
    _refreshButton = root.Q<Button>($"btn-refresh-{index}");
    _goButton = root.Q<Button>($"btn-go-{index}");
    _statusLabel = root.Q<Label>($"lbl-status-{index}");

    if (_webView == null)
        Debug.LogError($"[WebViewCard {index}] WebViewElement not found!");
}
```
Uses indexed element names (e.g., `"webview-1"`, `"webview-2"`) to query the correct UI elements for each card.

**Lines 358-383: Event Binding**
```csharp
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
        _webView.NavigationStarted += OnNavigationStarted;
        _webView.NavigationCompleted += OnNavigationCompleted;
    }
}
```
Each card manages its own event subscriptions independently.

**Lines 409-428: Scheduled Navigation**
```csharp
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
```
Uses the same reliable initialization pattern from previous samples, adapted for multi-instance use.

**Lines 468-477: Navigation State Management**
```csharp
private void OnNavigationStarted(object sender, string url)
{
    _currentUrl = url;
    UpdateStatus("Loading...");
    UpdateNavigationButtons();
    _controller?.OnWebViewNavigationStarted(_index, url);
}

private void OnNavigationCompleted(object sender, NavigationCompletedEventArgs args)
{
    if (_urlField != null)
        _urlField.value = args.Url;

    UpdateStatus(args.Success ? "Ready" : "Failed");
    UpdateNavigationButtons();
    _controller?.OnWebViewNavigationCompleted(_index, args.Url, args.Success);
}
```
Updates UI state and notifies the controller when navigation events occur.

**Lines 479-489: Button State Updates**
```csharp
private void UpdateNavigationButtons()
{
    if (_webView == null) return;

    if (_backButton != null)
        _backButton.SetEnabled(_webView.CanGoBack());

    if (_forwardButton != null)
        _forwardButton.SetEnabled(_webView.CanGoForward());
}
```
Enables/disables navigation buttons based on history state.

## Key Concepts

### Multi-Instance Management

**Why use multiple WebViews?**
- Display multiple data sources simultaneously
- Compare different web pages side-by-side
- Create dashboard-style UIs with web content
- Maintain reference documentation while working
- Build workflow tools with multiple web panels

**Resource considerations:**
- Each WebView is a full browser instance
- Memory usage scales linearly with instance count
- CPU usage increases with active WebViews
- Recommended maximum: 4-6 WebViews on most hardware
- Consider lazy loading for > 4 instances

### Scalable Architecture Pattern

**Helper Class Pattern:**
```csharp
// Main Controller - Manages the collection
public class MultipleWebViewsController : MonoBehaviour
{
    private List<WebViewCard> _webViewCards;

    private void BuildUI()
    {
        for (int i = 1; i <= 4; i++)
        {
            var card = new WebViewCard(i, root, this);
            _webViewCards.Add(card);
        }
    }
}

// Helper Class - Manages individual instance
public class WebViewCard
{
    private WebViewElement _webView;
    // ... individual controls

    public void Navigate() { /* ... */ }
    public void Dispose() { /* ... */ }
}
```

**Benefits of this pattern:**
- Code organization - each card manages its own state
- Scalability - easily add more instances
- Maintainability - changes are localized
- Testability - can test cards independently

### Proper Cleanup

**Why cleanup is critical:**
```csharp
private void OnDestroy()
{
    // CRITICAL: Dispose all instances
    foreach (var card in _webViewCards)
    {
        card.Dispose(); // Unsubscribe events, release resources
    }
    _webViewCards.Clear();
}
```

**What happens without cleanup:**
- Memory leaks from event subscriptions
- Orphaned WebView native instances
- Accumulating resources over scene reloads
- Potential crashes in longer sessions

### Performance Monitoring

**Metrics to track:**
- **FPS**: Frame rate impact of multiple WebViews
- **Memory**: RAM usage (expect ~100-200MB per WebView)
- **Active Views**: How many are currently rendering

**Performance tips:**
- Pause or hide inactive WebViews to save resources
- Use lower resolution for background WebViews
- Implement lazy loading for dashboards with many panels
- Monitor and optimize based on target hardware

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Add Pause/Resume
Implement pause functionality to stop rendering inactive WebViews:
- Add pause/resume buttons to each card
- Track paused state
- Call `WebView.Pause()` and `WebView.Resume()` (if available)
- Show visual indicator when paused
- Observe performance improvement when pausing 3 out of 4 WebViews

### Exercise 2: Implement Tab System
Convert from 2x2 grid to tabbed interface:
- Show only one WebView at a time
- Keep others loaded in background but hidden
- Add tab switcher UI
- Preserve navigation state when switching tabs
- Compare performance vs. showing all 4 simultaneously

### Exercise 3: Add Screenshot Capability
Capture screenshots from each WebView:
- Add screenshot button to each card
- Use `WebView.CaptureScreenshot()` (if available)
- Save to file or display in overlay
- Allow comparison of multiple screenshots
- Hint: Use `Texture2D` to store captured images

### Exercise 4: Dynamic Instance Management
Allow users to add/remove WebView instances:
- Add "+" button to create new WebView
- Add "×" button on each card to remove it
- Dynamically create/destroy WebViewElements
- Update grid layout based on instance count
- Test with different numbers of instances (1-8)

### Exercise 5: Synchronized Navigation
Add "sync mode" to navigate all WebViews together:
- Add toggle for synchronized navigation
- When enabled, all WebViews navigate to same URL
- Useful for comparing how different versions render the same page
- Add offset URLs (e.g., different pages on same site)

### Exercise 6: Memory Profiling
Create detailed memory profiling:
- Track memory before/after creating WebViews
- Show per-WebView memory estimate
- Add warnings when memory exceeds threshold
- Implement automatic cleanup when low on memory
- Use Unity Profiler to verify measurements

## Common Issues

### Issue 1: Performance Degradation
**Problem**: FPS drops significantly with multiple WebViews

**Solution**:
- Reduce number of active WebViews (start with 2-3, not 4)
- Pause inactive WebViews
- Lower resolution of WebView textures
- Simplify web content (avoid heavy JavaScript sites)
- Check hardware capabilities (integrated vs. dedicated GPU)

### Issue 2: Memory Leaks
**Problem**: Memory usage keeps growing over time

**Solution**:
```csharp
// Ensure proper cleanup
private void OnDestroy()
{
    foreach (var card in _webViewCards)
    {
        card.UnbindEvents();  // Unsubscribe all events
        card.Dispose();       // Release resources
    }
    _webViewCards.Clear();
}

// In WebViewCard
public void Dispose()
{
    UnbindEvents();
    _webView = null;  // Clear references
}
```

### Issue 3: Some WebViews Don't Load
**Problem**: 1-2 WebViews fail to initialize while others work

**Solution**:
- Add delays between initializations (100-200ms)
- Check for null references before navigating
- Verify all WebViewElements are correctly named in UXML
- Check console for initialization errors
- Ensure sufficient system resources available

### Issue 4: Navigation Buttons Not Updating
**Problem**: Back/Forward buttons don't enable/disable correctly

**Solution**:
```csharp
private void OnNavigationCompleted(object sender, NavigationCompletedEventArgs args)
{
    // ALWAYS update button state after navigation
    UpdateNavigationButtons();
}

private void UpdateNavigationButtons()
{
    _backButton?.SetEnabled(_webView.CanGoBack());
    _forwardButton?.SetEnabled(_webView.CanGoForward());
}
```

### Issue 5: Inconsistent Behavior Across Instances
**Problem**: Some WebView cards behave differently than others

**Solution**:
- Ensure all cards use the same initialization pattern
- Check for hardcoded values instead of parameterized code
- Verify event bindings are consistent
- Test with same URL in all instances to isolate issues
- Add debug logging to track initialization order

### Issue 6: Crashes with > 4 WebViews
**Problem**: Application crashes when creating many instances

**Solution**:
- Limit maximum instances based on hardware
- Implement lazy loading (create on-demand)
- Add memory checks before creating new instances
- Use lower resolution textures for additional instances
- Consider pooling/reusing instances instead of creating more

## Performance Tips

### Resource Management

**Optimize memory usage:**
```csharp
// Lazy loading pattern
private void CreateWebViewOnDemand(int index)
{
    if (_webViewCards[index] == null)
    {
        _webViewCards[index] = new WebViewCard(index, root, this);
        _webViewCards[index].BindEvents();
    }
}

// Pooling pattern for many instances
private Queue<WebViewCard> _inactiveCards = new Queue<WebViewCard>();

private WebViewCard GetOrCreateCard(int index)
{
    if (_inactiveCards.Count > 0)
    {
        var card = _inactiveCards.Dequeue();
        card.Reset(index);
        return card;
    }
    return new WebViewCard(index, root, this);
}
```

### Staggered Initialization

**Avoid simultaneous loading:**
```csharp
private IEnumerator InitializeWebViewsSequentially()
{
    foreach (var card in _webViewCards)
    {
        card.Navigate();
        yield return new WaitForSeconds(0.5f); // 500ms between loads
    }
}
```

### Conditional Rendering

**Only render visible WebViews:**
```csharp
// Pause off-screen WebViews
private void UpdateVisibility()
{
    foreach (var card in _webViewCards)
    {
        bool isVisible = IsCardVisible(card);

        if (isVisible && card.IsPaused)
            card.Resume();
        else if (!isVisible && !card.IsPaused)
            card.Pause();
    }
}
```

## Next Steps

Continue your learning journey:

- **Next Sample**: [Sample 07 - React Integration](../07_ReactIntegration/README.md)
  - Learn how to load modern React applications
  - Understand JavaScript framework integration
  - Build complex SPAs in Unity

- **Related Concepts**:
  - Sample 02 (Navigation) - Navigation control foundations
  - Sample 05 (Interactive Input) - Input handling patterns

## Additional Resources

### Performance Optimization
- [Unity Profiler Documentation](https://docs.unity3d.com/Manual/Profiler.html)
- [C# Memory Management](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)
- [WebView2 Performance Best Practices](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/performance)

### Architecture Patterns
- [Unity Design Patterns](https://github.com/QianMo/Unity-Design-Pattern)
- [Object Pooling in Unity](https://learn.unity.com/tutorial/object-pooling)
- [SOLID Principles in C#](https://www.c-sharpcorner.com/UploadFile/damubetha/solid-principles-in-C-Sharp/)

### WebView API Reference
- See main package documentation for multi-instance guidelines
- Check `WebViewElement` API reference for lifecycle management

## Summary

This sample demonstrated:

✅ Managing multiple WebView instances simultaneously
✅ Using helper classes for scalable architecture
✅ Independent navigation for each instance
✅ Proper lifecycle management and cleanup
✅ Performance monitoring (FPS, memory, active count)
✅ Preset URL groups for quick testing
✅ Resource considerations and optimization strategies

**Key Takeaway**: Multiple WebViews enable powerful dashboard and multi-panel UIs, but require careful resource management. Use helper classes to organize code, monitor performance closely, and implement proper cleanup to avoid memory leaks. Start with 2-4 instances and scale based on your hardware and performance requirements.
