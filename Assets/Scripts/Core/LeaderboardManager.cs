using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DesertRider.MP3;

namespace DesertRider.Core
{
    /// <summary>
    /// Manages per-song leaderboards with JSON persistence.
    /// Stores leaderboards in Application.persistentDataPath/Leaderboards/
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static LeaderboardManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        [Header("Configuration")]
        [Tooltip("Maximum entries per song")]
        public int maxEntriesPerSong = 50;

        [Tooltip("Default player name")]
        public string defaultPlayerName = "Player";

        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool debugMode = false;

        // Cached leaderboards (in-memory)
        private Dictionary<string, SongLeaderboard> cachedLeaderboards = new Dictionary<string, SongLeaderboard>();

        // File path helpers
        private string LeaderboardsDirectory => Path.Combine(Application.persistentDataPath, "Leaderboards");

        private string GetLeaderboardFilePath(string songHash) =>
            Path.Combine(LeaderboardsDirectory, $"{songHash}.json");

        /// <summary>
        /// Loads a leaderboard for a specific song hash.
        /// Returns null if no leaderboard exists.
        /// </summary>
        public SongLeaderboard LoadLeaderboard(string songHash)
        {
            if (string.IsNullOrEmpty(songHash))
            {
                Debug.LogError("LeaderboardManager: Cannot load leaderboard with empty song hash");
                return null;
            }

            // Check cache first
            if (cachedLeaderboards.ContainsKey(songHash))
            {
                if (debugMode)
                    Debug.Log($"LeaderboardManager: Loaded {songHash} from cache");
                return cachedLeaderboards[songHash];
            }

            // Load from file
            string filePath = GetLeaderboardFilePath(songHash);

            if (!File.Exists(filePath))
            {
                if (debugMode)
                    Debug.Log($"LeaderboardManager: No leaderboard file for {songHash}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SongLeaderboard leaderboard = JsonUtility.FromJson<SongLeaderboard>(json);

                if (leaderboard == null || leaderboard.entries == null)
                {
                    Debug.LogWarning($"LeaderboardManager: Invalid leaderboard data for {songHash}");
                    return null;
                }

                // Cache it
                cachedLeaderboards[songHash] = leaderboard;

                if (debugMode)
                    Debug.Log($"LeaderboardManager: Loaded {leaderboard.entries.Count} entries for {songHash}");

                return leaderboard;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LeaderboardManager: Failed to load leaderboard: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves a leaderboard to disk.
        /// </summary>
        public void SaveLeaderboard(SongLeaderboard leaderboard)
        {
            if (leaderboard == null)
            {
                Debug.LogWarning("LeaderboardManager: Cannot save null leaderboard");
                return;
            }

            if (string.IsNullOrEmpty(leaderboard.songHash))
            {
                Debug.LogError("LeaderboardManager: Cannot save leaderboard with empty song hash");
                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(LeaderboardsDirectory))
            {
                Directory.CreateDirectory(LeaderboardsDirectory);
                if (debugMode)
                    Debug.Log($"LeaderboardManager: Created directory {LeaderboardsDirectory}");
            }

            string filePath = GetLeaderboardFilePath(leaderboard.songHash);

            try
            {
                string json = JsonUtility.ToJson(leaderboard, prettyPrint: true);
                File.WriteAllText(filePath, json);

                // Update cache
                cachedLeaderboards[leaderboard.songHash] = leaderboard;

                if (debugMode)
                    Debug.Log($"LeaderboardManager: Saved {leaderboard.entries.Count} entries to {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LeaderboardManager: Failed to save leaderboard: {e.Message}");
            }
        }

        /// <summary>
        /// Submits a score to the leaderboard for a specific song.
        /// </summary>
        public bool SubmitScore(SongData songData, string playerName, int score, int coins, int maxCombo)
        {
            if (songData == null || songData.AnalysisData == null)
            {
                Debug.LogError("LeaderboardManager: Cannot submit score without song data");
                return false;
            }

            if (string.IsNullOrEmpty(songData.Hash))
            {
                Debug.LogError("LeaderboardManager: Cannot submit score - song hash is missing");
                return false;
            }

            if (score <= 0)
            {
                if (debugMode)
                    Debug.LogWarning("LeaderboardManager: Score is zero or negative - skipping submission");
                return false;
            }

            string songHash = songData.Hash;
            int levelSeed = songData.AnalysisData.LevelSeed;

            // Load or create leaderboard
            SongLeaderboard leaderboard = LoadLeaderboard(songHash);

            if (leaderboard == null)
            {
                // Create new leaderboard
                leaderboard = new SongLeaderboard(
                    songHash,
                    songData.Title ?? "Unknown",
                    levelSeed
                );

                if (debugMode)
                    Debug.Log($"LeaderboardManager: Created new leaderboard for {songData.Title}");
            }
            else
            {
                // Validate levelSeed matches
                if (leaderboard.levelSeed != levelSeed)
                {
                    Debug.LogWarning($"LeaderboardManager: LevelSeed mismatch! Expected {leaderboard.levelSeed}, got {levelSeed}. This may indicate the song file was modified.");
                }
            }

            // Create entry
            LeaderboardEntry entry = new LeaderboardEntry(
                string.IsNullOrEmpty(playerName) ? defaultPlayerName : playerName,
                score,
                coins,
                maxCombo,
                levelSeed
            );

            // Add entry
            leaderboard.AddEntry(entry, maxEntriesPerSong);

            // Save
            SaveLeaderboard(leaderboard);

            if (debugMode)
            {
                int rank = leaderboard.GetRankForScore(score);
                Debug.Log($"LeaderboardManager: Submitted score {score} for {songData.Title} (Rank: {rank})");
            }

            return true;
        }

        /// <summary>
        /// Gets the high score for a specific song.
        /// Returns 0 if no leaderboard exists.
        /// </summary>
        public int GetHighScore(string songHash)
        {
            if (string.IsNullOrEmpty(songHash))
                return 0;

            SongLeaderboard leaderboard = LoadLeaderboard(songHash);
            if (leaderboard == null)
                return 0;

            return leaderboard.GetHighScore();
        }

        /// <summary>
        /// Checks if a score would make the leaderboard for a specific song.
        /// </summary>
        public bool IsHighScore(string songHash, int score)
        {
            if (string.IsNullOrEmpty(songHash))
                return false;

            SongLeaderboard leaderboard = LoadLeaderboard(songHash);
            if (leaderboard == null)
                return true; // First score always makes it

            return leaderboard.IsHighScore(score, maxEntriesPerSong);
        }

        /// <summary>
        /// Clears all cached leaderboards (forces reload from disk).
        /// </summary>
        public void ClearCache()
        {
            cachedLeaderboards.Clear();
            if (debugMode)
                Debug.Log("LeaderboardManager: Cache cleared");
        }

        /// <summary>
        /// Deletes a leaderboard for a specific song.
        /// </summary>
        public void DeleteLeaderboard(string songHash)
        {
            if (string.IsNullOrEmpty(songHash))
                return;

            string filePath = GetLeaderboardFilePath(songHash);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    cachedLeaderboards.Remove(songHash);

                    if (debugMode)
                        Debug.Log($"LeaderboardManager: Deleted leaderboard for {songHash}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"LeaderboardManager: Failed to delete leaderboard: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Gets list of all song hashes with leaderboards.
        /// </summary>
        public List<string> GetAllSongHashes()
        {
            List<string> hashes = new List<string>();

            if (!Directory.Exists(LeaderboardsDirectory))
                return hashes;

            try
            {
                string[] files = Directory.GetFiles(LeaderboardsDirectory, "*.json");
                foreach (string file in files)
                {
                    string hash = Path.GetFileNameWithoutExtension(file);
                    hashes.Add(hash);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LeaderboardManager: Failed to get song hashes: {e.Message}");
            }

            return hashes;
        }

        /// <summary>
        /// Gets the path where leaderboards are stored for debugging.
        /// </summary>
        public string GetLeaderboardsPath()
        {
            return LeaderboardsDirectory;
        }
    }
}
