using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertRider.Core;

namespace DesertRider.UI
{
    /// <summary>
    /// Manages song selection UI with file browsing and song list display.
    /// Scans a default music folder and allows custom file selection.
    /// </summary>
    public class SongSelectionController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Container for song list items")]
        public Transform songListContainer;

        [Tooltip("Prefab for song list item (should have Button and TextMeshProUGUI)")]
        public GameObject songListItemPrefab;

        [Tooltip("Start button (begins loading)")]
        public Button startButton;

        [Tooltip("Back button (returns to main menu)")]
        public Button backButton;

        [Tooltip("Browse button (opens file dialog)")]
        public Button browseButton;

        [Tooltip("Refresh button (rescans music folder)")]
        public Button refreshButton;

        [Tooltip("Current selection display text")]
        public TextMeshProUGUI selectionText;

        [Tooltip("Music folder path display")]
        public TextMeshProUGUI folderPathText;

        [Header("Configuration")]
        [Tooltip("Default music folder to scan")]
        public string defaultMusicFolder = "";

        [Tooltip("Scan subfolders")]
        public bool scanSubfolders = true;

        private List<string> availableSongs = new List<string>();
        private string selectedSongPath = "";

        void Start()
        {
            // Set default music folder if not specified
            if (string.IsNullOrEmpty(defaultMusicFolder))
            {
                defaultMusicFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic));
            }

            // Setup button listeners
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
                startButton.interactable = false; // Disabled until song is selected
            }

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (browseButton != null)
                browseButton.onClick.AddListener(OnBrowseClicked);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);

            // Display folder path
            if (folderPathText != null)
                folderPathText.text = $"Scanning: {defaultMusicFolder}";

            // Load last selected song if available
            if (GameFlowManager.Instance != null && !string.IsNullOrEmpty(GameFlowManager.Instance.selectedSongPath))
            {
                selectedSongPath = GameFlowManager.Instance.selectedSongPath;
                UpdateSelectionDisplay();
            }

            // FIX: Ensure Viewport is properly configured (scene setup bug)
            FixScrollViewSetup();

            // Scan for songs
            ScanMusicFolder();

            Debug.Log("SongSelectionController: Initialized");
        }

        /// <summary>
        /// Fixes the ScrollView Viewport setup (corrects scene configuration bug).
        /// </summary>
        private void FixScrollViewSetup()
        {
            if (songListContainer == null)
                return;

            // Find the ScrollRect (should be on grandparent of Content)
            Transform contentParent = songListContainer.parent; // Viewport
            if (contentParent != null)
            {
                RectTransform viewportRect = contentParent.GetComponent<RectTransform>();
                if (viewportRect != null)
                {
                    // Fix Viewport anchors to stretch fill
                    viewportRect.anchorMin = new Vector2(0, 0);
                    viewportRect.anchorMax = new Vector2(1, 1);
                    viewportRect.sizeDelta = Vector2.zero; // Remove offset
                    viewportRect.anchoredPosition = Vector2.zero;

                    // FIX: The Viewport has a Mask component with an Image that has alpha=0
                    // This causes the mask to not work properly and blocks all content from rendering
                    // Set the Image alpha to 1 so the Mask has a proper stencil area
                    UnityEngine.UI.Image viewportImage = contentParent.GetComponent<UnityEngine.UI.Image>();
                    if (viewportImage != null)
                    {
                        Color col = viewportImage.color;
                        col.a = 1f; // Make fully opaque
                        viewportImage.color = col;
                        Debug.Log($"SongSelectionController: Fixed Viewport Image alpha from 0 to 1");
                    }

                    Debug.Log("SongSelectionController: Fixed Viewport RectTransform");
                }

                // Also ensure Content has proper anchors
                RectTransform contentRect = songListContainer.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    contentRect.anchorMin = new Vector2(0, 1); // Top-left anchor
                    contentRect.anchorMax = new Vector2(1, 1); // Top-right anchor
                    contentRect.pivot = new Vector2(0.5f, 1f); // Pivot at top center
                }
            }
        }

        /// <summary>
        /// Scans the default music folder for MP3 files.
        /// </summary>
        private void ScanMusicFolder()
        {
            availableSongs.Clear();

            if (!Directory.Exists(defaultMusicFolder))
            {
                Debug.LogWarning($"SongSelectionController: Music folder not found: {defaultMusicFolder}");
                UpdateSelectionDisplay("No music folder found. Use Browse to select an MP3 file.");
                return;
            }

            try
            {
                SearchOption searchOption = scanSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] mp3Files = Directory.GetFiles(defaultMusicFolder, "*.mp3", searchOption);

                availableSongs.AddRange(mp3Files);
                availableSongs.Sort(); // Sort alphabetically

                Debug.Log($"SongSelectionController: Found {availableSongs.Count} MP3 files");

                PopulateSongList();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SongSelectionController: Error scanning folder: {e.Message}");
                UpdateSelectionDisplay("Error scanning music folder. Use Browse to select a file.");
            }
        }

        /// <summary>
        /// Populates the UI list with available songs.
        /// </summary>
        private void PopulateSongList()
        {
            if (songListContainer == null || songListItemPrefab == null)
            {
                Debug.LogWarning("SongSelectionController: Missing UI references for song list");
                return;
            }

            // Clear existing list
            foreach (Transform child in songListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create list items
            foreach (string songPath in availableSongs)
            {
                GameObject listItem = Instantiate(songListItemPrefab, songListContainer);

                // Get components
                Button button = listItem.GetComponent<Button>();
                TextMeshProUGUI text = listItem.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(songPath);
                    text.text = fileName;

                    // Force text to be visible
                    text.color = Color.white;
                    text.fontSize = 24;
                    text.enabled = true;
                    text.gameObject.SetActive(true);

                    Debug.Log($"Created song item: {fileName}");
                    Debug.Log($"Text - Color: {text.color}, Size: {text.fontSize}, Enabled: {text.enabled}, Active: {text.gameObject.activeSelf}");
                    Debug.Log($"Text - Font: {(text.font != null ? text.font.name : "NULL")}, Canvas: {text.canvas != null}");
                }
                else
                {
                    Debug.LogWarning($"Song item created but TextMeshProUGUI component not found!");
                }

                if (button != null)
                {
                    string capturedPath = songPath; // Capture for closure
                    button.onClick.AddListener(() => OnSongSelected(capturedPath));
                }
                else
                {
                    Debug.LogWarning($"Song item created but Button component not found!");
                }

                // Add LayoutElement to ensure proper sizing in VerticalLayoutGroup
                UnityEngine.UI.LayoutElement layoutElement = listItem.GetComponent<UnityEngine.UI.LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = listItem.AddComponent<UnityEngine.UI.LayoutElement>();
                }
                layoutElement.minHeight = 50f;
                layoutElement.preferredHeight = 50f;

                // Debug: Log item details
                RectTransform rectTransform = listItem.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Debug.Log($"Item position: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}, Active: {listItem.activeSelf}");
                }
            }

            // Force layout rebuild
            if (songListContainer != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(songListContainer.GetComponent<RectTransform>());
            }

            Debug.Log($"SongSelectionController: Populated list with {availableSongs.Count} songs");
            Debug.Log($"Song list container child count: {songListContainer.childCount}");

            // Debug: Check Content RectTransform
            RectTransform contentRect = songListContainer.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                Debug.Log($"Content - Position: {contentRect.anchoredPosition}, Size: {contentRect.rect.size}, Anchors: Min{contentRect.anchorMin} Max{contentRect.anchorMax}");

                // Check parent (Viewport)
                if (contentRect.parent != null)
                {
                    RectTransform viewportRect = contentRect.parent.GetComponent<RectTransform>();
                    if (viewportRect != null)
                    {
                        Debug.Log($"Viewport - Position: {viewportRect.anchoredPosition}, Size: {viewportRect.rect.size}");
                    }
                }
            }
        }

        /// <summary>
        /// Called when a song is selected from the list.
        /// </summary>
        private void OnSongSelected(string songPath)
        {
            selectedSongPath = songPath;
            UpdateSelectionDisplay();

            if (startButton != null)
                startButton.interactable = true;

            Debug.Log($"SongSelectionController: Selected {Path.GetFileNameWithoutExtension(songPath)}");
        }

        /// <summary>
        /// Updates the selection display text.
        /// </summary>
        private void UpdateSelectionDisplay(string customMessage = null)
        {
            if (selectionText == null)
                return;

            if (!string.IsNullOrEmpty(customMessage))
            {
                selectionText.text = customMessage;
            }
            else if (!string.IsNullOrEmpty(selectedSongPath))
            {
                string fileName = Path.GetFileNameWithoutExtension(selectedSongPath);
                selectionText.text = $"Selected: {fileName}";
                selectionText.color = Color.cyan;
            }
            else
            {
                selectionText.text = "No song selected";
                selectionText.color = Color.gray;
            }
        }

        /// <summary>
        /// Called when Start button is clicked.
        /// </summary>
        private void OnStartClicked()
        {
            if (string.IsNullOrEmpty(selectedSongPath))
            {
                Debug.LogWarning("SongSelectionController: No song selected");
                return;
            }

            if (!File.Exists(selectedSongPath))
            {
                Debug.LogError($"SongSelectionController: File not found: {selectedSongPath}");
                UpdateSelectionDisplay("File not found! Please select another song.");
                return;
            }

            Debug.Log($"SongSelectionController: Starting with {selectedSongPath}");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToLoading(selectedSongPath);
            }
            else
            {
                Debug.LogError("SongSelectionController: GameFlowManager not found!");
            }
        }

        /// <summary>
        /// Called when Back button is clicked.
        /// </summary>
        private void OnBackClicked()
        {
            Debug.Log("SongSelectionController: Back clicked");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToMainMenu();
            }
        }

        /// <summary>
        /// Called when Browse button is clicked (opens file dialog).
        /// </summary>
        private void OnBrowseClicked()
        {
            Debug.Log("SongSelectionController: Browse clicked");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // Use Windows file dialog
            string selectedFile = OpenFileDialog("Select MP3 File", "MP3 Files", "mp3");

            if (!string.IsNullOrEmpty(selectedFile))
            {
                selectedSongPath = selectedFile;
                UpdateSelectionDisplay();

                if (startButton != null)
                    startButton.interactable = true;

                Debug.Log($"SongSelectionController: File selected via dialog: {selectedFile}");
            }
#else
            Debug.LogWarning("SongSelectionController: File dialog not implemented for this platform");
#endif
        }

        /// <summary>
        /// Called when Refresh button is clicked.
        /// </summary>
        private void OnRefreshClicked()
        {
            Debug.Log("SongSelectionController: Refreshing song list");
            ScanMusicFolder();
        }

        /// <summary>
        /// Opens a file dialog (platform-specific).
        /// Note: Windows file dialog requires System.Windows.Forms which is not available in Unity.
        /// For now, users should place MP3s in the Music folder or use the file path directly.
        /// </summary>
        private string OpenFileDialog(string title, string filterName, string filterExtension)
        {
            // TODO: Implement file browser using Unity's EditorUtility.OpenFilePanel (Editor only)
            // or a third-party file browser for runtime
            Debug.LogWarning("SongSelectionController: File dialog not implemented. Please use the Music folder.");
            return null;
        }

        void OnDestroy()
        {
            // Clean up listeners
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            if (browseButton != null)
                browseButton.onClick.RemoveListener(OnBrowseClicked);

            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
        }
    }
}
