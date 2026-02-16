# Beat Detection Test Instructions

## Overview
Week 3 implementation complete! This guide will help you test the beat detection and audio analysis system.

## What Was Implemented

### ✅ New Files Created:
1. **SimpleFFT.cs** - Fast Fourier Transform implementation (Cooley-Tukey algorithm)
2. **BeatDetectionTest.cs** - Test script with visualization

### ✅ Updated Files:
1. **PreAnalyzer.cs** - Complete implementation of:
   - FFT analysis with Hamming window
   - Spectral flux beat detection
   - Dynamic threshold calculation
   - RMS intensity curve generation
   - BPM estimation from inter-beat intervals
   - Deterministic seed generation

## How to Test

### Setup in Unity:

1. **Create Test Scene:**
   - Open Unity
   - File → New Scene
   - Save as `BeatDetectionTest.unity` in `Assets/Scenes/`

2. **Add Required GameObjects:**

   **GameObject 1: MP3Loader**
   - Create Empty GameObject (GameObject → Create Empty)
   - Name it: `MP3Loader`
   - Add Component: `MP3Loader` script

   **GameObject 2: PreAnalyzer**
   - Create Empty GameObject (GameObject → Create Empty)
   - Name it: `PreAnalyzer`
   - Add Component: `PreAnalyzer` script
   - Inspector settings:
     - FFT Size: `1024` (default)
     - Hop Size: `512` (default)
     - Threshold Multiplier: `1.5` (default)

   **GameObject 3: BeatDetectionTest**
   - Create Empty GameObject (GameObject → Create Empty)
   - Name it: `BeatDetectionTest`
   - Add Component: `BeatDetectionTest` script
   - Inspector settings:
     - Mp3 File Path: `E:\Downloads\Heavy Is The Crown (Original Score).mp3`
     - Display Rect: X=10, Y=10, Width=1200, Height=300
     - Beat Color: Red (default)
     - Intensity Color: Cyan (default)
     - Show Visualization: ✓ Checked

3. **Save Scene**

---

## Running the Test

### Step 1: Enter Play Mode
- Click ▶️ **Play** button in Unity

### Step 2: Trigger Analysis
- In **Hierarchy**, select **BeatDetectionTest** GameObject
- In **Inspector**, right-click **BeatDetectionTest (Script)** component
- Select **"Analyze MP3"** from context menu

### Step 3: Wait for Analysis
- Watch the **Console** - you'll see progress logs:
  ```
  PreAnalyzer: Starting analysis of [path]
  PreAnalyzer: Loaded X samples, Y.YYs
  PreAnalyzer: Processed X windows, detected Y beats
  PreAnalyzer: Estimated BPM = XXX.X from Y beats
  PreAnalyzer: Generated deterministic seed = XXXXXXX
  PreAnalyzer: Analysis complete! BPM=XXX.X, Seed=XXXXXXX
  ✅ Analysis complete!
    - Beats detected: XXX
    - BPM: XXX.X
    - Duration: XXX.XX s
    - Intensity samples: XXXX
    - Level seed: XXXXXXX
  First 10 beats:
    Beat 1: Time=X.XXXs, Strength=X.XXX
    ...
  ```

**Note:** Analysis may take **5-15 seconds** depending on song length and CPU speed.

### Step 4: View Visualization
Once analysis completes, you should see:
- **Cyan curve**: Intensity over time (volume/energy of the music)
- **Red vertical lines**: Detected beat positions
- **Info bar**: Beat count, BPM, duration, and seed

---

## Expected Results

### For "Heavy Is The Crown (Original Score).mp3":
- **Duration**: ~107 seconds
- **Expected beats**: 200-400 (varies by threshold)
- **Expected BPM**: 80-140 (depends on music structure)
- **Level seed**: Consistent number (same MP3 = same seed)

### Visual Appearance:
```
┌──────────────────────────────────────────────────┐
│ ╱╲  ╱╲    ╱╲  ╱╲╱╲    ╱╲  ╱╲    ╱╲  ╱╲  │  ← Cyan intensity curve
│ │ ││ │ ╱╲│ ││ │││ ╱╲│ ││ │ ╱╲│ ││ │││ │  ← Red beat markers
└──────────────────────────────────────────────────┘
Beats: 234 | BPM: 120.5 | Duration: 106.66s | Seed: 1234567
```

---

## Adjusting Parameters

If you want to tune the beat detection:

### PreAnalyzer Settings:

**FFT Size** (default: 1024)
- **Larger (2048, 4096)**: Better frequency resolution, slower processing
- **Smaller (512, 256)**: Faster processing, less accurate

**Hop Size** (default: 512)
- **Larger**: Faster processing, fewer beats detected (lower time resolution)
- **Smaller**: More beats detected, slower processing

**Threshold Multiplier** (default: 1.5)
- **Higher (2.0, 3.0)**: Fewer beats detected (only strong beats)
- **Lower (1.0, 0.5)**: More beats detected (including weak beats)

---

## Troubleshooting

### "No analysis data loaded"
- Make sure you clicked "Analyze MP3" context menu
- Check Console for errors

### Analysis fails with error
- Verify MP3 file path is correct
- Check that MP3Loader and PreAnalyzer GameObjects exist
- Look for red errors in Console

### Too many/few beats detected
- Adjust **Threshold Multiplier**:
  - Too many beats → Increase to 2.0 or higher
  - Too few beats → Decrease to 1.0 or lower

### Analysis is very slow
- Reduce **FFT Size** to 512
- Increase **Hop Size** to 1024
- This is normal for long songs (2-5 minutes)

### BPM seems wrong
- BPM estimation works best on songs with steady rhythm
- For irregular rhythms, manual BPM entry may be needed later

---

## What This Enables

With beat detection working, the game can now:

✅ **Detect beats in music** - Know when to spawn obstacles/coins
✅ **Calculate intensity** - Adjust difficulty based on loud/quiet sections
✅ **Estimate tempo (BPM)** - Sync game speed to music tempo
✅ **Generate deterministic seeds** - Same MP3 = same level layout (for leaderboards)

---

## Next Steps

Once beat detection is working:

### Phase 1 - Week 4: Basic Terrain Generation
- Generate road mesh procedurally
- Modulate terrain based on intensity curve
- Apply beat-synchronized obstacles

### Phase 1 - Week 5: Motorcycle Controller
- Arcade physics implementation
- Controller input handling
- Collision detection

### Phase 1 - Week 6: Music Synchronization
- Play AudioClip during gameplay
- Sync terrain generation with music timeline
- Beat-synchronized visual effects

---

## Testing Checklist

- [ ] Unity loads without errors
- [ ] Scene setup complete (3 GameObjects)
- [ ] Play mode starts successfully
- [ ] "Analyze MP3" context menu appears
- [ ] Analysis completes without errors
- [ ] Console shows beat count and BPM
- [ ] Visualization displays intensity curve
- [ ] Visualization displays beat markers
- [ ] Same MP3 produces same seed (determinism test)

---

**Status**: ✅ Ready for Testing

**Estimated Test Time**: 5-10 minutes

**Files to Review**:
- [SimpleFFT.cs](Assets/Scripts/Audio/SimpleFFT.cs)
- [PreAnalyzer.cs](Assets/Scripts/MP3/PreAnalyzer.cs)
- [BeatDetectionTest.cs](Assets/Scripts/Testing/BeatDetectionTest.cs)
