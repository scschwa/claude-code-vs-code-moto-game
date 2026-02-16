using UnityEngine;
using DesertRider.MP3;
using DesertRider.UI;

namespace DesertRider.Testing
{
    /// <summary>
    /// Test script for MP3Loader functionality.
    /// Provides a simple interface to test MP3 loading and display results.
    /// </summary>
    public class MP3LoadTest : MonoBehaviour
    {
        [Header("MP3 File Configuration")]
        [Tooltip("Full path to MP3 file to test")]
        public string mp3FilePath = @"C:\path\to\test.mp3";

        [Header("Visualization")]
        [Tooltip("Waveform visualizer component (will auto-find if not set)")]
        public WaveformVisualizer waveformVisualizer;

        [Header("Runtime Data")]
        [Tooltip("Loaded audio samples (mono, normalized -1.0 to 1.0)")]
        public float[] loadedSamples;

        [Tooltip("Duration of the loaded audio in seconds")]
        public float duration;

        [Tooltip("Sample rate of the loaded audio (Hz)")]
        public int sampleRate;

        [Tooltip("Total number of samples in the loaded audio")]
        public int sampleCount;

        private MP3Loader loader;

        private void Start()
        {
            Debug.Log("MP3LoadTest: Ready to load MP3. Use context menu 'Load MP3' to test.");

            // Find or create MP3Loader
            loader = FindFirstObjectByType<MP3Loader>();
            if (loader == null)
            {
                Debug.LogWarning("MP3Loader not found in scene. Please add a GameObject with MP3Loader component.");
            }

            // Find or create WaveformVisualizer
            if (waveformVisualizer == null)
            {
                waveformVisualizer = GetComponent<WaveformVisualizer>();
                if (waveformVisualizer == null)
                {
                    waveformVisualizer = gameObject.AddComponent<WaveformVisualizer>();
                    Debug.Log("WaveformVisualizer component added automatically.");
                }
            }
        }

        /// <summary>
        /// Loads and tests an MP3 file. Triggered via context menu.
        /// Right-click the component in Inspector and select "Load MP3".
        /// </summary>
        [ContextMenu("Load MP3")]
        public void LoadTestMP3()
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

            Debug.Log($"Loading MP3: {mp3FilePath}");

            try
            {
                // Find MP3Loader if not already found
                if (loader == null)
                {
                    loader = FindFirstObjectByType<MP3Loader>();
                    if (loader == null)
                    {
                        Debug.LogError("MP3Loader component not found in scene! Please add a GameObject with MP3Loader component.");
                        return;
                    }
                }

                // Load waveform
                loadedSamples = loader.LoadMP3Waveform(mp3FilePath);
                sampleRate = loader.SampleRate;
                sampleCount = loadedSamples.Length;
                duration = (float)sampleCount / sampleRate;

                // Display results
                Debug.Log($"✅ MP3 loaded successfully!");
                Debug.Log($"  - Samples: {sampleCount:N0}");
                Debug.Log($"  - Sample Rate: {sampleRate} Hz");
                Debug.Log($"  - Duration: {duration:F2} seconds");
                Debug.Log($"  - Channels: Mono (converted from original)");

                // Display sample range for verification
                if (loadedSamples.Length > 0)
                {
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    foreach (float sample in loadedSamples)
                    {
                        if (sample < min) min = sample;
                        if (sample > max) max = sample;
                    }
                    Debug.Log($"  - Sample Range: [{min:F3}, {max:F3}]");
                }

                // Update waveform visualizer
                if (waveformVisualizer != null)
                {
                    waveformVisualizer.LoadFromTest(loadedSamples, sampleRate);
                    Debug.Log("Waveform visualizer updated with loaded data.");
                }
                else
                {
                    Debug.LogWarning("WaveformVisualizer not found. Waveform will not be displayed.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load MP3: {e.Message}\n{e.StackTrace}");

                // Clear data on failure
                loadedSamples = null;
                sampleRate = 0;
                sampleCount = 0;
                duration = 0f;
            }
        }

        /// <summary>
        /// Clears loaded MP3 data. Triggered via context menu.
        /// </summary>
        [ContextMenu("Clear Data")]
        public void ClearData()
        {
            loadedSamples = null;
            sampleRate = 0;
            sampleCount = 0;
            duration = 0f;

            // Clear visualizer
            if (waveformVisualizer != null)
            {
                waveformVisualizer.samples = null;
            }

            Debug.Log("MP3 data cleared.");
        }
    }
}
