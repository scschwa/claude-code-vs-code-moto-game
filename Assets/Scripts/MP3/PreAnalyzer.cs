using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DesertRider.Audio;

namespace DesertRider.MP3
{
    /// <summary>
    /// Pre-analyzes MP3 files to extract beat map, intensity curve, and BPM.
    /// Runs complete analysis on full song during import (not real-time).
    /// Results are cached for deterministic level generation.
    /// See TECHNICAL_PLAN.md Section 4.1 and 4.3 for algorithm details.
    /// </summary>
    public class PreAnalyzer : MonoBehaviour
    {
        #region Singleton Pattern
        public static PreAnalyzer Instance { get; private set; }

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

        [Header("FFT Settings")]
        [Tooltip("FFT window size - higher = better frequency resolution, lower time resolution")]
        public int fftSize = 1024;

        [Tooltip("Number of samples to hop forward between FFT windows")]
        public int hopSize = 512;

        [Header("Beat Detection Settings")]
        [Tooltip("Multiplier for dynamic threshold - higher = fewer beats detected")]
        public float thresholdMultiplier = 1.5f;

        /// <summary>
        /// Pre-analyzes a song to generate complete AnalysisData.
        /// This is a computationally expensive operation that runs once during import.
        /// </summary>
        /// <param name="mp3Path">Path to the MP3 file.</param>
        /// <param name="songData">SongData object to populate with analysis results.</param>
        /// <returns>Complete AnalysisData containing beats, intensity curve, BPM, and level seed.</returns>
        public Task<AnalysisData> PreAnalyzeSong(string mp3Path, SongData songData)
        {
            Debug.Log($"PreAnalyzer: Starting analysis of {mp3Path}");

            // Load full waveform using MP3Loader
            MP3Loader loader = MP3Loader.Instance;
            if (loader == null)
            {
                Debug.LogError("PreAnalyzer: MP3Loader instance not found!");
                return Task.FromResult<AnalysisData>(null);
            }

            float[] samples = loader.LoadMP3Waveform(mp3Path);
            int sampleRate = loader.SampleRate;
            float duration = loader.Duration;

            Debug.Log($"PreAnalyzer: Loaded {samples.Length} samples, {duration:F2}s");

            // Initialize analysis data structures
            AnalysisData analysisData = new AnalysisData
            {
                Beats = new List<BeatEvent>(),
                IntensityCurve = new List<float>(),
                SampleRate = sampleRate,
                Duration = duration
            };

            List<float> fluxHistory = new List<float>();
            float[] previousSpectrum = null;

            // Perform FFT analysis across entire song
            int windowCount = 0;
            for (int i = 0; i + fftSize < samples.Length; i += hopSize)
            {
                // Extract window
                float[] window = new float[fftSize];
                Array.Copy(samples, i, window, 0, fftSize);

                // Calculate RMS intensity for this window
                float intensity = CalculateRMS(window);
                analysisData.IntensityCurve.Add(intensity);

                // Perform FFT (Hamming window applied inside)
                float[] spectrum = PerformFFT(window);

                // Calculate spectral flux (difference from previous spectrum)
                if (previousSpectrum != null)
                {
                    float flux = 0f;
                    for (int j = 0; j < spectrum.Length; j++)
                    {
                        // Only positive differences (increases in energy)
                        float diff = spectrum[j] - previousSpectrum[j];
                        if (diff > 0)
                            flux += diff;
                    }

                    fluxHistory.Add(flux);

                    // Detect beat if flux exceeds threshold
                    float threshold = GetThreshold(fluxHistory);
                    if (flux > threshold && flux > 0.1f) // Minimum flux to avoid noise
                    {
                        float time = (float)i / sampleRate;
                        analysisData.Beats.Add(new BeatEvent
                        {
                            Time = time,
                            Strength = flux
                        });
                    }
                }

                previousSpectrum = spectrum;
                windowCount++;
            }

            Debug.Log($"PreAnalyzer: Processed {windowCount} windows, detected {analysisData.Beats.Count} beats");

            // Estimate BPM from detected beats
            analysisData.BPM = EstimateBPM(analysisData.Beats);

            // Generate deterministic level seed
            analysisData.LevelSeed = GenerateSeed(analysisData.Beats, analysisData.IntensityCurve, analysisData.BPM);

            Debug.Log($"PreAnalyzer: Analysis complete! BPM={analysisData.BPM:F1}, Seed={analysisData.LevelSeed}");

            return Task.FromResult(analysisData);
        }

        /// <summary>
        /// Performs FFT on audio window and returns frequency spectrum.
        /// </summary>
        /// <param name="samples">Audio samples (should be fftSize length).</param>
        /// <returns>Frequency spectrum magnitudes.</returns>
        private float[] PerformFFT(float[] samples)
        {
            // Apply Hamming window to reduce spectral leakage
            SimpleFFT.ApplyHammingWindow(samples);

            // Perform FFT and get magnitude spectrum
            float[] spectrum = SimpleFFT.Compute(samples);

            return spectrum;
        }

