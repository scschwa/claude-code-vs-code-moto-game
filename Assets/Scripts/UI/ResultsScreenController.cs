using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertRider.Core;

namespace DesertRider.UI
{
    /// <summary>
    /// Displays post-game results with score breakdown and leaderboard.
    /// Shows player performance and ranking for the completed song.
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        [Header("Score Display")]
        [Tooltip("Final score text")]
        public TextMeshProUGUI scoreText;

        [Tooltip("Coins collected text")]
        public TextMeshProUGUI coinsText;

        [Tooltip("Max combo text")]
        public TextMeshProUGUI maxComboText;

        [Tooltip("Song duration text")]
        public TextMeshProUGUI durationText;

        [Tooltip("Song title text")]
        public TextMeshProUGUI songTitleText;

        [Tooltip("New high score indicator")]
        public GameObject newHighScoreIndicator;

        [Tooltip("Player rank text")]
        public TextMeshProUGUI rankText;

        [Header("Leaderboard")]
        [Tooltip("Leaderboard container")]
        public Transform leaderboardContainer;

        [Tooltip("Leaderboard entry prefab")]
        public GameObject leaderboardEntryPrefab;

        [Tooltip("Number of leaderboard entries to show")]
        public int maxLeaderboardEntries = 10;

        [Header("Buttons")]
        [Tooltip("Play again button")]
        public Button playAgainButton;

        [Tooltip("Main menu button")]
        public Button mainMenuButton;

        [Tooltip("Song select button")]
        public Button songSelectButton;

        void Start()
        {
            // Setup button listeners
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (songSelectButton != null)
                songSelectButton.onClick.AddListener(OnSongSelectClicked);

            // Display results
            DisplayResults();

            Debug.Log("ResultsScreenController: Initialized");
        }

        /// <summary>
        /// Displays the results from the last gameplay session.
        /// </summary>
        private void DisplayResults()
        {
            if (GameFlowManager.Instance == null)
            {
                Debug.LogError("ResultsScreenController: GameFlowManager not found!");
                return;
            }

            // Get session data
            int score = GameFlowManager.Instance.lastScore;
            int coins = GameFlowManager.Instance.lastCoins;
            int maxCombo = GameFlowManager.Instance.lastMaxCombo;
            float duration = GameFlowManager.Instance.lastDuration;
            string songTitle = GameFlowManager.Instance.GetCurrentSongTitle();

            // Display song title
            if (songTitleText != null)
                songTitleText.text = songTitle;

            // Display score breakdown
            if (scoreText != null)
                scoreText.text = $"{score:N0}";

            if (coinsText != null)
                coinsText.text = $"{coins}";

            if (maxComboText != null)
                maxComboText.text = $"{maxCombo}x";

            if (durationText != null)
            {
                int minutes = Mathf.FloorToInt(duration / 60f);
                int seconds = Mathf.FloorToInt(duration % 60f);
                durationText.text = $"{minutes:00}:{seconds:00}";
            }

            // Check leaderboard status
            CheckLeaderboardStatus(score);

            // Display leaderboard
            DisplayLeaderboard();
        }

        /// <summary>
        /// Checks if the score is a new high score and updates UI.
        /// </summary>
        private void CheckLeaderboardStatus(int score)
        {
            if (LeaderboardManager.Instance == null || GameFlowManager.Instance.currentSongData == null)
                return;

            string songHash = GameFlowManager.Instance.currentSongData.Hash;
            int previousHighScore = LeaderboardManager.Instance.GetHighScore(songHash);
            bool isNewHighScore = score > previousHighScore;

            // Show/hide new high score indicator
            if (newHighScoreIndicator != null)
                newHighScoreIndicator.SetActive(isNewHighScore);

            // Get rank
            SongLeaderboard leaderboard = LeaderboardManager.Instance.LoadLeaderboard(songHash);
            if (leaderboard != null && rankText != null)
            {
                int rank = leaderboard.GetRankForScore(score);
                if (rank > 0)
                {
                    rankText.text = $"Rank: #{rank}";
                    rankText.color = GetRankColor(rank);
                }
                else
                {
                    rankText.text = "Not Ranked";
                    rankText.color = Color.gray;
                }
            }
        }

