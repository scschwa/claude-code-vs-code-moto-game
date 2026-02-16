using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertRider.Core;

namespace DesertRider.UI
{
    /// <summary>
    /// Controls the main menu UI with play button, settings, and player name input.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Play button")]
        public Button playButton;

        [Tooltip("Settings button")]
        public Button settingsButton;

        [Tooltip("Quit button")]
        public Button quitButton;

        [Tooltip("Player name input field")]
        public TMP_InputField playerNameInput;

        [Tooltip("Settings panel (can be toggled)")]
        public GameObject settingsPanel;

        [Tooltip("Music volume slider")]
        public Slider musicVolumeSlider;

        [Header("Title Display")]
        [Tooltip("Game title text")]
        public TextMeshProUGUI titleText;

        [Tooltip("Version text")]
        public TextMeshProUGUI versionText;

        void Start()
        {
            // Setup button listeners
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Setup player name input
            if (playerNameInput != null && GameFlowManager.Instance != null)
            {
                playerNameInput.text = GameFlowManager.Instance.playerName;
                playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged);
            }

            // Setup volume slider
            if (musicVolumeSlider != null && GameFlowManager.Instance != null)
            {
                musicVolumeSlider.value = GameFlowManager.Instance.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            // Hide settings panel initially
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // Set version text
            if (versionText != null)
                versionText.text = $"v{Application.version}";

            Debug.Log("MainMenuController: Initialized");
        }

        /// <summary>
        /// Called when Play button is clicked.
        /// </summary>
        private void OnPlayClicked()
        {
            Debug.Log("MainMenuController: Play clicked");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.GoToSongSelection();
            }
            else
            {
                Debug.LogError("MainMenuController: GameFlowManager not found!");
            }
        }

        /// <summary>
        /// Called when Settings button is clicked.
        /// </summary>
        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        /// <summary>
        /// Called when Quit button is clicked.
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("MainMenuController: Quit clicked");

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.QuitGame();
            }
            else
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// Called when player name is changed.
        /// </summary>
        private void OnPlayerNameChanged(string newName)
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetPlayerName(newName);
                Debug.Log($"MainMenuController: Player name set to {newName}");
            }
        }

        /// <summary>
        /// Called when volume slider is changed.
        /// </summary>
        private void OnVolumeChanged(float value)
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetMusicVolume(value);
            }
        }

        void OnDestroy()
        {
            // Clean up listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);

            if (playerNameInput != null)
                playerNameInput.onEndEdit.RemoveListener(OnPlayerNameChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}
