# Sample 02: Navigation

**Difficulty**: Beginner
**Est. Time**: 15 minutes
**Prerequisites**: Complete [Sample 01: Hello WebView](../01_HelloWebView/README.md)

## Learning Objectives

By completing this sample, you will learn:

- How to programmatically navigate to URLs with `Navigate()`
- How to implement back/forward navigation with `GoBack()` and `GoForward()`
- How to check navigation history state with `CanGoBack()` and `CanGoForward()`
- How to respond to navigation events with `NavigationCompleted`
- How to enable/disable UI buttons based on navigation state
- How to build a custom browser toolbar with address bar

## What This Sample Demonstrates

This sample builds a simple web browser with a custom toolbar. You can:

- **Navigate** to any URL by typing in the address bar
- **Go Back/Forward** through browsing history
- **Refresh** the current page
- See **status updates** when pages load
- Notice that back/forward buttons **automatically enable/disable** based on history

**Key Feature**: This demonstrates how to build responsive, state-aware navigation UIs that adapt to the user's browsing context.

## Setup Instructions

### Step 1: Import the Sample

1. Open **Package Manager** (**Window > Package Manager**)
2. Select **"WebView Toolkit"** in the packages list
3. Click the **"Samples"** tab
4. Find **"02 - Navigation"** and click **"Import"**

### Step 2: Create the Scene

1. Create a new Unity scene (**File > New Scene**)
2. Delete the default Main Camera and Directional Light
3. Create an empty GameObject and rename it to **"NavigationUI"**

### Step 3: Add the UI Components

1. Select the **NavigationUI** GameObject
2. Add a **UI Document** component
3. Add the **NavigationController** component
4. In the NavigationController Inspector, assign:
   - **Uxml**: Drag `Navigation.uxml` from the sample folder
   - **Uss**: Drag `Navigation.uss`

### Step 4: Configure Panel Settings

1. Create or assign a **Panel Settings** asset to the UI Document
2. Set appropriate scale mode for your target resolution

### Step 5: Run the Sample

1. Press **Play**
2. Try the following:
   - Type a URL in the address bar and press **Enter** or click **Go**
   - Navigate to a few different pages
   - Click the **Back** button - notice it only enables when there's history
   - Click the **Forward** button after going back
   - Click **Refresh** to reload the current page

## Code Walkthrough

### Navigation.uxml

The UXML defines a three-section layout:

**Toolbar** (lines 4-13):
```xml
<ui:VisualElement name="toolbar" class="toolbar">
    <ui:Button name="btn-back" text="◀" class="nav-button" />
    <ui:Button name="btn-forward" text="▶" class="nav-button" />
    <ui:Button name="btn-refresh" text="↻" class="nav-button" />
    <ui:TextField name="address-bar" class="address-bar" />
    <ui:Button name="btn-go" text="Go" class="go-button" />
</ui:VisualElement>
```

**WebView Content** (lines 15-19):
- Full-screen WebViewElement that grows to fill available space

**Status Bar** (lines 21-24):
- Shows loading status and error messages

### NavigationController.cs

**Lines 65-88: UI Setup**
```csharp
private void BuildUI()
{
    var root = _uiDocument.rootVisualElement;
    root.Clear();
    _uxml.CloneTree(root);

    // Query all UI elements
    _webViewElement = root.Q<WebViewElement>("webview");
    _addressBar = root.Q<TextField>("address-bar");
    _backButton = root.Q<Button>("btn-back");
    // ... etc
}
```

This method loads the UXML and finds all the UI elements we need to interact with.

**Lines 90-126: Event Binding**
```csharp
private void BindEvents()
{
    // Listen for navigation completion
    _webViewElement.NavigationCompleted += OnNavigationCompleted;

    // Bind button clicks
    _backButton.clicked += OnBackClicked;
    _forwardButton.clicked += OnForwardClicked;
    _refreshButton.clicked += OnRefreshClicked;
    _goButton.clicked += OnGoClicked;

    // Handle Enter key in address bar
    _addressBar.RegisterCallback<KeyDownEvent>(OnAddressBarKeyDown);
}
```

Event binding is crucial - this is how we respond to user actions and WebView events.

**Lines 146-159: Navigate Method**
```csharp
private void Navigate(string url)
{
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
```

