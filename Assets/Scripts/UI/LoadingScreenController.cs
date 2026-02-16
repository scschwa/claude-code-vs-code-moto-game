using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertRider.Core;
using DesertRider.MP3;

namespace DesertRider.UI
{
    /// <summary>
    /// Manages loading screen with MP3 analysis progress display.
    /// Analyzes the selected song and transitions to gameplay when complete.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Progress bar slider")]
        public Slider progressBar;

        [Tooltip("Progress text (percentage)")]
        public TextMeshProUGUI progressText;

        [Tooltip("Status text (current operation)")]
        public TextMeshProUGUI statusText;

        [Tooltip("Song title text")]
        public TextMeshProUGUI songTitleText;

        [Tooltip("Cancel button")]
        public Button cancelButton;

        [Header("References")]
        [Tooltip("PreAnalyzer for MP3 analysis")]
        public PreAnalyzer preAnalyzer;

        [Tooltip("MP3Loader for loading audio")]
        public MP3Loader mp3Loader;

        private bool isAnalyzing = false;
        private bool isCancelled = false;

        void Start()
        {
            // Setup cancel button
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            // Ensure required components exist
            EnsureComponentsExist();

            // Display song title
            if (songTitleText != null && GameFlowManager.Instance != null)
            {
                songTitleText.text = GameFlowManager.Instance.GetCurrentSongTitle();
            }

            // Start analysis
            StartCoroutine(AnalyzeSong());

            Debug.Log("LoadingScreenController: Initialized");
        }

        /// <summary>
        /// Ensures PreAnalyzer and MP3Loader exist in the scene.
        /// </summary>
        private void EnsureComponentsExist()
        {
            if (preAnalyzer == null)
            {
                preAnalyzer = FindFirstObjectByType<PreAnalyzer>();
                if (preAnalyzer == null)
                {
                    GameObject go = new GameObject("PreAnalyzer");
                    preAnalyzer = go.AddComponent<PreAnalyzer>();
                    Debug.Log("LoadingScreenController: Created PreAnalyzer");
                }
            }

            if (mp3Loader == null)
            {
                mp3Loader = FindFirstObjectByType<MP3Loader>();
                if (mp3Loader == null)
                {
                    GameObject go = new GameObject("MP3Loader");
                    mp3Loader = go.AddComponent<MP3Loader>();
                    Debug.Log("LoadingScreenController: Created MP3Loader");
                }
            }
        }

        /// <summary>
        /// Analyzes the selected MP3 file.
        /// </summary>
        private IEnumerator AnalyzeSong()
        {
            if (GameFlowManager.Instance == null)
            {
                Debug.LogError("LoadingScreenController: GameFlowManager not found!");
                yield break;
            }

            string songPath = GameFlowManager.Instance.selectedSongPath;

            if (string.IsNullOrEmpty(songPath) || !System.IO.File.Exists(songPath))
            {
                UpdateStatus("Error: Song file not found!", 0f);
                Debug.LogError($"LoadingScreenController: Invalid song path: {songPath}");
                yield return new WaitForSeconds(2f);
                GameFlowManager.Instance.GoToSongSelection();
                yield break;
            }

            isAnalyzing = true;

            // Step 1: Load MP3
            UpdateStatus("Loading MP3 file...", 0.1f);
            yield return new WaitForSeconds(0.5f);

            if (isCancelled)
            {
                HandleCancellation();
                yield break;
            }

            // Step 2: Create temporary SongData for analysis
            string songTitle = System.IO.Path.GetFileNameWithoutExtension(songPath);
            SongData tempSongData = new SongData
            {
                Title = songTitle,
                Artist = "Unknown",
                LocalPath = songPath
            };

            UpdateStatus("Analyzing music (this may take a moment)...", 0.4f);
            yield return new WaitForSeconds(0.3f);

            if (isCancelled)
            {
                HandleCancellation();
                yield break;
            }

            // Step 3: Analyze audio using PreAnalyzer
            AnalysisData analysisData = null;

            if (preAnalyzer != null)
            {
                // PreAnalyzeSong returns a Task, we need to wait for it
                var analysisTask = preAnalyzer.PreAnalyzeSong(songPath, tempSongData);

                // Simulate progress while analyzing
                float analysisProgress = 0.4f;
                while (!analysisTask.IsCompleted)
                {
                    yield return null;
                    analysisProgress = Mathf.Min(0.9f, analysisProgress + Time.deltaTime * 0.05f);
                    UpdateStatus("Analyzing music...", analysisProgress);
                }

                analysisData = analysisTask.Result;
            }

            if (analysisData == null)
            {
                UpdateStatus("Error: Analysis failed!", 0f);
                Debug.LogError("LoadingScreenController: Analysis failed");
                yield return new WaitForSeconds(2f);
                GameFlowManager.Instance.GoToSongSelection();
                yield break;
            }

            UpdateStatus("Analysis complete!", 0.95f);
            yield return new WaitForSeconds(0.3f);

            if (isCancelled)
            {
                HandleCancellation();
                yield break;
            }

            // Step 4: Finalize SongData
            string hash = ComputeFileHash(songPath);

            SongData songData = new SongData
            {
                Title = songTitle,
                Artist = "Unknown",
                LocalPath = songPath,
                Hash = hash,
                AnalysisData = analysisData
            };

            // Store in GameFlowManager
            GameFlowManager.Instance.SetAnalysisData(analysisData, songData);

            UpdateStatus("Loading gameplay...", 1.0f);
            yield return new WaitForSeconds(0.5f);

            // Transition to gameplay
            isAnalyzing = false;
            GameFlowManager.Instance.GoToGameplay();
        }

        /// <summary>
        /// Updates the status display.
        /// </summary>
        private void UpdateStatus(string message, float progress)
        {
            if (statusText != null)
                statusText.text = message;

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{(progress * 100f):F0}%";
        }

        /// <summary>
        /// Computes MD5 hash of the file (simple version using file size + name).
        /// </summary>
        private string ComputeFileHash(string filePath)
        {
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                long fileSize = fileInfo.Length;
                string fileName = fileInfo.Name;
                return $"{fileName}_{fileSize}".GetHashCode().ToString("X");
            }
            catch
            {
                return System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Called when Cancel button is clicked.
        /// </summary>
        private void OnCancelClicked()
        {
            if (isAnalyzing)
            {
                isCancelled = true;
                Debug.Log("LoadingScreenController: Analysis cancelled");
            }
        }

        /// <summary>
        /// Handles cancellation cleanup.
        /// </summary>
        private void HandleCancellation()
        {
            isAnalyzing = false;
            UpdateStatus("Cancelled", 0f);

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToSongSelection();
            }
        }

        void OnDestroy()
        {
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }
    }
}
