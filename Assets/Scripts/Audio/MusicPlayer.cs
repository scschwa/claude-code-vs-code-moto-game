using UnityEngine;
using DesertRider.MP3;

namespace DesertRider.Audio
{
    /// <summary>
    /// Simple music player for playing the analyzed MP3 during gameplay.
    /// Synchronizes with terrain generation timeline.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Auto-play music on start")]
        public bool autoPlay = false;

        [Tooltip("Volume (0-1)")]
        [Range(0f, 1f)]
        public float volume = 0.7f;

        private AudioSource audioSource;
        private string currentMP3Path;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = volume;
        }

        /// <summary>
        /// Loads and plays an MP3 file.
        /// </summary>
        /// <param name="mp3Path">Path to MP3 file.</param>
        public void PlayMP3(string mp3Path)
        {
            if (string.IsNullOrEmpty(mp3Path))
            {
                Debug.LogError("MusicPlayer: MP3 path is null or empty!");
                return;
            }

            if (!System.IO.File.Exists(mp3Path))
            {
                Debug.LogError($"MusicPlayer: MP3 file not found: {mp3Path}");
                return;
            }

            // Load MP3 as AudioClip
            MP3Loader loader = MP3Loader.Instance;
            if (loader == null)
            {
                Debug.LogError("MusicPlayer: MP3Loader instance not found!");
                return;
            }

            try
            {
                AudioClip clip = loader.LoadMP3AsAudioClip(mp3Path);
                audioSource.clip = clip;
                audioSource.Play();
                currentMP3Path = mp3Path;

                Debug.Log($"MusicPlayer: Now playing {System.IO.Path.GetFileName(mp3Path)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"MusicPlayer: Failed to load/play MP3: {e.Message}");
            }
        }

        /// <summary>
        /// Stops music playback.
        /// </summary>
        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        /// <summary>
        /// Pauses music playback.
        /// </summary>
        public void Pause()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// Resumes music playback.
        /// </summary>
        public void Resume()
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }

        /// <summary>
        /// Gets current playback time in seconds.
        /// </summary>
        public float CurrentTime => audioSource != null ? audioSource.time : 0f;

        /// <summary>
        /// Gets whether music is currently playing.
        /// </summary>
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;

        /// <summary>
        /// Sets playback volume.
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }
}