This helper method handles URL normalization (adding https://) and calls the WebView's `Navigate()` method.

**Lines 161-174: Back/Forward Navigation**
```csharp
private void GoBack()
{
    if (_webViewElement?.WebView == null) return;

    // Only go back if there's history
    if (_webViewElement.WebView.CanGoBack())
    {
        SetStatus("Going back...", false);
        _webViewElement.WebView.GoBack();
    }
}
```

**Key Point**: Always check `CanGoBack()` before calling `GoBack()`, and `CanGoForward()` before calling `GoForward()`. This prevents errors when there's no history.

**Lines 223-241: Navigation Completed Handler**
```csharp
private void OnNavigationCompleted(string url, bool isSuccess)
{
    if (isSuccess)
    {
        // Update address bar with actual URL
        _addressBar.value = url;
        SetStatus("Ready", false);
    }
    else
    {
        SetStatus("Navigation failed", true);
    }

    // Update button states
    UpdateNavigationButtons();
}
```

This event handler is called when navigation finishes. It:
1. Updates the address bar with the final URL (may differ from what you typed due to redirects)
2. Shows success or error status
3. Updates the enabled state of back/forward buttons

**Lines 249-271: Update Navigation Buttons**
```csharp
private void UpdateNavigationButtons()
{
    if (_webViewElement?.WebView == null) return;

    // Update back button
    bool canGoBack = _webViewElement.WebView.CanGoBack();
    _backButton.SetEnabled(canGoBack);
    _backButton.style.opacity = canGoBack ? 1f : 0.4f;

    // Update forward button
    bool canGoForward = _webViewElement.WebView.CanGoForward();
    _forwardButton.SetEnabled(canGoForward);
    _forwardButton.style.opacity = canGoForward ? 1f : 0.4f;
}
```

This method queries the history state and updates the UI accordingly. Disabled buttons are visually dimmed with reduced opacity.

## Key Concepts

### Navigation Methods

| Method | Description | When to Use |
|--------|-------------|-------------|
| `Navigate(url)` | Load a URL | Navigate to a new page |
| `NavigateToString(html)` | Load HTML string | Display dynamic content (covered in Sample 04) |
| `GoBack()` | Navigate backwards in history | Implement back button |
| `GoForward()` | Navigate forwards in history | Implement forward button |

### History State Queries

| Method | Returns | Purpose |
|--------|---------|---------|
| `CanGoBack()` | `bool` | Check if back navigation is possible |
| `CanGoForward()` | `bool` | Check if forward navigation is possible |

**Best Practice**: Always check these before enabling back/forward buttons or calling the navigation methods.

### Navigation Events

```csharp
// Subscribe to navigation completion
webViewElement.NavigationCompleted += (url, isSuccess) => {
    if (isSuccess) {
        Debug.Log($"Successfully navigated to: {url}");
    } else {
        Debug.LogError($"Failed to navigate to: {url}");
    }
};
```

The `NavigationCompleted` event provides:
- **url**: The final URL (may differ from requested URL due to redirects)
- **isSuccess**: Whether navigation succeeded

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Add a Home Button

1. Open `Navigation.uxml`
2. Add a new button in the toolbar:
   ```xml
   <ui:Button name="btn-home" text="⌂" class="nav-button" tooltip="Home" />
   ```
3. In `NavigationController.cs`, add a home URL field and implement the home button click handler
4. Bonus: Make the home URL configurable in the Inspector

### Exercise 2: Show Page Title

1. Modify the status bar to show the current page title
2. Hint: You can get the title using JavaScript: `ExecuteScript("document.title")`
3. This is a preview of Sample 03's JavaScript interop!

### Exercise 3: Navigation History Count

1. Add a label showing how many pages are in the back history
2. Update it in `OnNavigationCompleted`
3. Note: There's no direct API for this, but you can track it yourself

### Exercise 4: URL Validation

1. Add visual feedback when the user types an invalid URL
2. Change the address bar border color to red for invalid URLs
3. Only enable the Go button when the URL is valid

## Common Issues

### Issue 1: Buttons Always Disabled

**Problem**: Back/Forward buttons never enable even after navigating.

**Solution**: Make sure `UpdateNavigationButtons()` is called in `OnNavigationCompleted`. The history state updates asynchronously, so you must check it after navigation completes.

### Issue 2: Address Bar Not Updating

**Problem**: Address bar still shows old URL after navigation.

**Solution**: Check that you're updating `_addressBar.value` in the `OnNavigationCompleted` handler.

### Issue 3: Can't Navigate to Local Files

**Problem**: Navigation to `file:///` URLs fails.

**Solution**:
- Use forward slashes even on Windows: `file:///C:/path/to/file.html`
- Ensure the file exists and is accessible
- Check Unity console for WebView error messages

### Issue 4: Enter Key Not Working in Address Bar

**Problem**: Pressing Enter in the address bar doesn't navigate.

**Solution**: Verify you've registered the KeyDownEvent callback and check for both `KeyCode.Return` and `KeyCode.KeypadEnter`.

## What You Learned

Congratulations! You now understand navigation in WebViewToolkit. You've learned:

- ✅ How to navigate programmatically with `Navigate()`
- ✅ How to implement back/forward navigation
- ✅ How to check and respond to history state
- ✅ How to handle navigation events
- ✅ How to build responsive, state-aware navigation UIs
- ✅ How to normalize URLs (adding protocols)

## Next Steps

Ready to level up? Continue your journey:

- **Next Sample**: [03 - JavaScript Bridge](../03_JavaScriptBridge/README.md) - Learn two-way communication between C# and JavaScript
- **Challenge**: Combine this sample with Sample 01 to build a tabbed browser with multiple WebViews
- **Explore**: Look at the `WebViewPanel` component in the package for a more complete browser UI

## Additional Resources

- **WebViewElement API**: See package README for `Navigate()`, `GoBack()`, `GoForward()` documentation
- **WebViewInstance**: Low-level API if you need more control
- **UIToolkit Events**: [Unity Documentation on UI Events](https://docs.unity3d.com/Manual/UIE-Events.html)

---

**Pro Tip**: The `CurrentUrl` property on WebViewElement always reflects the current page URL. You can use this to track navigation without events!
