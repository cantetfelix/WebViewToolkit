# Sample 05: Interactive Input

## Learning Objectives

- Understand how WebViewElement automatically forwards mouse and keyboard input
- Learn about the normalized coordinate system [0-1] used by WebView
- Handle mouse position tracking and click events
- Implement scroll wheel interaction
- Capture form submissions from web content
- Use JavaScript postMessage to send interaction data to Unity
- Create responsive web forms that integrate with Unity

## Prerequisites

- Complete Sample 01 (Hello WebView) first
- Complete Sample 03 (JavaScript Bridge) - understanding of C# ↔ JS communication
- Basic understanding of HTML forms
- Familiarity with mouse event handling

## What This Sample Demonstrates

This sample shows how WebViewElement automatically forwards input events (mouse, keyboard, scroll) to the embedded web content, enabling natural interaction with web-based UIs. You'll learn:

- **Automatic Input Forwarding**: WebView automatically handles mouse clicks, movements, and keyboard input
- **Normalized Coordinates**: Understanding the [0-1] coordinate system for cross-platform compatibility
- **Form Interaction**: Users can type, select, and submit forms just like in a regular browser
- **Real-time Feedback**: Track mouse position, clicks, and scrolling from Unity
- **Visual Effects**: See hover effects, click ripples, and other web interactions

## Setup Instructions

1. Import this sample from Package Manager (Window > Package Manager > WebView Toolkit > Samples)
2. Open the `InteractiveInput.unity` scene
3. Press Play in the Unity Editor
4. Move your mouse over the form to see position tracking
5. Click anywhere to see click detection
6. Fill out and submit the form to see data capture
7. Scroll the page to see scroll tracking

## Scene Layout

The sample uses a **two-panel layout**:

- **Left Panel (Status Monitor)**: Real-time display of input events
  - Mouse position (absolute and normalized coordinates)
  - Last click position and time
  - Scroll wheel delta and total
  - Form submission count and last data
  - Instructions for interaction
- **Right Panel (WebView)**: Interactive form with visual feedback
  - Text inputs, dropdowns, checkboxes, textarea
  - Submit button with visual confirmation
  - Mouse follower effect
  - Click ripple animations
  - Scroll indicator

## Code Walkthrough

### Main Controller ([InteractiveInputController.cs](Scripts/InteractiveInputController.cs))

**Lines 66-94: Initialization Pattern**
```csharp
private void Start()
{
    BuildUI();

    // Check if WebView is already attached
    if (_webViewElement != null && _webViewElement.panel != null)
    {
        _webViewElement.schedule.Execute(() => LoadInteractiveForm()).ExecuteLater(100);
    }
    else if (_webViewElement != null)
    {
        _webViewElement.RegisterCallback<AttachToPanelEvent>(OnWebViewAttached);
    }
}
```
Uses the same reliable initialization pattern from Sample 04 to ensure WebView is ready.

**Lines 121-129: Message Event Subscription**
```csharp
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
```
Subscribes to messages from JavaScript for receiving interaction data.

**Lines 149-477: Interactive HTML Generation**
The `GenerateInteractiveFormHTML()` method creates a complete HTML page with:
- Modern, responsive form design
- CSS animations for hover and click effects
- JavaScript for tracking mouse, click, scroll, and form events
- Visual indicators (mouse follower, click ripples, scroll position)
- Complete form with various input types

**Lines 179-202: HTML Form Structure**
```html
<input type='text' id='name' name='name' placeholder='Enter your name' required>
<input type='email' id='email' name='email' placeholder='Enter your email' required>
<input type='number' id='age' name='age' placeholder='Enter your age' min='1' max='120'>
<select id='country' name='country'>
    <option value='USA'>United States</option>
    <!-- More options -->
</select>
```
Standard HTML form elements that work automatically with WebView input forwarding.

**Lines 429-443: JavaScript Mouse Tracking**
```javascript
document.addEventListener('mousemove', (e) => {
    // Visual effect
    mouseFollower.style.left = e.clientX + 'px';
    mouseFollower.style.top = e.clientY + 'px';

    // Send to Unity
    sendToUnity('mouseMove', {
        x: e.clientX,
        y: e.clientY,
        normalizedX: e.clientX / window.innerWidth,
        normalizedY: e.clientY / window.innerHeight
    });
});
```
Captures mouse movements and sends both absolute and normalized coordinates to Unity.

