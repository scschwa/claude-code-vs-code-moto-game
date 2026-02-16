# Motorcycle Controller Test Guide

## Overview
Week 5 implementation complete! This guide shows you how to test the complete gameplay loop: music analysis → terrain generation → motorcycle driving.

## What Was Implemented

### ✅ New Components Created:

1. **MotorcycleController.cs** - Arcade-style player controller
   - Responsive acceleration and steering
   - Two control modes: Free steering or Lane-based
   - Hover system (stays above ground automatically)
   - Visual lean angle feedback
   - Speed management and braking

2. **CameraFollow.cs** - Third-person camera system
   - Smooth following behind motorcycle
   - Look-ahead offset (focuses ahead of player)
   - Dynamic FOV (zooms out at high speed)
   - Camera shake at high speeds
   - Instant snap for resets

3. **MotorcycleTest.cs** - Complete gameplay integration
   - One-click setup (analyze → generate → spawn → drive!)
   - Auto-generates terrain ahead of player
   - Reset and testing controls
   - Debug HUD

---

## Quick Setup (10 minutes)

### Step 1: Create Test Scene

1. **Open Unity**
2. **File → New Scene** → Save as `MotorcycleTest.unity` in `Assets/Scenes/`

### Step 2: Setup Camera

1. **Select Main Camera** in Hierarchy
2. **Add Component:** `CameraFollow`
3. **Configure Inspector:**
   - Follow Distance: `10`
   - Follow Height: `5`
   - Look Ahead Distance: `5`
   - Position Smooth Speed: `5`
   - Rotation Smooth Speed: `5`
   - Dynamic FOV: ✓ Checked
   - Base FOV: `60`
   - Max Speed FOV: `75`
   - Enable Shake: ✓ Checked

### Step 3: Add Test Controller

1. **GameObject → Create Empty**
2. **Name:** `MotorcycleTest`
3. **Add Component:** `MotorcycleTest`
4. **Configure Inspector:**
   - Mp3 File Path: `E:\Downloads\Heavy Is The Crown (Original Score).mp3`
   - Motorcycle Spawn Position: `(0, 2, 0)`
   - Auto Generate Terrain: ✓ Checked
   - Generate Ahead Distance: `50`

### Step 4: Add Supporting Components

These will be auto-created if missing, but you can add manually:

**MP3Loader** (optional - auto-created)
- GameObject → Create Empty → Name: `MP3Loader`
- Add Component: `MP3Loader`

**PreAnalyzer** (optional - auto-created)
- GameObject → Create Empty → Name: `PreAnalyzer`
- Add Component: `PreAnalyzer`

**TerrainGenerator** (optional - auto-created)
- GameObject → Create Empty → Name: `TerrainGenerator`
- Add Component: `TerrainGenerator`
- **Important:** Assign **Road Material** (create a gray material)

### Step 5: Create Road Material

1. **Project window:** Right-click → Create → Material
2. **Name:** `RoadMaterial`
3. **Color:** Gray (RGB: 128, 128, 128)
4. **Drag** into TerrainGenerator's **Road Material** slot

### Step 6: Save Scene
- File → Save (Ctrl+S)

---

## Running the Test

### Method 1: Full Gameplay Loop (Recommended)

1. **Enter Play Mode:** Click ▶️ Play
2. **Initialize:**
   - Select **MotorcycleTest** in Hierarchy
   - Right-click **MotorcycleTest (Script)** in Inspector
   - Select **"Initialize Gameplay"**
3. **Wait for Setup:** (5-15 seconds)
   - Watch Console for progress:
     ```
     Step 1: Analyzing MP3...
     Step 2: Generating terrain...
     Step 3: Spawning motorcycle...
     Step 4: Setting up camera...
     🎉 Gameplay initialized!
     ```
4. **Start Driving!**
   - Use **WASD** or **Arrow Keys** to control
   - Terrain generates automatically as you drive

### Method 2: Quick Test (No Analysis)

If you just want to test controls without music:
1. Enter Play Mode
2. Right-click MotorcycleTest → **"Quick Test - Spawn Motorcycle Only"**
3. Motorcycle spawns on flat ground
4. Test controls immediately

---

## Controls

### Driving:
- **W / Up Arrow** - Accelerate (auto-accelerates by default)
- **A / Left Arrow** - Steer left
- **D / Right Arrow** - Steer right
- **S / Down Arrow** - Brake

### Testing:
- **R** - Reset motorcycle to start position
- **T** - Regenerate terrain with same seed
- **Esc** - Exit Play mode

