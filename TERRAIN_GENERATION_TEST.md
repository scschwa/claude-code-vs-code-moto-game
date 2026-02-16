# Terrain Generation Test Guide

## Overview
Week 4 implementation complete! This guide shows you how to test the music-reactive procedural terrain generation system.

## What Was Implemented

### ✅ New Components Created:

1. **RoadSegment.cs** - Individual road chunk
   - Procedural mesh generation
   - Configurable width, length, subdivisions
   - Height modulation based on intensity
   - Curve modulation for turns
   - Smooth terrain with proper normals/UVs

2. **TerrainGenerator.cs** - Infinite terrain system
   - Infinite chunking (generates/destroys segments)
   - Music-reactive modulation (intensity → height, curves)
   - Perlin noise integration for organic variation
   - Deterministic generation using level seed
   - Manages active segment pool

3. **TerrainTest.cs** - Test & visualization script
   - Auto-analyzes MP3 files
   - Auto-generates terrain based on music
   - Camera controls for viewing
   - Manual generation controls

---

## Quick Setup (10 minutes)

### Step 1: Create Test Scene

1. **Open Unity**
2. **File → New Scene** → Save as `TerrainTest.unity` in `Assets/Scenes/`
3. **Adjust camera position:**
   - Select **Main Camera** in Hierarchy
   - Set Transform Position: `(0, 10, -20)`
   - Set Transform Rotation: `(20, 0, 0)`
   - This gives an aerial view of the road

### Step 2: Create Road Material

We need a simple material for the road:

1. **In Project window:** Right-click → Create → Material
2. **Name it:** `RoadMaterial`
3. **Configure:**
   - Shader: `Standard` (or `URP/Lit` if using URP)
   - Albedo Color: Gray (`RGB: 128, 128, 128`) or any color you like
   - Metallic: `0`
   - Smoothness: `0.5`

### Step 3: Add GameObjects

Create these 4 GameObjects:

**GameObject 1: MP3Loader**
- GameObject → Create Empty
- Name: `MP3Loader`
- Add Component: `MP3Loader`

**GameObject 2: PreAnalyzer**
- GameObject → Create Empty
- Name: `PreAnalyzer`
- Add Component: `PreAnalyzer`
- Inspector settings (defaults are fine):
  - FFT Size: `1024`
  - Hop Size: `512`
  - Threshold Multiplier: `1.5`

**GameObject 3: TerrainGenerator**
- GameObject → Create Empty
- Name: `TerrainGenerator`
- Add Component: `TerrainGenerator`
- Inspector settings:
  - Road Material: Drag `RoadMaterial` here
  - Segment Length: `20`
  - Road Width: `10`
  - Active Segment Count: `10`
  - Intensity Height Multiplier: `5`
  - Curve Multiplier: `0.5`
  - Use Perlin Noise: ✓ Checked
  - Perlin Scale: `0.1`
  - Perlin Strength: `2`
  - Debug Mode: ✓ Checked (optional, shows logs)

**GameObject 4: TerrainTest**
- GameObject → Create Empty
- Name: `TerrainTest`
- Add Component: `TerrainTest`
- Inspector settings:
  - Mp3 File Path: `E:\Downloads\Heavy Is The Crown (Original Score).mp3` (or your MP3 path)
  - Auto Generate: ✓ Checked
  - Auto Generate Interval: `2` seconds
  - Auto Move Camera: ✓ Checked (optional)
  - Camera Speed: `5`

### Step 4: Save Scene
- File → Save (Ctrl+S)

---

## Running the Test

### Method 1: Auto-Analysis (Recommended)

1. **Enter Play Mode:** Click ▶️ Play button
2. **Trigger Analysis:**
   - In Hierarchy, select **TerrainTest** GameObject
   - In Inspector, right-click **TerrainTest (Script)** component
   - Select **"Analyze and Generate"** from context menu
3. **Wait for Analysis:** (5-15 seconds)
   - Watch Console for progress logs
4. **Watch Terrain Generate:**
   - Road segments appear automatically
   - Height varies based on music intensity
   - Curves appear randomly (deterministic from seed)