**Lines 446-461: JavaScript Click Tracking**
```javascript
document.addEventListener('click', (e) => {
    // Visual ripple effect
    const ripple = document.createElement('div');
    ripple.className = 'click-ripple';
    ripple.style.left = e.clientX + 'px';
    ripple.style.top = e.clientY + 'px';
    document.body.appendChild(ripple);

    // Send to Unity
    sendToUnity('click', {
        x: e.clientX,
        y: e.clientY,
        normalizedX: e.clientX / window.innerWidth,
        normalizedY: e.clientY / window.innerHeight,
        time: new Date().toLocaleTimeString()
    });
});
```
Tracks clicks with visual feedback and sends data to Unity.

**Lines 473-483: JavaScript Form Submission**
```javascript
document.getElementById('userForm').addEventListener('submit', (e) => {
    e.preventDefault(); // Prevent page reload

    const formData = new FormData(e.target);
    const data = {};

    // Convert FormData to object
    for (const [key, value] of formData.entries()) {
        if (key === 'interests') {
            if (!data.interests) data.interests = [];
            data.interests.push(value);
        } else {
            data[key] = value;
        }
    }

    sendToUnity('formSubmit', data);
});
```
Captures form submission, prevents default browser behavior, and sends data to Unity.

**Lines 495-503: Unity-WebView Communication Bridge**
```javascript
function sendToUnity(type, data) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(JSON.stringify({ type, data }));
    }
}
```
Helper function to send structured messages from JavaScript to Unity.

**Lines 510-541: Message Routing in Unity**
```csharp
private void OnMessageReceived(object sender, string message)
{
    Debug.Log($"[InteractiveInput] Received message: {message}");

    try
    {
        var messageData = JsonUtility.FromJson<WebMessage>(message);

        switch (messageData.type)
        {
            case "mouseMove":
                HandleMouseMove(messageData.data);
                break;
            case "click":
                HandleClick(messageData.data);
                break;
            case "scroll":
                HandleScroll(messageData.data);
                break;
            case "formSubmit":
                HandleFormSubmit(messageData.data);
                break;
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"[InteractiveInput] Error parsing message: {ex.Message}");
    }
}
```
Routes incoming messages to appropriate handlers based on message type.

**Lines 546-559: Mouse Move Handler**
```csharp
private void HandleMouseMove(string dataJson)
{
    var data = JsonUtility.FromJson<MouseData>(dataJson);

    if (_mouseXLabel != null)
        _mouseXLabel.text = $"X: {data.x:F0}px";

    if (_mouseYLabel != null)
        _mouseYLabel.text = $"Y: {data.y:F0}px";

    if (_mouseNormalizedLabel != null)
        _mouseNormalizedLabel.text = $"Normalized: ({data.normalizedX:F3}, {data.normalizedY:F3})";
}
```
Updates Unity UI with real-time mouse position data.

**Lines 604-650: Data Classes for Deserialization**
```csharp
[System.Serializable]
private class WebMessage
{
    public string type;
    public string data;
}

[System.Serializable]
private class MouseData
{
    public float x;
    public float y;
    public float normalizedX;
    public float normalizedY;
}
```
C# classes marked with `[System.Serializable]` for JSON deserialization using `JsonUtility`.

## Key Concepts

### Automatic Input Forwarding

**What it does:**
WebViewElement automatically forwards input events to the embedded web content without any extra code:
- Mouse movements
- Mouse clicks (left, right, middle)
- Scroll wheel events
- Keyboard input (typing, shortcuts)

**How it works:**
When you interact with a WebViewElement in Unity, the input events are automatically translated and sent to the underlying WebView2 control, which then forwards them to the web page as if you were using a regular browser.

**What you need to do:**
Nothing! Just ensure the WebViewElement is receiving input focus (it has hover or is the active element).

### Normalized Coordinate System

**Understanding [0-1] Coordinates:**

WebView uses normalized coordinates where:
- `(0, 0)` = Top-left corner
- `(1, 1)` = Bottom-right corner
- `(0.5, 0.5)` = Center of the WebView

**Why normalized coordinates?**
- Resolution-independent: Works at any screen size
- Platform-independent: Same values on all devices
- Easy to calculate: `normalizedX = pixelX / viewWidth`

**Converting between coordinate systems:**
```csharp
// Pixel to Normalized
float normalizedX = pixelX / webViewWidth;
float normalizedY = pixelY / webViewHeight;

// Normalized to Pixel
float pixelX = normalizedX * webViewWidth;
float pixelY = normalizedY * webViewHeight;
```

### Form Interaction Flow

**Complete interaction flow:**

1. **User fills form** in WebView (automatic input forwarding)
2. **User submits form** (clicks submit button or presses Enter)
3. **JavaScript prevents default** (`e.preventDefault()` to avoid page reload)
4. **JavaScript collects form data** (using FormData API)
5. **JavaScript sends to Unity** (using `window.chrome.webview.postMessage()`)
6. **Unity receives message** (`MessageReceived` event fires)
7. **Unity processes data** (deserialize JSON and handle accordingly)
8. **Unity updates game state** (based on form data)

