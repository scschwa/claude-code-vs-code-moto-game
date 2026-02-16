using UnityEngine;
using DesertRider.MP3;
using DesertRider.Terrain;

namespace DesertRider.Testing
{
    /// <summary>
    /// Test script for terrain generation system.
    /// Loads analysis data and generates music-reactive road terrain.
    /// </summary>
    public class TerrainTest : MonoBehaviour
    {
        [Header("MP3 Analysis")]
        [Tooltip("Full path to MP3 file to analyze")]
        public string mp3FilePath = @"E:\Downloads\Heavy Is The Crown (Original Score).mp3";

        [Tooltip("Loaded analysis data")]
        public AnalysisData analysisData;

        [Header("Auto Generation")]
        [Tooltip("Automatically generate more segments as time passes")]
        public bool autoGenerate = true;

        [Tooltip("Time between auto-generation (seconds)")]
        public float autoGenerateInterval = 2f;

        [Header("Camera Control")]
        [Tooltip("Move camera along the road automatically")]
        public bool autoMoveCamera = true;

        [Tooltip("Camera movement speed")]
        public float cameraSpeed = 5f;

        [Header("References")]
        [Tooltip("Reference to TerrainGenerator (will auto-find if not set)")]
        public TerrainGenerator terrainGenerator;

        [Tooltip("Reference to PreAnalyzer (will auto-find if not set)")]
        public PreAnalyzer preAnalyzer;

        [Tooltip("Reference to MP3Loader (will auto-find if not set)")]
        public MP3Loader mp3Loader;

        private float timeSinceLastGeneration = 0f;
        private Camera mainCamera;

        void Start()
        {
            Debug.Log("TerrainTest: Ready. Use context menu 'Analyze and Generate' to test.");

            mainCamera = Camera.main;

            // Find required components
            if (terrainGenerator == null)
            {
                terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
            }

            if (preAnalyzer == null)
            {
                preAnalyzer = FindFirstObjectByType<PreAnalyzer>();
            }

            if (mp3Loader == null)
            {
                mp3Loader = FindFirstObjectByType<MP3Loader>();
            }
        }

        void Update()
        {
            // Auto-generate more segments
            if (autoGenerate && analysisData != null && terrainGenerator != null)
            {
                timeSinceLastGeneration += Time.deltaTime;

                if (timeSinceLastGeneration >= autoGenerateInterval)
                {
                    terrainGenerator.AdvanceGeneration();
                    timeSinceLastGeneration = 0f;
                }
            }

            // Auto-move camera
            if (autoMoveCamera && mainCamera != null)
            {
                mainCamera.transform.position += mainCamera.transform.forward * cameraSpeed * Time.deltaTime;
            }

            // Manual controls
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (terrainGenerator != null)
                {
                    terrainGenerator.AdvanceGeneration();
                    Debug.Log("Manually generated next segment");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (analysisData != null && terrainGenerator != null)
                {
                    terrainGenerator.Initialize(analysisData);
                    Debug.Log("Reset terrain generation");
                }
            }
        }

        /// <summary>
        /// Analyzes MP3 and generates terrain.
        /// Right-click component and select "Analyze and Generate".
        /// </summary>
        [ContextMenu("Analyze and Generate")]
        public async void AnalyzeAndGenerate()
        {
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

            Debug.Log($"TerrainTest: Analyzing {mp3FilePath}...");

            // Ensure components exist
            if (preAnalyzer == null)
            {
                GameObject go = new GameObject("PreAnalyzer");
                preAnalyzer = go.AddComponent<PreAnalyzer>();
            }

            if (mp3Loader == null)
            {
                GameObject go = new GameObject("MP3Loader");
                mp3Loader = go.AddComponent<MP3Loader>();
            }

            if (terrainGenerator == null)
            {
                GameObject go = new GameObject("TerrainGenerator");
                terrainGenerator = go.AddComponent<TerrainGenerator>();
            }

            // Run analysis
            try
            {
                SongData songData = new SongData();
                analysisData = await preAnalyzer.PreAnalyzeSong(mp3FilePath, songData);

                if (analysisData != null)
                {
                    Debug.Log($"✅ Analysis complete! Beats={analysisData.Beats.Count}, BPM={analysisData.BPM:F1}");

                    // Initialize terrain generation
                    terrainGenerator.Initialize(analysisData);

                    Debug.Log("✅ Terrain generation initialized!");
                    Debug.Log("Controls:");
                    Debug.Log("  - SPACE: Generate next segment manually");
                    Debug.Log("  - R: Reset terrain generation");
                }
                else
                {
                    Debug.LogError("Analysis failed - returned null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Analysis failed: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Loads existing analysis data and generates terrain without re-analyzing.
        /// </summary>
        [ContextMenu("Generate from Existing Data")]
        public void GenerateFromExistingData()
        {
            if (analysisData == null)
            {
                Debug.LogError("No analysis data available! Use 'Analyze and Generate' first.");
                return;
            }

            if (terrainGenerator == null)
            {
                GameObject go = new GameObject("TerrainGenerator");
                terrainGenerator = go.AddComponent<TerrainGenerator>();
            }

            terrainGenerator.Initialize(analysisData);
            Debug.Log("✅ Terrain regenerated from existing data!");
        }

        /// <summary>
        /// Clears all terrain segments.
        /// </summary>
        [ContextMenu("Clear Terrain")]
        public void ClearTerrain()
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.ClearAllSegments();
                Debug.Log("Terrain cleared.");
            }
        }

        void OnGUI()
        {
            if (analysisData == null)
                return;

            // Display info
            GUI.Box(new Rect(10, 10, 300, 120), "");
            GUI.Label(new Rect(20, 20, 280, 20), $"Beats: {analysisData.Beats.Count}");
            GUI.Label(new Rect(20, 40, 280, 20), $"BPM: {analysisData.BPM:F1}");
            GUI.Label(new Rect(20, 60, 280, 20), $"Seed: {analysisData.LevelSeed}");
            GUI.Label(new Rect(20, 80, 280, 20), $"Auto Generate: {autoGenerate}");
            GUI.Label(new Rect(20, 100, 280, 20), $"SPACE: Next | R: Reset");
        }
    }
}
