using UnityEngine;
using UnityEngine.SceneManagement;
using DesertRider.MP3;

namespace DesertRider.Core
{
    /// <summary>
    /// Manages game flow between scenes and persistent session data.
    /// Singleton that persists across scene loads using DontDestroyOnLoad.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static GameFlowManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadPlayerPreferences();
        }
        #endregion

        #region Scene Names
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_SONG_SELECTION = "SongSelection";
        public const string SCENE_LOADING = "Loading";
        public const string SCENE_GAMEPLAY = "Gameplay";
        public const string SCENE_RESULTS = "Results";
        #endregion

        #region Session Data
        [Header("Current Session")]
        [Tooltip("Path to currently selected MP3 file")]
        public string selectedSongPath = "";

        [Tooltip("Analysis data for current song")]
        public AnalysisData currentAnalysisData;

        [Tooltip("Song data for current song")]
        public SongData currentSongData;

        [Header("Player Data")]
        [Tooltip("Player name for leaderboard")]
        public string playerName = "Player";

        [Header("Results Data")]
        [Tooltip("Final score from last gameplay session")]
        public int lastScore = 0;

        [Tooltip("Coins collected in last session")]
        public int lastCoins = 0;

        [Tooltip("Max combo in last session")]
        public int lastMaxCombo = 0;

        [Tooltip("Gameplay duration in last session")]
        public float lastDuration = 0f;
        #endregion

        #region Player Preferences
        private const string PREF_PLAYER_NAME = "PlayerName";
        private const string PREF_MUSIC_VOLUME = "MusicVolume";
        private const string PREF_LAST_SONG_PATH = "LastSongPath";

        [Header("Settings")]
        [Range(0f, 1f)]
        public float musicVolume = 1f;

        /// <summary>
        /// Loads player preferences from PlayerPrefs.
        /// </summary>
        private void LoadPlayerPreferences()
        {
            playerName = PlayerPrefs.GetString(PREF_PLAYER_NAME, "Player");
            musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, 1f);
            selectedSongPath = PlayerPrefs.GetString(PREF_LAST_SONG_PATH, "");

            Debug.Log($"GameFlowManager: Loaded preferences - Player: {playerName}, Volume: {musicVolume}");
        }

        /// <summary>
        /// Saves player preferences to PlayerPrefs.
        /// </summary>
        public void SavePlayerPreferences()
        {
            PlayerPrefs.SetString(PREF_PLAYER_NAME, playerName);
            PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, musicVolume);
            PlayerPrefs.SetString(PREF_LAST_SONG_PATH, selectedSongPath);
            PlayerPrefs.Save();

            Debug.Log($"GameFlowManager: Saved preferences - Player: {playerName}");
        }

        /// <summary>
        /// Sets the player name and saves it.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                playerName = name.Trim();
                SavePlayerPreferences();
            }
        }

        /// <summary>
        /// Sets the music volume and saves it.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            SavePlayerPreferences();
        }
        #endregion

        #region Scene Transitions
        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public void GoToMainMenu()
        {
            ClearSessionData();
            SceneManager.LoadScene(SCENE_MAIN_MENU);
            Debug.Log("GameFlowManager: Loading Main Menu");
        }

        /// <summary>
        /// Loads the song selection scene.
        /// </summary>
        public void GoToSongSelection()
        {
            SceneManager.LoadScene(SCENE_SONG_SELECTION);
            Debug.Log("GameFlowManager: Loading Song Selection");
        }

        /// <summary>
        /// Loads the loading screen scene.
        /// </summary>
        public void GoToLoading(string songPath)
        {
            selectedSongPath = songPath;
            SavePlayerPreferences(); // Save last song path
            SceneManager.LoadScene(SCENE_LOADING);
            Debug.Log($"GameFlowManager: Loading screen for {songPath}");
        }

        /// <summary>
        /// Loads the gameplay scene.
        /// </summary>
        public void GoToGameplay()
        {
            if (currentAnalysisData == null)
            {
                Debug.LogError("GameFlowManager: Cannot start gameplay without analysis data!");
                GoToSongSelection();
                return;
            }

            SceneManager.LoadScene(SCENE_GAMEPLAY);
            Debug.Log("GameFlowManager: Starting gameplay");
        }

        /// <summary>
        /// Loads the results scene with final score data.
        /// </summary>
        public void GoToResults(int score, int coins, int maxCombo, float duration)
        {
            lastScore = score;
            lastCoins = coins;
            lastMaxCombo = maxCombo;
            lastDuration = duration;

            SceneManager.LoadScene(SCENE_RESULTS);
            Debug.Log($"GameFlowManager: Loading results - Score: {score}");
        }

        /// <summary>
        /// Clears current session data (used when returning to menu).
        /// </summary>
        private void ClearSessionData()
        {
            currentAnalysisData = null;
            currentSongData = null;
            lastScore = 0;
            lastCoins = 0;
            lastMaxCombo = 0;
            lastDuration = 0f;
        }
        #endregion

        #region Data Management
        /// <summary>
        /// Stores analysis data for the current session.
        /// </summary>
        public void SetAnalysisData(AnalysisData data, SongData songData)
        {
            currentAnalysisData = data;
            currentSongData = songData;
            Debug.Log($"GameFlowManager: Analysis data set - {songData?.Title ?? "Unknown"}");
        }

        /// <summary>
        /// Gets the current song title for display.
        /// </summary>
        public string GetCurrentSongTitle()
        {
            if (currentSongData != null && !string.IsNullOrEmpty(currentSongData.Title))
                return currentSongData.Title;

            if (!string.IsNullOrEmpty(selectedSongPath))
                return System.IO.Path.GetFileNameWithoutExtension(selectedSongPath);

            return "Unknown Song";
        }
        #endregion

        #region Utility
        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            SavePlayerPreferences();
            Debug.Log("GameFlowManager: Quitting game");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