### JavaScript Event Listeners

**Common event patterns used in this sample:**

```javascript
// Mouse movement
document.addEventListener('mousemove', (e) => {
    console.log(e.clientX, e.clientY);
});

// Click events
document.addEventListener('click', (e) => {
    console.log('Clicked at', e.clientX, e.clientY);
});

// Scroll events
document.addEventListener('scroll', () => {
    console.log('Scrolled to', window.scrollY);
});

// Form submission
form.addEventListener('submit', (e) => {
    e.preventDefault(); // IMPORTANT: Prevents page reload
    const data = new FormData(e.target);
    // Process form data
});
```

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Add Keyboard Tracking
Add a text input monitor that shows the last key pressed:
- Add a label to Unity UI: "Last Key: None"
- Add JavaScript `keydown` event listener
- Send key data to Unity via postMessage
- Display the key name in Unity UI
- Hint: Use `e.key` to get the key name

### Exercise 2: Add Mouse Button Detection
Distinguish between left-click, right-click, and middle-click:
- Modify click handler to detect button type
- Use `e.button` (0=left, 1=middle, 2=right)
- Send button type to Unity
- Display different colors for different button types
- Hint: Add visual indicators for each button type

### Exercise 3: Create a Drawing Canvas
Add an HTML canvas for mouse drawing:
- Add `<canvas>` element to HTML
- Track mouse down/up/move events
- Draw lines as user drags mouse
- Add "Clear Canvas" button
- Send drawing data to Unity as a base64 image
- Hint: Use `canvas.toDataURL()` to get image data

### Exercise 4: Implement Drag and Drop
Create draggable elements in the web page:
- Add draggable cards using HTML5 drag-and-drop API
- Track drag start, drag over, and drop events
- Send drop position to Unity
- Update Unity UI to show last drop location
- Hint: Use `draggable="true"` attribute

### Exercise 5: Add Touch/Multi-Touch Support
Extend to support touch input (for future touch devices):
- Add `touchstart`, `touchmove`, `touchend` listeners
- Track multiple touch points simultaneously
- Send touch data to Unity
- Display touch point count and positions
- Hint: `e.touches` contains all active touch points

### Exercise 6: Implement Copy/Paste
Add copy-paste functionality:
- Add buttons to copy text from web to clipboard
- Use Clipboard API (`navigator.clipboard`)
- Send copied text to Unity
- Allow Unity to push text to web clipboard
- Display last copied text in Unity UI

## Common Issues

### Issue 1: Input Not Working
**Problem**: Mouse clicks or keyboard input not responding in WebView

**Solution**:
- Ensure WebViewElement has input focus
- Check that `enable-input="true"` in UXML (default is true)
- Verify WebView is not obscured by other UI elements
- Check that the web page is fully loaded
- Use DevTools (`enable-dev-tools="true"`) to verify JavaScript is running

### Issue 2: Coordinates Are Wrong
**Problem**: Click positions don't match visual location

**Solution**:
- Remember WebView uses its own coordinate system
- Check if you're using normalized vs pixel coordinates correctly
- Verify WebView size matches your expectations
- Ensure no CSS transforms are affecting positions
- Use `e.clientX/Y` (relative to viewport) not `e.pageX/Y` (relative to document)

### Issue 3: Form Doesn't Submit
**Problem**: Form submit button does nothing

**Solution**:
- Ensure JavaScript is enabled in WebView
- Check browser console for errors (enable DevTools)
- Verify `e.preventDefault()` is called to prevent page reload
- Confirm `window.chrome.webview` exists (only available in WebView2)
- Add debug logs in JavaScript to track execution

### Issue 4: Messages Not Reaching Unity
**Problem**: JavaScript sends messages but Unity doesn't receive them

**Solution**:
- Verify `MessageReceived` event is subscribed
- Check message format is valid JSON
- Ensure `window.chrome.webview` exists before calling `postMessage`
- Add try-catch around message parsing in Unity
- Log received messages to see if they're arriving

### Issue 5: Performance Issues with High-Frequency Events
**Problem**: Mouse move events cause lag or performance drops

**Solution**:
- Throttle or debounce high-frequency events:
```javascript
let lastUpdate = 0;
document.addEventListener('mousemove', (e) => {
    const now = Date.now();
    if (now - lastUpdate > 16) { // Max 60 FPS
        sendToUnity('mouseMove', { x: e.clientX, y: e.clientY });
        lastUpdate = now;
    }
});
```
- Process only essential data
- Avoid sending data on every pixel movement
- Use `requestAnimationFrame` for smoother updates

