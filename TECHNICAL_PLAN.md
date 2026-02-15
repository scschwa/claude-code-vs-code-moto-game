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
| **Audio Capture** | NAudio + WASAPI Loopback | Industry-standard Windows audio capture, low latency, captures all system audio sources |
| **Beat Detection** | Unity-Beat-Detection (GitHub) | Open-source, FFT-based, proven in music games, easy integration |
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
┌─────────────────────────────────────────────────────────────┐
│                     SYSTEM AUDIO                            │
│              (Spotify, YouTube, Local Files)                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                 AUDIO CAPTURE MODULE                         │
│  ┌────────────────────────────────────────────────────┐    │
│  │  WASAPI Loopback (NAudio)                          │    │
│  │  - Captures system audio in real-time              │    │
│  │  - Sample rate: 44.1kHz or 48kHz                   │    │
│  │  - Buffer size: 512-2048 samples                   │    │
│  └────────────────────────────────────────────────────┘    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│               BEAT DETECTION ENGINE                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Audio Analyzer                                     │    │
│  │  - FFT Analysis (frequency spectrum)               │    │
│  │  - Spectral Flux (energy changes)                  │    │
│  │  - Beat Detection (peak picking)                   │    │
│  │  - BPM Estimation                                  │    │
│  │  - Intensity Calculation                           │    │
│  └────────────────────────────────────────────────────┘    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              GAMEPLAY CONTROLLER (Hub)                       │
│  - Receives beat/BPM/intensity data                         │
│  - Translates to gameplay parameters                        │
│  - Manages game state and synchronization                   │
└─────┬──────────┬──────────┬──────────┬────────────┬─────────┘
      │          │          │          │            │
      ▼          ▼          ▼          ▼            ▼
