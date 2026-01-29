# Sample 03: JavaScript Bridge

**Difficulty**: Intermediate
**Est. Time**: 30 minutes
**Prerequisites**: Complete [Sample 02: Navigation](../02_Navigation/README.md)

## Learning Objectives

By completing this sample, you will learn:

- How to send data from C# to JavaScript using `ExecuteScript()`
- How to receive messages from JavaScript using `window.chrome.webview.postMessage()`
- How to serialize C# objects to JSON for web consumption
- How to parse JSON messages from JavaScript in C#
- How to build interactive web UIs that control Unity game logic
- Best practices for two-way communication patterns

## What This Sample Demonstrates

This sample shows a **real-time game dashboard** where:

- **Unity controls** simulate game events (score changes, damage, level ups)
- **Web dashboard** displays game state with live updates and visual feedback
- **C# → JavaScript**: Unity sends game state updates to the web UI
- **JavaScript → C#**: Web buttons trigger Unity game actions

**Key Feature**: This demonstrates the core pattern for building web-based game UIs, settings panels, inventory systems, and more!

## Setup Instructions

### Step 1: Import the Sample

1. Open **Package Manager** (**Window > Package Manager**)
2. Select **"WebView Toolkit"**
3. Click the **"Samples"** tab
4. Find **"03 - JavaScript Bridge"** and click **"Import"**

### Step 2: Create the Scene

1. Create a new Unity scene
2. Create an empty GameObject named **"JavaScriptBridgeUI"**
3. Add **UI Document** component
4. Add **JavaScriptBridgeController** component
5. Assign the UXML and USS assets in the Inspector

### Step 3: Run the Sample

1. Press **Play**
2. The web dashboard loads automatically with the initial game state
3. Try these interactions:

**Unity → Web**:
- Click "Add Score (+10)" in Unity
- Click "Update Web Dashboard" button
- Watch the score update in the web UI

**Web → Unity**:
- Click "Jump" button in the web dashboard
- See the message appear in Unity's message log

**Two-way flow**:
- Modify game state in Unity (damage, level up)
- Click "Update Web Dashboard"
- Click web buttons to trigger Unity actions
- Observe the complete cycle of communication!

## Code Walkthrough

This sample has three main parts: the data model, the C# controller, and the HTML/JavaScript dashboard.

### Part 1: GameState.cs - The Data Model

**Lines 17-42: Data Class with Validation**
```csharp
[Serializable]
public class GameState
{
    [SerializeField] private int score;
    [SerializeField] private int health;
    [SerializeField] private int level;

    public int Score
    {
        get => score;
        set => score = Mathf.Max(0, value); // Prevent negative
    }

    public int Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, 100); // Keep 0-100
    }

    public int Level
    {
        get => level;
        set => level = Mathf.Max(1, value); // Minimum level 1
    }
}
```

**Key Points**:
- `[Serializable]` attribute enables JSON conversion
- `[SerializeField]` makes fields visible to Unity's JSON serializer
- Properties add validation (no negative scores, health stays 0-100, etc.)

**Lines 64-71: JSON Serialization**
```csharp
public string ToJson()
{
    return JsonUtility.ToJson(this);
}

public static GameState FromJson(string json)
{
    return JsonUtility.FromJson<GameState>(json);
}
```

Unity's `JsonUtility` handles serialization/deserialization automatically!

### Part 2: JavaScriptBridgeController.cs - The Bridge

**Lines 227-245: C# → JavaScript (Sending Data)**
```csharp
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
    string script = $"if (typeof updateGameState === 'function') {{ updateGameState({json}); }}";

    _webViewElement.ExecuteScript(script);

    LogMessage($"📤 Sent to web: {json}");
}
```

**How it works**:
1. Check if WebView is ready
2. Convert GameState to JSON string
3. Build JavaScript code that calls `updateGameState(json)`
4. Execute the script in the WebView
5. The `updateGameState()` function must exist in the HTML (we define it later)