### Issue 6: Right-Click Context Menu Interfering
**Problem**: Right-click shows browser context menu instead of custom behavior

**Solution**:
```javascript
// Disable default context menu
document.addEventListener('contextmenu', (e) => {
    e.preventDefault();
    // Your custom right-click behavior here
});
```

### Issue 7: Cursor Latency / Input Delay
**Problem**: Noticeable delay between system cursor and WebView cursor response

**Explanation**:
This is inherent to the WebView2 architecture and cannot be completely eliminated:
1. Unity captures the input event
2. Event is forwarded to WebView2 native process
3. WebView2 processes and renders the update
4. Updated frame is copied to Unity texture
5. Unity displays the texture

This multi-step process introduces ~16-50ms of latency depending on system performance.

**Ways to minimize perceived latency**:
- Ensure your application runs at high framerates (60+ FPS)
- Avoid heavy rendering on the main thread
- Use hardware acceleration (WebView2 default)
- Keep web content lightweight and responsive
- Avoid CSS transitions that add additional smoothing delays
- For critical interactions, consider using native Unity UI overlays

**Note**: This is normal behavior for off-screen rendering solutions like WebView2. The trade-off is full web rendering capabilities within Unity at the cost of some input latency.

## Performance Tips

### Optimize Event Handling

**Throttle high-frequency events:**
```javascript
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

// Use throttled handler
document.addEventListener('mousemove', throttle((e) => {
    sendToUnity('mouseMove', { x: e.clientX, y: e.clientY });
}, 16)); // 60 FPS max
```

**Debounce infrequent events:**
```javascript
function debounce(func, delay) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), delay);
    };
}

// Use debounced handler for scroll end detection
document.addEventListener('scroll', debounce(() => {
    sendToUnity('scrollEnd', { scrollY: window.scrollY });
}, 200)); // Wait 200ms after scrolling stops
```

### Minimize Data Transfer

**Send only necessary data:**
```javascript
// ❌ Sending too much data
sendToUnity('mouseMove', {
    x: e.clientX,
    y: e.clientY,
    screenX: e.screenX,
    screenY: e.screenY,
    pageX: e.pageX,
    pageY: e.pageY,
    target: e.target.outerHTML, // Very large!
    timestamp: Date.now()
});

// ✅ Send only what's needed
sendToUnity('mouseMove', {
    x: e.clientX,
    y: e.clientY
});
```

### Use Passive Event Listeners

For scroll and touch events:
```javascript
// Improves scroll performance
document.addEventListener('scroll', handleScroll, { passive: true });
document.addEventListener('touchstart', handleTouch, { passive: true });
```

## Next Steps

Continue your learning journey:

- **Next Sample**: [Sample 06 - Multiple WebViews](../06_MultipleWebViews/README.md)
  - Learn how to manage multiple WebView instances
  - Understand resource management and performance
  - Build multi-panel dashboards

- **Related Concepts**:
  - Sample 03 (JavaScript Bridge) - For JavaScript ↔ C# communication patterns
  - Sample 04 (Dynamic HTML) - For generating interactive content

## Additional Resources

### Web APIs Used
- [MDN - Mouse Events](https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent)
- [MDN - Keyboard Events](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent)
- [MDN - Form Data API](https://developer.mozilla.org/en-US/docs/Web/API/FormData)
- [MDN - Scroll Events](https://developer.mozilla.org/en-US/docs/Web/API/Document/scroll_event)

### Input Handling
- [HTML5 Drag and Drop API](https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API)
- [Touch Events](https://developer.mozilla.org/en-US/docs/Web/API/Touch_events)
- [Pointer Events](https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events)

### Performance Optimization
- [Debouncing and Throttling](https://css-tricks.com/debouncing-throttling-explained-examples/)
- [Passive Event Listeners](https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener#passive)

### WebView API Reference
- See main package documentation for input-related APIs
- Check `WebViewElement` API reference for event handling

## Summary

This sample demonstrated:

✅ Automatic input forwarding from Unity to WebView
✅ Normalized coordinate system for cross-platform compatibility
✅ Real-time mouse position and click tracking
✅ Scroll wheel interaction
✅ Form interaction and data submission
✅ JavaScript event listeners for user input
✅ Visual feedback for user interactions
✅ Performance optimization for high-frequency events

**Key Takeaway**: WebViewElement automatically handles all input forwarding, allowing users to interact with web content naturally. You can monitor these interactions by using JavaScript event listeners and postMessage to send data back to Unity. Understanding normalized coordinates ensures your input handling works across different screen sizes and resolutions.
