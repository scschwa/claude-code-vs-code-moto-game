using UnityEngine;
using DesertRider.MP3;
using DesertRider.Terrain;
using DesertRider.Gameplay;
using DesertRider.Audio;

namespace DesertRider.Testing
{
    /// <summary>
    /// Complete gameplay test: combines terrain generation with motorcycle controls.
    /// Tests the full game loop - analyze music, generate terrain, drive motorcycle.
    /// </summary>
    public class MotorcycleTest : MonoBehaviour
    {
        [Header("MP3 Analysis")]
        [Tooltip("Full path to MP3 file to analyze")]
        public string mp3FilePath = @"E:\Downloads\Heavy Is The Crown (Original Score).mp3";

        [Tooltip("Loaded analysis data")]
        public AnalysisData analysisData;

        [Header("Motorcycle")]
        [Tooltip("Motorcycle prefab (or will create simple one)")]
        public GameObject motorcyclePrefab;

        [Tooltip("Spawn position for motorcycle")]
        public Vector3 motorcycleSpawnPosition = new Vector3(0, 2, 0);

        [Header("Terrain Generation")]
        [Tooltip("Generate terrain ahead of motorcycle automatically")]
        public bool autoGenerateTerrain = true;

        [Tooltip("Distance ahead to start generating new segments")]
        public float generateAheadDistance = 50f;

        [Header("References")]
        [Tooltip("Reference to TerrainGenerator")]
        public TerrainGenerator terrainGenerator;

        [Tooltip("Reference to PreAnalyzer")]
        public PreAnalyzer preAnalyzer;

        [Tooltip("Reference to MP3Loader")]
        public MP3Loader mp3Loader;

        [Tooltip("Reference to spawned motorcycle")]
        public MotorcycleController motorcycle;

        [Tooltip("Reference to main camera")]
        public CameraFollow cameraFollow;

        [Tooltip("Music player for audio playback")]
        public MusicPlayer musicPlayer;

        [Header("Music Playback")]
        [Tooltip("Enable music playback during gameplay")]
        public bool enableMusic = true;

        private float furthestGeneratedZ = 0f;

        void Start()
        {
            Debug.Log("MotorcycleTest: Ready. Use context menu 'Initialize Gameplay' to start.");

            // Find camera
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
        }

        void Update()
        {
            // Auto-generate terrain ahead of motorcycle
            if (autoGenerateTerrain && motorcycle != null && terrainGenerator != null && analysisData != null)
            {
                float motorcycleZ = motorcycle.transform.position.z;

                // Generate new segment if motorcycle is getting close to the end
                if (motorcycleZ + generateAheadDistance > furthestGeneratedZ)
                {
                    terrainGenerator.AdvanceGeneration();
                    furthestGeneratedZ += terrainGenerator.segmentLength;
                }
            }

            // Reset controls
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetMotorcycle();
            }

