using UnityEngine;
using DesertRider.MP3;

namespace DesertRider.Core
{
    /// <summary>
    /// Central game manager that coordinates all systems.
    /// Handles game state, mode switching, and system initialization.
    /// See TECHNICAL_PLAN.md Section 2 for system architecture.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static GameManager Instance { get; private set; }

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

        [Header("Game Mode")]
        [Tooltip("Current game mode (MP3 or Free Play)")]
        [SerializeField] private GameMode currentMode = GameMode.MP3;

        [Header("Current Session")]
        [Tooltip("Song data for current MP3 mode session (null in Free Play)")]
        [SerializeField] private SongData currentSong;

        [Header("Game State")]
        [Tooltip("Is a gameplay session currently active?")]
        [SerializeField] private bool isPlaying = false;

        /// <summary>
        /// Gets the current game mode.
        /// </summary>
        public GameMode CurrentMode => currentMode;

        /// <summary>
        /// Gets the current song (MP3 mode only, null in Free Play).
        /// </summary>
        public SongData CurrentSong => currentSong;

        /// <summary>
        /// Gets whether a gameplay session is active.
        /// </summary>
        public bool IsPlaying => isPlaying;

        private void Start()
        {
            InitializeSystems();
        }

        /// <summary>
        /// Initializes all game systems on startup.
        /// </summary>
        private void InitializeSystems()
        {
            // TODO: Initialize MP3LibraryManager
            // TODO: Initialize MP3Loader
            // TODO: Initialize PreAnalyzer
            // TODO: Initialize LevelSeeder
            // TODO: Initialize AudioCaptureManager (if Free Play mode)
            // TODO: Initialize BeatDetector
            // TODO: Initialize TerrainGenerator
            // TODO: Initialize ObstacleSpawner
            // TODO: Initialize ScoreSystem
            // TODO: Initialize LeaderboardManager (MP3 mode only)
            // TODO: Load saved settings (volume, controls, calibration)

            Debug.Log("Game systems initialized");
        }

        /// <summary>
        /// Starts a new gameplay session in MP3 mode.
        /// </summary>
        /// <param name="songData">Song to play.</param>
        public void StartMP3Mode(SongData songData)
        {
            // TODO: Validate songData is not null
            // TODO: Set currentMode to MP3
            // TODO: Set currentSong to songData
            // TODO: Load song's AnalysisData
            // TODO: Initialize terrain generator with level seed
            // TODO: Initialize obstacle spawner with level seed
            // TODO: Load MP3 as AudioClip for playback
            // TODO: Start music playback
            // TODO: Set isPlaying to true
            // TODO: Load gameplay scene

            Debug.Log($"Starting MP3 mode with song: {songData.Title}");
            throw new System.NotImplementedException("See TECHNICAL_PLAN.md Phase 1");
        }

        /// <summary>
        /// Starts a new gameplay session in Free Play mode.
        /// </summary>
        public void StartFreePlayMode()
        {
            // TODO: Set currentMode to FreePlay
            // TODO: Set currentSong to null
            // TODO: Initialize AudioCaptureManager for system audio
            // TODO: Initialize terrain generator for dynamic generation
            // TODO: Initialize obstacle spawner for dynamic spawning
            // TODO: Start real-time beat detection
            // TODO: Set isPlaying to true
            // TODO: Load gameplay scene

            Debug.Log("Starting Free Play mode");
            throw new System.NotImplementedException("See TECHNICAL_PLAN.md Phase 2");
        }

        /// <summary>
        /// Ends the current gameplay session.
        /// </summary>
        /// <param name="finalScore">Player's final score.</param>
        public void EndSession(int finalScore)
        {
            // TODO: Stop music playback
            // TODO: Stop audio capture (if Free Play)
            // TODO: Calculate statistics (distance, coins, combo record)
            // TODO: Save high score (if MP3 mode and score > previous)
            // TODO: Update leaderboard (if MP3 mode)
            // TODO: Set isPlaying to false
            // TODO: Load results scene with statistics

            Debug.Log($"Session ended with score: {finalScore}");
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Pauses the current gameplay session.
        /// </summary>
        public void PauseGame()
        {
            // TODO: Pause music playback
            // TODO: Pause audio capture (if Free Play)
            // TODO: Pause all game systems (motorcycle, spawners, etc.)
            // TODO: Set Time.timeScale = 0 (or use custom pause system)
            // TODO: Show pause menu

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Resumes the current gameplay session.
        /// </summary>
        public void ResumeGame()
        {
            // TODO: Resume music playback
            // TODO: Resume audio capture (if Free Play)
            // TODO: Resume all game systems
            // TODO: Set Time.timeScale = 1
            // TODO: Hide pause menu

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Switches between MP3 and Free Play modes (from main menu).
        /// </summary>
        /// <param name="newMode">Mode to switch to.</param>
        public void SwitchMode(GameMode newMode)
        {
            // TODO: Validate not currently playing
            // TODO: Set currentMode to newMode
            // TODO: Update UI to reflect mode change
            // TODO: Initialize mode-specific systems

            currentMode = newMode;
            Debug.Log($"Switched to {newMode} mode");
        }

        // TODO: Add save/load system for settings
        // TODO: Add crash recovery (save state during gameplay)
        // TODO: Add analytics tracking (play time, favorite songs, etc.)
        // TODO: Add achievement system
    }
}
