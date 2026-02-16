using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using DesertRider.UI;
using DesertRider.Core;
using DesertRider.Gameplay;

/// <summary>
/// Automated scene setup tool for Desert Rider.
/// Creates all 5 game scenes with complete UI hierarchies and component assignments.
/// Run via: Tools → Desert Rider → Setup All Scenes
/// </summary>
public class DesertRiderSceneSetup : EditorWindow
{
    private static string scenesPath = "Assets/Scenes";
    private static string prefabsPath = "Assets/Prefabs/UI";

    [MenuItem("Tools/Desert Rider/Setup All Scenes")]
    static void SetupAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Desert Rider Scene Setup",
            "This will create 5 new scenes:\n\n" +
            "• MainMenu.unity\n" +
            "• SongSelection.unity\n" +
            "• Loading.unity\n" +
            "• Gameplay.unity\n" +
            "• Results.unity\n\n" +
            "Any existing scenes will be overwritten. Continue?",
            "Yes, Create Scenes", "Cancel"))
        {
            return;
        }

        Debug.Log("=== Desert Rider Scene Setup Started ===");

        // Create directories
        EnsureDirectoriesExist();

        // Create scenes
        CreateMainMenuScene();
        CreateSongSelectionScene();
        CreateLoadingScene();
        CreateGameplayScene();
        CreateResultsScene();

        // Add scenes to build settings
        AddScenesToBuildSettings();

        Debug.Log("=== Desert Rider Scene Setup Complete! ===");
        EditorUtility.DisplayDialog("Setup Complete!",
            "All scenes have been created successfully!\n\n" +
            "Scenes are in: Assets/Scenes/\n" +
            "Prefabs are in: Assets/Prefabs/UI/\n\n" +
            "Open MainMenu scene and press Play to start!",
            "OK");
    }

    #region Directory Setup
    static void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(scenesPath))
        {
            Directory.CreateDirectory(scenesPath);
            AssetDatabase.Refresh();
            Debug.Log($"Created directory: {scenesPath}");
        }

        if (!Directory.Exists(prefabsPath))
        {
            Directory.CreateDirectory(prefabsPath);
            AssetDatabase.Refresh();
            Debug.Log($"Created directory: {prefabsPath}");
        }
    }
    #endregion

    #region Scene 1: Main Menu
    static void CreateMainMenuScene()
    {
        Debug.Log("Creating MainMenu scene...");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create GameFlowManager (persists across scenes)
        GameObject gfm = new GameObject("GameFlowManager");
        gfm.AddComponent<GameFlowManager>();

        // Create Canvas
        GameObject canvasGO = CreateCanvas();
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // Create Event System
        CreateEventSystem();

        // Title Text
        GameObject titleText = CreateTextTMP(canvasGO.transform, "TitleText", "DESERT RIDER", 72);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -100);
        titleRect.sizeDelta = new Vector2(800, 100);

        // Version Text
        GameObject versionText = CreateTextTMP(canvasGO.transform, "VersionText", "v1.0", 18);
        RectTransform versionRect = versionText.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(1f, 0f);
        versionRect.anchorMax = new Vector2(1f, 0f);
        versionRect.anchoredPosition = new Vector2(-20, 20);
        versionRect.sizeDelta = new Vector2(100, 30);
        versionText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.BottomRight;

        // Play Button
        GameObject playButton = CreateButton(canvasGO.transform, "PlayButton", "PLAY", new Vector2(0, 0), new Vector2(300, 60));

        // Settings Button
        GameObject settingsButton = CreateButton(canvasGO.transform, "SettingsButton", "SETTINGS", new Vector2(0, -80), new Vector2(300, 60));

        // Quit Button
        GameObject quitButton = CreateButton(canvasGO.transform, "QuitButton", "QUIT", new Vector2(0, -160), new Vector2(300, 60));

        // Settings Panel (hidden by default)
        GameObject settingsPanel = CreatePanel(canvasGO.transform, "SettingsPanel", new Vector2(0, 0), new Vector2(600, 400));
        settingsPanel.SetActive(false);

        // Settings Panel Contents
        GameObject nameLabel = CreateTextTMP(settingsPanel.transform, "NameLabel", "Player Name:", 24);
        SetRectTransform(nameLabel, new Vector2(0, 100), new Vector2(500, 30));

        GameObject nameInput = CreateInputField(settingsPanel.transform, "PlayerNameInput", "Enter your name", new Vector2(0, 60), new Vector2(500, 40));

        GameObject volumeLabel = CreateTextTMP(settingsPanel.transform, "VolumeLabel", "Music Volume:", 24);
        SetRectTransform(volumeLabel, new Vector2(0, 0), new Vector2(500, 30));

        GameObject volumeSlider = CreateSlider(settingsPanel.transform, "MusicVolumeSlider", new Vector2(0, -40), new Vector2(500, 30));

        // Add MainMenuController
        GameObject controller = new GameObject("MainMenuController");
        controller.transform.SetParent(canvasGO.transform);
        MainMenuController menuController = controller.AddComponent<MainMenuController>();

        // Assign references
        menuController.playButton = playButton.GetComponent<Button>();
        menuController.settingsButton = settingsButton.GetComponent<Button>();
        menuController.quitButton = quitButton.GetComponent<Button>();
        menuController.playerNameInput = nameInput.GetComponent<TMP_InputField>();
        menuController.settingsPanel = settingsPanel;
        menuController.musicVolumeSlider = volumeSlider.GetComponent<Slider>();
        menuController.titleText = titleText.GetComponent<TextMeshProUGUI>();
        menuController.versionText = versionText.GetComponent<TextMeshProUGUI>();

        // Save scene
        EditorSceneManager.SaveScene(scene, $"{scenesPath}/MainMenu.unity");
        Debug.Log("✅ MainMenu scene created");
    }
    #endregion

    #region Scene 2: Song Selection
    static void CreateSongSelectionScene()
    {
        Debug.Log("Creating SongSelection scene...");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject canvasGO = CreateCanvas();
        CreateEventSystem();

        // Title
        GameObject titleText = CreateTextTMP(canvasGO.transform, "TitleText", "SELECT SONG", 48);
        SetRectTransform(titleText, new Vector2(0, 450), new Vector2(600, 80));

        // Folder Path Text
        GameObject folderText = CreateTextTMP(canvasGO.transform, "FolderPathText", "Scanning: My Music", 16);
        SetRectTransform(folderText, new Vector2(0, 380), new Vector2(800, 30));

        // Selection Text
        GameObject selectionText = CreateTextTMP(canvasGO.transform, "SelectionText", "No song selected", 20);
        SetRectTransform(selectionText, new Vector2(0, -350), new Vector2(600, 40));
        selectionText.GetComponent<TextMeshProUGUI>().color = Color.gray;

        // Scroll View for song list
        GameObject scrollView = CreateScrollView(canvasGO.transform, "SongListScrollView",
            new Vector2(0, 0), new Vector2(800, 600));

        Transform content = scrollView.transform.Find("Viewport/Content");

        // Create Song List Item Prefab
        GameObject songListItemPrefab = CreateSongListItemPrefab();

        // Buttons
        GameObject startButton = CreateButton(canvasGO.transform, "StartButton", "START", new Vector2(0, -420), new Vector2(200, 50));
        startButton.GetComponent<Button>().interactable = false;

        GameObject browseButton = CreateButton(canvasGO.transform, "BrowseButton", "BROWSE FILE", new Vector2(-220, -420), new Vector2(200, 50));
        GameObject refreshButton = CreateButton(canvasGO.transform, "RefreshButton", "REFRESH", new Vector2(220, -420), new Vector2(150, 50));
        GameObject backButton = CreateButton(canvasGO.transform, "BackButton", "BACK", new Vector2(-850, -500), new Vector2(150, 50));

        // Add SongSelectionController
        GameObject controller = new GameObject("SongSelectionController");
        controller.transform.SetParent(canvasGO.transform);
        SongSelectionController songController = controller.AddComponent<SongSelectionController>();

        songController.songListContainer = content;
        songController.songListItemPrefab = songListItemPrefab;
        songController.startButton = startButton.GetComponent<Button>();
        songController.backButton = backButton.GetComponent<Button>();
        songController.browseButton = browseButton.GetComponent<Button>();
        songController.refreshButton = refreshButton.GetComponent<Button>();
        songController.selectionText = selectionText.GetComponent<TextMeshProUGUI>();
        songController.folderPathText = folderText.GetComponent<TextMeshProUGUI>();
        songController.scanSubfolders = true;

        EditorSceneManager.SaveScene(scene, $"{scenesPath}/SongSelection.unity");
        Debug.Log("✅ SongSelection scene created");
    }

    static GameObject CreateSongListItemPrefab()
    {
        GameObject prefab = CreateButton(null, "SongListItem", "Song Name", Vector2.zero, new Vector2(800, 50));

        // Save as prefab
        string prefabPath = $"{prefabsPath}/SongListItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

        GameObject.DestroyImmediate(prefab);

        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }
    #endregion

    #region Scene 3: Loading
    static void CreateLoadingScene()
    {
        Debug.Log("Creating Loading scene...");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject canvasGO = CreateCanvas();
        CreateEventSystem();

        // Song Title
        GameObject songTitle = CreateTextTMP(canvasGO.transform, "SongTitleText", "Loading...", 48);
        SetRectTransform(songTitle, new Vector2(0, 300), new Vector2(800, 80));

        // Status Text
        GameObject statusText = CreateTextTMP(canvasGO.transform, "StatusText", "Analyzing music...", 24);
        SetRectTransform(statusText, new Vector2(0, 0), new Vector2(600, 40));

        // Progress Bar
        GameObject progressBar = CreateSlider(canvasGO.transform, "ProgressBar", new Vector2(0, -100), new Vector2(600, 30));
        Slider slider = progressBar.GetComponent<Slider>();
        slider.interactable = false;
        // Remove handle
        Transform handleSlideArea = progressBar.transform.Find("Handle Slide Area");
        if (handleSlideArea != null)
            GameObject.DestroyImmediate(handleSlideArea.gameObject);

        // Progress Text
        GameObject progressText = CreateTextTMP(canvasGO.transform, "ProgressText", "0%", 20);
        SetRectTransform(progressText, new Vector2(0, -70), new Vector2(100, 30));

        // Cancel Button
        GameObject cancelButton = CreateButton(canvasGO.transform, "CancelButton", "CANCEL", new Vector2(0, -300), new Vector2(200, 50));

        // Add LoadingScreenController
        GameObject controller = new GameObject("LoadingScreenController");
        controller.transform.SetParent(canvasGO.transform);
        LoadingScreenController loadingController = controller.AddComponent<LoadingScreenController>();

        loadingController.progressBar = progressBar.GetComponent<Slider>();
        loadingController.progressText = progressText.GetComponent<TextMeshProUGUI>();
        loadingController.statusText = statusText.GetComponent<TextMeshProUGUI>();
        loadingController.songTitleText = songTitle.GetComponent<TextMeshProUGUI>();
        loadingController.cancelButton = cancelButton.GetComponent<Button>();

        EditorSceneManager.SaveScene(scene, $"{scenesPath}/Loading.unity");
        Debug.Log("✅ Loading scene created");
    }
    #endregion

    #region Scene 4: Gameplay
    static void CreateGameplayScene()
    {
        Debug.Log("Creating Gameplay scene...");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add GameplayController
        GameObject controller = new GameObject("GameplayController");
        GameplayController gameplayController = controller.AddComponent<GameplayController>();

        gameplayController.autoGenerateTerrain = true;
        gameplayController.generateAheadDistance = 50f;
        gameplayController.motorcycleSpawnPosition = new Vector3(0, 3, 5);

        EditorSceneManager.SaveScene(scene, $"{scenesPath}/Gameplay.unity");
        Debug.Log("✅ Gameplay scene created");
    }
    #endregion

    #region Scene 5: Results
    static void CreateResultsScene()
    {
        Debug.Log("Creating Results scene...");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject canvasGO = CreateCanvas();
        CreateEventSystem();

        // Title
        GameObject titleText = CreateTextTMP(canvasGO.transform, "TitleText", "RESULTS", 56);
        SetRectTransform(titleText, new Vector2(0, 450), new Vector2(600, 80));

        // Song Title
        GameObject songTitle = CreateTextTMP(canvasGO.transform, "SongTitleText", "Song Name", 36);
        SetRectTransform(songTitle, new Vector2(0, 380), new Vector2(700, 50));

        // Score Panel
        GameObject scorePanel = CreatePanel(canvasGO.transform, "ScorePanel", new Vector2(-400, 200), new Vector2(350, 300));

        GameObject scoreLabel = CreateTextTMP(scorePanel.transform, "ScoreLabel", "SCORE", 20);
        SetRectTransform(scoreLabel, new Vector2(0, 80), new Vector2(300, 30));

        GameObject scoreText = CreateTextTMP(scorePanel.transform, "ScoreText", "0", 48);
        SetRectTransform(scoreText, new Vector2(0, 40), new Vector2(300, 60));
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.cyan;

        GameObject coinsLabel = CreateTextTMP(scorePanel.transform, "CoinsLabel", "COINS", 18);
        SetRectTransform(coinsLabel, new Vector2(0, -20), new Vector2(300, 25));

        GameObject coinsText = CreateTextTMP(scorePanel.transform, "CoinsText", "0", 24);
        SetRectTransform(coinsText, new Vector2(0, -50), new Vector2(300, 30));

        GameObject comboLabel = CreateTextTMP(scorePanel.transform, "ComboLabel", "MAX COMBO", 18);
        SetRectTransform(comboLabel, new Vector2(0, -90), new Vector2(300, 25));

        GameObject comboText = CreateTextTMP(scorePanel.transform, "MaxComboText", "0x", 24);
        SetRectTransform(comboText, new Vector2(0, -120), new Vector2(300, 30));

        // Rank Text
        GameObject rankText = CreateTextTMP(canvasGO.transform, "RankText", "Rank: #1", 32);
        SetRectTransform(rankText, new Vector2(-400, 50), new Vector2(300, 50));
        rankText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.84f, 0f); // Gold

        // New High Score Indicator
        GameObject highScoreIndicator = CreatePanel(canvasGO.transform, "NewHighScoreIndicator",
            new Vector2(-400, -50), new Vector2(350, 80));
        GameObject highScoreText = CreateTextTMP(highScoreIndicator.transform, "HighScoreText",
            "NEW HIGH SCORE!", 24);
        SetRectTransform(highScoreText, Vector2.zero, new Vector2(330, 60));
        highScoreText.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        highScoreIndicator.SetActive(false);

        // Leaderboard Panel
        GameObject leaderboardPanel = CreatePanel(canvasGO.transform, "LeaderboardPanel",
            new Vector2(350, 50), new Vector2(600, 500));

        GameObject leaderboardTitle = CreateTextTMP(leaderboardPanel.transform, "LeaderboardTitle",
            "LEADERBOARD", 28);
        SetRectTransform(leaderboardTitle, new Vector2(0, 210), new Vector2(550, 40));

        GameObject leaderboardScroll = CreateScrollView(leaderboardPanel.transform, "LeaderboardScrollView",
            new Vector2(0, -30), new Vector2(580, 400));

        Transform leaderboardContent = leaderboardScroll.transform.Find("Viewport/Content");

        // Create Leaderboard Entry Prefab
        GameObject leaderboardEntryPrefab = CreateLeaderboardEntryPrefab();

        // Buttons
        GameObject playAgainButton = CreateButton(canvasGO.transform, "PlayAgainButton", "PLAY AGAIN",
            new Vector2(-200, -450), new Vector2(180, 50));

        GameObject songSelectButton = CreateButton(canvasGO.transform, "SongSelectButton", "SONG SELECT",
            new Vector2(0, -450), new Vector2(180, 50));

        GameObject mainMenuButton = CreateButton(canvasGO.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(200, -450), new Vector2(180, 50));

        // Add ResultsScreenController
        GameObject controller = new GameObject("ResultsScreenController");
        controller.transform.SetParent(canvasGO.transform);
        ResultsScreenController resultsController = controller.AddComponent<ResultsScreenController>();

        resultsController.scoreText = scoreText.GetComponent<TextMeshProUGUI>();
        resultsController.coinsText = coinsText.GetComponent<TextMeshProUGUI>();
        resultsController.maxComboText = comboText.GetComponent<TextMeshProUGUI>();
        resultsController.songTitleText = songTitle.GetComponent<TextMeshProUGUI>();
        resultsController.newHighScoreIndicator = highScoreIndicator;
        resultsController.rankText = rankText.GetComponent<TextMeshProUGUI>();
        resultsController.leaderboardContainer = leaderboardContent;
        resultsController.leaderboardEntryPrefab = leaderboardEntryPrefab;
        resultsController.maxLeaderboardEntries = 10;
        resultsController.playAgainButton = playAgainButton.GetComponent<Button>();
        resultsController.mainMenuButton = mainMenuButton.GetComponent<Button>();
        resultsController.songSelectButton = songSelectButton.GetComponent<Button>();

        EditorSceneManager.SaveScene(scene, $"{scenesPath}/Results.unity");
        Debug.Log("✅ Results scene created");
    }

    static GameObject CreateLeaderboardEntryPrefab()
    {
        // Create a panel for the entry
        GameObject entry = new GameObject("LeaderboardEntry");
        RectTransform rect = entry.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(550, 60);

        Image img = entry.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        HorizontalLayoutGroup layout = entry.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 10;

        // Rank Text
        GameObject rankText = CreateTextTMP(entry.transform, "RankText", "#1", 24);
        rankText.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
        rankText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Player Text
        GameObject playerText = CreateTextTMP(entry.transform, "PlayerText", "Player Name", 20);
        playerText.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        playerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Score Text
        GameObject scoreText = CreateTextTMP(entry.transform, "ScoreText", "10000", 22);
        scoreText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
        scoreText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Details Text
        GameObject detailsText = CreateTextTMP(entry.transform, "DetailsText", "Coins: 50", 14);
        detailsText.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 50);
        detailsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

        // Save as prefab
        string prefabPath = $"{prefabsPath}/LeaderboardEntry.prefab";
        PrefabUtility.SaveAsPrefabAsset(entry, prefabPath);

        GameObject.DestroyImmediate(entry);

        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }
    #endregion

    #region Build Settings
    static void AddScenesToBuildSettings()
    {
        Debug.Log("Adding scenes to Build Settings...");

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene($"{scenesPath}/MainMenu.unity", true),
            new EditorBuildSettingsScene($"{scenesPath}/SongSelection.unity", true),
            new EditorBuildSettingsScene($"{scenesPath}/Loading.unity", true),
            new EditorBuildSettingsScene($"{scenesPath}/Gameplay.unity", true),
            new EditorBuildSettingsScene($"{scenesPath}/Results.unity", true),
        };

        EditorBuildSettings.scenes = scenes;

        Debug.Log("✅ Scenes added to Build Settings");
    }
    #endregion

    #region UI Helper Methods
    static GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        return canvasGO;
    }

    static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    static GameObject CreateTextTMP(Transform parent, string name, string text, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return textGO;
    }

    static GameObject CreateButton(Transform parent, string name, string buttonText, Vector2 position, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name);
        if (parent != null)
            buttonGO.transform.SetParent(parent, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = buttonGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = buttonText;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return buttonGO;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject panelGO = new GameObject(name);
        panelGO.transform.SetParent(parent, false);

        RectTransform rect = panelGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = panelGO.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        return panelGO;
    }

    static GameObject CreateInputField(Transform parent, string name, string placeholder, Vector2 position, Vector2 size)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent, false);

        RectTransform rect = inputGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = inputGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        // Create text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGO.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);
        textArea.AddComponent<RectMask2D>();

        // Create placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Create text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(textArea.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.color = Color.white;

        inputField.textViewport = textAreaRect;
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;

        return inputGO;
    }

    static GameObject CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);

        RectTransform rect = sliderGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.cyan;

        slider.fillRect = fillRect;

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRect;

        return sliderGO;
    }

    static GameObject CreateScrollView(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject scrollViewGO = new GameObject(name);
        scrollViewGO.transform.SetParent(parent, false);

        RectTransform rect = scrollViewGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = scrollViewGO.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewGO.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar Vertical");
        scrollbar.transform.SetParent(scrollViewGO.transform, false);
        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = Vector2.one;
        scrollbarRect.sizeDelta = new Vector2(20, 0);

        Image scrollbarImg = scrollbar.AddComponent<Image>();
        scrollbarImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;

        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform, false);
        RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(5, 5);
        slidingRect.offsetMax = new Vector2(-5, -5);

        GameObject scrollHandle = new GameObject("Handle");
        scrollHandle.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRect = scrollHandle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImg = scrollHandle.AddComponent<Image>();
        handleImg.color = Color.cyan;

        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImg;

        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.verticalScrollbar = scrollbarComponent;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        return scrollViewGO;
    }

    static void SetRectTransform(GameObject go, Vector2 position, Vector2 size)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
    #endregion
}