### Method 2: Use Pre-Analyzed Data

If you already ran beat detection test:
1. Load your `BeatDetectionTest` scene
2. Copy the `analysisData` from BeatDetectionTest component
3. Go to TerrainTest scene
4. Paste into TerrainTest's `analysisData` field
5. Right-click TerrainTest → **"Generate from Existing Data"**

---

## Expected Results

### Visual Appearance:
```
     ╱╲___╱╲___           Camera view (aerial)
    ╱        ╲╱╲          ↓
   ╱  Road    ╱ ╲      ┌─────────────────┐
  ╱  Segment ╱   ╲     │   ╱───╲         │
 ╱   (gray) ╱     ╲    │  ╱     ╲___     │
╱__________╱       ╲   │ ╱          ╲    │
                        └─────────────────┘
```

You should see:
- ✅ **Gray road** extending forward
- ✅ **Height variations** - hills and valleys (taller when music is loud)
- ✅ **Curves** - road bends left/right randomly
- ✅ **Smooth mesh** - no sharp edges
- ✅ **Continuous generation** - new segments appear every 2 seconds

### Console Output:
```
PreAnalyzer: Starting analysis of [path]
PreAnalyzer: Loaded 4,704,768 samples, 106.66s
PreAnalyzer: Processed 9192 windows, detected 234 beats
PreAnalyzer: Estimated BPM = 120.5 from 234 beats
PreAnalyzer: Generated deterministic seed = 1234567
TerrainGenerator: Initialized with seed 1234567, 9192 intensity samples
Generated segment 1 at (0.0, 0.0, 20.0), curve=0.32
Generated segment 2 at (1.5, 1.2, 40.0), curve=-0.18
...
```

---

## Camera Controls

### While in Play Mode:

**Automatic Movement:**
- If "Auto Move Camera" is checked, camera flies forward along road

**Manual Controls:**
- **SPACE** - Generate next road segment manually
- **R** - Reset and regenerate all terrain (tests determinism)
- **Esc** - Exit Play mode

**Viewing Tips:**
- Adjust Main Camera position to get different views
- Try `(0, 5, -10)` for closer view
- Try `(0, 20, -30)` for wider overview

---

## Adjusting Parameters

### TerrainGenerator Settings:

**Segment Length** (default: 20)
- **Larger (50, 100)**: Longer, smoother road segments
- **Smaller (10, 5)**: More frequent variation, sharper transitions

**Intensity Height Multiplier** (default: 5)
- **Higher (10, 20)**: More dramatic hills/valleys
- **Lower (2, 1)**: Flatter terrain
- **Effect:** Loud music = taller hills

**Curve Multiplier** (default: 0.5)
- **Higher (1.0, 2.0)**: More extreme curves and turns
- **Lower (0.1, 0)**: Straighter road

**Perlin Scale** (default: 0.1)
- **Smaller (0.05)**: More variation, "bumpier" terrain
- **Larger (0.2, 0.5)**: Smoother, more gradual changes

**Perlin Strength** (default: 2)
- **Higher (5, 10)**: Stronger organic height variation
- **Lower (1, 0)**: Less Perlin influence, more music-driven

---

## Troubleshooting

### "No analysis data available"
- Make sure you clicked **"Analyze and Generate"** context menu
- Check Console for analysis errors
- Verify MP3 file path is correct

### Road not appearing
- Check that **RoadMaterial** is assigned in TerrainGenerator
- Look in Scene view (not just Game view)
- Try moving camera closer: `(0, 5, -10)`

### Road is flat/boring
- Increase **Intensity Height Multiplier** to 10 or higher
- Check that your MP3 has dynamic volume (loud/quiet sections)
- Enable **Debug Mode** in TerrainGenerator to see logs

### Road is too curvy/chaotic
- Decrease **Curve Multiplier** to 0.1 or 0
- Adjust **Perlin Scale** to 0.2 (larger = smoother)

