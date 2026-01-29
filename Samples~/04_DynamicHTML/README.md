# Sample 04: Dynamic HTML

## Learning Objectives

- Generate HTML content dynamically from C# data structures
- Use `NavigateToString()` to load HTML without external files
- Build interactive game UIs using web technologies
- Update web content in real-time based on game state changes
- Create reusable HTML template methods

## Prerequisites

- Complete Sample 01 (Hello WebView) first
- Complete Sample 02 (Navigation) - understanding of navigation methods
- Basic understanding of C# string manipulation
- Familiarity with HTML/CSS concepts (helpful but not required)

## What This Sample Demonstrates

This sample shows how to generate and display HTML content dynamically from your C# game data. Instead of loading external web pages, you'll build HTML strings programmatically and display them in the WebView. This technique is perfect for:

- **Game Inventories**: Display items with icons, descriptions, and rarity
- **Quest Logs**: Show quest progress with dynamic updates
- **Settings Menus**: Create polished settings UIs using web styling
- **Leaderboards**: Generate sortable, filterable player rankings
- **Any dynamic content**: Transform game data into beautiful web UIs

## Setup Instructions

1. Import this sample from Package Manager (Window > Package Manager > WebView Toolkit > Samples)
2. Open the `DynamicHTML.unity` scene
3. Press Play in the Unity Editor
4. Use the view buttons to switch between Inventory, Quest Log, and Settings
5. Use the action buttons to modify data and see real-time updates

## Scene Layout

The sample uses a **two-panel layout**:

- **Left Panel (Control Panel)**: View selection and action buttons
- **Right Panel (WebView)**: Displays the dynamically generated HTML content

## Code Walkthrough

### Data Structures ([InventoryData.cs](Scripts/InventoryData.cs))

**Lines 18-31: InventoryItem Class**
```csharp
[Serializable]
public class InventoryItem
{
    public string name;
    public string description;
    public int quantity;
    public string rarity; // Common, Uncommon, Rare, Epic, Legendary
    public string iconUrl;
}
```
Simple data class representing an inventory item. These fields will be used to generate HTML.

**Lines 40-54: Quest Class**
```csharp
[Serializable]
public class Quest
{
    public string title;
    public string description;
    public string status; // Active, Completed, Failed
    public int progress;
    public int maxProgress;
    public string reward;
}
```
Represents a quest with progress tracking.

**Lines 77-140: GameData Class**
```csharp
public class GameData
{
    public List<InventoryItem> Inventory { get; private set; }
    public List<Quest> Quests { get; private set; }
    public GameSettings Settings { get; private set; }

    private void InitializeSampleData()
    {
        // Creates sample inventory items, quests, and settings
    }
}
```
Container for all game data with sample initialization.

### HTML Generation ([HTMLTemplates.cs](Scripts/HTMLTemplates.cs))

**Lines 24-83: HTML Structure with Embedded CSS**
```csharp
public static string GenerateInventoryHTML(List<InventoryItem> items)
{
    var sb = new StringBuilder();

    sb.Append(@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            /* Modern CSS styling */
        </style>
    </head>
    <body>
        <div class='header'>
            <h1>🎒 Inventory</h1>
        </div>
    ");
    // ... continue building HTML
}
```
Uses C# string interpolation and StringBuilder for efficient HTML generation. Embeds CSS directly in the HTML for a self-contained document.

**Lines 116-136: Dynamic Content Generation**
```csharp
foreach (var item in items)
{
    string rarityClass = $"rarity-{item.rarity.ToLower()}";
    string icon = string.IsNullOrEmpty(item.iconUrl) ? "📦" : item.iconUrl;

    sb.Append($@"
        <div class='item-card'>
            <div class='item-icon'>{icon}</div>
            <div class='item-name'>{item.name}</div>
            <div class='item-quantity'>x{item.quantity}</div>
            <div class='item-description'>{item.description}</div>
            <span class='item-rarity {rarityClass}'>{item.rarity}</span>
        </div>
    ");
}
```
Loops through items and generates HTML cards with dynamic styling based on rarity.

**Key Techniques:**
- **StringBuilder**: Efficient string concatenation for large HTML
- **String Interpolation**: `$"{variable}"` for dynamic values
- **Verbatim Strings**: `@"..."` for multi-line HTML blocks
- **CSS Classes**: Dynamic class names based on data properties