            // Regenerate terrain
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (analysisData != null && terrainGenerator != null)
                {
                    terrainGenerator.Initialize(analysisData);
                    furthestGeneratedZ = terrainGenerator.segmentLength * terrainGenerator.activeSegmentCount;
                    Debug.Log("Terrain regenerated");
                }
            }
        }

        /// <summary>
        /// Initializes complete gameplay: analyzes MP3, generates terrain, spawns motorcycle.
        /// Right-click component and select "Initialize Gameplay".
        /// </summary>
        [ContextMenu("Initialize Gameplay")]
        public async void InitializeGameplay()
        {
            Debug.Log("MotorcycleTest: Initializing gameplay...");

            // Validate MP3 path
            if (string.IsNullOrEmpty(mp3FilePath))
            {
                Debug.LogError("MP3 file path not set!");
                return;
            }

            if (!System.IO.File.Exists(mp3FilePath))
            {
                Debug.LogError($"MP3 file not found: {mp3FilePath}");
                return;
            }

            // Find/create required components
            EnsureComponentsExist();

            // Step 1: Analyze MP3
            Debug.Log("Step 1: Analyzing MP3...");
            try
            {
                SongData songData = new SongData();
                analysisData = await preAnalyzer.PreAnalyzeSong(mp3FilePath, songData);

                if (analysisData == null)
                {
                    Debug.LogError("Analysis failed!");
                    return;
                }

                Debug.Log($"✅ Analysis complete! Beats={analysisData.Beats.Count}, BPM={analysisData.BPM:F1}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Analysis error: {e.Message}");
                return;
            }

            // Step 2: Initialize CollectibleSpawner with analysis data
            Debug.Log("Step 2: Initializing CollectibleSpawner...");
            if (DesertRider.Gameplay.CollectibleSpawner.Instance != null)
            {
                DesertRider.Gameplay.CollectibleSpawner.Instance.Initialize(analysisData);
                Debug.Log("✅ CollectibleSpawner initialized");
            }

            // Initialize ObstacleSpawner with analysis data
            if (DesertRider.Gameplay.ObstacleSpawner.Instance != null)
            {
                DesertRider.Gameplay.ObstacleSpawner.Instance.Initialize(analysisData);
                Debug.Log("✅ ObstacleSpawner initialized");
            }

            // Step 3: Generate initial terrain
            Debug.Log("Step 3: Generating terrain...");
            terrainGenerator.Initialize(analysisData);
            furthestGeneratedZ = terrainGenerator.segmentLength * terrainGenerator.activeSegmentCount;
            Debug.Log($"✅ Terrain initialized ({terrainGenerator.activeSegmentCount} segments)");

            // Step 4: Spawn motorcycle
            Debug.Log("Step 4: Spawning motorcycle...");

            // Adjust spawn height to be above the first terrain segment
            // The first segment might have height variation from intensity values
            float spawnHeight = 3f; // Start 3 units above ground to ensure it lands properly
            motorcycleSpawnPosition = new Vector3(0, spawnHeight, 5f); // Start at Z=5 to be on first segment

            SpawnMotorcycle();
            Debug.Log("✅ Motorcycle spawned");

            // Step 5: Setup camera
            Debug.Log("Step 5: Setting up camera...");
            if (cameraFollow != null && motorcycle != null)
            {
                cameraFollow.SetTarget(motorcycle.transform);
                cameraFollow.SnapToTarget();
                Debug.Log("✅ Camera configured");
            }

            // Step 6: Start music playback
            if (enableMusic && musicPlayer != null)
            {
                Debug.Log("Step 6: Starting music playback...");
                musicPlayer.PlayMP3(mp3FilePath);
                Debug.Log("✅ Music playing");
            }

            Debug.Log("🎉 Gameplay initialized! Use WASD/Arrows to drive!");
            Debug.Log("Controls:");
            Debug.Log("  - W/Up: Accelerate (auto by default)");
            Debug.Log("  - A/D or Left/Right: Steer");
            Debug.Log("  - S/Down: Brake");
            Debug.Log("  - R: Reset motorcycle position");
            Debug.Log("  - T: Regenerate terrain");
        }

        /// <summary>
        /// Ensures all required components exist in the scene.
        /// </summary>
        private void EnsureComponentsExist()
        {
            // PreAnalyzer
            if (preAnalyzer == null)
            {
                preAnalyzer = FindFirstObjectByType<PreAnalyzer>();
                if (preAnalyzer == null)
                {
                    GameObject go = new GameObject("PreAnalyzer");
                    preAnalyzer = go.AddComponent<PreAnalyzer>();
                }
            }

            // MP3Loader
            if (mp3Loader == null)
            {
                mp3Loader = FindFirstObjectByType<MP3Loader>();
                if (mp3Loader == null)
                {
                    GameObject go = new GameObject("MP3Loader");
                    mp3Loader = go.AddComponent<MP3Loader>();
                }
            }

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
                    go.AddComponent<AudioSource>();
                    musicPlayer = go.AddComponent<MusicPlayer>();
                }
            }

            // CollectibleSpawner
            if (DesertRider.Gameplay.CollectibleSpawner.Instance == null)
            {
                GameObject go = new GameObject("CollectibleSpawner");
                go.AddComponent<DesertRider.Gameplay.CollectibleSpawner>();
                Debug.Log("MotorcycleTest: Created CollectibleSpawner");
            }

            // ObstacleSpawner
            if (DesertRider.Gameplay.ObstacleSpawner.Instance == null)
            {
                GameObject go = new GameObject("ObstacleSpawner");
                go.AddComponent<DesertRider.Gameplay.ObstacleSpawner>();
                Debug.Log("MotorcycleTest: Created ObstacleSpawner");
            }

            // ScoreManager
            if (DesertRider.Gameplay.ScoreManager.Instance == null)
            {
                GameObject go = new GameObject("ScoreManager");
                go.AddComponent<DesertRider.Gameplay.ScoreManager>();
                Debug.Log("MotorcycleTest: Created ScoreManager");
            }

            // ObjectPoolManager
            if (DesertRider.Gameplay.ObjectPoolManager.Instance == null)
            {
                GameObject go = new GameObject("ObjectPoolManager");
                go.AddComponent<DesertRider.Gameplay.ObjectPoolManager>();
                Debug.Log("MotorcycleTest: Created ObjectPoolManager (configure coin pool in Inspector!)");
            }
        }

        /// <summary>
        /// Spawns the motorcycle at the starting position.
        /// </summary>
        private void SpawnMotorcycle()
        {
            // Destroy existing motorcycle if any
            if (motorcycle != null)
            {
                Destroy(motorcycle.gameObject);
            }

            GameObject motorcycleGO;

            // Use prefab if provided, otherwise create simple placeholder
            if (motorcyclePrefab != null)
            {
                motorcycleGO = Instantiate(motorcyclePrefab, motorcycleSpawnPosition, Quaternion.identity);
            }
            else
            {
                // Create simple motorcycle placeholder
                motorcycleGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                motorcycleGO.name = "Motorcycle";
                motorcycleGO.transform.position = motorcycleSpawnPosition;
                motorcycleGO.transform.rotation = Quaternion.identity;
                motorcycleGO.transform.localScale = new Vector3(1f, 0.5f, 2f); // Motorcycle-ish shape

                // Add Rigidbody
                Rigidbody rb = motorcycleGO.AddComponent<Rigidbody>();
                rb.mass = 100f;

                // Add MotorcycleController
                motorcycleGO.AddComponent<MotorcycleController>();

                // Change color
                Renderer renderer = motorcycleGO.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.2f, 0.6f, 1f); // Blue
                }
            }

            motorcycle = motorcycleGO.GetComponent<MotorcycleController>();
            if (motorcycle == null)
            {
                motorcycle = motorcycleGO.AddComponent<MotorcycleController>();
            }

            Debug.Log($"Motorcycle spawned at {motorcycleSpawnPosition}");
        }

        /// <summary>
        /// Resets motorcycle to starting position.
        /// </summary>
        private void ResetMotorcycle()
        {
            if (motorcycle != null)
            {
                motorcycle.ResetPosition(motorcycleSpawnPosition, Quaternion.identity);

                if (cameraFollow != null)
                {
                    cameraFollow.SnapToTarget();
                }

                Debug.Log("Motorcycle reset to start position");
            }
        }

        /// <summary>
        /// Just spawns motorcycle for quick testing (no analysis).
        /// </summary>
        [ContextMenu("Quick Test - Spawn Motorcycle Only")]
        public void QuickTestMotorcycle()
        {
            SpawnMotorcycle();

            if (cameraFollow != null && motorcycle != null)
            {
                cameraFollow.SetTarget(motorcycle.transform);
                cameraFollow.SnapToTarget();
            }

            Debug.Log("✅ Motorcycle spawned for quick test");
        }

        void OnGUI()
        {
            // Display info
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            GUI.Box(new Rect(10, 10, 300, 200), "");

            if (analysisData != null)
            {
                GUI.Label(new Rect(20, 20, 280, 20), $"BPM: {analysisData.BPM:F1}", style);
                GUI.Label(new Rect(20, 40, 280, 20), $"Seed: {analysisData.LevelSeed}", style);
            }

            if (motorcycle != null)
            {
                GUI.Label(new Rect(20, 60, 280, 20), $"Speed: {motorcycle.CurrentSpeed:F1} / {motorcycle.maxSpeed}", style);
                GUI.Label(new Rect(20, 80, 280, 20), $"Grounded: {motorcycle.IsGrounded}", style);

                // Show input state for debugging
                string inputState = "";
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputState += "W ";
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputState += "A ";
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputState += "S ";
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputState += "D ";
                GUI.Label(new Rect(20, 100, 280, 20), $"Input: {(string.IsNullOrEmpty(inputState) ? "None" : inputState)}", style);
            }

            // Display score and combo (if ScoreManager exists)
            if (DesertRider.Gameplay.ScoreManager.Instance != null)
            {
                GUI.Label(new Rect(20, 120, 280, 20), $"Score: {DesertRider.Gameplay.ScoreManager.Instance.currentScore}", style);

                if (DesertRider.Gameplay.ScoreManager.Instance.currentCombo > 1)
                {
                    GUIStyle comboStyle = new GUIStyle(style);
                    comboStyle.normal.textColor = Color.yellow;
                    GUI.Label(new Rect(20, 140, 280, 20), $"COMBO x{DesertRider.Gameplay.ScoreManager.Instance.currentCombo} (Multiplier: {DesertRider.Gameplay.ScoreManager.Instance.CurrentMultiplier:F2}x)!", comboStyle);
                }
                else
                {
                    GUI.Label(new Rect(20, 140, 280, 20), $"Coins: {DesertRider.Gameplay.ScoreManager.Instance.coinsCollected}", style);
                }
            }

            GUI.Label(new Rect(20, 160, 280, 20), "WASD/Arrows: Drive", style);
            GUI.Label(new Rect(20, 180, 280, 20), "R: Reset | T: Regen Terrain", style);
        }
    }
}
