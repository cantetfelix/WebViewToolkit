# WebView Toolkit for Unity - Package Documentation

**Version 1.3.0**

Native WebView2 integration for Unity with DirectX 11/12 support. This package provides high-performance, off-screen rendering of modern web content directly to Unity textures.

For installation instructions and project overview, see the [main repository README](https://github.com/cantetfelix/WebViewToolkit).

## Table of Contents

- [Quick Start](#quick-start)
- [Integration Patterns](#integration-patterns)
- [API Reference](#api-reference)
- [Advanced Usage](#advanced-usage)
- [Samples](#samples)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Quick Start

### UIToolkit Integration (Recommended)

The primary integration path uses UIToolkit VisualElements:

**In UXML:**
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <WebViewElement
        initial-url="https://unity.com"
        enable-dev-tools="false"
        style="width: 1280px; height: 720px;" />
</ui:UXML>
```

**In C#:**
```csharp
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

public class MyUIController : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var webView = root.Q<WebViewElement>();

        webView.NavigationCompleted += (url, success) => {
            Debug.Log($"Navigated to: {url}");
        };

        webView.Navigate("https://example.com");
    }
}
```

### MonoBehaviour Component

For traditional GameObject-based workflows:

```csharp
using UnityEngine;
using WebViewToolkit;

public class MyScript : MonoBehaviour
{
    public WebViewBehaviour webView;

    void Start()
    {
        webView.Navigate("https://unity.com");

        webView.WebView.NavigationCompleted += (url, success) => {
            if (success)
                Debug.Log($"Loaded: {url}");
        };
    }
}
```

### Direct API

For advanced scenarios requiring full control:

```csharp
using UnityEngine;
using UnityEngine.UI;
using WebViewToolkit;

public class CustomWebView : MonoBehaviour
{
    public RawImage targetImage;
    private WebViewInstance _webView;

    void Start()
    {
        _webView = WebViewManager.Instance.CreateWebView(
            width: 1920,
            height: 1080,
            initialUrl: "https://unity.com",
            enableDevTools: false
        );

        if (_webView != null)
        {
            targetImage.texture = _webView.Texture;
        }
    }

    void OnDestroy()
    {
        _webView?.Dispose();
    }
}
```

---

## Integration Patterns

### 1. UIToolkit - WebViewElement

A VisualElement that automatically handles sizing, mouse input, and texture rendering.

**Features:**
- Automatic resizing when element geometry changes
- Built-in mouse event forwarding (click, move, wheel)
- Background texture rendering
- Delayed creation in Editor to avoid preview crashes

**UXML Attributes:**
- `initial-url` (string): Initial URL to load
- `enable-dev-tools` (bool): Enable Chrome DevTools
- `auto-create` (bool): Automatically create WebView on attach

**Example:**
```xml
<WebViewElement
    initial-url="https://google.com"
    enable-dev-tools="true"
    style="flex-grow: 1;" />
```

**In UIBuilder:**
1. Open your UXML in UIBuilder
2. Find `WebViewElement` in Library panel under Project
3. Drag onto canvas
4. Configure properties in Inspector

### 2. UIToolkit - WebViewPanel

A complete browser UI with navigation controls, address bar, and optional toolbar.

**Features:**
- Back/forward/refresh navigation buttons
- Address bar with URL input
- Optional home button
- Loading indicator
- DevTools button
- Status bar
- Fully customizable via UXML attributes

**UXML Attributes:**
- `initial-url`, `enable-dev-tools`: WebView settings
- `show-toolbar`, `show-address-bar`: UI toggles
- `show-navigation-buttons`, `show-refresh-button`: Button visibility
- `show-home-button`, `show-devtools-button`: Additional buttons
- `show-loading-indicator`, `show-status-bar`: Status UI
- `toolbar-height` (int): Height in pixels
- `home-url`: URL for home button

**Example:**
```xml
<WebViewPanel
    initial-url="https://google.com"
    show-toolbar="true"
    show-address-bar="true"
    show-navigation-buttons="true"
    show-home-button="true"
    home-url="https://unity.com"
    toolbar-height="40" />
```

**USS Styling:**
```css
.webview-panel { }
.webview-panel__toolbar { background-color: #333; }
.webview-panel__address-bar { flex-grow: 1; }
.webview-panel__button { width: 32px; height: 32px; }
.webview-panel__content { flex-grow: 1; }
.webview-panel__status-bar { background-color: #222; }
.webview-panel__loading { color: yellow; }
```

### 3. MonoBehaviour - WebViewBehaviour

A MonoBehaviour component for traditional Unity workflows.

**Inspector Properties:**
- `Width`, `Height`: Texture dimensions
- `Initial URL`: Starting URL
- `Enable Dev Tools`: Enable debugging
- `Create On Start`: Auto-create on Start()

**Events (UnityEvent):**
- `On Navigation Completed`: (string url, bool success)
- `On Message Received`: (string message)

**Usage:**
1. Add `WebViewBehaviour` component to GameObject
2. Configure properties in Inspector
3. Access via `webViewBehaviour.WebView` in scripts
4. Use `webViewBehaviour.Texture` for rendering

### 4. Direct API - WebViewManager & WebViewInstance

Low-level API for maximum control.

**When to use:**
- Custom rendering pipelines
- Multiple WebView management
- Advanced input handling
- Non-standard integration scenarios

**Pattern:**
```csharp
// Create
var webView = WebViewManager.Instance.CreateWebView(1280, 720, "https://unity.com");

// Use
material.mainTexture = webView.Texture;
webView.Navigate("https://example.com");
webView.SendMouseEvent(MouseEventType.Down, MouseButton.Left, 0.5f, 0.5f);

// Cleanup
webView.Dispose();
```

---

## API Reference

### WebViewManager (Singleton)

The singleton manager for native plugin lifecycle and WebView instance creation.

```csharp
// Access singleton
WebViewManager manager = WebViewManager.Instance;

// Properties
bool IsInitialized { get; }                    // Plugin initialization status
GraphicsAPI CurrentGraphicsAPI { get; }        // Detected graphics API (D3D11/D3D12)

// Methods
WebViewInstance CreateWebView(
    int width,                                 // Texture width in pixels
    int height,                                // Texture height in pixels
    string initialUrl = null,                  // Initial URL (optional)
    bool enableDevTools = false                // Enable Chrome DevTools
);

void Shutdown();                               // Shutdown plugin and destroy all WebViews
```

**Notes:**
- Automatically initializes on first access
- Thread-safe singleton implementation
- Automatically shuts down on application quit
- Returns `null` during application shutdown

---

### WebViewInstance

Represents a single WebView instance with its texture and APIs.

#### Properties

```csharp
uint Handle { get; }                           // Native handle
int Width { get; }                             // Texture width in pixels
int Height { get; }                            // Texture height in pixels
Texture2D Texture { get; }                     // External texture (BGRA32 format)
bool IsDestroyed { get; }                      // Whether disposed
string CurrentUrl { get; }                     // Currently loaded URL
```

#### Events

```csharp
event Action<string, bool> NavigationCompleted;  // (url, isSuccess)
event Action<string> MessageReceived;            // (message from JavaScript)
```

#### Navigation

```csharp
bool Navigate(string url);                     // Navigate to URL
bool NavigateToString(string html);            // Load HTML string
bool GoBack();                                 // Navigate back
bool GoForward();                              // Navigate forward
bool CanGoBack();                              // Check if can go back
bool CanGoForward();                           // Check if can go forward
```

#### JavaScript Interop

```csharp
bool ExecuteScript(string script);             // Execute JavaScript code
```

**From JavaScript, send messages to C#:**
```javascript
window.chrome.webview.postMessage("Hello Unity!");
```

**In C#, receive messages:**
```csharp
webView.MessageReceived += (message) => {
    Debug.Log($"From JS: {message}");
};
```

#### Input Handling

```csharp
bool SendMouseEvent(
    MouseEventType eventType,                  // Move, Down, Up, Wheel, Leave
    MouseButton button,                        // None, Left, Right, Middle
    float normalizedX,                         // X position [0-1]
    float normalizedY,                         // Y position [0-1]
    float wheelDelta = 0                       // Wheel delta (for Wheel events)
);

bool SendKeyEvent(
    uint virtualKeyCode,                       // Virtual key code
    uint scanCode,                             // Scan code
    bool isKeyDown,                            // true = down, false = up
    bool isSystemKey = false                   // true for Alt, etc.
);
```

**Coordinate System:**
- Normalized [0-1] range
- (0, 0) = top-left
- (1, 1) = bottom-right

#### Sizing

```csharp
bool Resize(int width, int height);            // Resize WebView and texture
```

#### Lifecycle

```csharp
void Dispose();                                // Dispose WebView and cleanup resources
```

**Important:** Always dispose WebView instances when done to prevent memory leaks.

---

### WebViewBehaviour (MonoBehaviour)

MonoBehaviour component for easy GameObject integration.

#### Inspector Properties

```csharp
[SerializeField] int Width = 1280;
[SerializeField] int Height = 720;
[SerializeField] string InitialUrl = "https://www.google.com";
[SerializeField] bool EnableDevTools = false;
[SerializeField] bool CreateOnStart = true;
[SerializeField] UnityEvent<string, bool> OnNavigationCompleted;
[SerializeField] UnityEvent<string> OnMessageReceived;
```

#### Public Properties

```csharp
WebViewInstance WebView { get; }               // Underlying WebView instance
Texture2D Texture { get; }                     // WebView texture
bool IsReady { get; }                          // Whether WebView is created
int Width { get; set; }                        // Get/set width (auto-resize)
int Height { get; set; }                       // Get/set height (auto-resize)
```

#### Public Methods

```csharp
void CreateWebView();                          // Create WebView instance
void DestroyWebView();                         // Destroy WebView instance
void Navigate(string url);                     // Navigate to URL
void NavigateToString(string html);            // Load HTML string
void ExecuteScript(string script);             // Execute JavaScript

void SendMouseEvent(
    MouseEventType eventType,
    MouseButton button,
    Vector2 normalizedPosition,                // [0-1] coordinates
    float wheelDelta = 0
);

void Resize(int width, int height);            // Resize WebView
```

---

### WebViewElement (UIToolkit VisualElement)

UIToolkit VisualElement for seamless UI integration.

#### Properties

```csharp
string InitialUrl { get; set; }                // Initial URL
bool EnableDevTools { get; set; }              // Enable DevTools
WebViewInstance WebView { get; }               // Underlying instance
bool IsReady { get; }                          // Whether created
string CurrentUrl { get; }                     // Current URL
```

#### Events

```csharp
event Action<string, bool> NavigationCompleted; // (url, isSuccess)
event Action<string> MessageReceived;           // (message)
```

#### Methods

```csharp
void CreateWebView();                          // Create WebView
void DestroyWebView();                         // Destroy WebView
void Navigate(string url);                     // Navigate to URL
void NavigateToString(string html);            // Load HTML string
void ExecuteScript(string script);             // Execute JavaScript

void SendMouseEvent(
    Native.MouseEventType eventType,
    Native.MouseButton button,
    Vector2 localPosition,                     // Local element coordinates
    float wheelDelta = 0
);

void Resize(int width, int height);            // Manual resize
```

**Automatic Features:**
- Resizes when element geometry changes
- Forwards mouse events automatically
- Updates background texture automatically
- Captures mouse during drag

---

### WebViewPanel (UIToolkit VisualElement)

Full-featured browser panel with UI chrome.

#### WebView Settings

```csharp
string InitialUrl { get; set; }                // Initial URL
bool EnableDevTools { get; set; }              // Enable DevTools
string HomeUrl { get; set; }                   // Home page URL
```

#### UI Feature Toggles

```csharp
bool ShowToolbar { get; set; }                 // Show toolbar
bool ShowAddressBar { get; set; }              // Show address bar
bool ShowNavigationButtons { get; set; }       // Show back/forward buttons
bool ShowRefreshButton { get; set; }           // Show refresh button
bool ShowHomeButton { get; set; }              // Show home button
bool ShowLoadingIndicator { get; set; }        // Show loading indicator
bool ShowDevToolsButton { get; set; }          // Show DevTools button
bool ShowStatusBar { get; set; }               // Show status bar
int ToolbarHeight { get; set; }                // Toolbar height in pixels
```

#### Properties

```csharp
WebViewElement WebViewElement { get; }         // Underlying WebViewElement
WebViewInstance WebView { get; }               // Underlying WebView instance
bool IsReady { get; }                          // Whether WebView is ready
string CurrentUrl { get; }                     // Current URL
bool IsLoading { get; }                        // Whether page is loading
```

#### Events

```csharp
event Action<string> NavigationStarted;        // (url)
event Action<string, bool> NavigationCompleted; // (url, isSuccess)
event Action<string> MessageReceived;           // (message)
event Action<string> UrlChanged;                // (url) - address bar changed
```

#### Navigation Methods

```csharp
void Navigate(string url);                     // Navigate to URL (auto-adds https://)
void NavigateToString(string html);            // Load HTML string
void ExecuteScript(string script);             // Execute JavaScript
void GoBack();                                 // Go back in history
void GoForward();                              // Go forward in history
void Refresh();                                // Refresh current page
void GoHome();                                 // Navigate to home URL
bool CanGoBack();                              // Check if can go back
bool CanGoForward();                           // Check if can go forward
```

---

### Enums and Types

#### GraphicsAPI
```csharp
enum GraphicsAPI : int
{
    Unknown = 0,
    Direct3D11 = 2,
    Direct3D12 = 18
}
```

#### MouseEventType
```csharp
enum MouseEventType : int
{
    Move = 0,
    Down = 1,
    Up = 2,
    Wheel = 3,
    Leave = 4
}
```

#### MouseButton
```csharp
enum MouseButton : int
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3
}
```

#### NativeResult
```csharp
enum NativeResult : int
{
    Success = 0,
    ErrorUnknown = -1,
    ErrorInvalidHandle = -2,
    ErrorNotInitialized = -3,
    // ... (see WebViewNative.cs for full list)
}
```

---

## Advanced Usage

### JavaScript Interop

#### Execute JavaScript from C#

```csharp
webView.ExecuteScript("document.title = 'Hello from Unity';");
webView.ExecuteScript("console.log('Unity integration active');");

// Return values are not supported - use postMessage instead
webView.ExecuteScript(@"
    var result = 2 + 2;
    window.chrome.webview.postMessage('Result: ' + result);
");
```

#### Receive Messages from JavaScript

**In JavaScript:**
```javascript
// Send simple message
window.chrome.webview.postMessage("Hello Unity!");

// Send JSON data
const data = { type: "click", x: 100, y: 200 };
window.chrome.webview.postMessage(JSON.stringify(data));
```

**In C#:**
```csharp
webView.MessageReceived += (message) => {
    Debug.Log($"Received: {message}");

    // Parse JSON
    var data = JsonUtility.FromJson<MyData>(message);
    HandleEvent(data);
};
```

### Input Handling

#### Mouse Input

**Normalized Coordinates [0-1]:**
```csharp
void OnMouseClick(Vector2 screenPos)
{
    // Convert screen to normalized coordinates
    var rect = GetComponent<RectTransform>().rect;
    float normalizedX = screenPos.x / rect.width;
    float normalizedY = screenPos.y / rect.height;

    webView.SendMouseEvent(
        MouseEventType.Down,
        MouseButton.Left,
        normalizedX,
        normalizedY
    );
}
```

**Mouse Wheel:**
```csharp
void OnScroll(float delta)
{
    webView.SendMouseEvent(
        MouseEventType.Wheel,
        MouseButton.None,
        lastMouseX,
        lastMouseY,
        wheelDelta: delta
    );
}
```

#### Keyboard Input

```csharp
void Update()
{
    foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
    {
        if (Input.GetKeyDown(key))
        {
            uint vk = KeyCodeToVirtualKey(key);
            webView.SendKeyEvent(vk, 0, isKeyDown: true);
        }
        else if (Input.GetKeyUp(key))
        {
            uint vk = KeyCodeToVirtualKey(key);
            webView.SendKeyEvent(vk, 0, isKeyDown: false);
        }
    }
}
```

### Dynamic Resizing

```csharp
public void OnWindowResize(int newWidth, int newHeight)
{
    if (webView != null && webView.IsReady)
    {
        webView.Resize(newWidth, newHeight);
    }
}
```

### Multiple WebView Instances

```csharp
private List<WebViewInstance> _webViews = new List<WebViewInstance>();

void CreateWebViews()
{
    for (int i = 0; i < 4; i++)
    {
        var webView = WebViewManager.Instance.CreateWebView(
            640, 480,
            $"https://example.com?instance={i}"
        );

        _webViews.Add(webView);
    }
}

void OnDestroy()
{
    foreach (var webView in _webViews)
    {
        webView?.Dispose();
    }
    _webViews.Clear();
}
```

### Chrome DevTools

Enable DevTools for debugging web content:

```csharp
var webView = WebViewManager.Instance.CreateWebView(
    1280, 720,
    "https://unity.com",
    enableDevTools: true  // Enable DevTools
);
```

A separate DevTools window will open alongside the WebView.

### Loading Local HTML

#### Option 1: NavigateToString
```csharp
string html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Local HTML</title>
    <style>
        body { background: #333; color: white; font-family: Arial; }
    </style>
</head>
<body>
    <h1>Hello from Unity!</h1>
    <button onclick=""sendToUnity()"">Send Message</button>
    <script>
        function sendToUnity() {
            window.chrome.webview.postMessage('Button clicked!');
        }
    </script>
</body>
</html>
";

webView.NavigateToString(html);
```

#### Option 2: Local File URL
```csharp
string localPath = Path.Combine(Application.streamingAssetsPath, "index.html");
string fileUrl = "file:///" + localPath.Replace("\\", "/");
webView.Navigate(fileUrl);
```

---

## Samples

The package includes six progressive samples that teach you WebViewToolkit from beginner to intermediate level.

### Importing Samples

1. Open Package Manager (`Window` > `Package Manager`)
2. Select **WebView Toolkit** in the packages list
3. Expand the **Samples** section
4. Click **Import** next to the desired sample

### Learning Path

We recommend completing the samples in order for the best learning experience:

#### 01 - Hello WebView
**Difficulty:** Beginner | **Time:** 5 minutes

Your first WebView! Shows the minimum code needed to display a web page in Unity using UIToolkit.

**What you'll learn:**
- Creating a WebViewElement in UXML
- Basic scene setup with UIDocument
- Initial URL configuration

#### 02 - Navigation
**Difficulty:** Beginner | **Time:** 15 minutes

Learn how to navigate between pages, manage history, and respond to navigation events.

**What you'll learn:**
- Programmatic navigation with `Navigate()`
- Back/Forward history with `GoBack()`/`GoForward()`
- Checking navigation state with `CanGoBack()`/`CanGoForward()`
- Handling `NavigationCompleted` events
- Enabling/disabling UI based on state

#### 03 - JavaScript Bridge
**Difficulty:** Intermediate | **Time:** 30 minutes

Master two-way communication between C# and JavaScript. Essential for building interactive web-based UIs.

**What you'll learn:**
- Calling JavaScript from C# with `ExecuteScript()`
- Receiving messages from JavaScript via `window.chrome.webview.postMessage()`
- JSON serialization for structured data
- Building game UIs that respond to player actions
- Event-driven communication patterns

#### 04 - Dynamic HTML
**Difficulty:** Intermediate | **Time:** 30 minutes

Generate web content dynamically from C# data. Perfect for inventory systems, quest logs, and leaderboards.

**What you'll learn:**
- Loading HTML strings with `NavigateToString()`
- Building HTML programmatically with C# templates
- Real-time content updates
- Creating data-driven UIs (inventory, quest log, settings)
- HTML templating best practices

#### 05 - Interactive Input
**Difficulty:** Intermediate | **Time:** 20 minutes

Understand how WebView handles mouse and keyboard input, and how to interact with web forms.

**What you'll learn:**
- How WebViewElement forwards mouse events automatically
- Understanding normalized [0-1] coordinate system
- Interacting with web forms (inputs, buttons, checkboxes)
- Mouse hover effects and wheel scrolling
- Debugging input coordinate mapping

#### 06 - Multiple WebViews
**Difficulty:** Intermediate | **Time:** 25 minutes

Learn to manage multiple WebView instances simultaneously for dashboards or split-screen browsers.

**What you'll learn:**
- Creating multiple WebViewElements in one scene
- Proper lifecycle management (creation and disposal)
- Independent navigation for each instance
- Performance considerations and optimization
- Memory management best practices

### Sample Structure

Each sample includes:
- **Unity Scene** - Ready-to-play demo scene
- **C# Scripts** - Thoroughly commented code
- **UXML/USS Files** - UIToolkit layouts and styles
- **README.md** - Detailed walkthrough and explanations

For the complete samples overview and additional learning resources, see `Samples~/README.md` after importing.

---

## Best Practices

### Performance Optimization

**Resolution Management:**
```csharp
// Use lower resolution for better performance
var webView = WebViewManager.Instance.CreateWebView(
    1280, 720,  // Instead of 1920x1080
    initialUrl
);

// Dynamically adjust based on quality settings
int width = QualitySettings.GetQualityLevel() > 2 ? 1920 : 1280;
int height = QualitySettings.GetQualityLevel() > 2 ? 1080 : 720;
```

**Limit Active Instances:**
```csharp
// Pool WebView instances instead of creating/destroying frequently
private Queue<WebViewInstance> _webViewPool = new Queue<WebViewInstance>();

WebViewInstance GetPooledWebView()
{
    if (_webViewPool.Count > 0)
        return _webViewPool.Dequeue();

    return WebViewManager.Instance.CreateWebView(1280, 720);
}

void ReturnToPool(WebViewInstance webView)
{
    webView.Navigate("about:blank");
    _webViewPool.Enqueue(webView);
}
```

**Frame Throttling:**
The WebViewManager automatically throttles texture updates to ~30 FPS. For static content, consider navigating to cached pages.

### Memory Management

**Always Dispose:**
```csharp
void OnDestroy()
{
    webView?.Dispose();
    webView = null;
}
```

**Check IsDestroyed:**
```csharp
void UpdateWebView()
{
    if (webView != null && !webView.IsDestroyed)
    {
        webView.Navigate(newUrl);
    }
}
```

**Clean Up on Scene Load:**
```csharp
void OnDisable()
{
    webView?.Dispose();
}
```

### Thread Safety

**Main Thread Only:**
All WebView operations must occur on the main Unity thread. Never call WebView methods from background threads.

```csharp
// WRONG - Will crash or fail
Task.Run(() => {
    webView.Navigate(url);  // Don't do this!
});

// CORRECT - Use main thread
async Task LoadAsync(string url)
{
    await Task.Delay(1000);  // Background work

    // Return to main thread
    UnityMainThreadDispatcher.Enqueue(() => {
        webView.Navigate(url);
    });
}
```

### Texture Usage

**Texture Format:**
WebView textures use BGRA32 format. Don't attempt to change the format.

**Read Texture Pixels:**
```csharp
// Works, but slow
Texture2D texture = webView.Texture;
Color[] pixels = texture.GetPixels();

// Better - use native GPU texture directly in materials
material.mainTexture = webView.Texture;
```

**Texture Filtering:**
```csharp
// Texture is created with bilinear filtering by default
// Can be changed if needed
webView.Texture.filterMode = FilterMode.Point;
```

### Error Handling

**Check Results:**
```csharp
bool success = webView.Navigate(url);
if (!success)
{
    Debug.LogError($"Failed to navigate to {url}");
}
```

**Listen to Events:**
```csharp
webView.NavigationCompleted += (url, success) => {
    if (!success)
    {
        Debug.LogError($"Navigation failed: {url}");
        // Show error UI or retry
    }
};
```

---

## Troubleshooting

### WebView2 Runtime Not Found

**Symptoms:**
- Console errors about WebView2 not being available
- Black or empty WebView texture
- Initialization failures

**Solution:**
1. Download WebView2 Runtime from: https://developer.microsoft.com/en-us/microsoft-edge/webview2/
2. Install the **Evergreen Bootstrapper** or **Evergreen Standalone Installer**
3. Restart Unity after installation

**Check Installation:**
- Windows 10/11 usually has WebView2 pre-installed
- Check `Windows Settings` > `Apps & features` for "Microsoft Edge WebView2 Runtime"

### Black or Empty Texture

**Symptoms:**
- WebView texture is completely black
- No web content visible

**Possible Causes & Solutions:**

1. **Wrong Graphics API:**
   - Go to `Edit` > `Project Settings` > `Player`
   - Under `Other Settings`, check `Graphics APIs for Windows`
   - Ensure **Direct3D11** or **Direct3D12** is in the list
   - Remove OpenGL if present

2. **WebView Not Created:**
   ```csharp
   if (webView == null || webView.IsDestroyed)
   {
       Debug.LogError("WebView not created or destroyed");
   }
   ```

3. **Initial URL Failed:**
   - Check console for navigation errors
   - Try navigating to a simple page: `https://example.com`
   - Verify internet connection

4. **Texture Not Assigned:**
   ```csharp
   // Ensure texture is assigned to UI element
   rawImage.texture = webView.Texture;
   ```

### Mouse Input Not Working

**Symptoms:**
- Clicks and mouse movements don't interact with web content
- Links not clickable

**Possible Causes & Solutions:**

1. **Wrong Coordinate Range:**
   ```csharp
   // Coordinates must be [0-1], not pixel coordinates
   float normalizedX = pixelX / textureWidth;
   float normalizedY = pixelY / textureHeight;
   ```

2. **Input Not Forwarded:**
   - WebViewElement forwards input automatically
   - For custom integrations, ensure you're calling `SendMouseEvent()`

3. **UI Blocking Input:**
   - Check if another UI element is blocking raycasts
   - Verify CanvasGroup is not blocking interactions

### Navigation Failures

**Symptoms:**
- Navigate() returns false
- NavigationCompleted event fires with success=false
- Pages fail to load

**Possible Causes & Solutions:**

1. **Invalid URL:**
   ```csharp
   // Ensure URL has protocol
   string url = userInput;
   if (!url.StartsWith("http://") && !url.StartsWith("https://"))
   {
       url = "https://" + url;
   }
   webView.Navigate(url);
   ```

2. **SSL Certificate Errors:**
   - Some internal sites may have certificate issues
   - WebView2 respects system certificate settings

3. **CORS Restrictions:**
   - Local file access may be restricted
   - Use NavigateToString() for local content

### Performance Issues

**Symptoms:**
- Low frame rate
- High CPU usage
- Stuttering

**Solutions:**

1. **Reduce Resolution:**
   ```csharp
   webView.Resize(1280, 720);  // Instead of 1920x1080
   ```

2. **Limit Active WebViews:**
   - Destroy offscreen WebViews
   - Use pooling for frequent create/destroy

3. **Disable DevTools:**
   ```csharp
   // DevTools window uses additional resources
   enableDevTools: false
   ```

4. **Optimize Web Content:**
   - Reduce animations and complex CSS
   - Minimize JavaScript execution
   - Use lightweight frameworks

### Editor-Specific Issues

**UI Builder Preview Crashes:**
- WebViewElement delays creation by 100ms in Editor
- Prevents crashes during static UI preview
- Not an issue in Play mode or builds

**Hot Reload:**
- Domain reload may leave WebViews in invalid state
- Restart Play mode if issues occur after script changes

**Scene Cleanup:**
- Ensure WebViews are properly disposed in OnDisable/OnDestroy
- Check for leaked instances in Console

### Build-Specific Issues

**DLL Not Found:**
- Ensure `WebViewToolkit.dll` is in `Packages/com.webviewtoolkit.core/Runtime/Plugins/x86_64/`
- Check DLL is set to x86_64 platform in import settings

**IL2CPP Compatibility:**
- Package is compatible with IL2CPP
- Uses P/Invoke marshaling (not C++/CLI)

**Missing Dependencies:**
- WebView2 Runtime must be installed on target machine
- Can be bundled with installer or require user installation

### Platform Limitations

**Unsupported Platforms:**
- **macOS/Linux:** Not supported (WebView2 is Windows-only)
- **Mobile:** Not supported (use platform-specific WebViews)
- **Consoles:** Not supported

**Graphics API:**
- **OpenGL:** Not supported
- **Vulkan:** Not supported
- **DirectX 11:** ✅ Supported
- **DirectX 12:** ✅ Supported

### Debug Strategies

**Enable DevTools:**
```csharp
var webView = WebViewManager.Instance.CreateWebView(
    1280, 720, url,
    enableDevTools: true  // Opens DevTools window
);
```

**Check Initialization:**
```csharp
if (!WebViewManager.Instance.IsInitialized)
{
    Debug.LogError("WebViewManager not initialized!");
    Debug.Log($"Graphics API: {WebViewManager.Instance.CurrentGraphicsAPI}");
}
```

**Log Navigation Events:**
```csharp
webView.NavigationCompleted += (url, success) => {
    Debug.Log($"Navigation to {url}: {(success ? "SUCCESS" : "FAILED")}");
};
```

**Inspect JavaScript Messages:**
```csharp
webView.ExecuteScript("console.log('Test from Unity')");

webView.MessageReceived += (msg) => {
    Debug.Log($"[JS Message] {msg}");
};
```

---

## Additional Resources

- **Main Repository:** https://github.com/cantetfelix/WebViewToolkit
- **Issue Tracker:** https://github.com/cantetfelix/WebViewToolkit/issues
- **Discussions:** https://github.com/cantetfelix/WebViewToolkit/discussions
- **WebView2 Documentation:** https://developer.microsoft.com/en-us/microsoft-edge/webview2/

---

**Package Version:** 1.3.0
**Last Updated:** 2026-01-29
**License:** MIT