### Main Controller ([DynamicHTMLController.cs](Scripts/DynamicHTMLController.cs))

**Lines 79-86: Initialization**
```csharp
private void Awake()
{
    _uiDocument = GetComponent<UIDocument>();
    _gameData = new GameData(); // Initialize with sample data
}

private void Start()
{
    BuildUI();
    BindEvents();
    ShowInventoryView(); // Load initial view
}
```
Follows the recommended lifecycle pattern: Awake for setup, Start for UI initialization.

**Lines 238-245: Loading HTML into WebView**
```csharp
private void ShowInventoryView()
{
    _currentView = "inventory";
    UpdateActionButtons();

    string html = HTMLTemplates.GenerateInventoryHTML(_gameData.Inventory);
    _webViewElement?.NavigateToString(html);

    Debug.Log("[DynamicHTML] Loaded Inventory view");
}
```
The key method! Generates HTML from current data and loads it using `NavigateToString()`.

**Lines 322-330: Real-Time Updates**
```csharp
private void OnAddItem()
{
    _gameData.AddRandomItem();      // Modify data
    ShowInventoryView();            // Regenerate and reload HTML
    Debug.Log("[DynamicHTML] Added random item to inventory");
}
```
Updates data and immediately refreshes the view to show changes.

## Key Concepts

### NavigateToString() Method

The core feature of this sample:

```csharp
string html = "<html><body><h1>Hello World</h1></body></html>";
_webViewElement.NavigateToString(html);
```

**What it does:**
- Loads HTML content directly from a C# string
- No external files needed
- Perfect for dynamic, generated content
- Supports full HTML, CSS, and JavaScript

**Use cases:**
- Game UIs generated from runtime data
- Reports and statistics
- Dynamic forms
- Data visualizations

### HTML Template Patterns

**Pattern 1: Static Structure + Dynamic Content**
```csharp
sb.Append("<div class='container'>");  // Static
foreach (var item in items)            // Dynamic loop
{
    sb.Append($"<div>{item.name}</div>");  // Dynamic content
}
sb.Append("</div>");                   // Static
```

**Pattern 2: Conditional Rendering**
```csharp
if (items.Count == 0)
{
    sb.Append("<div class='empty-state'>No items</div>");
}
else
{
    // Render items
}
```

**Pattern 3: Dynamic Styling**
```csharp
string colorClass = item.rarity == "Legendary" ? "gold" : "silver";
sb.Append($"<div class='{colorClass}'>{item.name}</div>");
```

### StringBuilder Performance

For large HTML generation:

```csharp
// ❌ Slow - creates new string each time
string html = "";
foreach (var item in items)
{
    html += "<div>" + item.name + "</div>";  // String concatenation
}

// ✅ Fast - efficient append operations
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.Append($"<div>{item.name}</div>");
}
string html = sb.ToString();
```

**Why StringBuilder?**
- Strings are immutable in C#
- Each `+=` creates a new string object (slow)
- StringBuilder maintains a mutable buffer (fast)
- Critical for loops with many iterations

## Exercises

Try these challenges to deepen your understanding:

### Exercise 1: Add Filtering
Add a search/filter feature to the inventory:
- Add a search TextField in the Unity UI
- Filter items by name when searching
- Regenerate HTML with filtered results
- Hint: Use `items.FindAll(i => i.name.Contains(searchTerm))`

### Exercise 2: Add Animation
Enhance the HTML with CSS animations:
- Add fade-in animation when items appear
- Add hover effects on cards
- Add progress bar animations for quests
- Hint: Use CSS `@keyframes` and `transition`

### Exercise 3: Create a New View
Build a "Statistics" view:
- Create a `PlayerStats` data class
- Generate HTML showing level, XP, achievements
- Add buttons to modify stats
- Use charts or progress bars for visual appeal

### Exercise 4: Implement Sorting Options
Add multiple sort options for inventory:
- Sort by name (A-Z)
- Sort by quantity (high to low)
- Sort by rarity (legendary to common)
- Add dropdown or buttons to select sort method

