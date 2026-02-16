using UnityEngine;
using DesertRider.MP3;
using System.Collections.Generic;

namespace DesertRider.Testing
{
    /// <summary>
    /// Test script for beat detection and pre-analysis functionality.
    /// Loads an MP3, analyzes it, and visualizes the detected beats and intensity curve.
    /// </summary>
    public class BeatDetectionTest : MonoBehaviour
    {
        [Header("MP3 File Configuration")]
        [Tooltip("Full path to MP3 file to analyze")]
        public string mp3FilePath = @"E:\Downloads\Heavy Is The Crown (Original Score).mp3";

        [Header("Analysis Results")]
        [Tooltip("Analysis data from PreAnalyzer")]
        public AnalysisData analysisData;

        [Header("Visualization Settings")]
        [Tooltip("Display rectangle for visualization")]
        public Rect displayRect = new Rect(10, 10, 1200, 300);

        [Tooltip("Color for beat markers")]
        public Color beatColor = Color.red;

        [Tooltip("Color for intensity curve")]
        public Color intensityColor = Color.cyan;

        [Tooltip("Show beat detection visualization")]
        public bool showVisualization = true;

        private Texture2D beatTexture;
        private Texture2D intensityTexture;
        private GUIStyle labelStyle;

        void Start()
        {
            Debug.Log("BeatDetectionTest: Ready. Use context menu 'Analyze MP3' to test.");

            // Create textures
            beatTexture = MakeTex(2, 2, beatColor);
            intensityTexture = MakeTex(2, 2, intensityColor);

            // Create label style
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 14;
            labelStyle.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// Analyzes an MP3 file and stores the results.
        /// Right-click the component in Inspector and select "Analyze MP3".
        /// </summary>
        [ContextMenu("Analyze MP3")]
        public async void AnalyzeMP3()
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

            Debug.Log($"BeatDetectionTest: Starting analysis of {mp3FilePath}");

            // Find or create PreAnalyzer
            PreAnalyzer analyzer = FindFirstObjectByType<PreAnalyzer>();
            if (analyzer == null)
            {
                GameObject go = new GameObject("PreAnalyzer");
                analyzer = go.AddComponent<PreAnalyzer>();
                Debug.Log("PreAnalyzer component created automatically.");
            }

            // Run analysis (this may take a few seconds)
            try
            {
                SongData songData = new SongData(); // Temporary, not used yet
                analysisData = await analyzer.PreAnalyzeSong(mp3FilePath, songData);

                if (analysisData != null)
                {
                    Debug.Log($"✅ Analysis complete!");
                    Debug.Log($"  - Beats detected: {analysisData.Beats.Count}");
                    Debug.Log($"  - BPM: {analysisData.BPM:F1}");
                    Debug.Log($"  - Duration: {analysisData.Duration:F2}s");
                    Debug.Log($"  - Intensity samples: {analysisData.IntensityCurve.Count}");
                    Debug.Log($"  - Level seed: {analysisData.LevelSeed}");

                    // Print first 10 beats
                    Debug.Log("First 10 beats:");
                    for (int i = 0; i < Mathf.Min(10, analysisData.Beats.Count); i++)
                    {
                        var beat = analysisData.Beats[i];
                        Debug.Log($"  Beat {i + 1}: Time={beat.Time:F3}s, Strength={beat.Strength:F3}");
                    }
                }
                else
                {
                    Debug.LogError("Analysis failed - returned null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Analysis failed: {e.Message}\n{e.StackTrace}");
                analysisData = null;
            }
        }

        /// <summary>
        /// Clears analysis data.
        /// </summary>
        [ContextMenu("Clear Data")]
        public void ClearData()
        {
            analysisData = null;
            Debug.Log("Analysis data cleared.");
        }

        void OnGUI()
        {
            if (!showVisualization || analysisData == null || analysisData.Beats == null)
            {
                GUI.Box(displayRect, "No analysis data loaded. Use 'Analyze MP3' context menu.", labelStyle);
                return;
            }

            // Draw background
            GUI.Box(displayRect, "");

            float width = displayRect.width;
            float height = displayRect.height;
            float duration = analysisData.Duration;

            // Draw intensity curve
            if (analysisData.IntensityCurve != null && analysisData.IntensityCurve.Count > 0)
            {
                Vector2? previousPoint = null;
                for (int i = 0; i < analysisData.IntensityCurve.Count; i++)
                {
                    float normalizedX = (float)i / analysisData.IntensityCurve.Count;
                    float x = displayRect.x + normalizedX * width;

                    float intensity = analysisData.IntensityCurve[i];
                    float y = displayRect.y + height - (intensity * height * 0.8f); // Scale to 80% of height

                    Vector2 currentPoint = new Vector2(x, y);

                    if (previousPoint.HasValue)
                    {
                        DrawLine(previousPoint.Value, currentPoint, intensityColor);
                    }

                    previousPoint = currentPoint;
                }
            }

            // Draw beat markers
            if (analysisData.Beats != null)
            {
                foreach (var beat in analysisData.Beats)
                {
                    float normalizedX = beat.Time / duration;
                    float x = displayRect.x + normalizedX * width;

                    // Draw vertical line for beat
                    DrawLine(
                        new Vector2(x, displayRect.y),
                        new Vector2(x, displayRect.y + height),
                        beatColor
                    );
                }
            }

            // Draw info text
            string info = $"Beats: {analysisData.Beats.Count} | BPM: {analysisData.BPM:F1} | Duration: {duration:F2}s | Seed: {analysisData.LevelSeed}";
            GUI.Label(new Rect(displayRect.x + 5, displayRect.y + height + 5, 1000, 25), info, labelStyle);
        }

        void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.color = color;
            Texture2D tex = (color == beatColor) ? beatTexture : intensityTexture;
            GUI.DrawTexture(new Rect(start.x, start.y - 1f, length, 2f), tex);
            GUIUtility.RotateAroundPivot(-angle, start);
            GUI.color = Color.white;
        }

        Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
