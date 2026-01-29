// ============================================================================
// WebViewToolkit - Dynamic HTML Sample - Data Structures
// ============================================================================
// Simple data classes representing game data that will be converted to HTML
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WebViewToolkit.Samples.DynamicHTML
{
    /// <summary>
    /// Represents an item in the player's inventory
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string name;
        public string description;
        public int quantity;
        public string rarity; // Common, Uncommon, Rare, Epic, Legendary
        public string iconUrl;

        public InventoryItem(string name, string description, int quantity, string rarity, string iconUrl = "")
        {
            this.name = name;
            this.description = description;
            this.quantity = quantity;
            this.rarity = rarity;
            this.iconUrl = iconUrl;
        }
    }

    /// <summary>
    /// Represents a quest in the game
    /// </summary>
    [Serializable]
    public class Quest
    {
        public string title;
        public string description;
        public string status; // Active, Completed, Failed
        public int progress;
        public int maxProgress;
        public string reward;

        public Quest(string title, string description, string status, int progress, int maxProgress, string reward)
        {
            this.title = title;
            this.description = description;
            this.status = status;
            this.progress = progress;
            this.maxProgress = maxProgress;
            this.reward = reward;
        }
    }

    /// <summary>
    /// Represents game settings
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public float volume;
        public int graphicsQuality;
        public bool fullscreen;
        public string difficulty;

        public GameSettings()
        {
            volume = 0.75f;
            graphicsQuality = 2;
            fullscreen = true;
            difficulty = "Normal";
        }
    }

    /// <summary>
    /// Container for all game data used in this sample
    /// </summary>
    public class GameData
    {
        public List<InventoryItem> Inventory { get; private set; }
        public List<Quest> Quests { get; private set; }
        public GameSettings Settings { get; private set; }

        public GameData()
        {
            Inventory = new List<InventoryItem>();
            Quests = new List<Quest>();
            Settings = new GameSettings();
            InitializeSampleData();
        }

        /// <summary>
        /// Creates sample data for demonstration
        /// </summary>
        private void InitializeSampleData()
        {
            // Sample inventory items
            Inventory.Add(new InventoryItem(
                "Health Potion",
                "Restores 50 HP",
                5,
                "Common",
                "🧪"
            ));

            Inventory.Add(new InventoryItem(
                "Mana Potion",
                "Restores 30 MP",
                3,
                "Common",
                "🔵"
            ));

            Inventory.Add(new InventoryItem(
                "Steel Sword",
                "A well-crafted blade",
                1,
                "Uncommon",
                "⚔️"
            ));

            Inventory.Add(new InventoryItem(
                "Dragon Scale",
                "Rare crafting material",
                2,
                "Rare",
                "🐉"
            ));

            Inventory.Add(new InventoryItem(
                "Phoenix Feather",
                "Grants resurrection ability",
                1,
                "Legendary",
                "🔥"
            ));

            // Sample quests
            Quests.Add(new Quest(
                "Defeat the Goblin King",
                "Clear the goblin camp and defeat their leader",
                "Active",
                15,
                20,
                "500 Gold, Rare Weapon"
            ));

            Quests.Add(new Quest(
                "Gather Moonflowers",
                "Collect 10 Moonflowers from the enchanted forest",
                "Active",
                7,
                10,
                "Magic Potion Recipe"
            ));

            Quests.Add(new Quest(
                "Rescue the Villagers",
                "Save the captured villagers from the bandits",
                "Completed",
                5,
                5,
                "Village Hero Title"
            ));

            Quests.Add(new Quest(
                "Find the Ancient Artifact",
                "Locate the lost artifact in the Temple of Shadows",
                "Active",
                0,
                1,
                "Ancient Artifact, 1000 XP"
            ));
        }

        /// <summary>
        /// Adds a random item to the inventory (for testing dynamic updates)
        /// </summary>
        public void AddRandomItem()
        {
            string[] names = { "Gold Coin", "Silver Ring", "Ruby Gem", "Magic Scroll", "Elixir" };
            string[] rarities = { "Common", "Uncommon", "Rare" };
            string[] icons = { "💰", "💍", "💎", "📜", "⚗️" };

            int index = UnityEngine.Random.Range(0, names.Length);
            Inventory.Add(new InventoryItem(
                names[index],
                "A valuable item",
                1,
                rarities[UnityEngine.Random.Range(0, rarities.Length)],
                icons[index]
            ));
        }

        /// <summary>
        /// Removes the last item from inventory
        /// </summary>
        public void RemoveLastItem()
        {
            if (Inventory.Count > 0)
            {
                Inventory.RemoveAt(Inventory.Count - 1);
            }
        }
    }
}
