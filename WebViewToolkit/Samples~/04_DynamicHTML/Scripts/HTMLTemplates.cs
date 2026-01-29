// ============================================================================
// WebViewToolkit - Dynamic HTML Sample - HTML Template Generator
// ============================================================================
// Static class containing methods to generate HTML from game data.
// Demonstrates building HTML strings programmatically for dynamic content.
// ============================================================================

using System.Collections.Generic;
using System.Text;

namespace WebViewToolkit.Samples.DynamicHTML
{
    /// <summary>
    /// Generates HTML content from game data
    /// </summary>
    public static class HTMLTemplates
    {
        // ====================================================================
        // Inventory Templates
        // ====================================================================

        /// <summary>
        /// Generates a complete HTML page displaying inventory items
        /// </summary>
        public static string GenerateInventoryHTML(List<InventoryItem> items)
        {
            var sb = new StringBuilder();

            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Inventory</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #1e1e1e 0%, #2d2d2d 100%);
            color: #e0e0e0;
            padding: 20px;
            min-height: 100vh;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #444;
        }

        h1 {
            font-size: 32px;
            color: #4a9eff;
            margin-bottom: 10px;
        }

        .subtitle {
            color: #999;
            font-size: 14px;
        }

        .inventory-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
            gap: 16px;
            max-width: 1200px;
            margin: 0 auto;
        }

        .item-card {
            background: #2a2a2a;
            border-radius: 12px;
            padding: 16px;
            border: 2px solid #3a3a3a;
            transition: all 0.3s ease;
            cursor: pointer;
        }

        .item-card:hover {
            transform: translateY(-4px);
            border-color: #4a9eff;
            box-shadow: 0 8px 16px rgba(74, 158, 255, 0.2);
        }

        .item-header {
            display: flex;
            align-items: center;
            margin-bottom: 12px;
        }

        .item-icon {
            font-size: 32px;
            margin-right: 12px;
        }

        .item-info {
            flex: 1;
        }

        .item-name {
            font-size: 16px;
            font-weight: bold;
            color: #fff;
            margin-bottom: 4px;
        }

        .item-quantity {
            font-size: 12px;
            color: #999;
        }

        .item-description {
            font-size: 13px;
            color: #bbb;
            margin-bottom: 12px;
            line-height: 1.4;
        }

        .item-rarity {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: bold;
            text-transform: uppercase;
        }