┌──────────┐ ┌──────┐ ┌──────────┐ ┌───────┐ ┌──────────┐
│ TERRAIN  │ │MOTOR-│ │OBSTACLE  │ │ SCORE │ │  VISUAL  │
│GENERATOR │ │CYCLE │ │ SPAWNER  │ │ SYSTEM│ │  EFFECTS │
│          │ │      │ │          │ │       │ │          │
│- Perlin  │ │- Phy-│ │- Traffic │ │- Coins│ │- Partcl. │
│  noise   │ │  sics│ │- Rocks   │ │- Combo│ │- Camera  │
│- Music   │ │- Ctrl│ │- Beat    │ │- Multi│ │- Shader  │
│  driven  │ │  input│ │  sync   │ │  plier│ │  effects │
└──────────┘ └──────┘ └──────────┘ └───────┘ └──────────┘
```

### Core Modules

#### 1. **Audio Capture Module** (`AudioCaptureManager.cs`)
- Initializes WASAPI loopback recording
- Continuously captures system audio buffer
- Provides raw audio samples to beat detector
- Handles audio device changes and errors

#### 2. **Beat Detection Engine** (`BeatDetector.cs`, `MusicAnalyzer.cs`)
- Performs FFT on audio samples
- Calculates spectral flux
- Detects beat onsets using peak detection
- Estimates BPM via inter-beat interval analysis
- Outputs normalized intensity (0-1 scale)

#### 3. **Terrain Generator** (`TerrainGenerator.cs`)
- Procedurally generates road chunks ahead of player
- Uses Perlin noise for heightmap elevation
- Modulates noise parameters based on music intensity
- Manages chunk lifecycle (creation, pooling, destruction)

#### 4. **Motorcycle Controller** (`MotorcycleController.cs`)
- Handles player input (controller/keyboard)
- Arcade physics simulation
- Speed tied to BPM
- Collision detection and response

#### 5. **Obstacle Spawner** (`ObstacleSpawner.cs`)
- Spawns traffic, rocks, barriers on beats
- Density scales with music intensity
- Pools objects for performance

#### 6. **Score & Collectibles Manager** (`CollectibleManager.cs`, `ScoreSystem.cs`)
- Spawns coins in music-reactive patterns
- Tracks collection, combo multiplier
- Calculates score with bonuses

#### 7. **Visual Effects System** (`VisualEffectsController.cs`)
- Particle effects synced to beats
- Camera shake and zoom on intensity changes
- Shader effects (bloom, color grading)
- Motorcycle trail renderer

---

## 3. Development Phases

### Phase 1: Foundation & Prototype (Weeks 1-4)
**Goal:** Prove the core concept—music reactivity works and feels good.

**Tasks:**
1. **Unity Project Setup**
   - Create new Unity 2022 LTS project
   - Configure URP (Universal Render Pipeline)
   - Set up Git repository with .gitignore for Unity

2. **Audio Capture Implementation**
   - Install NAudio via NuGet
   - Implement `AudioCaptureManager.cs` with WASAPI loopback
   - Test capturing system audio (play music, verify waveform in debug)
   - Handle audio device enumeration and selection

3. **Basic Beat Detection**
   - Integrate Unity-Beat-Detection from GitHub
   - Implement FFT analysis on captured audio
   - Detect simple beat onsets (spectral flux peaks)
   - Visualize beats with debug UI (flash screen on beat)

4. **Simple Motorcycle Movement**
   - Create basic motorcycle GameObject with Rigidbody
   - Implement forward movement at constant speed
   - Add left/right steering with keyboard/controller
   - Camera follows motorcycle (third-person view)

5. **Flat Terrain Test**
   - Create simple flat road mesh
   - Motorcycle drives on road surface
   - Test physics (gravity, collision)

6. **Synchronization Test**
   - Link detected beats to simple visual feedback
   - Verify audio-visual sync (<50ms latency)
   - Adjust buffer sizes if needed

**Deliverable:** Prototype where music plays, beats are detected, and a motorcycle drives on a flat road with visual beat feedback.

---

### Phase 2: Core Gameplay (Weeks 5-10)
**Goal:** Full music-reactive gameplay loop—terrain changes with music, obstacles appear, coins collectible.

**Tasks:**
1. **Refine Beat Detection**
   - Implement BPM estimation algorithm
   - Calculate music intensity (RMS energy, spectral flux variance)
   - Smooth intensity values to avoid jittery changes
   - Add calibration settings (sensitivity sliders)

2. **Procedural Terrain Generation**
   - Implement Perlin noise heightmap generator
   - Create terrain chunks (e.g., 100m x 20m)
   - Spawn chunks ahead of player, despawn behind
   - Modulate noise amplitude/frequency based on music intensity

3. **Music-Reactive Terrain Modulation**
   - High intensity → Higher amplitude noise (more hills)
   - High intensity → Higher frequency noise (more curves)
   - Low intensity → Smooth, flat terrain
   - Road width narrows/widens based on intensity

4. **Obstacle System**
   - Create traffic vehicle prefabs (simple models)
   - Create rock/barrier prefabs
   - Implement `ObstacleSpawner.cs` that spawns on beats
   - Obstacle density scales with intensity
   - Object pooling for performance

5. **Controller Input with Vibration**
   - Integrate XInput for Xbox controllers
   - Map steering to left stick, boost to A button
   - Implement rumble on beats and collisions
   - Add keyboard controls as fallback

6. **Coin Collection & Scoring**
   - Create coin prefabs with glow/pulse effects
   - Spawn coins along road in patterns
   - Implement collection on trigger enter
   - Display score and combo multiplier in UI
   - Combo resets on miss or crash

7. **Crash & Respawn System**
   - Detect collisions with obstacles
   - Quick respawn (no game over in Free Ride mode)
   - Briefly reduce speed, reset combo
   - Visual/audio feedback on crash

**Deliverable:** Fully playable core loop—ride motorcycle through music-reactive terrain, avoid obstacles, collect coins, see score.

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

### 4.1 Audio Capture Implementation (WASAPI + NAudio)

**WASAPI (Windows Audio Session API):**
- Windows API for low-latency audio capture
- **Loopback mode:** Captures output audio ("what you hear")
- Works with all audio sources (Spotify, YouTube, system sounds)

**NAudio Library:**
- Open-source .NET audio library
- Provides `WasapiLoopbackCapture` class
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

### 4.2 Beat Detection Algorithm

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

### 4.3 Procedural Terrain Generation

**Approach: Perlin Noise Heightmap with Music Modulation**

**Perlin Noise:**
- Generates smooth, organic-looking terrain
- 2D noise function: `Noise(x, z)` returns height value
- Parameters: amplitude (height scale), frequency (detail level), octaves (layering)

**Music-Driven Modulation:**
```csharp
public class TerrainGenerator : MonoBehaviour
{
    // Noise parameters
    public float baseAmplitude = 5f;
    public float baseFrequency = 0.1f;
    public int octaves = 3;