---

## Expected Results

### Visual:
```
┌─────────────────────────────┐
│      Camera (behind)        │
│           ↓                 │
│      ╔════╗  ← Motorcycle   │
│      ║    ║  (blue capsule) │
│ ═════╩════╩═════════════    │ ← Gray road
│    Terrain with hills       │
└─────────────────────────────┘
```

You should see:
- ✅ **Blue capsule** - Motorcycle placeholder
- ✅ **Camera follows** - Smooth third-person view
- ✅ **Terrain generates** - Road appears ahead automatically
- ✅ **Responsive controls** - Instant steering response
- ✅ **Speed changes FOV** - Camera zooms out when fast
- ✅ **Hills and curves** - Music-reactive terrain
- ✅ **Stays on road** - Hover system keeps motorcycle grounded

### Console Output:
```
MotorcycleTest: Initializing gameplay...
Step 1: Analyzing MP3...
PreAnalyzer: Starting analysis of [path]
PreAnalyzer: Loaded 4,704,768 samples, 106.66s
PreAnalyzer: Detected 234 beats, BPM = 120.5
✅ Analysis complete!
Step 2: Generating terrain...
TerrainGenerator: Initialized with seed 1234567
✅ Terrain initialized (10 segments)
Step 3: Spawning motorcycle...
Motorcycle spawned at (0, 2, 0)
✅ Motorcycle spawned
Step 4: Setting up camera...
✅ Camera configured
🎉 Gameplay initialized!
```

### HUD Display:
```
┌─────────────────────┐
│ BPM: 120.5         │
│ Seed: 1234567      │
│ Speed: 15.3 / 30   │
│ Grounded: True     │
│ WASD/Arrows: Drive │
│ R: Reset | T: Regen│
│ Auto-Gen: True     │
└─────────────────────┘
```

---

## Gameplay Feel

### What You Should Experience:

**Acceleration:**
- Smooth ramp-up to max speed (~30 units/s)
- Feels responsive and arcade-like
- Auto-accelerates by default (like endless runner)

**Steering:**
- Instant response (no delay)
- Smooth turning animations
- Easy to control at all speeds

**Camera:**
- Follows smoothly behind
- Looks ahead for better view
- Zooms out at high speed (cinematic!)
- Slight shake at max speed (intensity!)

**Terrain Interaction:**
- Motorcycle hugs the ground
- Climbs hills automatically (hover system)
- Follows terrain height smoothly
- No bouncing or jittering

---

## Adjusting Parameters

### MotorcycleController Settings:

**Max Speed** (default: 30)
- Higher (50, 100) = Faster, more challenging
- Lower (15, 20) = Slower, easier control

**Turn Speed** (default: 120 degrees/s)
- Higher = Sharper turns
- Lower = Wider, smoother turns

**Hover Height** (default: 0.5)
- Higher = Floats more above terrain
- Lower = Hugs terrain closer

**Use Lane System** (default: false)
- ✓ Checked = Snap to discrete lanes (like Temple Run)
- Unchecked = Free steering (like racing games)

### CameraFollow Settings:

**Follow Distance** (default: 10)
- Higher (15, 20) = Further back, wider view
- Lower (5, 7) = Closer, more intense

**Follow Height** (default: 5)
- Higher = More aerial view
- Lower = More level with motorcycle

**Look Ahead Distance** (default: 5)
- Higher = Camera focuses further ahead
- Lower = Camera focuses closer to motorcycle

---

## Troubleshooting

### "Motorcycle falls through terrain"
- Check that terrain has a **Mesh Collider**
- Set TerrainGenerator's ground layer correctly
- Increase motorcycle's **Hover Height** to 1.0

### "Motorcycle doesn't appear"
- Check Console for spawn errors
- Verify spawn position is above terrain: `(0, 2, 0)`
- Try **"Quick Test - Spawn Motorcycle Only"**

### "Camera is choppy/jittery"
- Make sure CameraFollow script is on Main Camera
- Check **Position Smooth Speed** (try 3-7)
- Verify Update is in **LateUpdate** (it is)

### "Controls don't work"
- Make sure you're in Play mode
- Check that motorcycle has **MotorcycleController** component
- Try clicking on Game view (ensure it has focus)

### "Terrain doesn't generate ahead"
- Check **Auto Generate Terrain** is enabled
- Verify **Generate Ahead Distance** is 50+
- Check TerrainGenerator is assigned in MotorcycleTest

### "Too fast/slow"
- Adjust **Max Speed** in MotorcycleController
- Try 15 for slower, 50 for faster