        /// <summary>
        /// Calculates dynamic threshold for beat detection based on recent flux history.
        /// </summary>
        /// <param name="fluxHistory">Recent spectral flux values.</param>
        /// <returns>Threshold value for beat detection.</returns>
        private float GetThreshold(List<float> fluxHistory)
        {
            if (fluxHistory.Count == 0)
                return 0f;

            // Use sliding window of last 100 values for adaptive threshold
            int windowSize = Mathf.Min(100, fluxHistory.Count);
            int startIndex = Mathf.Max(0, fluxHistory.Count - windowSize);

            float sum = 0f;
            for (int i = startIndex; i < fluxHistory.Count; i++)
            {
                sum += fluxHistory[i];
            }

            float mean = sum / windowSize;
            return mean * thresholdMultiplier;
        }

        /// <summary>
        /// Calculates RMS (Root Mean Square) intensity of audio window.
        /// </summary>
        /// <param name="samples">Audio samples.</param>
        /// <returns>RMS intensity value (0.0 to 1.0).</returns>
        private float CalculateRMS(float[] samples)
        {
            if (samples.Length == 0)
                return 0f;

            // Sum squares of all samples
            float sumOfSquares = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sumOfSquares += samples[i] * samples[i];
            }

            // Calculate RMS: sqrt(mean(squares))
            float rms = Mathf.Sqrt(sumOfSquares / samples.Length);

            // Clamp to 0.0-1.0 range (samples are already normalized to -1 to 1)
            return Mathf.Clamp01(rms);
        }

        /// <summary>
        /// Estimates BPM from detected beat events using inter-beat interval analysis.
        /// </summary>
        /// <param name="beats">List of detected beat events.</param>
        /// <returns>Estimated BPM value.</returns>
        private float EstimateBPM(List<BeatEvent> beats)
        {
            if (beats.Count < 2)
                return 120f; // Default BPM if not enough beats

            // Calculate intervals between consecutive beats
            List<float> intervals = new List<float>();
            for (int i = 1; i < beats.Count; i++)
            {
                float interval = beats[i].Time - beats[i - 1].Time;
                // Filter out very short/long intervals (likely false positives)
                if (interval > 0.2f && interval < 2.0f)
                {
                    intervals.Add(interval);
                }
            }

            if (intervals.Count == 0)
                return 120f;

            // Find median interval (more robust than mean)
            intervals.Sort();
            float medianInterval = intervals[intervals.Count / 2];

            // Convert interval to BPM: BPM = 60 / interval
            float bpm = 60f / medianInterval;

            // Clamp to reasonable range (40-200 BPM)
            bpm = Mathf.Clamp(bpm, 40f, 200f);

            Debug.Log($"PreAnalyzer: Estimated BPM = {bpm:F1} from {beats.Count} beats");

            return bpm;
        }

        /// <summary>
        /// Generates deterministic seed from audio features.
        /// Same MP3 always produces same seed = same level layout.
        /// </summary>
        /// <param name="beats">Detected beat events.</param>
        /// <param name="intensityCurve">Intensity curve data.</param>
        /// <param name="bpm">Estimated BPM.</param>
        /// <returns>Deterministic integer seed.</returns>
        private int GenerateSeed(List<BeatEvent> beats, List<float> intensityCurve, float bpm)
        {
            int seed = 0;

            // Hash beat positions (first 100 beats)
            int beatCount = Mathf.Min(100, beats.Count);
            for (int i = 0; i < beatCount; i++)
            {
                // Use beat time and strength to create unique hash
                int beatHash = (int)(beats[i].Time * 1000) ^ (int)(beats[i].Strength * 1000);
                seed ^= beatHash << (i % 16);
            }

            // Include BPM in seed
            seed ^= (int)(bpm * 100);

            // Include average intensity
            if (intensityCurve.Count > 0)
            {
                float avgIntensity = intensityCurve.Average();
                seed ^= (int)(avgIntensity * 10000);
            }

            // Include song duration for additional uniqueness
            if (intensityCurve.Count > 0)
            {
                seed ^= intensityCurve.Count;
            }

            Debug.Log($"PreAnalyzer: Generated deterministic seed = {seed}");

            return seed;
        }

        /// <summary>
        /// Saves analysis data to disk as JSON file.
        /// </summary>
        /// <param name="hash">Song hash for file naming.</param>
        /// <param name="analysis">Analysis data to save.</param>
        private void SaveAnalysis(string hash, AnalysisData analysis)
        {
            // TODO: Convert AnalysisData to JSON using JsonUtility
            // TODO: Save to library/[hash]/analysis.json
            // TODO: Handle file write errors

            throw new NotImplementedException();
        }

        // TODO: Add support for Essentia library integration (optional, more accurate)
        // TODO: Add genre-specific analysis profiles
        // TODO: Add section detection (verse, chorus, bridge)
        // TODO: Add frequency band analysis (bass, mid, treble)
    }
}
