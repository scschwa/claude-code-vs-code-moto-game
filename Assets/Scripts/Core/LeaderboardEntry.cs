using System;
using UnityEngine;

namespace DesertRider.Core
{
    /// <summary>
    /// Represents a single leaderboard entry for a song.
    /// Stores score, player info, and metadata.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        [Tooltip("Player name")]
        public string playerName;

        [Tooltip("Total score achieved")]
        public int score;

        [Tooltip("Number of coins collected")]
        public int coins;

        [Tooltip("Maximum combo achieved")]
        public int maxCombo;

        [Tooltip("Timestamp of when score was achieved (ISO 8601 format)")]
        public string timestamp;

        [Tooltip("Level seed used (for validation that same level was played)")]
        public int levelSeed;

        /// <summary>
        /// Creates a new leaderboard entry.
        /// </summary>
        public LeaderboardEntry(string name, int score, int coins, int combo, int seed)
        {
            this.playerName = string.IsNullOrEmpty(name) ? "Player" : name;
            this.score = score;
            this.coins = coins;
            this.maxCombo = combo;
            this.levelSeed = seed;
            this.timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        }

        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        public LeaderboardEntry()
        {
            playerName = "Player";
            score = 0;
            coins = 0;
            maxCombo = 0;
            levelSeed = 0;
            timestamp = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Gets formatted date/time string for display.
        /// </summary>
        public string FormattedDate
        {
            get
            {
                try
                {
                    DateTime dt = DateTime.Parse(timestamp);
                    return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                }
                catch
                {
                    return "Unknown Date";
                }
            }
        }

        /// <summary>
        /// Returns string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{playerName}: {score} pts (Coins: {coins}, Combo: {maxCombo})";
        }
    }
}
