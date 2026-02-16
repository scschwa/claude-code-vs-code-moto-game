using UnityEngine;
using NAudio.Wave;
using System;
using System.IO;

namespace DesertRider.MP3
{
    /// <summary>
    /// Handles loading and decoding MP3 files to PCM waveform data.
    /// Uses NAudio's Mp3FileReader for decoding.
    /// See TECHNICAL_PLAN.md Section 4.1 for implementation details.
    /// </summary>
    public class MP3Loader : MonoBehaviour
    {
        #region Singleton Pattern
        public static MP3Loader Instance { get; private set; }

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

        #region Properties
        /// <summary>
        /// Sample rate of the last loaded MP3 file.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Number of channels in the last loaded MP3 file.
        /// </summary>
        public int ChannelCount { get; private set; }

        /// <summary>
        /// Duration in seconds of the last loaded MP3 file.
        /// </summary>
        public float Duration { get; private set; }
        #endregion

        #region Public Methods

        /// <summary>
        /// Loads an MP3 file and decodes it to a mono waveform (array of float samples).
        /// Converts stereo to mono by averaging left and right channels.
        /// </summary>
        /// <param name="mp3Path">Path to the MP3 file.</param>
        /// <returns>Float array containing normalized audio samples (-1.0 to 1.0).</returns>
        /// <exception cref="FileNotFoundException">Thrown when the MP3 file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file is not a valid MP3.</exception>
        public float[] LoadMP3Waveform(string mp3Path)
        {
            if (string.IsNullOrEmpty(mp3Path))
            {
                Debug.LogError("MP3Loader: MP3 path is null or empty.");
                throw new ArgumentNullException(nameof(mp3Path), "MP3 path cannot be null or empty.");
            }

            if (!File.Exists(mp3Path))
            {
                Debug.LogError($"MP3Loader: File not found at path: {mp3Path}");
                throw new FileNotFoundException($"MP3 file not found: {mp3Path}", mp3Path);
            }

            try
            {
                using (var reader = new Mp3FileReader(mp3Path))
                {
                    // Store metadata
                    ChannelCount = reader.WaveFormat.Channels;
                    SampleRate = reader.WaveFormat.SampleRate;
                    int bitsPerSample = reader.WaveFormat.BitsPerSample;

                    // Validate format
                    if (bitsPerSample != 16)
                    {
                        Debug.LogWarning($"MP3Loader: Expected 16-bit PCM, got {bitsPerSample}-bit. Results may be incorrect.");
                    }

                    // Calculate total samples
                    long totalSamples = reader.Length / (bitsPerSample / 8);
                    long monoSamples = totalSamples / ChannelCount;

                    // Calculate duration
                    Duration = (float)monoSamples / SampleRate;

                    // Read all bytes from MP3
                    byte[] buffer = new byte[reader.Length];
                    int bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead != buffer.Length)
                    {
                        Debug.LogWarning($"MP3Loader: Expected to read {buffer.Length} bytes, but read {bytesRead} bytes.");
                    }

                    // Convert to mono float samples
                    float[] samples = ConvertToMonoSamples(buffer, ChannelCount);

                    Debug.Log($"MP3Loader: Successfully loaded {samples.Length} samples from {Path.GetFileName(mp3Path)} " +
                              $"(Duration: {Duration:F2}s, Sample Rate: {SampleRate} Hz, Channels: {ChannelCount})");

                    return samples;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MP3Loader: Error loading MP3 file '{mp3Path}': {ex.Message}\n{ex.StackTrace}");
                throw new InvalidOperationException($"Failed to load MP3 file: {mp3Path}", ex);
            }
        }

