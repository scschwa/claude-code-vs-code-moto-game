using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesertRider.MP3
{
    /// <summary>
    /// Represents a single beat event detected in the audio.
    /// </summary>
    [Serializable]
    public struct BeatEvent
    {
        /// <summary>
        /// Time in seconds when the beat occurs.
        /// </summary>
        public float Time;

        /// <summary>
        /// Strength/intensity of the beat (higher = stronger beat).
        /// </summary>
        public float Strength;
    }

    /// <summary>
    /// Data class containing complete audio analysis results from pre-analysis.
    /// Generated during MP3 import and cached for deterministic gameplay.
    /// See TECHNICAL_PLAN.md Section 4.1 for generation details.
    /// </summary>
    [Serializable]
    public class AnalysisData
    {
        /// <summary>
        /// List of all detected beat events throughout the song.
        /// </summary>
        public List<BeatEvent> Beats;

        /// <summary>
        /// Intensity curve sampled at regular intervals throughout the song.
        /// Values range from 0.0 (quiet) to 1.0 (loud).
        /// Used to modulate terrain generation and obstacle density.
        /// </summary>
        public List<float> IntensityCurve;

        /// <summary>
        /// Estimated beats per minute (BPM) for the entire song.
        /// </summary>
        public float BPM;

        /// <summary>
        /// Deterministic seed generated from audio features.
        /// Same MP3 always produces same seed = same level layout.
        /// See TECHNICAL_PLAN.md Section 4.1 for seed generation algorithm.
        /// </summary>
        public int LevelSeed;

        /// <summary>
        /// Sample rate of the analyzed audio (typically 44100 Hz).
        /// </summary>
        public int SampleRate;

        /// <summary>
        /// Total duration of the analyzed audio in seconds.
        /// </summary>
        public float Duration;

        // TODO: Add spectral centroid data for visual effects
        // TODO: Add frequency band energy (bass, mid, treble) for advanced reactivity
        // TODO: Add section markers (verse, chorus, bridge) if using Essentia
    }
}
