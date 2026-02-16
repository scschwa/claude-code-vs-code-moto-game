# Unity Setup Guide - Desert Rider Menu System

This guide explains how to set up the complete menu flow system for Desert Rider.

## Overview

The game flow consists of 5 scenes:
1. **MainMenu** - Entry point with play button and settings
2. **SongSelection** - Browse and select MP3 files
3. **Loading** - Shows analysis progress
4. **Gameplay** - The main game
5. **Results** - Post-game score and leaderboard

---

## Quick Setup Checklist

✅ **Prerequisites:**
- Unity 6 or later
- TextMeshPro package installed
- All scripts compiled without errors

📁 **Files Created:**
- `GameFlowManager.cs` - Scene management
- `MainMenuController.cs` - Main menu UI
- `SongSelectionController.cs` - Song browser
- `LoadingScreenController.cs` - Loading screen
- `ResultsScreenController.cs` - Results display
- `GameplayController.cs` - Gameplay manager

---

## Scene 1: MainMenu

### Create the Scene
1. **File → New Scene** → Name it `MainMenu`
2. **Save** to `Assets/Scenes/MainMenu.unity`

### Add GameFlowManager
1. **Create empty GameObject** → Name: `GameFlowManager`
2. **Add Component** → `GameFlowManager` script
3. This persists across all scenes (don't add it to other scenes!)

### Create UI
1. **Right-click Hierarchy** → **UI → Canvas**
2. **Set Canvas Scaler**:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

3. **Create UI Elements** (all as children of Canvas):

   **Title Text:**
   - Right-click Canvas → **UI → TextMeshPro - Text**
   - Name: `TitleText`
   - Text: `DESERT RIDER`
   - Font Size: `72`
   - Alignment: Center/Top
   - Position: Top center of screen

   **Play Button:**
   - Right-click Canvas → **UI → Button - TextMeshPro**
   - Name: `PlayButton`
   - Text: `PLAY`
   - Position: Center of screen

   **Settings Button:**
   - Duplicate Play Button
   - Name: `SettingsButton`
   - Text: `SETTINGS`
   - Position: Below Play button

   **Quit Button:**
   - Duplicate Settings Button
   - Name: `QuitButton`
   - Text: `QUIT`
   - Position: Below Settings button

   **Settings Panel** (initially hidden):
   - Right-click Canvas → **UI → Panel**
   - Name: `SettingsPanel`
   - Add child: **UI → TextMeshPro - Text** → Text: `Player Name:`
   - Add child: **UI → InputField - TextMeshPro** → Name: `PlayerNameInput`
   - Add child: **UI → TextMeshPro - Text** → Text: `Music Volume:`
   - Add child: **UI → Slider** → Name: `MusicVolumeSlider`
   - Set SettingsPanel Active: `false` (uncheck in Inspector)

   **Version Text:**
   - Right-click Canvas → **UI → TextMeshPro - Text**
   - Name: `VersionText`
   - Text: `v1.0`
   - Font Size: `18`
   - Position: Bottom-right corner

### Add MainMenuController
1. **Create empty GameObject** → Name: `MainMenuController`
2. **Add Component** → `MainMenuController` script
3. **Assign References** in Inspector:
   - Play Button → Drag `PlayButton`
   - Settings Button → Drag `SettingsButton`
   - Quit Button → Drag `QuitButton`
   - Player Name Input → Drag `PlayerNameInput`
   - Settings Panel → Drag `SettingsPanel`
   - Music Volume Slider → Drag `MusicVolumeSlider`
   - Title Text → Drag `TitleText`
   - Version Text → Drag `VersionText`

### Save Scene
**File → Save**

---

## Scene 2: SongSelection

### Create the Scene
1. **File → New Scene** → Name it `SongSelection`
2. **Save** to `Assets/Scenes/SongSelection.unity`

### Create UI
1. **Right-click Hierarchy** → **UI → Canvas**
2. **Set Canvas Scaler** (same as MainMenu)

3. **Create UI Elements**:

   **Title Text:**
   - TextMeshPro - Text
   - Name: `TitleText`
   - Text: `SELECT SONG`
   - Position: Top center

   **Folder Path Text:**
   - TextMeshPro - Text
   - Name: `FolderPathText`
   - Text: `Scanning: ...`
   - Font Size: `14`
   - Position: Below title

   **Song List Scroll View:**
   - Right-click Canvas → **UI → Scroll View**
   - Name: `SongListScrollView`
   - Position: Center of screen (large area)
   - Delete `Scrollbar Horizontal` (only need vertical)
   - In Content (child object): Add **Vertical Layout Group** component
   - Content: Add **Content Size Fitter** component (Vertical Fit: Preferred Size)

   **Create Song List Item Prefab:**
   - In Project: **Create → Folder** → Name: `Prefabs/UI`
   - Right-click Hierarchy → **UI → Button - TextMeshPro**
   - Name: `SongListItem`
   - Resize to: Width `800`, Height `50`
   - Drag to `Prefabs/UI` folder to make prefab
   - Delete from Hierarchy

   **Selection Text:**
   - TextMeshPro - Text
   - Name: `SelectionText`
   - Text: `No song selected`
   - Position: Above bottom buttons

   **Start Button:**
   - Button - TextMeshPro
   - Name: `StartButton`
   - Text: `START`
   - Position: Bottom center

   **Browse Button:**
   - Button - TextMeshPro
   - Name: `BrowseButton`
   - Text: `BROWSE FILE`
   - Position: Bottom left of Start

   **Refresh Button:**
   - Button - TextMeshPro
   - Name: `RefreshButton`
   - Text: `REFRESH`
   - Position: Bottom right of Start

   **Back Button:**
   - Button - TextMeshPro
   - Name: `BackButton`
   - Text: `BACK`
   - Position: Bottom-left corner

### Add SongSelectionController
1. **Create empty GameObject** → Name: `SongSelectionController`
2. **Add Component** → `SongSelectionController` script
3. **Assign References**:
   - Song List Container → Drag `Viewport/Content` (from ScrollView)
   - Song List Item Prefab → Drag `SongListItem` prefab from Project
   - Start Button → Drag `StartButton`
   - Back Button → Drag `BackButton`
   - Browse Button → Drag `BrowseButton`
   - Refresh Button → Drag `RefreshButton`
   - Selection Text → Drag `SelectionText`
   - Folder Path Text → Drag `FolderPathText`
4. **Configuration**:
   - Default Music Folder: (leave empty to use default My Music folder, or set custom path)
   - Scan Subfolders: `true`

### Save Scene

---

## Scene 3: Loading

### Create the Scene
1. **File → New Scene** → Name it `Loading`
2. **Save** to `Assets/Scenes/Loading.unity`

### Create UI
1. **Create Canvas** (same setup as before)

2. **Create UI Elements**:

   **Song Title Text:**
   - TextMeshPro - Text
   - Name: `SongTitleText`
   - Text: `Loading...`
   - Font Size: `48`
   - Position: Top center

   **Status Text:**
   - TextMeshPro - Text
   - Name: `StatusText`
   - Text: `Analyzing music...`
   - Font Size: `24`
   - Position: Center

   **Progress Bar:**
   - Right-click Canvas → **UI → Slider**
   - Name: `ProgressBar`
   - Set to fill from left to right
   - Remove handle (delete Handle Slide Area child)
   - Adjust Fill color to cyan
   - Position: Below status text

   **Progress Text:**
   - TextMeshPro - Text
   - Name: `ProgressText`
   - Text: `0%`
   - Position: Above or on progress bar

   **Cancel Button:**
   - Button - TextMeshPro
   - Name: `CancelButton`
   - Text: `CANCEL`
   - Position: Bottom center

### Add LoadingScreenController
1. **Create empty GameObject** → Name: `LoadingScreenController`
2. **Add Component** → `LoadingScreenController` script
3. **Assign References**:
   - Progress Bar → Drag `ProgressBar`
   - Progress Text → Drag `ProgressText`
   - Status Text → Drag `StatusText`
   - Song Title Text → Drag `SongTitleText`
   - Cancel Button → Drag `CancelButton`
   - Pre Analyzer → (leave empty, will auto-create)
   - MP3 Loader → (leave empty, will auto-create)

### Save Scene

---

## Scene 4: Gameplay

### Create the Scene
1. **File → New Scene** → Name it `Gameplay`
2. **Save** to `Assets/Scenes/Gameplay.unity`

### Setup Scene
1. **Delete default Main Camera** (GameplayController creates one)

2. **Create empty GameObject** → Name: `GameplayController`
3. **Add Component** → `GameplayController` script
4. **Configuration** (leave all references empty - auto-creates):
   - Auto Generate Terrain: `true`
   - Generate Ahead Distance: `50`
   - Motorcycle Spawn Position: `(0, 3, 5)`

### That's it!
GameplayController handles all setup automatically.

### Save Scene

---

## Scene 5: Results

### Create the Scene
1. **File → New Scene** → Name it `Results`
2. **Save** to `Assets/Scenes/Results.unity`

### Create UI
1. **Create Canvas** (same setup)

2. **Create UI Elements**:

   **Title:**
   - TextMeshPro - Text
   - Name: `TitleText`
   - Text: `RESULTS`
   - Position: Top center

   **Song Title:**
   - TextMeshPro - Text
   - Name: `SongTitleText`
   - Text: `Song Name`
   - Font Size: `36`
   - Position: Below title

   **Score Display Panel:**
   - Create Panel
   - Add children:
     - TextMeshPro - Text → Name: `ScoreLabel` → Text: `SCORE:`
     - TextMeshPro - Text → Name: `ScoreText` → Text: `0`
     - TextMeshPro - Text → Name: `CoinsLabel` → Text: `COINS:`
     - TextMeshPro - Text → Name: `CoinsText` → Text: `0`
     - TextMeshPro - Text → Name: `ComboLabel` → Text: `MAX COMBO:`
     - TextMeshPro - Text → Name: `MaxComboText` → Text: `0x`
     - TextMeshPro - Text → Name: `DurationLabel` → Text: `TIME:`
     - TextMeshPro - Text → Name: `DurationText` → Text: `00:00`

   **Rank Text:**
   - TextMeshPro - Text
   - Name: `RankText`
   - Text: `Rank: #1`
   - Font Size: `32`
   - Color: Gold

   **New High Score Indicator:**
   - Create Panel or Image
   - Name: `NewHighScoreIndicator`
   - Add child TextMeshPro - Text: `NEW HIGH SCORE!`
   - Set active: `false`

   **Leaderboard Panel:**
   - Create Panel
   - Name: `LeaderboardPanel`
   - Add title: TextMeshPro - Text → `LEADERBOARD`
   - Add Scroll View for entries
   - In Scroll View Content: Add Vertical Layout Group

   **Create Leaderboard Entry Prefab:**
   - Right-click Hierarchy → **UI → Panel**
   - Name: `LeaderboardEntry`
   - Add children (all TextMeshPro):
     - `RankText` → Text: `#1`
     - `PlayerText` → Text: `Player Name`
     - `ScoreText` → Text: `10000`
     - `DetailsText` → Text: `Coins: 50 | Combo: 25x`
   - Adjust layout with Horizontal Layout Group
   - Drag to `Prefabs/UI` folder
   - Delete from Hierarchy

   **Buttons:**
   - Button - TextMeshPro → Name: `PlayAgainButton` → Text: `PLAY AGAIN`
   - Button - TextMeshPro → Name: `SongSelectButton` → Text: `SONG SELECT`
   - Button - TextMeshPro → Name: `MainMenuButton` → Text: `MAIN MENU`
   - Position at bottom

### Add ResultsScreenController
1. **Create empty GameObject** → Name: `ResultsScreenController`
2. **Add Component** → `ResultsScreenController` script
3. **Assign References**:
   - Score Text → Drag `ScoreText`
   - Coins Text → Drag `CoinsText`
   - Max Combo Text → Drag `MaxComboText`
   - Duration Text → Drag `DurationText`
   - Song Title Text → Drag `SongTitleText`
   - New High Score Indicator → Drag `NewHighScoreIndicator`
   - Rank Text → Drag `RankText`
   - Leaderboard Container → Drag `Viewport/Content` from leaderboard scroll view
   - Leaderboard Entry Prefab → Drag `LeaderboardEntry` prefab
   - Max Leaderboard Entries: `10`
   - Play Again Button → Drag `PlayAgainButton`
   - Main Menu Button → Drag `MainMenuButton`
   - Song Select Button → Drag `SongSelectButton`

### Save Scene

---

## Build Settings

### Add Scenes to Build
1. **File → Build Settings**
2. **Add Open Scenes** (or drag scenes from Project):
   - MainMenu (index 0)
   - SongSelection (index 1)
   - Loading (index 2)
   - Gameplay (index 3)
   - Results (index 4)

3. **Set MainMenu as first scene** (drag to top if not already)

---

## Final Configuration

### Player Settings
1. **Edit → Project Settings → Player**
2. **Company Name**: Your company name
3. **Product Name**: `Desert Rider`
4. **Version**: `1.0`

### Quality Settings
1. **Edit → Project Settings → Quality**
2. Set VSync Count: `Every V Blank` (for 60 FPS)

### Input Settings
If using old Input Manager:
- Ensure "Jump" button exists (for boost on controller)
- Maps to Joystick Button 0 (A button / Cross button)

---

## Testing the Flow

### Test Sequence:
1. **Open MainMenu scene**
2. **Press Play** in Unity
3. **Enter player name** (click Settings button)
4. **Click Play button** → Should go to SongSelection
5. **Select an MP3** from the list (or use Browse)
6. **Click Start** → Should go to Loading
7. **Wait for analysis** → Auto-transitions to Gameplay
8. **Play the game** → Drive, collect coins, boost
9. **Let song finish** → Auto-transitions to Results
10. **View score and leaderboard**
11. **Click button** → Return to menu or play again

---

## Troubleshooting

### "Scene not found" errors:
- Check Build Settings → Ensure all scenes are added
- Check scene names match exactly (case-sensitive)

### GameFlowManager not found:
- Must be in MainMenu scene
- Should have DontDestroyOnLoad (automatic)
- Don't create multiple instances

### MP3 not loading:
- Check file path is valid
- Check file is actually MP3 format
- Check permissions (can Unity access the folder?)

### UI references null:
- Recheck all Inspector assignments
- Make sure TextMeshPro package is installed
- Reimport TMP Essentials if needed

### No leaderboard data:
- Check `Application.persistentDataPath` location
- On Windows: `C:\Users\[Username]\AppData\LocalLow\[CompanyName]\[ProductName]\Leaderboards\`
- Leaderboards save automatically after each session

---

## Optional: Background Music for Menus

Add ambient music to menu scenes:

1. **Add AudioSource** to Canvas in each menu scene
2. **Assign AudioClip** (menu music)
3. **Set Loop**: `true`
4. **Set Volume**: `0.3`
5. **Play On Awake**: `true`

---

## Next Steps

✅ **All scenes created and configured**
✅ **Full menu flow working**
✅ **Ready to play with any MP3!**

### Recommended Enhancements:
1. Add visual polish (particles, glow effects)
2. Add pause menu to Gameplay
3. Add settings persistence (volume, graphics quality)
4. Add more detailed stats tracking
5. Add achievements system
6. Add online leaderboards

---

## File Structure Summary

```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── SongSelection.unity
│   ├── Loading.unity
│   ├── Gameplay.unity
│   └── Results.unity
├── Scripts/
│   ├── Core/
│   │   ├── GameFlowManager.cs
│   │   ├── LeaderboardManager.cs
│   │   ├── LeaderboardEntry.cs
│   │   └── SongLeaderboard.cs
│   ├── UI/
│   │   ├── MainMenuController.cs
│   │   ├── SongSelectionController.cs
│   │   ├── LoadingScreenController.cs
│   │   └── ResultsScreenController.cs
│   ├── Gameplay/
│   │   ├── GameplayController.cs
│   │   ├── MotorcycleController.cs
│   │   ├── MotorcycleVisualController.cs
│   │   ├── BoostSystem.cs
│   │   ├── ScoreManager.cs
│   │   └── CollectibleSpawner.cs
│   └── ... (other scripts)
└── Prefabs/
    └── UI/
        ├── SongListItem.prefab
        └── LeaderboardEntry.prefab
```

**🎉 Setup Complete! Your game is ready to play with full menu system!**