        /// <summary>
        /// Loads MP3 file as Unity AudioClip for playback during gameplay.
        /// </summary>
        /// <param name="mp3Path">Path to the MP3 file.</param>
        /// <returns>AudioClip ready for playback.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the MP3 file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file is not a valid MP3.</exception>
        public AudioClip LoadMP3AsAudioClip(string mp3Path)
        {
            if (string.IsNullOrEmpty(mp3Path))
            {
                Debug.LogError("MP3Loader: MP3 path is null or empty.");
                throw new ArgumentNullException(nameof(mp3Path), "MP3 path cannot be null or empty.");
            }

            if (!File.Exists(mp3Path))
            {
                Debug.LogError($"MP3Loader: File not found at path: {mp3Path}");
                throw new FileNotFoundException($"MP3 file not found: {mp3Path}", mp3Path);
            }

            try
            {
                using (var reader = new Mp3FileReader(mp3Path))
                {
                    // Get audio format
                    int channels = reader.WaveFormat.Channels;
                    int sampleRate = reader.WaveFormat.SampleRate;
                    int bitsPerSample = reader.WaveFormat.BitsPerSample;

                    // Calculate total samples
                    long totalSamples = reader.Length / (bitsPerSample / 8);
                    long samplesPerChannel = totalSamples / channels;

                    // Read all bytes
                    byte[] buffer = new byte[reader.Length];
                    int bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead != buffer.Length)
                    {
                        Debug.LogWarning($"MP3Loader: Expected to read {buffer.Length} bytes, but read {bytesRead} bytes.");
                    }

                    // Convert to float samples (preserve original channel count for AudioClip)
                    float[] samples = ConvertToFloatSamples(buffer, channels);

                    // Create AudioClip
                    string clipName = Path.GetFileNameWithoutExtension(mp3Path);
                    AudioClip audioClip = AudioClip.Create(clipName, (int)samplesPerChannel, channels, sampleRate, false);

                    // Set audio data
                    if (!audioClip.SetData(samples, 0))
                    {
                        Debug.LogError("MP3Loader: Failed to set audio data on AudioClip.");
                        throw new InvalidOperationException("Failed to set audio data on AudioClip.");
                    }

                    Debug.Log($"MP3Loader: Successfully created AudioClip '{clipName}' " +
                              $"(Duration: {audioClip.length:F2}s, Sample Rate: {sampleRate} Hz, Channels: {channels})");

                    return audioClip;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MP3Loader: Error creating AudioClip from '{mp3Path}': {ex.Message}\n{ex.StackTrace}");
                throw new InvalidOperationException($"Failed to create AudioClip from MP3 file: {mp3Path}", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Converts byte array (16-bit PCM) to float array (mono).
        /// Averages stereo channels if needed.
        /// </summary>
        /// <param name="buffer">Byte buffer containing 16-bit PCM data.</param>
        /// <param name="channels">Number of audio channels (1 for mono, 2 for stereo).</param>
        /// <returns>Float array with mono samples normalized to -1.0 to 1.0.</returns>
        private float[] ConvertToMonoSamples(byte[] buffer, int channels)
        {
            if (buffer == null || buffer.Length == 0)
            {
                Debug.LogError("MP3Loader: Buffer is null or empty.");
                return new float[0];
            }

            int bytesPerSample = 2; // 16-bit = 2 bytes
            long totalSamples = buffer.Length / bytesPerSample;
            long monoSamples = totalSamples / channels;

            float[] samples = new float[monoSamples];
            int sampleIndex = 0;

            for (int i = 0; i < buffer.Length; i += bytesPerSample * channels)
            {
                // Read left channel (16-bit signed PCM)
                short left = BitConverter.ToInt16(buffer, i);

                if (channels == 2)
                {
                    // Read right channel
                    short right = BitConverter.ToInt16(buffer, i + bytesPerSample);

                    // Average stereo channels to mono
                    float mono = ((left + right) / 2.0f) / 32768f;
                    samples[sampleIndex++] = mono;
                }
                else if (channels == 1)
                {
                    // Already mono
                    samples[sampleIndex++] = left / 32768f;
                }
                else
                {
                    Debug.LogWarning($"MP3Loader: Unsupported channel count: {channels}. Only mono and stereo are supported.");
                    break;
                }
            }

            return samples;
        }

        /// <summary>
        /// Converts byte array (16-bit PCM) to float array, preserving all channels.
        /// Used for creating AudioClip with original channel count.
        /// </summary>
        /// <param name="buffer">Byte buffer containing 16-bit PCM data.</param>
        /// <param name="channels">Number of audio channels.</param>
        /// <returns>Float array with all channel samples normalized to -1.0 to 1.0.</returns>
        private float[] ConvertToFloatSamples(byte[] buffer, int channels)
        {
            if (buffer == null || buffer.Length == 0)
            {
                Debug.LogError("MP3Loader: Buffer is null or empty.");
                return new float[0];
            }

            int bytesPerSample = 2; // 16-bit = 2 bytes
            long totalSamples = buffer.Length / bytesPerSample;

            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                // Read 16-bit signed PCM sample
                short pcmSample = BitConverter.ToInt16(buffer, i * bytesPerSample);

                // Normalize to -1.0 to 1.0
                samples[i] = pcmSample / 32768f;
            }

            return samples;
        }

        #endregion
    }
}
