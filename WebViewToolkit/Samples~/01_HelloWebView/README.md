# Sample 01: Hello WebView

**Difficulty**: Beginner
**Est. Time**: 5 minutes
**Prerequisites**: None - this is where you start!

## Learning Objectives

By completing this sample, you will learn:

- How to create a basic WebViewElement in UXML
- How to configure the initial URL
- How to set up a Unity scene with UIDocument for WebView
- The absolute minimum code needed to display web content

## What This Sample Demonstrates

This is the simplest possible WebView setup. It displays a full-screen web browser loading Google's homepage. While minimal, it demonstrates all the core concepts you need to get started with WebViewToolkit.

**Key Feature**: The WebViewElement handles everything automatically - creation, resizing, input forwarding, and rendering - with zero code required!

## Setup Instructions

### Step 1: Import the Sample

1. Open **Package Manager** (**Window > Package Manager**)
2. Select **"WebView Toolkit"** in the packages list
3. Click the **"Samples"** tab
4. Find **"01 - Hello WebView"** and click **"Import"**

### Step 2: Create the Scene

1. Create a new Unity scene (**File > New Scene**)
2. Delete the default Main Camera and Directional Light (not needed for UI-only scene)
3. Create an empty GameObject (**GameObject > Create Empty**)
4. Rename it to **"WebViewUI"**

### Step 3: Add the UI Components

1. Select the **WebViewUI** GameObject
2. Add a **UI Document** component (**Add Component > UI Toolkit > UI Document**)
3. In the Inspector, assign the **UI Document** settings:
   - **Source Asset**: Drag `HelloWebView.uxml` from the sample folder
4. Add the **HelloWebViewController** component (**Add Component > Hello Web View Controller**)
5. In the HelloWebViewController Inspector, assign:
   - **Uxml**: Drag `HelloWebView.uxml`
   - **Uss**: Drag `HelloWebView.uss`

### Step 4: Configure the Panel Settings

1. In the **UI Document** component, look for **Panel Settings**
2. If it's empty, right-click in the Project window and create one:
   - **Create > UI Toolkit > Panel Settings Asset**
3. Assign the Panel Settings asset to the UI Document
4. Set the following in Panel Settings:
   - **Target Texture**: None (render to screen)
   - **Scale Mode**: Constant Pixel Size or Scale With Screen Size

### Step 5: Run the Sample

1. Press **Play** in the Unity Editor
2. You should see Google's homepage loading in a full-screen WebView
3. You can interact with the page using your mouse!

## Code Walkthrough

### HelloWebView.uxml

This UXML file defines the UI structure:

```xml
<wv:WebViewElement
    name="webview"
    initial-url="https://www.google.com"
    enable-dev-tools="false"
    style="flex-grow: 1;" />
```

**Key attributes**:
- `initial-url`: The URL to load when the WebView is created
- `enable-dev-tools`: Set to `true` to open Chrome DevTools for debugging
- `flex-grow: 1`: Makes the WebView fill the entire screen

### HelloWebViewController.cs

**Lines 31-40: Scene Setup**
```csharp
var root = _uiDocument.rootVisualElement;
root.Clear();
_uxml.CloneTree(root);

if (_uss != null)
{
    root.styleSheets.Add(_uss);
}
```

This is all the code needed! It simply loads the UXML into the UIDocument. The WebViewElement handles everything else automatically.

**What happens automatically**:
1. WebViewElement creates a WebView instance when attached to the panel
2. It loads the URL specified in `initial-url`
3. It resizes the WebView when the window size changes
4. It forwards mouse input to the web page
5. It updates the texture every frame (~30 FPS)

## Key Concepts

### WebViewElement

`WebViewElement` is a UIToolkit VisualElement that manages a WebView instance. It's the modern, recommended way to use WebViewToolkit.

**Auto-creation behavior**:
- Creates the WebView automatically when attached to a panel
- Destroys it automatically when detached
- You don't need to manage the lifecycle manually

### Initial URL

You can change the initial URL in the UXML file. Examples:
- `https://www.google.com` - External website
- `https://unity.com` - Another website
- `file:///C:/path/to/local.html` - Local HTML file (use forward slashes)

### DevTools

Set `enable-dev-tools="true"` in the UXML to open Chrome DevTools when the WebView is created. This is incredibly useful for debugging web content!

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Change the URL

1. Open `HelloWebView.uxml`
2. Change `initial-url` to `https://unity.com`
3. Run the scene - you should now see Unity's website

### Exercise 2: Enable DevTools

1. In `HelloWebView.uxml`, set `enable-dev-tools="true"`
2. Run the scene
3. A Chrome DevTools window should open - use it to inspect the web page!

### Exercise 3: Add Background Color

1. Open `HelloWebView.uss`
2. Change the `background-color` of `#root` to a different color (e.g., `rgb(0, 50, 100)`)
3. Run the scene - you'll see the background color while the page is loading

## Common Issues

### Issue 1: Black Screen

**Problem**: The WebView shows a black screen instead of web content.

**Solutions**:
- Ensure you're on Windows 10/11 (x64)
- Check that DirectX 11 or 12 is selected in Project Settings
- Verify WebView2 Runtime is installed (comes with Windows 11)
- Check the Console for error messages

### Issue 2: "UXML not assigned" Error

**Problem**: Console shows "UIDocument or UXML not assigned!"

**Solution**: Make sure you've assigned both the UXML and USS assets in the HelloWebViewController Inspector.

### Issue 3: Mouse Clicks Don't Work

**Problem**: Can't interact with the web page.

**Solution**: Ensure the UI Document's **Panel Settings** is properly configured with a valid asset.

### Issue 4: WebView Doesn't Fill Screen

**Problem**: WebView is tiny or doesn't resize properly.

**Solution**:
- Check that `flex-grow: 1` is set in the UXML
- Verify Panel Settings **Scale Mode** is set appropriately

## What You Learned

Congratulations! You've successfully created your first WebView in Unity. You now know:

- ✅ How to set up a UIDocument with WebViewElement
- ✅ How to configure the initial URL in UXML
- ✅ That WebViewElement handles creation, resizing, and input automatically
- ✅ How to enable Chrome DevTools for debugging

## Next Steps

Ready for more? Continue your learning journey:

- **Next Sample**: [02 - Navigation](../02_Navigation/README.md) - Learn how to navigate between pages and manage history
- **Explore the Code**: Open `HelloWebViewController.cs` and read the comments to understand what's happening
- **Experiment**: Try loading different websites and enabling DevTools to inspect them

## Additional Resources

- **WebViewElement API**: See package README for complete API documentation
- **UIToolkit Basics**: [Unity UIToolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- **Troubleshooting**: See main Samples README for common issues

---

**Pro Tip**: While this sample shows the UXML approach, you can also create WebViewElements programmatically in C#. We'll explore that in later samples!