    // Music reactive scaling
    private float intensityScale = 1f;

    // Chunk management
    private float chunkSize = 100f;
    private int chunksAhead = 5;
    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Update()
    {
        // Get music intensity from BeatDetector
        float intensity = BeatDetector.Instance.CurrentIntensity;

        // Scale terrain difficulty
        intensityScale = Mathf.Lerp(0.5f, 2f, intensity);

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

### 4.4 Motorcycle Physics

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
│   │   ├── GameScene.unity
│   │   └── Calibration.unity
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   └── SceneLoader.cs
│   │   │
│   │   ├── Audio/
│   │   │   ├── AudioCaptureManager.cs
│   │   │   ├── BeatDetector.cs
│   │   │   ├── MusicAnalyzer.cs
│   │   │   └── CalibrationSystem.cs
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
│   │   │   └── Leaderboard.cs
│   │   │
│   │   ├── UI/
│   │   │   ├── HUDManager.cs
│   │   │   ├── MenuController.cs
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
├── .gitignore
├── README.md
├── GDD.md
└── TECHNICAL_PLAN.md (this file)
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

#### NAudio (Audio Capture)
- **Source:** NuGet package
- **Installation:**
  1. Download NAudio.dll from NuGet
  2. Place in `Assets/Plugins/`
  3. Import in Unity

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
- **Audio Capture:** Verify WASAPI captures system audio correctly
- **Beat Detection:** Test with known BPM songs, verify accuracy
- **Terrain Generation:** Ensure no seams between chunks
- **Collision Detection:** Verify all obstacles trigger crashes
- **Score System:** Validate combo multipliers, bonuses

### Integration Testing
- **End-to-End:** Play music, ride motorcycle, verify reactivity
- **Audio-Visual Sync:** Measure latency (should be <50ms)
- **Performance:** Profile during 5+ minute sessions

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

### Challenge 1: WASAPI Latency
**Problem:** Audio capture may have noticeable delay between music and visuals.

**Solutions:**
- Reduce buffer size (test 512, 1024, 2048 samples)
- Implement manual latency offset in calibration
- Use exclusive mode WASAPI (if possible)
- Fallback: Virtual Audio Cable (VB-Cable)

**Mitigation:** Allow players to calibrate latency manually.

---

### Challenge 2: Beat Detection Accuracy Varies by Genre
**Problem:** Electronic music has clear beats; classical music is complex and variable.

**Solutions:**
- Adjustable sensitivity slider (per-genre profiles)
- Use Essentia library for ML-based beat tracking (more accurate)
- Frequency-specific beat detection (focus on bass for kicks)
- Smoothing/averaging to reduce false positives

**Mitigation:** Default to "good enough" with option for advanced tuning.

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

### Week 2: Audio Capture POC
1. Implement `AudioCaptureManager.cs` with WASAPI
2. Test capturing Spotify, YouTube, local music
3. Visualize waveform in Unity (debug display)
4. Verify low latency

### Week 3: Beat Detection POC
1. Integrate Unity-Beat-Detection
2. Detect beats, display visual flash on screen
3. Estimate BPM, display in UI
4. Test with different music genres

### Week 4: Prototype Gameplay
1. Create simple motorcycle controller
2. Generate flat road
3. Sync motorcycle speed to BPM
4. Add one obstacle that spawns on beats
5. Test the "feel"—does music reactivity work?

**Milestone:** If Week 4 prototype feels good, proceed with full development!

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