**Lines 253-289: JavaScript → C# (Receiving Messages)**
```csharp
private void OnMessageReceived(string message)
{
    LogMessage($"📥 Received from web: {message}");

    try
    {
        // Parse JSON message
        var messageData = JsonUtility.FromJson<WebMessage>(message);

        // Handle different actions
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
```

**How it works**:
1. Subscribe to `MessageReceived` event on WebViewElement
2. When JavaScript calls `window.chrome.webview.postMessage()`, this fires
3. Parse the JSON message to extract the action type
4. Route to appropriate handler based on action
5. Handle errors gracefully

**Lines 291-303: Message Format**
```csharp
[Serializable]
private class WebMessage
{
    public string action;
    public string data;
}
```

All messages from JavaScript should follow this format:
```json
{
  "action": "jump",
  "data": null
}
```

### Part 3: HTML Dashboard - The Web UI

**Lines 367-809: Generated HTML**

The HTML is generated dynamically and loaded via `NavigateToString()`. Let's look at the key JavaScript parts:

**JavaScript → Unity (Sending Messages)**
```javascript
function sendToUnity(action, data = null) {
    const message = {
        action: action,
        data: data
    };

    // WebView2 API for sending messages to C#
    window.chrome.webview.postMessage(JSON.stringify(message));
    console.log('Sent to Unity:', message);
}

function sendJump() {
    sendToUnity('jump');
}

function sendShoot() {
    sendToUnity('shoot');
}
```

**How it works**:
1. Create a message object with action and data
2. Convert to JSON string
3. Call `window.chrome.webview.postMessage()` - this is the WebView2 API
4. Unity's `MessageReceived` event fires with this string

**Unity → JavaScript (Receiving Updates)**
```javascript
function updateGameState(state) {
    console.log('Received game state from Unity:', state);

    // Update the UI
    document.getElementById('score').textContent = state.score;
    document.getElementById('health').textContent = state.health;
    document.getElementById('level').textContent = state.level;

    // Update health bar
    const healthBar = document.getElementById('health-bar');
    healthBar.style.width = state.health + '%';

    // Change color based on health
    if (state.health > 66) {
        healthBar.style.background = 'linear-gradient(90deg, #4CAF50, #8BC34A)';
    } else if (state.health > 33) {
        healthBar.style.background = 'linear-gradient(90deg, #FF9800, #FFC107)';
    } else {
        healthBar.style.background = 'linear-gradient(90deg, #F44336, #E91E63)';
    }
}
```

**How it works**:
1. Unity calls `ExecuteScript("updateGameState({...})")`
2. This function receives the JSON object (already parsed!)
3. Update DOM elements with new values
4. Add visual feedback (health bar color changes)

## Key Concepts

### The JavaScript Bridge Pattern

The complete communication cycle:

```
┌─────────────┐                    ┌─────────────┐
│   Unity C#  │                    │  JavaScript │
│             │                    │   (WebView) │
└─────────────┘                    └─────────────┘
       │                                  │
       │  1. ExecuteScript()              │
       │  "updateGameState({score:10})"   │
       │─────────────────────────────────>│
       │                                  │
       │                                  │  2. Update UI
       │                                  │     with new data
       │                                  │
       │  3. postMessage()                │
       │     "{action:'jump'}"            │
       │<─────────────────────────────────│
       │                                  │
       │  4. MessageReceived event        │
       │     triggers handler             │
       │                                  │
```

### Message Format Best Practices

**From JavaScript to C#**:
```json
{
  "action": "actionName",
  "data": { ... optional payload ... }
}
```

**From C# to JavaScript**:
- Call a function: `ExecuteScript("functionName({...json...})")`
- The function must exist in the loaded HTML
- JSON is automatically parsed in JavaScript

### Error Handling

Always wrap message parsing in try-catch:

```csharp
try
{
    var data = JsonUtility.FromJson<MessageType>(message);
    // Process data
}
catch (Exception ex)
{
    Debug.LogError($"Failed to parse: {ex.Message}");
}
```

Always check WebView is ready before sending:

```csharp
if (!_webViewElement.IsReady)
{
    Debug.LogWarning("WebView not ready!");
    return;
}
```

### JSON Serialization Tips