### Exercise 5: Persist Data
Save inventory changes between play sessions:
- Use `JsonUtility.ToJson()` to serialize data
- Save to `PlayerPrefs` or a file
- Load saved data on startup
- Hint: See Sample 03 (JavaScript Bridge) for JSON examples

## Common Issues

### Issue 1: HTML Not Displaying
**Problem**: WebView shows blank after calling `NavigateToString()`

**Solution**:
- Check that HTML string is not null or empty
- Verify HTML is valid (use online HTML validator)
- Add Debug.Log to confirm NavigateToString() is called
- Check console for WebView errors

### Issue 2: Styling Not Applied
**Problem**: HTML displays but looks unstyled

**Solution**:
- Ensure CSS is embedded in `<style>` tags within HTML
- Check for CSS syntax errors (missing semicolons, braces)
- Use browser DevTools to inspect generated HTML (if enable-dev-tools is true)
- Verify class names match between HTML and CSS

### Issue 3: Special Characters Break HTML
**Problem**: Item names with quotes or special characters cause errors

**Solution**:
- Escape HTML special characters:
```csharp
string EscapeHtml(string text)
{
    return text
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&#39;");
}
```

### Issue 4: Updates Not Showing
**Problem**: Data changes but view doesn't update

**Solution**:
- Ensure you call the view refresh method (e.g., `ShowInventoryView()`) after modifying data
- Verify `NavigateToString()` is being called with new HTML
- Check that WebView reference is valid

### Issue 5: Performance Issues with Large Lists
**Problem**: Slow HTML generation with thousands of items

**Solution**:
- Implement pagination (show 50 items per page)
- Use virtual scrolling (only render visible items)
- Cache generated HTML if data doesn't change often
- Consider moving to external HTML file with JavaScript for very large datasets

## Performance Tips

### Optimize HTML Generation

**Cache static parts:**
```csharp
// Cache CSS and header HTML (doesn't change)
private static readonly string _headerHtml = GenerateHeaderHtml();

public static string GenerateInventoryHTML(List<InventoryItem> items)
{
    var sb = new StringBuilder();
    sb.Append(_headerHtml);  // Reuse cached header
    // ... generate dynamic content
}
```

**Use capacity hint:**
```csharp
// Estimate final size to avoid resizing
var sb = new StringBuilder(capacity: items.Count * 200);
```

**Batch updates:**
```csharp
// Instead of regenerating HTML on every item change
private bool _needsRefresh = false;

private void OnAddItem()
{
    _gameData.AddRandomItem();
    _needsRefresh = true;  // Mark for refresh
}

private void LateUpdate()
{
    if (_needsRefresh)
    {
        ShowInventoryView();  // Refresh once per frame
        _needsRefresh = false;
    }
}
```

## Next Steps

Continue your learning journey:

- **Next Sample**: [Sample 05 - Interactive Input](../05_InteractiveInput/README.md)
  - Learn how to handle mouse and keyboard input
  - Understand normalized coordinate systems
  - Build interactive forms and controls

- **Related Concepts**:
  - Sample 03 (JavaScript Bridge) - For JavaScript ↔ C# communication
  - Sample 06 (Multiple WebViews) - Managing multiple HTML views

## Additional Resources

### HTML/CSS Learning
- [MDN Web Docs - HTML](https://developer.mozilla.org/en-US/docs/Web/HTML)
- [MDN Web Docs - CSS](https://developer.mozilla.org/en-US/docs/Web/CSS)
- [CSS Tricks](https://css-tricks.com/)

### C# String Handling
- [Microsoft Docs - StringBuilder](https://docs.microsoft.com/en-us/dotnet/api/system.text.stringbuilder)
- [String Interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)

### WebView API Reference
- See main package documentation for `NavigateToString()` details
- Check `WebViewElement` API reference for other navigation methods

## Summary

This sample demonstrated:

✅ Generating HTML dynamically from C# data structures
✅ Using `NavigateToString()` to load generated content
✅ Building reusable HTML template methods
✅ Real-time updates when data changes
✅ Creating game UIs with web technologies
✅ Efficient string building with StringBuilder

**Key Takeaway**: You can build beautiful, dynamic game UIs using familiar web technologies (HTML/CSS) while keeping all your game logic in C#. This approach combines the best of both worlds: C#'s type safety and performance with the web's flexible styling and layout capabilities.
