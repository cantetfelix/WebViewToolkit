# WebViewToolkit Samples

Welcome to the WebViewToolkit sample collection! These samples are designed to teach you how to use WebViewToolkit in a progressive, hands-on way.

## Learning Path

We recommend completing the samples in order:

1. **Hello WebView** - Get started with the basics
2. **Navigation** - Learn URL navigation and history
3. **JavaScript Bridge** - Master two-way communication
4. **Dynamic HTML** - Generate web content from C#
5. **Interactive Input** - Handle mouse and keyboard events
6. **Multiple WebViews** - Manage multiple instances
7. **React Integration** - Load modern web frameworks (Advanced)
8. **Production Example** - See it all come together

## Quick Start

1. Open Package Manager (**Window > Package Manager**)
2. Select **"WebView Toolkit"** package
3. Click **"Samples"** tab
4. Import a sample
5. Open the scene and press **Play**!

## Requirements

- **Platform**: Windows 10/11 (x64 only)
- **Unity Version**: 6000.3 or later
- **WebView2 Runtime**: Microsoft Edge WebView2 Runtime (pre-installed on modern Windows)
- **Graphics API**: DirectX 11 or DirectX 12

## Sample Descriptions

### 01 - Hello WebView
**Difficulty**: Beginner
**Learning Time**: 5 minutes

Your first WebView! This sample shows the absolute minimum code needed to display a web page in Unity using UIToolkit. Perfect for understanding the basic setup.

**What you'll learn**:
- Creating a WebViewElement in UXML
- Basic scene setup with UIDocument
- Initial URL configuration

---

### 02 - Navigation
**Difficulty**: Beginner
**Learning Time**: 15 minutes

Learn how to navigate between pages, manage history, and respond to navigation events.

**What you'll learn**:
- Programmatic navigation with `Navigate()`
- Back/Forward history with `GoBack()`/`GoForward()`
- Checking navigation state with `CanGoBack()`/`CanGoForward()`
- Handling `NavigationCompleted` events
- Enabling/disabling UI based on state

---

### 03 - JavaScript Bridge
**Difficulty**: Intermediate
**Learning Time**: 30 minutes

Master two-way communication between C# and JavaScript. This is essential for building interactive web-based UIs in your Unity projects.

**What you'll learn**:
- Calling JavaScript from C# with `ExecuteScript()`
- Receiving messages from JavaScript via `window.chrome.webview.postMessage()`
- JSON serialization for structured data
- Building game UIs that respond to player actions
- Event-driven communication patterns

---

### 04 - Dynamic HTML
**Difficulty**: Intermediate
**Learning Time**: 30 minutes

Generate web content dynamically from C# data. Perfect for inventory systems, quest logs, leaderboards, and custom game UIs.

**What you'll learn**:
- Loading HTML strings with `NavigateToString()`
- Building HTML programmatically with C# templates
- Real-time content updates
- Creating data-driven UIs (inventory, quest log, settings)
- HTML templating best practices

---

### 05 - Interactive Input
**Difficulty**: Intermediate
**Learning Time**: 20 minutes

Understand how WebView handles mouse and keyboard input, and how to interact with web forms.

**What you'll learn**:
- How WebViewElement forwards mouse events automatically
- Understanding normalized [0-1] coordinate system
- Interacting with web forms (inputs, buttons, checkboxes)
- Mouse hover effects and wheel scrolling
- Debugging input coordinate mapping

---

### 06 - Multiple WebViews
**Difficulty**: Intermediate
**Learning Time**: 25 minutes

Learn to manage multiple WebView instances simultaneously for dashboards, split-screen browsers, or multi-monitor setups.

**What you'll learn**:
- Creating multiple WebViewElements in one scene
- Proper lifecycle management (creation and disposal)
- Independent navigation for each instance
- Performance considerations and optimization
- Memory management best practices

---

### 07 - React Integration (Advanced)
**Difficulty**: Advanced
**Learning Time**: 45-60 minutes

Load and interact with a modern React single-page application. This showcases how to integrate modern web development workflows with Unity.

**What you'll learn**:
- Loading bundled React apps from StreamingAssets
- Communication between React and Unity via `postMessage`
- Handling React Router navigation from Unity
- Building React components that interact with Unity
- Development workflow: npm → build → Unity integration

**Prerequisites**: Basic understanding of React and Node.js

---

### 08 - Production Example
**Difficulty**: Advanced
**Learning Time**: 45-60 minutes

A complete, production-ready documentation viewer that combines all the concepts you've learned. This is a fully-functional feature you could ship in your game.

**What you'll learn**:
- Building a complete feature from scratch
- Professional error handling and loading states
- Combining JavaScript bridge, dynamic HTML, and navigation
- Persistent data with PlayerPrefs (bookmarks)
- Theme management and responsive design
- Search functionality and code syntax highlighting

## Getting Help

### Documentation
- **Package README**: See the main package README for API documentation
- **Sample READMEs**: Each sample includes a detailed README with code walkthroughs
- **Code Comments**: All sample code is thoroughly commented

### Troubleshooting
- Check that WebView2 Runtime is installed (bundled with Windows 11)
- Ensure you're using DirectX 11 or 12 (not OpenGL)
- Verify Unity version is 6000.3 or later
- See individual sample READMEs for specific issues

### Support
- **GitHub Issues**: [Report bugs or request features](https://github.com/cantetfelix/WebViewToolkit/issues)
- **Discussions**: Ask questions in GitHub Discussions

## Best Practices

As you work through the samples, keep these best practices in mind:

1. **Always check IsReady** before calling WebView methods
2. **Handle NavigationCompleted events** to know when pages finish loading
3. **Use normalized coordinates [0-1]** for resolution-independent input
4. **Dispose WebViews properly** to avoid memory leaks
5. **Test in builds**, not just the Editor - behavior can differ slightly
6. **Use JSON** for structured data in JavaScript communication

## What's Next?

After completing the samples:

1. **Read the main package README** for complete API documentation
2. **Explore the legacy demo** in `Runtime/UIToolkit/Demo/` for additional patterns
3. **Build your own features** - adapt sample code for your projects
4. **Share your creations** - we'd love to see what you build!

## License

MIT License - See LICENSE.md in package root

---

**Happy coding!** 🚀
