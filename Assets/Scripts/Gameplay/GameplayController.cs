using UnityEngine;
using DesertRider.MP3;
using DesertRider.Terrain;
using DesertRider.Audio;
using DesertRider.Core;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Main gameplay controller for menu flow system.
    /// Manages terrain generation, motorcycle, camera, scoring, and music playback.
    /// Integrates with GameFlowManager for scene transitions.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("Gameplay Configuration")]
        [Tooltip("Auto-generate terrain ahead of motorcycle")]
        public bool autoGenerateTerrain = true;

        [Tooltip("Distance ahead to start generating new segments")]
        public float generateAheadDistance = 50f;

        [Tooltip("Spawn position for motorcycle")]
        public Vector3 motorcycleSpawnPosition = new Vector3(0, 1, 5);

        [Header("References")]
        public TerrainGenerator terrainGenerator;
        public MotorcycleController motorcycle;
        public CameraFollow cameraFollow;
        public MusicPlayer musicPlayer;

        private AnalysisData analysisData;
        private SongData songData;
        private float furthestGeneratedZ = 0f;
        private float sessionStartTime = 0f;
        private bool gameplayActive = false;
        private Vector3 actualMotorcycleSpawnPosition; // Actual spawn position after raycast
        private float lastDebugTime = 0f; // For periodic debug logging

        void Start()
        {
            // Get data from GameFlowManager
            if (GameFlowManager.Instance == null)
            {
                Debug.LogError("GameplayController: GameFlowManager not found! Returning to menu.");
                ReturnToMenu();
                return;
            }

            analysisData = GameFlowManager.Instance.currentAnalysisData;
            songData = GameFlowManager.Instance.currentSongData;

            if (analysisData == null || songData == null)
            {
                Debug.LogError("GameplayController: Missing analysis or song data! Returning to menu.");
                ReturnToMenu();
                return;
            }

            Debug.Log($"GameplayController: Starting gameplay for {songData.Title}");

            // Initialize gameplay
            InitializeGameplay();

            // DIAGNOSTIC: Check segments after 1 frame
            StartCoroutine(CheckSegmentsAfterFrame());
        }

        System.Collections.IEnumerator CheckSegmentsAfterFrame()
        {
            yield return null; // Wait 1 frame

            GameObject seg0 = GameObject.Find("RoadSegment_0");
            if (seg0 != null)
            {
                Debug.Log($"[AFTER 1 FRAME] RoadSegment_0 STILL EXISTS at {seg0.transform.position}");
            }
            else
            {
                Debug.LogError($"[AFTER 1 FRAME] RoadSegment_0 WAS DESTROYED!");
            }
        }

        /// <summary>
        /// Initializes all gameplay systems.
        /// </summary>
        private void InitializeGameplay()
        {
            Debug.Log("GameplayController: Initializing gameplay systems...");

            // Ensure all required components exist
            EnsureComponentsExist();

            // Step 1: Generate terrain
            Debug.Log("Step 1: Generating terrain...");
            if (terrainGenerator != null)
            {
                terrainGenerator.Initialize(analysisData);
                // TerrainGenerator auto-generates initial segments in Initialize
                Debug.Log($"✅ Terrain generated");

                // VERIFY: Check if segments exist immediately after Initialize()
                GameObject checkSegment0 = GameObject.Find("RoadSegment_0");
                if (checkSegment0 != null)
                {
                    Debug.Log($"✅ VERIFY: RoadSegment_0 EXISTS immediately after Initialize at {checkSegment0.transform.position}");
                    Debug.Log($"✅ VERIFY: RoadSegment_0 parent: {checkSegment0.transform.parent?.name ?? "NULL"}");
                    Debug.Log($"✅ VERIFY: RoadSegment_0 parent active: {checkSegment0.transform.parent?.gameObject.activeSelf}");
                }
                else
                {
                    Debug.LogError("❌ VERIFY: RoadSegment_0 DOES NOT EXIST immediately after Initialize!");
                }

                // VERIFY: Check TerrainGenerator status
                Debug.Log($"✅ VERIFY: TerrainGenerator active: {terrainGenerator.gameObject.activeInHierarchy}, enabled: {terrainGenerator.enabled}");
            }

            // CRITICAL: Force physics system to register all terrain colliders before spawning motorcycle
            Physics.SyncTransforms();
            Debug.Log("✅ Physics synchronized - all colliders registered");

            // Step 2: Spawn motorcycle
            Debug.Log("Step 2: Spawning motorcycle...");
            SpawnMotorcycle();
            Debug.Log("✅ Motorcycle spawned");

            // Step 3: Setup camera
            Debug.Log("Step 3: Setting up camera...");
            if (cameraFollow != null && motorcycle != null)
            {
                cameraFollow.SetTarget(motorcycle.transform);
                cameraFollow.SnapToTarget();
                Debug.Log("✅ Camera configured");
            }

            // Step 4: Initialize scoring system
            Debug.Log("Step 4: Initializing scoring...");
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
                Debug.Log("✅ ScoreManager reset");
            }

            // Step 5: Initialize collectibles
            Debug.Log("Step 5: Spawning collectibles...");
            if (CollectibleSpawner.Instance != null)
            {
                CollectibleSpawner.Instance.Initialize(analysisData);
                Debug.Log("✅ Collectibles spawned");
            }

            // Step 6: Start music playback
            Debug.Log("Step 6: Starting music playback...");
            if (musicPlayer != null && !string.IsNullOrEmpty(songData.LocalPath))
            {
                musicPlayer.PlayMP3(songData.LocalPath);

                // Apply volume from settings
                if (GameFlowManager.Instance != null)
                {
                    AudioSource audioSource = musicPlayer.GetComponent<AudioSource>();
                    if (audioSource != null)
                        audioSource.volume = GameFlowManager.Instance.musicVolume;
                }

                sessionStartTime = Time.time;
                Debug.Log("✅ Music playing");
            }

            // Step 7: Initialize BoostSystem
            Debug.Log("Step 7: Initializing BoostSystem...");
            if (BoostSystem.Instance != null && motorcycle != null)
            {
                BoostSystem.Instance.Initialize(analysisData, motorcycle, musicPlayer);
                Debug.Log("✅ BoostSystem initialized");
            }

            // Step 8: Initialize VisualController
            Debug.Log("Step 8: Initializing VisualController...");
            if (motorcycle != null)
            {
                MotorcycleVisualController visualController =
                    motorcycle.GetComponent<MotorcycleVisualController>();
                if (visualController != null)
                {
                    visualController.Initialize(analysisData, musicPlayer, motorcycle, cameraFollow,
                        BoostSystem.Instance);
                    Debug.Log("✅ VisualController initialized");
                }
            }

            // Step 9: Initialize visual atmosphere (red sky, fog, lighting)
            Debug.Log("Step 9: Setting up visual atmosphere...");
            VisualAtmosphere atmosphere = FindFirstObjectByType<VisualAtmosphere>();
            if (atmosphere == null)
            {
                GameObject atmosphereObj = new GameObject("VisualAtmosphere");
                atmosphere = atmosphereObj.AddComponent<VisualAtmosphere>();
                atmosphere.mainCamera = cameraFollow != null ? cameraFollow.GetComponent<Camera>() : Camera.main;
                Debug.Log("✅ Visual atmosphere created and configured");
            }
            else
            {
                Debug.Log("✅ Visual atmosphere already exists");
            }

            // Step 10: Setup bloom for neon glow effects (CRITICAL for polished look!)
            Debug.Log("Step 10: Activating bloom post-processing...");
            BloomSetup bloom = FindFirstObjectByType<BloomSetup>();
            if (bloom == null)
            {
                GameObject bloomObj = new GameObject("BloomPostProcessing");
                bloom = bloomObj.AddComponent<BloomSetup>();
                Debug.Log("✅ Bloom setup created - check console for activation message");
            }
            else
            {
                Debug.Log("✅ Bloom already exists");
            }

            gameplayActive = true;

            Debug.Log("🎉 Gameplay initialized! Use WASD/Arrows to drive!");
            Debug.Log("Controls:");
            Debug.Log("  - W/Up: Accelerate (auto by default)");
            Debug.Log("  - A/D or Left/Right: Steer");
            Debug.Log("  - S/Down: Brake");
            Debug.Log("  - Space: Boost");
            Debug.Log("  - R: Reset position");
            Debug.Log("  - Esc: Pause/Return to menu");
        }

        /// <summary>
        /// Ensures all required components exist in the scene.
        /// </summary>
        private void EnsureComponentsExist()
        {
            // TerrainGenerator
            if (terrainGenerator == null)
            {
                terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
                if (terrainGenerator == null)
                {
                    GameObject go = new GameObject("TerrainGenerator");
                    terrainGenerator = go.AddComponent<TerrainGenerator>();
                }
            }

            // MusicPlayer
            if (musicPlayer == null)
            {
                musicPlayer = FindFirstObjectByType<MusicPlayer>();
                if (musicPlayer == null)
                {
                    GameObject go = new GameObject("MusicPlayer");
                    musicPlayer = go.AddComponent<MusicPlayer>();
                    go.AddComponent<AudioSource>();
                }
            }

            // CameraFollow
            if (cameraFollow == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraFollow = mainCam.GetComponent<CameraFollow>();
                    if (cameraFollow == null)
                    {
                        cameraFollow = mainCam.gameObject.AddComponent<CameraFollow>();
                    }
                }
            }

            // Ensure managers exist
            if (ScoreManager.Instance == null)
            {
                GameObject go = new GameObject("ScoreManager");
                go.AddComponent<ScoreManager>();
            }

            if (CollectibleSpawner.Instance == null)
            {
                GameObject go = new GameObject("CollectibleSpawner");
                go.AddComponent<CollectibleSpawner>();
            }

            if (BoostSystem.Instance == null)
            {
                GameObject go = new GameObject("BoostSystem");
                go.AddComponent<BoostSystem>();
            }

            if (LeaderboardManager.Instance == null)
            {
                GameObject go = new GameObject("LeaderboardManager");
                go.AddComponent<LeaderboardManager>();
            }
        }

        /// <summary>
        /// Spawns the motorcycle at the starting position.
        /// </summary>
        private void SpawnMotorcycle()
        {
            // Raycast downward from spawn position to find actual terrain height
            Vector3 raycastStart = new Vector3(motorcycleSpawnPosition.x, 100f, motorcycleSpawnPosition.z);
            RaycastHit hit;
            actualMotorcycleSpawnPosition = motorcycleSpawnPosition;

            if (Physics.Raycast(raycastStart, Vector3.down, out hit, 200f))
            {
                // Found terrain! Spawn 2 units above the surface
                actualMotorcycleSpawnPosition = hit.point + Vector3.up * 2f;
                Debug.Log($"Motorcycle: Raycast found terrain at y={hit.point.y:F2}, spawning at y={actualMotorcycleSpawnPosition.y:F2}");
            }
            else
            {
                Debug.LogWarning("Motorcycle: Raycast did not find terrain! Using default spawn position.");
            }

            GameObject motorcycleGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            motorcycleGO.name = "Motorcycle";
            motorcycleGO.transform.position = actualMotorcycleSpawnPosition;
            motorcycleGO.transform.localScale = new Vector3(0.5f, 1f, 1f);

            Rigidbody rb = motorcycleGO.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Most aggressive collision detection for fast movement
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent tipping
            // Leave isKinematic = false (dynamic physics from the start)

            Debug.Log($"Motorcycle Rigidbody: useGravity={rb.useGravity}, mass={rb.mass}, collisionMode={rb.collisionDetectionMode}, isKinematic={rb.isKinematic}");

            motorcycle = motorcycleGO.GetComponent<MotorcycleController>();
            if (motorcycle == null)
            {
                motorcycle = motorcycleGO.AddComponent<MotorcycleController>();
            }

            // Add visual controller (will be initialized later)
            motorcycleGO.AddComponent<MotorcycleVisualController>();

            Debug.Log($"Motorcycle spawned at {actualMotorcycleSpawnPosition} - physics ACTIVE from start");
        }

        void Update()
        {
            if (!gameplayActive)
                return;

            // Debug: Log motorcycle status every second
            if (Time.time - lastDebugTime >= 1f && motorcycle != null)
            {
                lastDebugTime = Time.time;
                Debug.Log($"[DEBUG] Motorcycle - Pos: {motorcycle.transform.position}, Speed: {motorcycle.CurrentSpeed:F1}, Grounded: {motorcycle.IsGrounded}");
            }

            // Auto-generate terrain ahead of player
            if (autoGenerateTerrain && motorcycle != null && terrainGenerator != null)
            {
                // Only generate if player is approaching the end of generated terrain
                float playerZ = motorcycle.transform.position.z;
                float furthestZ = terrainGenerator.FurthestGeneratedZ;
                float generateThreshold = terrainGenerator.segmentLength * 5f; // Generate when 5 segments away

                if (playerZ > furthestZ - generateThreshold)
                {
                    terrainGenerator.GenerateNextSegment();
                }
            }

            // Check for song end
            if (musicPlayer != null && analysisData != null && sessionStartTime > 0f)
            {
                if (!musicPlayer.IsPlaying && Time.time - sessionStartTime > analysisData.Duration + 1f)
                {
                    EndGameplay();
                }
            }

            // Handle pause/quit input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }

            // Reset position (R key)
            if (Input.GetKeyDown(KeyCode.R) && motorcycle != null)
            {
                motorcycle.ResetPosition(actualMotorcycleSpawnPosition, Quaternion.identity);
                if (cameraFollow != null)
                    cameraFollow.SnapToTarget();
                Debug.Log($"Motorcycle reset to start position {actualMotorcycleSpawnPosition}");
            }
        }

        /// <summary>
        /// Ends gameplay and transitions to results screen.
        /// </summary>
        private void EndGameplay()
        {
            if (!gameplayActive)
                return;

            gameplayActive = false;

            Debug.Log("=== GAMEPLAY ENDED ===");

            // Get final stats
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.currentScore : 0;
            int coins = ScoreManager.Instance != null ? ScoreManager.Instance.coinsCollected : 0;
            int maxCombo = ScoreManager.Instance != null ? ScoreManager.Instance.maxCombo : 0;
            float duration = Time.time - sessionStartTime;

            Debug.Log($"Final Score: {finalScore}");
            Debug.Log($"Coins: {coins}");
            Debug.Log($"Max Combo: {maxCombo}x");
            Debug.Log($"Duration: {duration:F1}s");

            // Submit to leaderboard
            if (LeaderboardManager.Instance != null && songData != null)
            {
                string playerName = GameFlowManager.Instance != null ?
                    GameFlowManager.Instance.playerName : "Player";

                bool submitted = LeaderboardManager.Instance.SubmitScore(
                    songData, playerName, finalScore, coins, maxCombo);

                if (submitted)
                {
                    Debug.Log("✅ Score submitted to leaderboard");
                }
            }

            // Stop music
            if (musicPlayer != null)
            {
                musicPlayer.Stop();
            }

            // Transition to results
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToResults(finalScore, coins, maxCombo, duration);
            }
            else
            {
                Debug.LogError("GameplayController: Cannot transition to results - GameFlowManager missing");
            }
        }

        /// <summary>
        /// Pauses the game and returns to main menu.
        /// </summary>
        private void PauseGame()
        {
            Debug.Log("GameplayController: Pause requested");

            // TODO: Implement proper pause menu
            // For now, just return to menu
            ReturnToMenu();
        }

        /// <summary>
        /// Returns to main menu (cleanup and transition).
        /// </summary>
        private void ReturnToMenu()
        {
            gameplayActive = false;

            if (musicPlayer != null)
            {
                musicPlayer.Stop();
            }

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToMainMenu();
            }
        }

        void OnGUI()
        {
            if (!gameplayActive)
                return;

            // Simple HUD display
            GUIStyle style = new GUIStyle();
            style.fontSize = 18;
            style.normal.textColor = Color.white;

            // Score
            if (ScoreManager.Instance != null)
            {
                GUI.Label(new Rect(20, 20, 280, 30),
                    $"Score: {ScoreManager.Instance.currentScore}", style);
                GUI.Label(new Rect(20, 50, 280, 30),
                    $"Coins: {ScoreManager.Instance.coinsCollected}", style);
                GUI.Label(new Rect(20, 80, 280, 30),
                    $"Combo: {ScoreManager.Instance.currentCombo}x (Max: {ScoreManager.Instance.maxCombo}x)", style);
            }

            // Boost meter
            if (BoostSystem.Instance != null)
            {
                string chargeDisplay = "";
                for (int i = 0; i < BoostSystem.Instance.MaxCharges; i++)
                {
                    chargeDisplay += (i < BoostSystem.Instance.CurrentCharges) ? "⚡" : "○";
                }

                GUIStyle boostStyle = new GUIStyle(style);
                boostStyle.fontSize = 20;
                boostStyle.normal.textColor = BoostSystem.Instance.IsBoosting ? Color.yellow : Color.cyan;

                GUI.Label(new Rect(20, 110, 280, 30), $"Boost: {chargeDisplay}", boostStyle);

                if (BoostSystem.Instance.IsBoosting)
                {
                    GUI.Label(new Rect(20, 140, 280, 20), ">>> BOOST ACTIVE <<<", boostStyle);
                }
            }

            // Song info
            if (songData != null)
            {
                GUI.Label(new Rect(20, Screen.height - 50, 400, 30),
                    $"♪ {songData.Title}", style);
            }

            // Time
            if (musicPlayer != null && analysisData != null)
            {
                float currentTime = musicPlayer.CurrentTime;
                float totalTime = analysisData.Duration;
                int currentMin = Mathf.FloorToInt(currentTime / 60f);
                int currentSec = Mathf.FloorToInt(currentTime % 60f);
                int totalMin = Mathf.FloorToInt(totalTime / 60f);
                int totalSec = Mathf.FloorToInt(totalTime % 60f);

                GUI.Label(new Rect(Screen.width - 150, 20, 140, 30),
                    $"{currentMin:00}:{currentSec:00} / {totalMin:00}:{totalSec:00}", style);
            }
        }
    }
}
