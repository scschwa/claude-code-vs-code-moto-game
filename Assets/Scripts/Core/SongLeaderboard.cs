using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DesertRider.Core
{
    /// <summary>
    /// Represents a leaderboard for a specific song.
    /// Maintains sorted list of high scores.
    /// </summary>
    [Serializable]
    public class SongLeaderboard
    {
        [Tooltip("MD5 hash of the MP3 file")]
        public string songHash;

        [Tooltip("Song title for display")]
        public string songTitle;

        [Tooltip("Expected level seed for this song")]
        public int levelSeed;

        [Tooltip("List of leaderboard entries sorted by score")]
        public List<LeaderboardEntry> entries;

        /// <summary>
        /// Default constructor for Unity serialization.
        /// </summary>
        public SongLeaderboard()
        {
            songHash = "";
            songTitle = "Unknown";
            levelSeed = 0;
            entries = new List<LeaderboardEntry>();
        }

        /// <summary>
        /// Creates a new leaderboard for a song.
        /// </summary>
        public SongLeaderboard(string hash, string title, int seed)
        {
            songHash = hash;
            songTitle = title;
            levelSeed = seed;
            entries = new List<LeaderboardEntry>();
        }

        /// <summary>
        /// Adds an entry to the leaderboard and maintains sorted order.
        /// Limits total entries to maxEntries.
        /// </summary>
        /// <param name="entry">Entry to add</param>
        /// <param name="maxEntries">Maximum number of entries to keep</param>
        public void AddEntry(LeaderboardEntry entry, int maxEntries = 50)
        {
            if (entry == null)
            {
                Debug.LogWarning("SongLeaderboard: Cannot add null entry");
                return;
            }

            // Add the entry
            entries.Add(entry);

            // Sort by score descending and limit to max entries
            entries = entries
                .OrderByDescending(e => e.score)
                .Take(maxEntries)
                .ToList();
        }

        /// <summary>
        /// Gets the top N entries from the leaderboard.
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(int count = 10)
        {
            return entries.Take(count).ToList();
        }

        /// <summary>
        /// Gets the player's rank for a given score (1-based).
        /// Returns -1 if score wouldn't make the leaderboard.
        /// </summary>
        public int GetRankForScore(int score)
        {
            int rank = 1;
            foreach (var entry in entries)
            {
                if (score > entry.score)
                    return rank;
                rank++;
            }

            // If we reached here, score is lower than all entries
            // but could still make the board if there's room
            if (entries.Count < 50) // Assuming 50 max entries
                return rank;

            return -1; // Doesn't make the leaderboard
        }

        /// <summary>
        /// Checks if a score would make the leaderboard.
        /// </summary>
        public bool IsHighScore(int score, int maxEntries = 50)
        {
            // Always true if leaderboard isn't full
            if (entries.Count < maxEntries)
                return true;

            // Check if score beats the lowest entry
            return score > entries[entries.Count - 1].score;
        }

        /// <summary>
        /// Gets the highest score on the leaderboard.
        /// </summary>
        public int GetHighScore()
        {
            if (entries.Count == 0)
                return 0;

            return entries[0].score;
        }

        /// <summary>
        /// Returns summary string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{songTitle} ({entries.Count} entries, High Score: {GetHighScore()})";
        }
    }
}