        /// <summary>
        /// Gets color based on rank (gold, silver, bronze, etc).
        /// </summary>
        private Color GetRankColor(int rank)
        {
            return rank switch
            {
                1 => new Color(1f, 0.84f, 0f), // Gold
                2 => new Color(0.75f, 0.75f, 0.75f), // Silver
                3 => new Color(0.8f, 0.5f, 0.2f), // Bronze
                _ => Color.cyan
            };
        }

        /// <summary>
        /// Displays the leaderboard for the current song.
        /// </summary>
        private void DisplayLeaderboard()
        {
            if (leaderboardContainer == null || leaderboardEntryPrefab == null)
            {
                Debug.LogWarning("ResultsScreenController: Missing leaderboard UI references");
                return;
            }

            // Clear existing entries
            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }

            if (LeaderboardManager.Instance == null || GameFlowManager.Instance.currentSongData == null)
                return;

            string songHash = GameFlowManager.Instance.currentSongData.Hash;
            SongLeaderboard leaderboard = LeaderboardManager.Instance.LoadLeaderboard(songHash);

            if (leaderboard == null || leaderboard.entries == null || leaderboard.entries.Count == 0)
            {
                CreateLeaderboardEntry("No scores yet", "Be the first!");
                return;
            }

            // Get top entries
            List<LeaderboardEntry> topEntries = leaderboard.GetTopEntries(maxLeaderboardEntries);

            for (int i = 0; i < topEntries.Count; i++)
            {
                LeaderboardEntry entry = topEntries[i];
                int rank = i + 1;

                string rankText = $"#{rank}";
                string playerText = $"{entry.playerName}";
                string scoreText = $"{entry.score:N0}";
                string detailsText = $"Coins: {entry.coins} | Combo: {entry.maxCombo}x";

                CreateLeaderboardEntry(rankText, playerText, scoreText, detailsText, GetRankColor(rank));
            }
        }

        /// <summary>
        /// Creates a leaderboard entry UI element.
        /// </summary>
        private void CreateLeaderboardEntry(string rankText, string playerText, string scoreText = "",
            string detailsText = "", Color? rankColor = null)
        {
            GameObject entryGO = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

            // Find text components (assumes standard naming)
            TextMeshProUGUI[] texts = entryGO.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var text in texts)
            {
                if (text.name.Contains("Rank"))
                {
                    text.text = rankText;
                    if (rankColor.HasValue)
                        text.color = rankColor.Value;
                }
                else if (text.name.Contains("Player"))
                {
                    text.text = playerText;
                }
                else if (text.name.Contains("Score"))
                {
                    text.text = scoreText;
                }
                else if (text.name.Contains("Details"))
                {
                    text.text = detailsText;
                }
            }
        }

        /// <summary>
        /// Called when Play Again button is clicked.
        /// </summary>
        private void OnPlayAgainClicked()
        {
            Debug.Log("ResultsScreenController: Play Again clicked");

            if (GameFlowManager.Instance != null && !string.IsNullOrEmpty(GameFlowManager.Instance.selectedSongPath))
            {
                // Reload the same song
                GameFlowManager.Instance.GoToLoading(GameFlowManager.Instance.selectedSongPath);
            }
            else
            {
                GameFlowManager.Instance.GoToSongSelection();
            }
        }

        /// <summary>
        /// Called when Main Menu button is clicked.
        /// </summary>
        private void OnMainMenuClicked()
        {
            Debug.Log("ResultsScreenController: Main Menu clicked");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToMainMenu();
            }
        }

        /// <summary>
        /// Called when Song Select button is clicked.
        /// </summary>
        private void OnSongSelectClicked()
        {
            Debug.Log("ResultsScreenController: Song Select clicked");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToSongSelection();
            }
        }

        void OnDestroy()
        {
            // Clean up listeners
            if (playAgainButton != null)
                playAgainButton.onClick.RemoveListener(OnPlayAgainClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

            if (songSelectButton != null)
                songSelectButton.onClick.RemoveListener(OnSongSelectClicked);
        }
    }
}