---

## Testing Checklist

- [ ] Scene created and saved
- [ ] Road material created and assigned
- [ ] Main Camera has CameraFollow component
- [ ] MotorcycleTest GameObject added
- [ ] MP3 file path configured
- [ ] Play mode starts successfully
- [ ] "Initialize Gameplay" completes without errors
- [ ] Motorcycle spawns (blue capsule)
- [ ] Camera follows motorcycle smoothly
- [ ] WASD/Arrow keys control motorcycle
- [ ] Terrain generates ahead automatically
- [ ] Speed changes camera FOV
- [ ] Motorcycle stays on terrain (doesn't fall)
- [ ] R key resets motorcycle
- [ ] T key regenerates terrain

---

## Advanced Features

### Lane-Based Movement:

Enable in MotorcycleController:
1. Set **Use Lane System** to ✓ Checked
2. Set **Lane Width** to 3
3. Use **A/D** to switch lanes instead of continuous steering
4. Better for rhythm-based gameplay!

### Custom Motorcycle Model:

Instead of the capsule:
1. Import your 3D motorcycle model
2. Add Rigidbody and Collider
3. Add MotorcycleController component
4. Assign as **Motorcycle Prefab** in MotorcycleTest
5. Next spawn will use your model!

### Controller Support:

The system automatically detects controllers:
1. Plug in Xbox/PS controller
2. Use **Left Stick** for steering
3. Works alongside keyboard

---

## Performance Metrics

**Expected Performance:**
- **FPS**: 60+ with motorcycle + 10 terrain segments
- **Controls**: <1ms input latency (instant response)
- **Camera**: Smooth 60fps following
- **Physics**: Stable, no jittering

**If performance is poor:**
- Reduce terrain **Active Segment Count** to 5
- Disable **Dynamic FOV** in CameraFollow
- Disable **Enable Shake** in CameraFollow
- Reduce terrain **Length Subdivisions**

---

## Understanding the System

### How It Works:

1. **Initialization:**
   ```
   Analyze MP3 → Generate Terrain → Spawn Motorcycle → Setup Camera
   ```

2. **Gameplay Loop:**
   ```
   Player Input → Motorcycle Movement → Camera Follow → Check Position
        ↓
   Generate More Terrain If Needed
   ```

3. **Auto-Generation:**
   - Tracks motorcycle Z position
   - When close to furthest segment, generates new one
   - Terrain never ends!

### Key Features:

✅ **Arcade Physics** - Responsive, fun, not realistic
✅ **Hover System** - Automatically stays above terrain
✅ **Dynamic Camera** - Adjusts to speed for cinematic feel
✅ **Auto-Generation** - Infinite terrain that never ends
✅ **Music-Reactive** - Terrain height matches music intensity
✅ **Deterministic** - Same song = same terrain layout

---

## Next Steps

Once motorcycle is working:

### Option 1: Add Collectibles
- Spawn coins along the road
- Position at beat locations
- Score system

### Option 2: Add Obstacles
- Traffic, barriers, jumps
- Spawn based on intensity
- Collision detection

### Option 3: Music Synchronization
- Play MP3 during gameplay
- Sync terrain to music timeline
- Beat-synced visual effects

### Option 4: Polish & Effects
- Particle trails behind motorcycle
- Speed lines at high speed
- Beat-synchronized road pulses
- Sky/environment art

---

## Files Created

**New Files:**
- [MotorcycleController.cs](Assets/Scripts/Gameplay/MotorcycleController.cs) - Player controls
- [CameraFollow.cs](Assets/Scripts/Gameplay/CameraFollow.cs) - Third-person camera
- [MotorcycleTest.cs](Assets/Scripts/Testing/MotorcycleTest.cs) - Complete integration

**Integrates With:**
- TerrainGenerator.cs (Week 4)
- PreAnalyzer.cs (Week 3)
- MP3Loader.cs (Week 2)

---

## Tips for Best Experience

1. **Start slow** - Get used to controls before increasing speed
2. **Watch the terrain** - Hills give preview of music intensity
3. **Use auto-acceleration** - Focus on steering, not speed
4. **Adjust camera** - Find your preferred follow distance
5. **Test different songs** - Each creates unique terrain!

---

**Status**: ✅ Ready for Testing

**Estimated Setup Time**: 10 minutes

**Estimated Test Time**: 5-15 minutes of pure driving fun!

---

Enjoy your first ride through music-generated terrain! 🏍️🎵🛣️