### Performance issues
- Reduce **Active Segment Count** to 5
- Reduce **Length Subdivisions** in RoadSegment (if using prefab)
- Increase **Auto Generate Interval** to 5 seconds

### Same seed produces different terrain
- Check **Debug Mode** logs to verify seed is same
- Make sure you're using the same MP3 file
- Ensure Perlin noise seed is derived from level seed (it is)

---

## Testing Checklist

- [ ] Unity scene created and saved
- [ ] Road material created and assigned
- [ ] All 4 GameObjects added (MP3Loader, PreAnalyzer, TerrainGenerator, TerrainTest)
- [ ] MP3 file path configured
- [ ] Play mode starts successfully
- [ ] Analysis completes without errors
- [ ] Road segments generate automatically
- [ ] Road has height variation (hills/valleys)
- [ ] Road has curves (bends left/right)
- [ ] Road material renders correctly (gray, textured)
- [ ] SPACE key generates next segment
- [ ] R key resets terrain with same seed
- [ ] Console shows generation debug info

---

## Understanding the System

### How It Works:

1. **Analysis Phase:**
   - MP3 is analyzed for beats and intensity
   - Intensity curve is sampled (~10 samples per second)
   - Deterministic seed is generated from audio features

2. **Generation Phase:**
   - TerrainGenerator reads intensity curve sequentially
   - Each segment gets ~10 intensity samples
   - High intensity → taller hills
   - Low intensity → flatter terrain
   - Seeded random determines curves

3. **Rendering:**
   - RoadSegment creates mesh vertices
   - Applies height from intensity + Perlin noise
   - Applies curve offset to vertices
   - Unity renders the mesh with material

### Key Features:

✅ **Deterministic** - Same MP3 always generates same road
✅ **Music-Reactive** - Terrain height matches music intensity
✅ **Infinite** - Generates forever, destroys old segments
✅ **Organic** - Perlin noise adds natural variation
✅ **Performant** - Only keeps 10 segments active at once

---

## Next Steps

Once terrain is working:

### Option 1: Add Visual Effects
- Beat-synchronized particles
- Lighting changes based on intensity
- Skybox/environment

### Option 2: Add Obstacles
- Spawn obstacles at beat positions
- Place coins along the road
- Dynamic difficulty based on intensity

### Option 3: Motorcycle Controller
- Drive on the generated terrain
- Camera follows player
- Test gameplay feel

### Option 4: Music Synchronization
- Play MP3 during generation
- Sync terrain to music timeline
- Real-time terrain updates

---

## Advanced Configuration

### Creating a Road Prefab (Optional):

1. Create an empty GameObject in scene
2. Add **RoadSegment** component
3. Add **MeshFilter** and **MeshRenderer**
4. Configure default values in RoadSegment
5. Drag to Project window to create prefab
6. Assign prefab to TerrainGenerator's **Road Segment Prefab** field

### Multiple Road Styles:

Create different materials:
- **Highway Material** - White with yellow lines
- **Desert Material** - Sand/dirt texture
- **Neon Material** - Glowing, Sayonara Wild Hearts style

Swap materials at runtime for visual variety!

---

## Performance Metrics

**Expected Performance:**
- **FPS**: 60+ with 10 active segments
- **Memory**: ~50MB for terrain meshes
- **Generation Time**: ~0.1ms per segment
- **Active Tris**: ~6,000 (10 segments × 600 tris)

**Optimization Tips:**
- Use object pooling (reuse segment GameObjects)
- Reduce subdivisions for simpler geometry
- Use LOD (Level of Detail) for distant segments
- Bake lighting if static environment

---

## Files to Review

- [RoadSegment.cs](Assets/Scripts/Terrain/RoadSegment.cs)
- [TerrainGenerator.cs](Assets/Scripts/Terrain/TerrainGenerator.cs)
- [TerrainTest.cs](Assets/Scripts/Testing/TerrainTest.cs)

---

**Status**: ✅ Ready for Testing

**Estimated Setup Time**: 10 minutes

**Estimated Test Time**: 5-10 minutes

---

Enjoy watching your music come to life as a procedurally generated road! 🛣️🎵
