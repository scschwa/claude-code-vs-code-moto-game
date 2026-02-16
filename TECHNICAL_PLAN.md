# Desert Rider - Technical Implementation Plan

**Game Design Document:** [GDD.md](GDD.md)

---

## Table of Contents
1. [Technology Stack](#1-technology-stack)
2. [System Architecture](#2-system-architecture)
3. [Development Phases](#3-development-phases)
4. [Technical Deep Dives](#4-technical-deep-dives)
5. [Project Structure](#5-project-structure)
6. [Dependencies & Setup](#6-dependencies--setup)
7. [Performance Targets](#7-performance-targets)
8. [Development Tools](#8-development-tools)
9. [Testing Strategy](#9-testing-strategy)
10. [Known Challenges & Solutions](#10-known-challenges--solutions)
11. [Alternative Technologies](#11-alternative-technologies)
12. [Development Timeline](#12-development-timeline)

---

## 1. Technology Stack

### Recommended Stack (Primary Choice)

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Game Engine** | Unity 2022 LTS (or newer) | Best ecosystem for music-reactive games, proven beat detection libraries, strong 3D support, excellent controller integration |
| **Programming Language** | C# | Native Unity support, excellent performance, strong type safety, great for game logic |
| **MP3 Loading & Decoding** | NAudio (Mp3FileReader) | Read MP3 files, decode to PCM, extract waveform data for analysis |
| **Beat Detection** | Unity-Beat-Detection (GitHub) + Essentia (optional) | FFT-based analysis, works on both pre-loaded MP3s and real-time audio |
| **System Audio Capture (Free Play)** | NAudio + WASAPI Loopback | Real-time audio capture for Free Play mode only |
| **Advanced Audio (Optional)** | Essentia C++ Library | Professional-grade rhythm extraction, ML-based BPM detection, multi-platform |
| **Controller Input** | XInput API | Native Windows Xbox controller support, vibration/haptics, up to 4 controllers |
| **Terrain Generation** | Perlin/Simplex Noise | Procedural heightmap generation, smooth organic shapes, music-driven parameters |
| **Graphics Pipeline** | Unity URP (Universal Render Pipeline) | Performance-optimized, modern shader graph, excellent for stylized visuals |
| **Version Control** | Git + GitHub/GitLab | Standard industry practice, free for indies |

### Why Unity?

**Pros:**
- Mature ecosystem with extensive music game libraries
- Native C# scripting with excellent performance
- Strong 3D rendering for arcade aesthetics
- Built-in physics for motorcycle simulation
- Controller support out-of-the-box
- Large asset store and community
- Free for indie developers (Unity Personal)
- Cross-platform potential (future console ports)

**Cons:**
- License fee if revenue exceeds $100k/year
- Heavier than some alternatives (larger build size)
- Frequent API changes between versions

---

## 2. System Architecture

### High-Level Architecture Diagram

```
                    ┌─── MP3 MODE ───┐         ┌─ FREE PLAY MODE ─┐
                    │                │         │                   │
                    ▼                │         ▼                   │
        ┌──────────────────────┐    │  ┌──────────────────┐      │
        │   MP3 FILE LIBRARY   │    │  │   SYSTEM AUDIO   │      │
        │  (Local Game Folder) │    │  │ (Spotify, YouTube│      │
        └──────────┬───────────┘    │  └────────┬─────────┘      │
                   │                │           │                 │
                   ▼                │           ▼                 │
      ┌────────────────────────┐   │  ┌──────────────────────┐  │
      │  MP3 LOADER & DECODER  │   │  │   WASAPI LOOPBACK    │  │
      │   (NAudio Mp3Reader)   │   │  │    (Real-time)       │  │
      │ - Load full MP3 file   │   │  │ - Capture streaming  │  │
      │ - Decode to PCM        │   │  │ - Live audio buffer  │  │
      │ - Extract waveform     │   │  └──────────┬───────────┘  │
      └────────────┬───────────┘   │             │               │
                   │                │             │               │
                   ▼                │             ▼               │
      ┌────────────────────────┐   │  ┌──────────────────────┐  │
      │  PRE-ANALYSIS ENGINE   │   │  │   REAL-TIME ANALYZER │  │
      │ - Analyze FULL song    │   │  │ - Live FFT analysis  │  │
      │ - Build complete beat  │   │  │ - Beat detection     │  │
      │   map & intensity curve│   │  │ - Dynamic intensity  │  │
      │ - Generate level seed  │   │  └──────────┬───────────┘  │
      │ - Hash for leaderboard │   │             │               │
      └────────────┬───────────┘   │             │               │
                   │                │             │               │
                   └────────────────┴─────────────┘               │
                                    │                             │
                                    ▼                             │
                   ┌─────────────────────────────────┐            │
                   │    BEAT DETECTION ENGINE        │            │
                   │  - Common analysis algorithms   │            │
                   │  - FFT, spectral flux, peaks   │            │
                   │  - BPM, intensity, beat times  │            │
                   └──────────────┬──────────────────┘            │
                                  │                               │
                                  ▼                               │
                   ┌─────────────────────────────────┐            │
                   │    GAMEPLAY CONTROLLER          │            │
                   │  - Receives beat/intensity data │            │
                   │  - Translates to game params    │            │
                   │  - Manages state & sync         │            │
                   │  - Level seeding (MP3 mode)     │            │
                   └────┬──────┬──────┬──────┬───────┘            │
                        │      │      │      │                    │
       ┌────────────────┴──────┴──────┴──────┴─────────┐          │
       ▼          ▼         ▼         ▼        ▼        ▼         │
  ┌────────┐ ┌───────┐ ┌────────┐ ┌──────┐ ┌────┐ ┌────────┐    │
  │TERRAIN │ │ MOTOR │ │OBSTACLE│ │ SCORE│ │VFX │ │LEADER- │    │
  │  GEN   │ │ CYCLE │ │SPAWNER │ │SYSTEM│ │    │ │ BOARD  │    │
  │        │ │       │ │        │ │      │ │    │ │(MP3 only)   │
  │-Seeded │ │-Phys  │ │-Seeded │ │-Coins│ │    │ │-Per-song│    │
  │ (MP3)  │ │-Input │ │ (MP3)  │ │-Combo│ │    │ │ scores │    │
  │-Dynamic│ │       │ │-Dynamic│ │      │ │    │ └────────┘    │
  │ (Free) │ │       │ │ (Free) │ │      │ │    │               │
  └────────┘ └───────┘ └────────┘ └──────┘ └────┘               │
                                                                  │
                                                                  │
                                                                  │
                                                                  │
```

### Core Modules

#### 1. **MP3 Library Manager** (`MP3LibraryManager.cs`)
- Handles MP3 file import and copying to local storage
- Extracts ID3 tags (title, artist, album, album art)
- Generates audio hash for leaderboard matching
- Manages song library (browse, search, filter)
- Stores high scores per MP3

#### 2. **MP3 Loader & Pre-Analyzer** (`MP3Loader.cs`, `PreAnalyzer.cs`)
- Loads MP3 files using NAudio's Mp3FileReader
- Decodes entire song to PCM waveform
- Performs complete FFT analysis on full audio
- Builds beat map, intensity curve, BPM for entire song
- Generates deterministic level seed from audio features

#### 3. **Audio Capture Module** (`AudioCaptureManager.cs`) - Free Play Only
- Initializes WASAPI loopback recording
- Continuously captures system audio buffer in real-time
- Provides raw audio samples to beat detector
- Handles audio device changes and errors

#### 4. **Beat Detection Engine** (`BeatDetector.cs`, `MusicAnalyzer.cs`)
- **MP3 Mode:** Analyzes complete waveform, returns full beat/intensity data structure
- **Free Play Mode:** Analyzes real-time audio stream, outputs current beat/intensity
- Performs FFT on audio samples
- Calculates spectral flux
- Detects beat onsets using peak detection
- Estimates BPM via inter-beat interval analysis
- Outputs normalized intensity (0-1 scale)

#### 5. **Level Seed Generator** (`LevelSeeder.cs`) - MP3 Mode Only
- Takes complete audio analysis data
- Generates deterministic seed from beat patterns and intensity curve
- Same MP3 always produces same seed = same level
- Seed controls: terrain noise parameters, obstacle spawn positions, coin placements

#### 6. **Terrain Generator** (`TerrainGenerator.cs`)
- **MP3 Mode:** Seeded procedural generation (same seed = same terrain)
- **Free Play Mode:** Dynamic real-time generation
- Procedurally generates road chunks ahead of player
- Uses Perlin noise with music-driven parameters
- Modulates noise amplitude/frequency based on intensity
- Manages chunk lifecycle (creation, pooling, destruction)

#### 7. **Motorcycle Controller** (`MotorcycleController.cs`)
- Handles player input (controller/keyboard)
- Arcade physics simulation
- Speed tied to BPM
- Collision detection and response

#### 8. **Obstacle Spawner** (`ObstacleSpawner.cs`)
- **MP3 Mode:** Seeded spawn positions (deterministic)
- **Free Play Mode:** Real-time reactive spawning
- Spawns traffic, rocks, barriers on beats
- Density scales with music intensity
- Object pooling for performance

#### 9. **Score & Collectibles Manager** (`CollectibleManager.cs`, `ScoreSystem.cs`)
- **MP3 Mode:** Seeded coin placements
- **Free Play Mode:** Dynamic coin patterns
- Spawns coins in music-reactive patterns
- Tracks collection, combo multiplier
- Calculates score with bonuses

#### 10. **Leaderboard System** (`LeaderboardManager.cs`) - MP3 Mode Only
- Per-song leaderboards (matched by audio hash)
- Local high score storage
- Optional: Online leaderboard integration
- Prevents cheating by validating level seed matches audio
- Friend comparisons and social features

#### 11. **Visual Effects System** (`VisualEffectsController.cs`)
- Particle effects synced to beats
- Camera shake and zoom on intensity changes
- Shader effects (bloom, color grading)
- Motorcycle trail renderer

---

## 3. Development Phases

### Phase 1: Foundation & Prototype (Weeks 1-4)
**Goal:** Prove the core concept—MP3 analysis and deterministic level generation works.

**Tasks:**
1. **Unity Project Setup**
   - Create new Unity 2022 LTS project
   - Configure URP (Universal Render Pipeline)
   - Set up Git repository with .gitignore for Unity
   - Create MusicLibrary folder structure

2. **MP3 Loading Implementation (Primary Focus)**
   - Install NAudio via NuGet
   - Implement `MP3Loader.cs` with Mp3FileReader
   - Load and decode sample MP3 to PCM
   - Extract full waveform data
   - Display waveform in debug UI

3. **Pre-Analysis System**
   - Integrate Unity-Beat-Detection from GitHub
   - Implement full-song FFT analysis (not real-time)
   - Detect all beats in complete MP3
   - Calculate BPM from full song
   - Build intensity curve for entire track
   - Visualize analysis results (beat markers, intensity graph)

4. **Level Seed Generation**
   - Implement `LevelSeeder.cs`
   - Generate deterministic seed from audio analysis data
   - Test: same MP3 produces same seed every time
   - Seed based on beat positions, BPM, intensity curve

5. **Simple Motorcycle Movement**
   - Create basic motorcycle GameObject with Rigidbody
   - Implement forward movement at constant speed
   - Add left/right steering with keyboard/controller
   - Camera follows motorcycle (third-person view)

6. **Seeded Terrain Test**
   - Create simple flat road that spawns from seed
   - Test: same seed = same terrain every time
   - Motorcycle drives on deterministic road surface
   - Test physics (gravity, collision)

7. **MP3 Playback Sync**
   - Play MP3 audio while riding
   - Sync visual beat markers to actual playback time
   - Verify music and gameplay stay in sync throughout song
   - Test with multiple MP3s of different lengths

**Deliverable:** Prototype where MP3 is loaded, analyzed, generates deterministic seed, and motorcycle rides on consistent level while music plays.

---

### Phase 2: Core Gameplay (Weeks 5-10)
**Goal:** Full MP3-based gameplay loop with seeded generation. Add Free Play mode as secondary feature.

**Tasks:**
1. **Seeded Procedural Terrain Generation (MP3 Mode)**
   - Implement seeded Perlin noise heightmap generator
   - Create terrain chunks (e.g., 100m x 20m) from seed
   - Spawn chunks ahead of player, despawn behind
   - Modulate noise parameters based on pre-analyzed intensity curve
   - Test: same MP3 creates same terrain every time

2. **Music-Reactive Terrain Modulation (Seeded)**
   - High intensity sections → Higher amplitude noise (more hills)
   - High intensity → Higher frequency noise (more curves)
   - Low intensity → Smooth, flat terrain
   - Road width narrows/widens based on intensity curve
   - All driven by pre-analyzed data, not real-time

3. **Seeded Obstacle System (MP3 Mode)**
   - Create traffic vehicle prefabs, rock/barrier prefabs
   - Implement `ObstacleSpawner.cs` with seeded spawning
   - Obstacles spawn at specific beat positions (from pre-analysis)
   - Obstacle density scales with intensity curve
   - Object pooling for performance
   - Test: same song = same obstacles in same positions

4. **Seeded Coin Collection & Scoring (MP3 Mode)**
   - Create coin prefabs with glow/pulse effects
   - Spawn coins in seeded patterns (deterministic positions)
   - Coin placements based on beat map
   - Implement collection on trigger enter
   - Display score and combo multiplier in UI
   - Combo resets on miss or crash
   - Test: same song = same coin positions

5. **Controller Input with Vibration**
   - Integrate XInput for Xbox controllers
   - Map steering to left stick, boost to A button
   - Implement rumble on beats and collisions
   - Add keyboard controls as fallback

6. **Crash & Respawn System**
   - Detect collisions with obstacles
   - Quick respawn with minimal penalty
   - Briefly reduce speed, reset combo
   - Visual/audio feedback on crash

7. **Free Play Mode Implementation (Secondary)**
   - Implement WASAPI loopback capture (`AudioCaptureManager.cs`)
   - Real-time beat detection (reuse existing beat detector)
   - Dynamic terrain/obstacle generation (non-seeded)
   - Test with Spotify, YouTube playback
   - No leaderboard integration for this mode
   - Simple UI toggle to switch between MP3 and Free Play modes

8. **High Score System (MP3 Mode Only)**
   - Save high scores locally per MP3 (by hash)
   - Display personal best on song selection
   - Results screen shows score improvement
   - Prepare data structures for future online leaderboards

**Deliverable:**
- **MP3 Mode:** Fully playable deterministic loop with high score tracking
- **Free Play Mode:** Working real-time system audio mode for casual play

---

### Phase 3: Visual Polish (Weeks 11-14)
**Goal:** Achieve the Sayonara Wild Hearts-inspired arcade aesthetic.

**Tasks:**
1. **Color Palette & Lighting**
   - Implement vibrant color scheme (purples, pinks, cyans)
   - Create gradient skybox (sunset/night transitions)
   - Add directional lighting for depth
   - Post-processing: bloom, color grading, vignette

2. **Particle Effects**
   - Dust trails from motorcycle wheels
   - Coin collection burst
   - Obstacle destruction particles
   - Ambient desert particles (dust, heat shimmer)
   - Sync particle density to music

3. **Motorcycle Visual Upgrade**
   - Sleek 3D model (asset store or custom)
   - Glowing neon accents
   - Trail renderer for motion path
   - Leaning animation when turning
   - Speed lines/motion blur

4. **Music-Synchronized Visual Effects**
   - Screen flash on strong beats
   - Camera shake scaled to intensity
   - Terrain edge glow pulses with bass
   - Sky color shifts with music mood

5. **UI/HUD Polish**
   - Stylized fonts matching arcade aesthetic
   - Score counter with growing/pulsing animation on combo increase
   - BPM display
   - Boost meter visualization
   - Minimal, non-intrusive layout

6. **Environmental Assets**
   - Background mountains/rock formations
   - Scattered cacti, desert plants
   - Distant horizon features
   - Optimize LODs for performance

**Deliverable:** Visually stunning game with cohesive arcade aesthetic and music-reactive visual effects.

---

### Phase 4: Features & Refinement (Weeks 15-18)
**Goal:** Polish the experience, add settings, optimize performance.

**Tasks:**
1. **Audio Calibration System**
   - In-game calibration wizard (tap along to beat)
   - Beat sensitivity slider
   - Intensity scaling slider
   - Latency offset adjustment
   - Genre preset system (EDM, Rock, Classical, etc.)
   - Save/load calibration profiles

2. **Multiple Music Source Testing**
   - Test with Spotify, YouTube, local files
   - Verify WASAPI captures all sources correctly
   - Handle edge cases (no audio playing, mic input)
   - Display warning if no audio detected

3. **Controller Configuration**
   - Button remapping UI
   - Sensitivity adjustments (steering, acceleration)
   - Rumble intensity toggle
   - Support multiple controller types (Xbox, PlayStation, generic)

4. **Performance Optimization**
   - Profile with Unity Profiler
   - Optimize terrain generation (reduce poly count)
   - Object pooling for all spawned objects
   - Reduce draw calls (batching, atlasing)
   - Target 60 FPS on GTX 1060 equivalent

5. **Bug Fixes & Playtesting**
   - Internal playtesting with diverse music genres
   - Fix collision bugs, physics glitches
   - Smooth out terrain generation seams
   - Adjust difficulty balancing

6. **Results Screen**
   - End-of-song score breakdown
   - Stats: distance, coins, combo record, accuracy
   - Grade calculation (F to S rank)
   - Animated score counting
   - Options: Replay, New Song, Main Menu

**Deliverable:** Polished, stable game with customization options and good performance across hardware.

---

### Phase 5: Final Polish & Modes (Weeks 19-24)
**Goal:** Add game modes, meta-progression, and launch-ready features.

**Tasks:**
1. **Additional Game Modes**
   - **Score Attack:** Competitive mode with leaderboards
   - **Endurance:** Playlist mode with multiple songs
   - **Challenge Mode:** Daily/weekly objectives

2. **Meta-Progression**
   - Unlock system for motorcycles
   - Customization shop (paint jobs, trails)
   - Environment theme selection (desert, neon city, etc.)
   - Currency system (coins earned in runs)

3. **Leaderboards**
   - Local high scores
   - Optional: Online leaderboards (if song fingerprinting works)
   - Friend comparisons
   - Per-genre leaderboards

4. **Menu System**
   - Main menu with music-reactive background
   - Garage/customization screen
   - Settings menu (graphics, audio, controls)
   - Credits screen

5. **Sound Effects (Non-Music Audio)**
   - Motorcycle engine sounds (subtle, not overwhelming)
   - Coin collection chime
   - Crash/impact sounds
   - Boost activation whoosh
   - UI click sounds

6. **Replay System** (Optional)
   - Save best runs
   - Replay viewer with free camera
   - Ghost rider for racing against yourself

7. **Final Optimization & Testing**
   - Performance tuning for various hardware
   - Extensive QA on different Windows versions
   - Test with wide variety of music genres
   - Fix any remaining bugs

8. **Build & Distribution**
   - Create standalone Windows build
   - Set up installer (or zip distribution)
   - Prepare Steam page (if launching on Steam)
   - Create marketing materials (trailer, screenshots)

**Deliverable:** Feature-complete, launch-ready game.

---

## 4. Technical Deep Dives

### 4.1 MP3 Loading & Pre-Analysis (MP3 Mode)

**Goal:** Load MP3 files, decode to PCM, and perform complete audio analysis before gameplay.

**NAudio MP3 Support:**
- NAudio includes `Mp3FileReader` class for decoding MP3s
- Supports MP3, MP2, MP1 formats
- Decodes to PCM (uncompressed audio samples)
- Can read entire file into memory or stream

**Implementation Steps:**

1. **File Selection & Import:**
```csharp
using NAudio.Wave;
using System.IO;
using System.Security.Cryptography;

public class MP3LibraryManager : MonoBehaviour
{
    private string libraryPath;

    void Start()
    {
        libraryPath = Path.Combine(Application.persistentDataPath, "MusicLibrary");
        Directory.CreateDirectory(libraryPath);
    }

    public async Task<SongData> ImportMP3(string filePath)
    {
        // Generate hash for file
        string hash = GenerateFileHash(filePath);

        // Create directory for this song
        string songDir = Path.Combine(libraryPath, hash);
        Directory.CreateDirectory(songDir);

        // Copy MP3 to library
        string destPath = Path.Combine(songDir, "song.mp3");
        File.Copy(filePath, destPath, overwrite: true);

        // Extract ID3 tags
        var tagFile = TagLib.File.Create(destPath);
        SongData songData = new SongData
        {
            Hash = hash,
            Title = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Artist = tagFile.Tag.FirstPerformer ?? "Unknown",
            Album = tagFile.Tag.Album,
            Duration = tagFile.Properties.Duration
        };

        // Trigger pre-analysis
        await PreAnalyzeSong(destPath, songData);

        return songData;
    }

    string GenerateFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
```

2. **MP3 Decoding & Waveform Extraction:**
```csharp
public class MP3Loader : MonoBehaviour
{
    public float[] LoadMP3Waveform(string mp3Path)
    {
        using (var reader = new Mp3FileReader(mp3Path))
        {
            // Get total sample count
            long totalSamples = reader.Length / (reader.WaveFormat.BitsPerSample / 8);

            // For stereo, convert to mono for analysis
            int channels = reader.WaveFormat.Channels;
            long monoSamples = totalSamples / channels;

            // Read all samples
            byte[] buffer = new byte[reader.Length];
            reader.Read(buffer, 0, buffer.Length);

            // Convert to float samples (assuming 16-bit PCM)
            float[] samples = new float[monoSamples];
            int sampleIndex = 0;

            for (int i = 0; i < buffer.Length; i += 2 * channels)
            {
                // Convert 16-bit PCM to float (-1.0 to 1.0)
                short left = BitConverter.ToInt16(buffer, i);

                if (channels == 2)
                {
                    short right = BitConverter.ToInt16(buffer, i + 2);
                    samples[sampleIndex++] = ((left + right) / 2) / 32768f;
                }
                else
                {
                    samples[sampleIndex++] = left / 32768f;
                }
            }

            Debug.Log($"Loaded {samples.Length} samples from {mp3Path}");
            return samples;
        }
    }
}
```

3. **Pre-Analysis System:**
```csharp
public class PreAnalyzer : MonoBehaviour
{
    public async Task<AnalysisData> PreAnalyzeSong(string mp3Path, SongData songData)
    {
        // Load full waveform
        float[] waveform = MP3Loader.LoadMP3Waveform(mp3Path);
        int sampleRate = 44100; // Typically 44.1kHz for MP3

        // Perform FFT analysis across entire song
        List<BeatEvent> beats = new List<BeatEvent>();
        List<float> intensityCurve = new List<float>();

        int fftSize = 1024;
        int hopSize = 512;
        float[] previousSpectrum = new float[fftSize / 2];

        for (int i = 0; i < waveform.Length - fftSize; i += hopSize)
        {
            // Extract window
            float[] window = new float[fftSize];
            Array.Copy(waveform, i, window, 0, fftSize);

            // Perform FFT
            float[] spectrum = PerformFFT(window);

            // Calculate spectral flux
            float flux = 0f;
            for (int j = 0; j < spectrum.Length; j++)
            {
                flux += Mathf.Max(0, spectrum[j] - previousSpectrum[j]);
            }

            // Detect beat
            if (flux > GetThreshold(intensityCurve))
            {
                float timeInSeconds = i / (float)sampleRate;
                beats.Add(new BeatEvent { Time = timeInSeconds, Strength = flux });
            }

            // Calculate RMS intensity for this window
            float intensity = CalculateRMS(window);
            intensityCurve.Add(intensity);

            previousSpectrum = spectrum;
        }

        // Estimate BPM from beat intervals
        float bpm = EstimateBPM(beats);

        // Generate deterministic seed
        int seed = GenerateSeed(beats, intensityCurve, bpm);

        AnalysisData analysis = new AnalysisData
        {
            Beats = beats,
            IntensityCurve = intensityCurve,
            BPM = bpm,
            LevelSeed = seed,
            SampleRate = sampleRate
        };

        // Save analysis to disk
        SaveAnalysis(songData.Hash, analysis);

        Debug.Log($"Analysis complete: {beats.Count} beats, {bpm} BPM, seed {seed}");
        return analysis;
    }

    int GenerateSeed(List<BeatEvent> beats, List<float> intensityCurve, float bpm)
    {
        // Create deterministic seed from audio features
        int seed = 0;

        // Hash beat positions (first 100 beats)
        for (int i = 0; i < Mathf.Min(100, beats.Count); i++)
        {
            seed ^= (int)(beats[i].Time * 1000) << (i % 16);
        }

        // Include BPM
        seed ^= (int)(bpm * 100);

        // Include average intensity
        float avgIntensity = intensityCurve.Average();
        seed ^= (int)(avgIntensity * 10000);

        return seed;
    }
}
```

**Benefits of Pre-Analysis:**
- **Accuracy:** Full song context improves beat detection
- **Performance:** Analysis happens once during import, not every playthrough
- **Determinism:** Same MP3 always produces same analysis = same level
- **Quality:** Can use more expensive algorithms (Essentia) without real-time constraints

**Storage:**
- Analysis data saved as JSON/binary file alongside MP3
- Loaded quickly when song is selected for play
- No need to re-analyze on subsequent plays

---

### 4.2 System Audio Capture (WASAPI + NAudio) - Free Play Mode Only

**Note:** This system is ONLY used in Free Play mode. MP3 mode uses direct file loading.

**WASAPI (Windows Audio Session API):**
- Windows API for low-latency audio capture
- **Loopback mode:** Captures output audio ("what you hear")
- Works with all audio sources (Spotify, YouTube, system sounds)
- Not needed for MP3 mode (we have the files directly)

**NAudio Library:**
- Open-source .NET audio library
- Provides `WasapiLoopbackCapture` class for real-time capture
- Easy integration with Unity (runs in background thread)

**Implementation Steps:**

1. **Install NAudio:**
   - Download NAudio NuGet package
   - Add `NAudio.dll` to Unity project (`Assets/Plugins/`)

2. **Create AudioCaptureManager:**
```csharp
using NAudio.Wave;
using UnityEngine;
using System;

public class AudioCaptureManager : MonoBehaviour
{
    private WasapiLoopbackCapture capture;
    private float[] audioBuffer;
    private int sampleRate;

    void Start()
    {
        InitializeCapture();
    }

    void InitializeCapture()
    {
        capture = new WasapiLoopbackCapture();
        sampleRate = capture.WaveFormat.SampleRate;

        capture.DataAvailable += OnDataAvailable;
        capture.RecordingStopped += OnRecordingStopped;

        capture.StartRecording();
        Debug.Log($"Audio capture started: {sampleRate} Hz");
    }

    void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        // Convert byte[] to float[] samples
        int samplesCount = e.BytesRecorded / 4; // 32-bit float = 4 bytes
        audioBuffer = new float[samplesCount];
        Buffer.BlockCopy(e.Buffer, 0, audioBuffer, 0, e.BytesRecorded);

        // Send to beat detector
        BeatDetector.Instance.ProcessAudio(audioBuffer);
    }

    void OnRecordingStopped(object sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            Debug.LogError($"Audio capture error: {e.Exception.Message}");
        }
    }

    void OnDestroy()
    {
        capture?.StopRecording();
        capture?.Dispose();
    }
}
```

**Key Considerations:**
- WASAPI requires Windows 10 (Build 20348+) or Windows 11 for per-app capture
- Fallback to system-wide capture on older Windows 10
- Handle audio device disconnects gracefully
- Buffer size affects latency (smaller = lower latency, but more CPU overhead)

---

### 4.3 Beat Detection Algorithm (Both Modes)

**Works in both MP3 and Free Play modes, but applied differently:**
- **MP3 Mode:** Applied to complete waveform during pre-analysis
- **Free Play Mode:** Applied to real-time audio stream

**Approach: Spectral Flux with Peak Detection**

**Theory:**
1. **FFT (Fast Fourier Transform):** Converts time-domain audio to frequency spectrum
2. **Spectral Flux:** Measures change in frequency energy over time
3. **Peak Detection:** Identify spikes in flux as beat onsets

**Unity-Beat-Detection Library:**
- GitHub: [allanpichardo/Unity-Beat-Detection](https://github.com/allanpichardo/Unity-Beat-Detection)
- Provides `AudioProcessor.cs` with FFT and beat detection

**Algorithm Breakdown:**

```csharp
public class BeatDetector : MonoBehaviour
{
    // FFT parameters
    private int fftSize = 1024;
    private float[] spectrum;
    private float[] previousSpectrum;

    // Beat detection
    private float[] spectralFlux;
    private float threshold = 1.5f;
    private float beatCooldown = 0.1f; // Minimum 100ms between beats
    private float lastBeatTime;

    // Output
    public float CurrentIntensity { get; private set; }
    public float CurrentBPM { get; private set; }
    public event Action OnBeat;

    public void ProcessAudio(float[] samples)
    {
        // 1. Perform FFT
        spectrum = PerformFFT(samples);

        // 2. Calculate spectral flux
        float flux = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            float diff = Mathf.Max(0, spectrum[i] - previousSpectrum[i]);
            flux += diff;
        }

        // 3. Detect beat (flux exceeds threshold)
        if (flux > threshold && Time.time - lastBeatTime > beatCooldown)
        {
            OnBeat?.Invoke();
            lastBeatTime = Time.time;
            UpdateBPM();
        }

        // 4. Calculate intensity (RMS energy)
        CurrentIntensity = CalculateRMS(samples);

        // 5. Store spectrum for next frame
        previousSpectrum = spectrum;
    }

    private float[] PerformFFT(float[] samples)
    {
        // Use Unity's or external FFT library
        // Returns frequency spectrum (magnitude)
    }

    private float CalculateRMS(float[] samples)
    {
        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += sample * sample;
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private void UpdateBPM()
    {
        // Track inter-beat intervals, calculate average BPM
        // Use rolling window for smoothing
    }
}
```

**Advanced: Essentia Integration (Optional)**

For more accurate BPM/beat detection, integrate Essentia C++ library:
- Pre-built Windows DLL available
- Call via P/Invoke from C#
- Provides `RhythmExtractor2013` algorithm
- More CPU-intensive but significantly more accurate

**Calibration Parameters:**
- **Threshold:** Lower = more sensitive (detects more beats)
- **Cooldown:** Prevents double-triggering on same beat
- **Frequency Range:** Focus on bass (20-200 Hz) for stronger beats
- **Smoothing:** Apply low-pass filter to intensity for smoother gameplay changes

---

### 4.4 Procedural Terrain Generation (Seeded vs Dynamic)

**Two Generation Modes:**
- **MP3 Mode (Seeded):** Uses level seed to initialize Random state. Same seed = same terrain every time. Allows mastery and competition.
- **Free Play Mode (Dynamic):** Uses current music intensity in real-time. Non-deterministic, unique every playthrough.

**Approach: Perlin Noise Heightmap with Music Modulation**

**Perlin Noise:**
- Generates smooth, organic-looking terrain
- 2D noise function: `Noise(x, z)` returns height value
- Parameters: amplitude (height scale), frequency (detail level), octaves (layering)
- Can be seeded for deterministic output

**Seeded vs Dynamic Implementation:**
```csharp
public class TerrainGenerator : MonoBehaviour
{
    // Noise parameters
    public float baseAmplitude = 5f;
    public float baseFrequency = 0.1f;
    public int octaves = 3;

    // Mode-specific
    public enum GenerationMode { MP3Seeded, FreePlayDynamic }
    public GenerationMode mode;

    private int levelSeed; // MP3 mode only
    private System.Random seededRandom; // MP3 mode only
    private float intensityScale = 1f;

    // Chunk management
    private float chunkSize = 100f;
    private int chunksAhead = 5;
    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    public void InitializeMP3Mode(int seed, AnalysisData analysisData)
    {
        mode = GenerationMode.MP3Seeded;
        levelSeed = seed;
        seededRandom = new System.Random(seed);
        this.analysisData = analysisData;
    }

    public void InitializeFreePlayMode()
    {
        mode = GenerationMode.FreePlayDynamic;
    }

    void Update()
    {
        if (mode == GenerationMode.FreePlayDynamic)
        {
            // Free Play: Get real-time intensity
            float intensity = BeatDetector.Instance.CurrentIntensity;
            intensityScale = Mathf.Lerp(0.5f, 2f, intensity);
        }
        // MP3 mode intensity comes from pre-analyzed curve (in GenerateChunk)

        // Generate chunks ahead of player
        GenerateChunksIfNeeded();
    }

    void GenerateChunksIfNeeded()
    {
        Vector3 playerPos = MotorcycleController.Instance.transform.position;

        while (activeChunks.Count < chunksAhead)
        {
            float chunkZ = activeChunks.Count * chunkSize;
            GenerateChunk(chunkZ);
        }

        // Despawn chunks behind player
        if (activeChunks.Count > 0)
        {
            GameObject oldestChunk = activeChunks.Peek();
            if (oldestChunk.transform.position.z < playerPos.z - chunkSize)
            {
                activeChunks.Dequeue();
                Destroy(oldestChunk);
            }
        }
    }

    void GenerateChunk(float startZ)
    {
        GameObject chunk = new GameObject($"Chunk_{startZ}");
        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();

        // Generate heightmap mesh
        Mesh mesh = CreateTerrainMesh(startZ);
        mf.mesh = mesh;
        mr.material = terrainMaterial;

        chunk.transform.position = new Vector3(0, 0, startZ);
        activeChunks.Enqueue(chunk);
    }

    Mesh CreateTerrainMesh(float offsetZ)
    {
        int resolution = 50; // vertices per side
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution-1) * (resolution-1) * 6];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = (x - resolution/2) * (chunkSize / resolution);
                float worldZ = offsetZ + z * (chunkSize / resolution);

                // Perlin noise with music scaling
                float height = GetHeight(worldX, worldZ);

                vertices[z * resolution + x] = new Vector3(worldX, height, worldZ);
            }
        }

        // Generate triangle indices (not shown for brevity)
        // ...

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    float GetHeight(float x, float z)
    {
        float height = 0f;
        float amplitude = baseAmplitude * intensityScale;
        float frequency = baseFrequency * intensityScale;

        // Multi-octave Perlin noise
        for (int i = 0; i < octaves; i++)
        {
            height += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
            amplitude *= 0.5f; // Each octave half the amplitude
            frequency *= 2f;   // Each octave double the frequency
        }

        return height;
    }
}
```

**Optimizations:**
- **Object Pooling:** Reuse chunk GameObjects instead of destroying/recreating
- **LOD (Level of Detail):** Lower resolution for distant chunks
- **Multithreading:** Generate meshes on background thread
- **Mesh Simplification:** Reduce vertex count where terrain is flat

---

### 4.5 Motorcycle Physics (Same for Both Modes)

**Arcade vs. Realistic:**
- Focus on **arcade feel**—responsive, forgiving, fun over realism
- Simplified physics model (no realistic suspension, weight transfer)

**Implementation:**
```csharp
public class MotorcycleController : MonoBehaviour
{
    private Rigidbody rb;

    // Movement parameters
    public float baseSpeed = 20f;
    public float steeringSpeed = 100f;
    public float acceleration = 5f;

    // Music-driven speed
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Get input
        float steerInput = Input.GetAxis("Horizontal");

        // Speed scales with BPM
        float bpm = BeatDetector.Instance.CurrentBPM;
        float targetSpeed = baseSpeed * Mathf.Clamp(bpm / 120f, 0.5f, 2f);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        // Apply movement
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, steerInput * steeringSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            HandleCrash();
        }
    }

    void HandleCrash()
    {
        // Reduce speed briefly
        currentSpeed *= 0.5f;

        // Reset combo
        ScoreSystem.Instance.ResetCombo();

        // Trigger visual/audio feedback
    }
}
```

**Controller Integration (XInput):**
```csharp
using XInputDotNetPure; // Import XInput library

public class ControllerInput : MonoBehaviour
{
    private GamePadState state;
    private int playerIndex = 0;

    void Update()
    {
        state = GamePad.GetState((PlayerIndex)playerIndex);

        if (state.IsConnected)
        {
            // Steering from left stick
            float steer = state.ThumbSticks.Left.X;

            // Boost from A button
            if (state.Buttons.A == ButtonState.Pressed)
            {
                MotorcycleController.Instance.Boost();
            }

            // Rumble on beat
            if (BeatDetector.Instance.OnBeat)
            {
                GamePad.SetVibration(0, 0.3f, 0.3f); // Left, Right motor
                Invoke("StopRumble", 0.1f);
            }
        }
    }

    void StopRumble()
    {
        GamePad.SetVibration(0, 0, 0);
    }
}
```

---

## 5. Project Structure

### Recommended File Organization

```
DesertRider/
│
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── SongSelection.unity
│   │   ├── GameScene.unity
│   │   └── Calibration.unity (Free Play mode only)
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── GameMode.cs (enum: MP3, FreePlay)
│   │   │   └── SceneLoader.cs
│   │   │
│   │   ├── MP3/
│   │   │   ├── MP3LibraryManager.cs
│   │   │   ├── MP3Loader.cs
│   │   │   ├── PreAnalyzer.cs
│   │   │   ├── LevelSeeder.cs
│   │   │   ├── SongData.cs (data class)
│   │   │   └── AnalysisData.cs (data class)
│   │   │
│   │   ├── Audio/
│   │   │   ├── AudioCaptureManager.cs (Free Play only)
│   │   │   ├── BeatDetector.cs (shared)
│   │   │   ├── MusicAnalyzer.cs (shared)
│   │   │   └── CalibrationSystem.cs (Free Play only)
│   │   │
│   │   ├── Gameplay/
│   │   │   ├── MotorcycleController.cs
│   │   │   ├── TerrainGenerator.cs
│   │   │   ├── ObstacleSpawner.cs
│   │   │   ├── TrafficVehicle.cs
│   │   │   ├── CollectibleManager.cs
│   │   │   ├── CoinBehavior.cs
│   │   │   └── ChunkManager.cs
│   │   │
│   │   ├── Input/
│   │   │   ├── ControllerInput.cs
│   │   │   └── InputManager.cs
│   │   │
│   │   ├── Scoring/
│   │   │   ├── ScoreSystem.cs
│   │   │   ├── ComboManager.cs
│   │   │   ├── LeaderboardManager.cs (MP3 mode)
│   │   │   └── HighScoreStorage.cs (local persistence)
│   │   │
│   │   ├── UI/
│   │   │   ├── HUDManager.cs
│   │   │   ├── MenuController.cs
│   │   │   ├── SongLibraryUI.cs (MP3 mode)
│   │   │   ├── ResultsScreen.cs
│   │   │   └── SettingsMenu.cs
│   │   │
│   │   └── Visual/
│   │       ├── VisualEffectsController.cs
│   │       ├── CameraController.cs
│   │       ├── ParticleManager.cs
│   │       └── ShaderController.cs
│   │
│   ├── Prefabs/
│   │   ├── Motorcycle.prefab
│   │   ├── TerrainChunk.prefab
│   │   ├── Obstacles/
│   │   │   ├── TrafficCar.prefab
│   │   │   ├── Rock.prefab
│   │   │   └── Barrier.prefab
│   │   ├── Collectibles/
│   │   │   └── Coin.prefab
│   │   └── VFX/
│   │       ├── DustTrail.prefab
│   │       ├── CoinBurst.prefab
│   │       └── CrashEffect.prefab
│   │
│   ├── Materials/
│   │   ├── Terrain/
│   │   │   └── DesertTerrain.mat
│   │   ├── Motorcycle/
│   │   │   └── NeonBike.mat
│   │   └── Environment/
│   │       └── Sky.mat
│   │
│   ├── Models/
│   │   ├── Motorcycle.fbx
│   │   ├── Obstacles/
│   │   └── Environment/
│   │
│   ├── Shaders/
│   │   ├── NeonGlow.shadergraph
│   │   ├── TerrainPulse.shadergraph
│   │   └── TrailEffect.shadergraph
│   │
│   ├── Textures/
│   │   ├── UI/
│   │   └── Terrain/
│   │
│   ├── Audio/
│   │   └── SFX/
│   │       ├── CoinCollect.wav
│   │       ├── Crash.wav
│   │       └── Boost.wav
│   │
│   ├── Plugins/
│   │   ├── NAudio.dll
│   │   ├── XInputDotNetPure.dll
│   │   └── Unity-Beat-Detection/
│   │
│   └── Settings/
│       ├── URP-Asset.asset
│       └── PostProcessing.asset
│
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
│
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   ├── InputManager.asset
│   └── QualitySettings.asset
│
├── MusicLibrary/ (User data - excluded from git)
│   ├── [song_hash_1]/
│   │   ├── song.mp3
│   │   ├── analysis.json
│   │   └── metadata.json
│   ├── [song_hash_2]/
│   │   └── ...
│   └── library_index.json
│
├── .gitignore (includes MusicLibrary/)
├── README.md
├── GDD.md
└── TECHNICAL_PLAN.md (this file)
```

**MusicLibrary Structure Notes:**
- Stored in `Application.persistentDataPath` (user's AppData on Windows)
- Each song gets its own folder named by hash
- `analysis.json`: Beat map, intensity curve, BPM, seed
- `metadata.json`: Title, artist, album, duration, import date
- `library_index.json`: Fast lookup for song library UI
- NOT included in version control (.gitignore)
- NOT included in game build (user imports their own music)
```

---

## 6. Dependencies & Setup

### Required Software

1. **Unity Hub** (Latest version)
   - Download: https://unity.com/download

2. **Unity Editor** (2022 LTS or newer)
   - Install via Unity Hub
   - Include modules: Windows Build Support, Visual Studio

3. **Visual Studio 2022** or **JetBrains Rider**
   - Unity integration tools
   - C# development

4. **Git** (for version control)
   - Download: https://git-scm.com/

### Required Libraries

#### NAudio (MP3 Loading & Audio Capture)
- **Source:** NuGet package
- **Installation:**
  1. Download NAudio.dll from NuGet
  2. Place in `Assets/Plugins/`
  3. Import in Unity
- **Used For:**
  - MP3 Mode: Mp3FileReader for loading/decoding MP3 files
  - Free Play Mode: WasapiLoopbackCapture for system audio

#### TagLib# (ID3 Tag Extraction)
- **Source:** NuGet package
- **Installation:**
  1. Download TagLibSharp.dll from NuGet
  2. Place in `Assets/Plugins/`
- **Purpose:** Extract song metadata (title, artist, album, album art) from MP3 files

#### Unity-Beat-Detection (Beat Detection)
- **Source:** GitHub - [allanpichardo/Unity-Beat-Detection](https://github.com/allanpichardo/Unity-Beat-Detection)
- **Installation:**
  1. Clone repository
  2. Copy scripts to `Assets/Plugins/Unity-Beat-Detection/`
  3. Import AudioProcessor.cs and dependencies

#### XInputDotNetPure (Controller Input)
- **Source:** GitHub - [speps/XInputDotNet](https://github.com/speps/XInputDotNet)
- **Installation:**
  1. Download release
  2. Copy DLL to `Assets/Plugins/`
  3. Include native XInput1_4.dll for Windows

#### Unity Packages (via Package Manager)
- **Universal Render Pipeline (URP):** Built-in, add via Window > Package Manager
- **Input System (New):** Optional, for advanced controller support
- **Post Processing Stack v2:** For visual effects

### Optional: Essentia (Advanced Audio Analysis)
- **Source:** https://essentia.upf.edu/
- **Platform:** Windows DLL (pre-compiled)
- **Usage:** Call via P/Invoke from C#
- **When to use:** If Unity-Beat-Detection isn't accurate enough

---

## 7. Performance Targets

### Minimum Specifications (Target Hardware)
- **CPU:** Intel i5-7400 or AMD Ryzen 5 1600
- **GPU:** NVIDIA GTX 1060 (6GB) or AMD RX 580
- **RAM:** 8 GB
- **OS:** Windows 10 (64-bit)

### Performance Goals
- **Frame Rate:** 60 FPS locked (with VSync)
- **Audio Latency:** <50ms (from audio event to visual response)
- **Terrain Generation:** No frame drops when spawning chunks
- **Beat Detection Overhead:** <5% CPU usage
- **Memory:** <2 GB RAM usage

### Optimization Strategies
1. **Object Pooling:** Reuse GameObjects for obstacles, coins, particles
2. **LOD System:** Lower detail for distant terrain chunks
3. **Occlusion Culling:** Don't render objects behind camera
4. **Texture Atlasing:** Combine textures to reduce draw calls
5. **Mesh Batching:** Combine static meshes
6. **Shader Optimization:** Avoid expensive operations in fragment shaders
7. **Audio Buffer Tuning:** Balance latency vs. CPU overhead (test 512, 1024, 2048 sample buffers)

### Profiling Tools
- **Unity Profiler:** Monitor CPU, GPU, memory usage
- **Frame Debugger:** Analyze rendering bottlenecks
- **Deep Profiling:** Identify expensive function calls

---

## 8. Development Tools

### Essential Tools
1. **Unity Hub + Unity Editor 2022 LTS**
2. **Visual Studio 2022** (Community Edition is free)
   - Unity Tools extension
   - C# IntelliSense
3. **Git** (version control)
   - GitHub Desktop (optional GUI)
4. **Audacity** (for testing audio, visualizing waveforms)

### Recommended Tools
5. **Blender** (3D modeling, if creating custom assets)
6. **GIMP / Photoshop** (texture creation)
7. **Unity Asset Store** (models, shaders, effects)
8. **Rider** (alternative to Visual Studio, better Unity integration)

### Optional Tools
9. **World Machine** (terrain generation for reference, ~$299)
10. **Shader Graph** (Unity's visual shader editor, built-in)
11. **FMOD / Wwise** (advanced audio middleware, probably overkill)

---

## 9. Testing Strategy

### Unit Testing
- **MP3 Loading:** Verify MP3s load correctly, decode to PCM without errors
- **Pre-Analysis:** Test with known BPM songs, verify analysis accuracy
- **Determinism:** Same MP3 produces same seed, same level every time
- **Audio Capture (Free Play):** Verify WASAPI captures system audio correctly
- **Beat Detection:** Test both pre-analysis and real-time modes
- **Terrain Generation:** Ensure no seams between chunks (both modes)
- **Collision Detection:** Verify all obstacles trigger crashes
- **Score System:** Validate combo multipliers, bonuses
- **High Scores:** Verify saving/loading per MP3 hash works correctly

### Integration Testing

**MP3 Mode:**
- **Import Flow:** Select MP3 → Analysis → Appears in library → Can play
- **Deterministic Playthrough:** Play same song 3 times, verify identical level
- **High Score Persistence:** Set high score, restart game, verify it loads
- **Leaderboard Matching:** Two players with same MP3 have same level/hash

**Free Play Mode:**
- **End-to-End:** Play music, ride motorcycle, verify reactivity
- **Audio-Visual Sync:** Measure latency (should be <50ms)
- **Multiple Sources:** Test Spotify, YouTube, local player

**Both Modes:**
- **Performance:** Profile during 5+ minute sessions
- **Mode Switching:** Switch between MP3 and Free Play without crashes

### Genre Testing
Test with diverse music styles to ensure beat detection works:
- **EDM / Electronic:** Clear beats (easiest)
- **Rock / Metal:** Fast, aggressive beats
- **Hip-Hop:** Strong bass, clear rhythm
- **Classical:** Varied tempo, complex rhythms (hardest)
- **Ambient / Downtempo:** Minimal beats (edge case)
- **Podcasts / Spoken Word:** Should gracefully handle (no beats detected)

### Hardware Testing
- **Controllers:** Xbox, PlayStation, Generic USB
- **Audio Devices:** Different sound cards, headphones, speakers
- **Windows Versions:** Windows 10 (multiple builds), Windows 11
- **Performance:** Low-end (GTX 1050), Mid-range (GTX 1060), High-end (RTX 3070)

### Playtesting
- **Internal:** Developer testing with personal playlists
- **Friends/Family:** Gather feedback on feel, difficulty, fun
- **Public Beta:** Steam Early Access or itch.io beta (optional)

---

## 10. Known Challenges & Solutions

### Challenge 1: MP3 Pre-Analysis Time
**Problem:** Large MP3 files (5+ minutes) may take 10-30 seconds to analyze, causing import delays.

**Solutions:**
- Show progress bar during analysis
- Allow background import (analyze while user browses library)
- Cache analysis results permanently
- Consider using Essentia (faster than Unity-Beat-Detection for batch processing)
- Future: Offer "Quick Import" with basic analysis, "Deep Analysis" as optional

**Mitigation:** Only happens once per song; subsequent plays load instantly from cached analysis.

---

### Challenge 2: Storage Space for MP3 Library
**Problem:** Users importing large music collections consume disk space.

**Solutions:**
- Optional: Reference MP3s instead of copying (but risky if user moves files)
- Compression for analysis data (JSON can be gzipped)
- Cleanup tool for unused songs
- Display storage usage in settings

**Mitigation:** Most users won't import entire library, just favorite songs for competition.

---

### Challenge 3: WASAPI Latency (Free Play Mode Only)
**Problem:** Audio capture may have noticeable delay between music and visuals.

**Solutions:**
- Reduce buffer size (test 512, 1024, 2048 samples)
- Implement manual latency offset in calibration
- Use exclusive mode WASAPI (if possible)
- Fallback: Virtual Audio Cable (VB-Cable)

**Mitigation:** Allow players to calibrate latency manually.

---

### Challenge 4: Beat Detection Accuracy Varies by Genre
**Problem:** Electronic music has clear beats; classical music is complex and variable.

**Solutions:**
- **MP3 Mode (Preferred):** Use Essentia for pre-analysis (no real-time constraints, very accurate)
- **Free Play Mode:** Adjustable sensitivity slider, per-genre profiles
- Frequency-specific beat detection (focus on bass for kicks)
- Smoothing/averaging to reduce false positives

**Mitigation:** MP3 mode can use expensive, accurate algorithms. Free Play mode defaults to "good enough" with calibration options.

---

### Challenge 3: Infinite Terrain Performance
**Problem:** Generating terrain in real-time may cause frame drops.

**Solutions:**
- Generate meshes on background thread (Unity Job System)
- Object pooling for chunks (reuse instead of destroy/instantiate)
- LOD system (lower resolution for distant chunks)
- Pre-generate several chunks at start before gameplay

**Mitigation:** Profile extensively, optimize mesh resolution.

---

### Challenge 4: Controller Compatibility
**Problem:** Not all controllers work with XInput (e.g., Nintendo Switch Pro, old DirectInput controllers).

**Solutions:**
- Use Unity's new Input System (supports wide range of controllers)
- Fallback to DirectInput for non-XInput devices
- Allow custom button mapping in settings

**Mitigation:** Prioritize Xbox controllers (most common on PC), add others later.

---

### Challenge 5: Music with No Clear Beats
**Problem:** Ambient, drone, or spoken word content has no rhythm.

**Solutions:**
- Detect low beat density, display warning to player
- Fallback mode: use RMS energy/loudness instead of beats
- Gradual difficulty scaling instead of beat-triggered events

**Mitigation:** Design game to be enjoyable even without strong beats (scenic cruising).

---

### Challenge 6: Audio Source Changes Mid-Game
**Problem:** User pauses Spotify, switches to YouTube—audio stops.

**Solutions:**
- Detect silence, pause game automatically
- Display "Waiting for audio..." message
- Resume when audio detected

**Mitigation:** Graceful handling with clear UI feedback.

---

## 11. Alternative Technologies Considered

### Alternative 1: Godot Engine
**Pros:**
- Completely free (no runtime fees, no Unity licensing)
- Lightweight, fast startup
- Open-source, community-driven
- Growing indie game community

**Cons:**
- Smaller ecosystem for music-reactive games
- Fewer beat detection libraries available
- WASAPI integration requires custom C# wrapper or GDExtension
- Smaller asset store

**Verdict:** Great for indie developers comfortable with DIY solutions, but Unity has better music game ecosystem.

---

### Alternative 2: Unreal Engine
**Pros:**
- AAA-quality graphics out of the box
- Professional audio tools (MetaSounds)
- Powerful Blueprint visual scripting
- No fees until $1M revenue

**Cons:**
- Overkill for arcade-style graphics
- Larger learning curve
- Heavier builds (multi-GB)
- C++ required for advanced audio capture

**Verdict:** Too heavy for this project's needs.

---

### Alternative 3: Web-Based (Babylon.js / Three.js)
**Pros:**
- No installation (runs in browser)
- Easy to share (just send URL)
- Cross-platform by default
- Web Audio API for beat detection

**Cons:**
- **Cannot capture system audio** (browser security limitation)
- Must use user-uploaded files or Web Audio API microphone
- Lower performance for complex 3D
- Limited controller support (Gamepad API is basic)

**Verdict:** Good for prototype/demo, but can't achieve core vision (system audio capture).

---

## 12. Development Timeline & Cost

### Estimated Timeline (Solo Developer, Part-Time)

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| **Phase 1: Foundation** | 4 weeks | Prototype with music reactivity proof-of-concept |
| **Phase 2: Core Gameplay** | 6 weeks | Fully playable loop (terrain, obstacles, coins, score) |
| **Phase 3: Visual Polish** | 4 weeks | Arcade aesthetic, music-synced effects |
| **Phase 4: Refinement** | 4 weeks | Settings, optimization, bug fixes |
| **Phase 5: Final Polish** | 6 weeks | Game modes, menus, launch-ready |
| **Total** | **24 weeks (6 months)** | Launch-ready game |

**Accelerated (Full-Time):** 3-4 months
**Extended (Hobby Project):** 8-12 months

### Cost Breakdown (Indie Budget)

| Item | Cost | Notes |
|------|------|-------|
| **Unity Personal Edition** | **Free** | Free until $100k revenue |
| **Visual Studio Community** | **Free** | Free for individuals |
| **NAudio Library** | **Free** | Open-source |
| **Unity-Beat-Detection** | **Free** | Open-source (MIT license) |
| **XInput API** | **Free** | Built into Windows |
| **Blender** | **Free** | Open-source 3D software |
| **Git / GitHub** | **Free** | Free tier sufficient |
| **Asset Store Assets** | **$0-500** | Optional (models, shaders, sounds) |
| **World Machine** | **$0-299** | Optional (terrain tool) |
| **Steam Direct Fee** | **$100** | One-time (if publishing on Steam) |
| **Marketing / Trailer** | **$0-500** | DIY or hire freelancer |
| **Total** | **$100-1,399** | Can be as low as $0 if avoiding Steam / premium assets |

**Realistic Indie Budget:** $100-300 (Steam fee + a few assets)

---

## 13. Next Steps: Getting Started

### Week 1: Setup
1. Install Unity Hub, Unity 2022 LTS, Visual Studio
2. Create new Unity project with URP template
3. Initialize Git repository
4. Download NAudio, XInput, Unity-Beat-Detection libraries
5. Set up project structure (folders for Scripts, Prefabs, etc.)

### Week 2: MP3 Loading & Analysis POC
1. Implement `MP3Loader.cs` with NAudio Mp3FileReader
2. Load sample MP3, decode to PCM, visualize waveform
3. Implement `PreAnalyzer.cs`
4. Analyze full MP3: extract beats, BPM, intensity curve
5. Display analysis results (beat markers on timeline)

### Week 3: Deterministic Level Seeding
1. Implement `LevelSeeder.cs`
2. Generate seed from audio analysis
3. Test: same MP3 produces same seed reliably
4. Create simple seeded terrain generator
5. Verify: same seed = same terrain every time

### Week 4: Prototype Gameplay (MP3 Mode)
1. Create simple motorcycle controller
2. Generate seeded terrain that plays alongside MP3
3. Sync motorcycle speed to BPM
4. Add seeded obstacle that spawns on beats
5. Test the "feel"—does deterministic music reactivity work?
6. Play same song 3 times, verify identical experience

**Milestone:** If Week 4 prototype demonstrates determinism and feels good, proceed with full MP3 mode development! Add Free Play mode in Phase 2.

---

## Appendix: Resources & References

### Documentation
- [Unity Manual](https://docs.unity3d.com/)
- [NAudio Documentation](https://github.com/naudio/NAudio/wiki)
- [WASAPI on Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/coreaudio/wasapi)
- [Unity-Beat-Detection GitHub](https://github.com/allanpichardo/Unity-Beat-Detection)
- [XInput API Reference](https://learn.microsoft.com/en-us/windows/win32/xinput/xinput-game-controller-apis-portal)

### Tutorials
- [Procedural Terrain Generation in Unity](https://catlikecoding.com/unity/tutorials/procedural-grid/)
- [Music Visualizer in Unity](https://www.youtube.com/results?search_query=unity+music+visualizer)
- [FFT Audio Analysis](https://en.wikipedia.org/wiki/Fast_Fourier_transform)

### Inspiration Games
- *Sayonara Wild Hearts* (aesthetic reference)
- *Thumper* (rhythm-action, intensity)
- *AudioSurf / Beat Hazard* (music-reactive gameplay)
- *OutRun* (arcade driving feel)

---

**End of Technical Plan**

Ready to build? Start with Phase 1 and iterate from there. Good luck, and enjoy the ride!