        .rarity-common { background: #6b7280; color: #fff; }
        .rarity-uncommon { background: #10b981; color: #fff; }
        .rarity-rare { background: #3b82f6; color: #fff; }
        .rarity-epic { background: #8b5cf6; color: #fff; }
        .rarity-legendary { background: #f59e0b; color: #fff; }

        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #666;
        }

        .empty-state-icon {
            font-size: 64px;
            margin-bottom: 16px;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎒 Inventory</h1>
        <div class='subtitle'>Total Items: " + items.Count + @"</div>
    </div>
");

            if (items.Count == 0)
            {
                sb.Append(@"
    <div class='empty-state'>
        <div class='empty-state-icon'>📦</div>
        <h2>Your inventory is empty</h2>
        <p>Collect items during your adventure!</p>
    </div>
");
            }
            else
            {
                sb.Append("    <div class='inventory-grid'>\n");

                foreach (var item in items)
                {
                    string rarityClass = $"rarity-{item.rarity.ToLower()}";
                    string icon = string.IsNullOrEmpty(item.iconUrl) ? "📦" : item.iconUrl;

                    sb.Append($@"
        <div class='item-card'>
            <div class='item-header'>
                <div class='item-icon'>{icon}</div>
                <div class='item-info'>
                    <div class='item-name'>{item.name}</div>
                    <div class='item-quantity'>x{item.quantity}</div>
                </div>
            </div>
            <div class='item-description'>{item.description}</div>
            <span class='item-rarity {rarityClass}'>{item.rarity}</span>
        </div>
");
                }

                sb.Append("    </div>\n");
            }

            sb.Append(@"
</body>
</html>
");

            return sb.ToString();
        }

        // ====================================================================
        // Quest Log Templates
        // ====================================================================

        /// <summary>
        /// Generates a complete HTML page displaying quests
        /// </summary>
        public static string GenerateQuestLogHTML(List<Quest> quests)
        {
            var sb = new StringBuilder();

            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Quest Log</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            color: #e0e0e0;
            padding: 20px;
            min-height: 100vh;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #444;
        }

        h1 {
            font-size: 32px;
            color: #ffd700;
            margin-bottom: 10px;
        }

        .subtitle {
            color: #999;
            font-size: 14px;
        }

        .quest-list {
            max-width: 800px;
            margin: 0 auto;
        }

        .quest-card {
            background: #1e2839;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 16px;
            border-left: 4px solid #444;
            transition: all 0.3s ease;
        }

        .quest-card:hover {
            transform: translateX(4px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        }

        .quest-card.active { border-left-color: #3b82f6; }
        .quest-card.completed { border-left-color: #10b981; }
        .quest-card.failed { border-left-color: #ef4444; }

        .quest-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
        }

        .quest-title {
            font-size: 18px;
            font-weight: bold;
            color: #fff;
        }

        .quest-status {
            padding: 4px 12px;
            border-radius: 4px;
            font-size: 11px;
            font-weight: bold;
            text-transform: uppercase;
        }

        .status-active { background: #3b82f6; color: #fff; }
        .status-completed { background: #10b981; color: #fff; }
        .status-failed { background: #ef4444; color: #fff; }

        .quest-description {
            color: #bbb;
            margin-bottom: 16px;
            line-height: 1.6;
        }

        .quest-progress {
            margin-bottom: 12px;
        }

        .progress-label {
            font-size: 12px;
            color: #999;
            margin-bottom: 6px;
        }

        .progress-bar {
            height: 8px;
            background: #2a2a2a;
            border-radius: 4px;
            overflow: hidden;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #3b82f6 0%, #60a5fa 100%);
            transition: width 0.3s ease;
        }

        .quest-reward {
            display: flex;
            align-items: center;
            padding: 12px;
            background: #2a2a2a;
            border-radius: 6px;
            margin-top: 12px;
        }

        .reward-icon {
            font-size: 24px;
            margin-right: 12px;
        }

        .reward-text {
            font-size: 13px;
            color: #ffd700;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>📜 Quest Log</h1>
        <div class='subtitle'>Active Quests: " + quests.FindAll(q => q.status == "Active").Count + @"</div>
    </div>
    <div class='quest-list'>
");

            foreach (var quest in quests)
            {
                string statusClass = $"status-{quest.status.ToLower()}";
                string cardClass = quest.status.ToLower();
                int progressPercent = quest.maxProgress > 0 ? (quest.progress * 100 / quest.maxProgress) : 0;

                sb.Append($@"
        <div class='quest-card {cardClass}'>
            <div class='quest-header'>
                <div class='quest-title'>{quest.title}</div>
                <span class='quest-status {statusClass}'>{quest.status}</span>
            </div>
            <div class='quest-description'>{quest.description}</div>
            <div class='quest-progress'>
                <div class='progress-label'>{quest.progress} / {quest.maxProgress}</div>
                <div class='progress-bar'>
                    <div class='progress-fill' style='width: {progressPercent}%'></div>
                </div>
            </div>
            <div class='quest-reward'>
                <div class='reward-icon'>🎁</div>
                <div class='reward-text'>{quest.reward}</div>
            </div>
        </div>
");
            }

            sb.Append(@"
    </div>
</body>
</html>
");

            return sb.ToString();
        }

        // ====================================================================
        // Settings Templates
        // ====================================================================

        /// <summary>
        /// Generates a complete HTML page displaying game settings
        /// </summary>
        public static string GenerateSettingsHTML(GameSettings settings)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Settings</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #0f0f0f 0%, #1a1a1a 100%);
            color: #e0e0e0;
            padding: 20px;
            min-height: 100vh;
        }}

        .header {{
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #444;
        }}

        h1 {{
            font-size: 32px;
            color: #9333ea;
            margin-bottom: 10px;
        }}

        .settings-container {{
            max-width: 600px;
            margin: 0 auto;
        }}

        .setting-card {{
            background: #1e1e1e;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 16px;
            border: 1px solid #333;
        }}

        .setting-title {{
            font-size: 16px;
            font-weight: bold;
            color: #fff;
            margin-bottom: 8px;
        }}

        .setting-description {{
            font-size: 13px;
            color: #999;
            margin-bottom: 12px;
        }}

        .setting-value {{
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 12px;
            background: #2a2a2a;
            border-radius: 6px;
        }}

        .value-label {{
            color: #bbb;
        }}

        .value-display {{
            font-weight: bold;
            color: #9333ea;
        }}

        .quality-indicator {{
            display: inline-block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            margin-right: 8px;
        }}

        .quality-0 {{ background: #ef4444; }}
        .quality-1 {{ background: #f59e0b; }}
        .quality-2 {{ background: #10b981; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>⚙️ Settings</h1>
    </div>
    <div class='settings-container'>
        <div class='setting-card'>
            <div class='setting-title'>🔊 Volume</div>
            <div class='setting-description'>Master volume level</div>
            <div class='setting-value'>
                <span class='value-label'>Level:</span>
                <span class='value-display'>{(int)(settings.volume * 100)}%</span>
            </div>
        </div>

        <div class='setting-card'>
            <div class='setting-title'>🎨 Graphics Quality</div>
            <div class='setting-description'>Visual quality and performance</div>
            <div class='setting-value'>
                <span class='value-label'>Quality:</span>
                <span class='value-display'>
                    <span class='quality-indicator quality-{settings.graphicsQuality}'></span>
                    {GetQualityName(settings.graphicsQuality)}
                </span>
            </div>
        </div>

        <div class='setting-card'>
            <div class='setting-title'>🖥️ Display Mode</div>
            <div class='setting-description'>Window or fullscreen mode</div>
            <div class='setting-value'>
                <span class='value-label'>Mode:</span>
                <span class='value-display'>{(settings.fullscreen ? "Fullscreen" : "Windowed")}</span>
            </div>
        </div>

        <div class='setting-card'>
            <div class='setting-title'>⚔️ Difficulty</div>
            <div class='setting-description'>Game challenge level</div>
            <div class='setting-value'>
                <span class='value-label'>Level:</span>
                <span class='value-display'>{settings.difficulty}</span>
            </div>
        </div>
    </div>
</body>
</html>
");

            return sb.ToString();
        }

        /// <summary>
        /// Helper method to convert quality index to name
        /// </summary>
        private static string GetQualityName(int quality)
        {
            return quality switch
            {
                0 => "Low",
                1 => "Medium",
                2 => "High",
                _ => "Unknown"
            };
        }
    }
}
