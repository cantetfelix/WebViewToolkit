// ============================================================================
// WebViewToolkit - JavaScript Bridge Sample - Game State
// ============================================================================
// Simple data class representing game state that can be serialized to JSON
// and sent to JavaScript via the WebView.
// ============================================================================

using System;
using UnityEngine;

namespace WebViewToolkit.Samples.JavaScriptBridge
{
    /// <summary>
    /// Represents the current state of the game.
    /// This class is serializable to JSON for sending to JavaScript.
    /// </summary>
    [Serializable]
    public class GameState
    {
        [SerializeField] private int score;
        [SerializeField] private int health;
        [SerializeField] private int level;

        // ====================================================================
        // Properties
        // ====================================================================

        public int Score
        {
            get => score;
            set => score = Mathf.Max(0, value); // Prevent negative score
        }

        public int Health
        {
            get => health;
            set => health = Mathf.Clamp(value, 0, 100); // Keep health between 0-100
        }

        public int Level
        {
            get => level;
            set => level = Mathf.Max(1, value); // Level starts at 1
        }

        // ====================================================================
        // Constructor
        // ====================================================================

        public GameState()
        {
            score = 0;
            health = 100;
            level = 1;
        }

        public GameState(int score, int health, int level)
        {
            this.score = score;
            this.health = health;
            this.level = level;
        }

        // ====================================================================
        // Methods
        // ====================================================================

        /// <summary>
        /// Converts the game state to a JSON string for sending to JavaScript
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Creates a GameState from a JSON string
        /// </summary>
        public static GameState FromJson(string json)
        {
            return JsonUtility.FromJson<GameState>(json);
        }

        /// <summary>
        /// Resets the game state to default values
        /// </summary>
        public void Reset()
        {
            score = 0;
            health = 100;
            level = 1;
        }

        /// <summary>
        /// Returns a human-readable string representation
        /// </summary>
        public override string ToString()
        {
            return $"Score: {score}, Health: {health}, Level: {level}";
        }
    }
}