**What works with JsonUtility**:
- ✅ Simple types (int, float, string, bool)
- ✅ Classes marked `[Serializable]`
- ✅ Arrays and Lists
- ✅ Nested objects

**What doesn't work**:
- ❌ Dictionary (use arrays of key-value pairs instead)
- ❌ Properties without backing fields (unless `[SerializeField]`)
- ❌ Circular references

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Add an Ammo Counter

1. Add an `ammo` field to `GameState`
2. Update the UI to display ammo
3. When "Shoot" is clicked in web UI, decrease ammo in Unity
4. Disable the shoot button when ammo reaches 0

### Exercise 2: Two-Way Color Sync

1. Add a color picker in the HTML dashboard
2. When the color changes, send it to Unity
3. Update Unity UI background color based on web selection
4. Make it work both ways - Unity can also send color updates to web

### Exercise 3: Inventory System

1. Create an `InventoryItem` class (name, quantity, icon)
2. Add a list of items to `GameState`
3. Display items in a grid in the web UI
4. Allow clicking items in web UI to "use" them in Unity
5. Send inventory updates back to web when items are used

### Exercise 4: Real-Time Combat Log

1. Create a combat log system
2. When actions happen in Unity (damage, kills, etc.), send events to web
3. Display them in a scrolling log with timestamps
4. Add color coding (damage = red, healing = green, etc.)

## Common Issues

### Issue 1: "WebView is not ready" Error

**Problem**: Trying to send messages before WebView finishes loading.

**Solution**: Always check `IsReady` before calling `ExecuteScript()`, or wait for the `NavigationCompleted` event.

### Issue 2: JavaScript Function Not Found

**Problem**: `ExecuteScript()` runs but nothing happens.

**Solution**:
- Open DevTools (set `enable-dev-tools="true"` in UXML)
- Check console for errors
- Verify the function exists: `if (typeof functionName === 'function')`

### Issue 3: JSON Parse Errors in C#

**Problem**: `JsonUtility.FromJson()` throws exception.

**Solution**:
- Verify JavaScript sends valid JSON
- Check that class fields are `[SerializeField]` and class is `[Serializable]`
- Test JSON format: Copy message from logs and validate at jsonlint.com

### Issue 4: Messages Not Received from JavaScript

**Problem**: Clicking web buttons does nothing in Unity.

**Solution**:
- Verify you're subscribed to `MessageReceived` event
- Check that `window.chrome.webview.postMessage()` is defined (it won't exist in regular browsers, only WebView2)
- Look for JavaScript errors in DevTools

### Issue 5: Health Bar Not Updating

**Problem**: Numbers update but health bar stays the same.

**Solution**: Check browser console for JavaScript errors in the `updateGameState()` function. Make sure element IDs match.

## What You Learned

Congratulations! You've mastered JavaScript bridging in WebViewToolkit. You now know:

- ✅ How to send data from C# to JavaScript with `ExecuteScript()`
- ✅ How to receive messages from JavaScript with `MessageReceived`
- ✅ How to serialize C# objects to JSON
- ✅ How to structure messages for reliable communication
- ✅ How to build interactive web UIs that control Unity
- ✅ Error handling for asynchronous communication
- ✅ Best practices for two-way messaging patterns

## Next Steps

Ready to continue? Move on to:

- **Next Sample**: [04 - Dynamic HTML](../04_DynamicHTML/README.md) - Generate web content dynamically from C# data
- **Challenge**: Build a live multiplayer scoreboard that updates in real-time
- **Advanced**: Create a web-based inventory system with drag-and-drop

## Additional Resources

- **WebViewElement API**: See package README for `ExecuteScript()` and `MessageReceived` documentation
- **JsonUtility**: [Unity JSON Serialization](https://docs.unity3d.com/ScriptReference/JsonUtility.html)
- **WebView2 JavaScript API**: [Microsoft WebView2 Documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/javascript/)

---

**Pro Tip**: For complex data structures, consider using [Newtonsoft.Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@latest) instead of JsonUtility for better flexibility (supports Dictionaries, properties, etc.)!
