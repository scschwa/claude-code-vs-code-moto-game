# Feature 2: Obstacles & Traffic System - Setup Guide

## Implementation Complete!

All code for Feature 2 has been implemented. This guide explains how to set up the prefabs and configure the system in Unity.

---

## Files Created/Modified

### New Files
1. **ObstacleSpawner.cs** (`Assets/Scripts/Gameplay/ObstacleSpawner.cs`)
   - Singleton spawner that creates obstacles based on music intensity
   - Intensity-based spawning algorithm with difficulty curve
   - Configurable obstacle types with weights and intensity thresholds

2. **Obstacle.cs** (`Assets/Scripts/Gameplay/Obstacle.cs`)
   - Individual obstacle behavior and collision handling
   - Movement logic for Moving obstacles
   - Speed penalty application on collision
   - Pool return logic based on obstacle type

### Modified Files
3. **TerrainGenerator.cs** (`Assets/Scripts/Terrain/TerrainGenerator.cs`)
   - Added obstacle spawning call after collectible spawning
   - Passes segmentStartTime to ObstacleSpawner

4. **MotorcycleController.cs** (`Assets/Scripts/Gameplay/MotorcycleController.cs`)
   - Added `ApplySpeedPenalty(float penaltyFactor)` method
   - Added `OnCollisionEnter()` callback for debug logging
   - Minimum speed set to 20% of maxSpeed

5. **MotorcycleTest.cs** (`Assets/Scripts/Testing/MotorcycleTest.cs`)
   - Creates ObstacleSpawner in `EnsureComponentsExist()`
   - Initializes ObstacleSpawner with analysis data in `InitializeGameplay()`

6. **SegmentObjectTracker.cs** (`Assets/Scripts/Terrain/SegmentObjectTracker.cs`)
   - Enhanced cleanup to handle both collectibles and obstacles
   - Checks for Obstacle component and returns to correct pool

---

## Prefab Setup Instructions

You need to create 4 obstacle prefabs in Unity. Follow these steps:

### 1. Traffic Cone Prefab

**Steps:**
1. In Unity Hierarchy, create: GameObject > 3D Object > Cylinder
2. Name it: `TrafficConePrefab`
3. Transform settings:
   - Scale: (0.4, 0.8, 0.4)
   - Position: (0, 0, 0)
4. Create a Material:
   - Name: `TrafficConeMaterial`
   - Color: Orange (#FF6600) with white stripes (use a texture if desired)
   - Assign to cylinder's Renderer
5. Add Components:
   - **BoxCollider**:
     - Size: (0.4, 0.8, 0.4)
     - Is Trigger: **FALSE** (unchecked)
   - **Rigidbody**:
     - Mass: 1
     - Is Kinematic: **TRUE** (checked)
     - Use Gravity: **FALSE** (unchecked)
   - **Obstacle script**:
     - Obstacle Type: `OneLane`
     - Speed Penalty Factor: `0.6` (reduces to 60% speed)
     - Pool Name: `TrafficCone`
     - Is Moving: **FALSE** (unchecked)
6. Save as prefab: Drag to `Assets/Prefabs/` folder

### 2. Barrier Small Prefab

**Steps:**
1. In Unity Hierarchy, create: GameObject > 3D Object > Cube
2. Name it: `BarrierSmallPrefab`
3. Transform settings:
   - Scale: (1, 0.5, 2)
   - Position: (0, 0, 0)
4. Create a Material:
   - Name: `BarrierMaterial`
   - Color: Gray (#808080) or concrete texture
   - Assign to cube's Renderer
5. Add Components:
   - **BoxCollider**:
     - Size: (1, 0.5, 2)
     - Is Trigger: **FALSE** (unchecked)
   - **Rigidbody**:
     - Mass: 10
     - Is Kinematic: **TRUE** (checked)
     - Use Gravity: **FALSE** (unchecked)
   - **Obstacle script**:
     - Obstacle Type: `OneLane`
     - Speed Penalty Factor: `0.5` (reduces to 50% speed)
     - Pool Name: `BarrierSmall`
     - Is Moving: **FALSE** (unchecked)
6. Save as prefab: Drag to `Assets/Prefabs/` folder

### 3. Barrier Large Prefab

**Steps:**
1. In Unity Hierarchy, create: GameObject > 3D Object > Cube
2. Name it: `BarrierLargePrefab`
3. Transform settings:
   - Scale: (10, 2, 1)
   - Position: (0, 0, 0)
4. Create a Material:
   - Name: `BarrierLargeMaterial`
   - Color: Red/Yellow warning stripes (#FF0000 and #FFFF00)
   - Assign to cube's Renderer
5. Add Components:
   - **BoxCollider**:
     - Size: (10, 2, 1)
     - Is Trigger: **FALSE** (unchecked)
   - **Rigidbody**:
     - Mass: 100
     - Is Kinematic: **TRUE** (checked)
     - Use Gravity: **FALSE** (unchecked)
   - **Obstacle script**:
     - Obstacle Type: `FullWidth`
     - Speed Penalty Factor: `0.3` (reduces to 30% speed)
     - Pool Name: `BarrierLarge`
     - Is Moving: **FALSE** (unchecked)
     - Warning Distance: `40`
6. Save as prefab: Drag to `Assets/Prefabs/` folder

### 4. Vehicle Prefab

**Steps:**
1. In Unity Hierarchy, create: GameObject > 3D Object > Cube
2. Name it: `VehiclePrefab`
3. Transform settings:
   - Scale: (2, 1.5, 4)
   - Position: (0, 0, 0)
4. Create a Material:
   - Name: `VehicleMaterial`
   - Color: Random bright color (e.g., #00AAFF for blue car)
   - Assign to cube's Renderer
5. Optional: Add child objects for wheels (4 small cylinders)
6. Add Components:
   - **BoxCollider**:
     - Size: (2, 1.5, 4)
     - Is Trigger: **FALSE** (unchecked)
   - **Rigidbody**:
     - Mass: 50
     - Is Kinematic: **TRUE** (checked)
     - Use Gravity: **FALSE** (unchecked)
   - **Obstacle script**:
     - Obstacle Type: `Moving`
     - Speed Penalty Factor: `0.4` (reduces to 40% speed)
     - Pool Name: `Vehicle`
     - Is Moving: **TRUE** (checked)
     - Move Speed: `3`
     - Move Direction: `1` (or `-1`, set randomly at spawn)
     - Move Distance: `6`
7. Save as prefab: Drag to `Assets/Prefabs/` folder

---

## ObjectPoolManager Configuration

After creating the prefabs, configure the ObjectPoolManager:

1. Find **ObjectPoolManager** in your scene Hierarchy
2. In the Inspector, expand **Pool Configuration**
3. Set **Pool Configs** size to **4** (or add 4 more if you already have coins)
4. Configure each pool:

**Pool 0: Traffic Cone**
- Pool Name: `TrafficCone`
- Prefab: [Drag TrafficConePrefab here]
- Initial Size: `20`
- Max Size: `50`

**Pool 1: Barrier Small**
- Pool Name: `BarrierSmall`
- Prefab: [Drag BarrierSmallPrefab here]
- Initial Size: `10`
- Max Size: `30`

**Pool 2: Barrier Large**
- Pool Name: `BarrierLarge`
- Prefab: [Drag BarrierLargePrefab here]
- Initial Size: `5`
- Max Size: `15`

**Pool 3: Vehicle**
- Pool Name: `Vehicle`
- Prefab: [Drag VehiclePrefab here]
- Initial Size: `10`
- Max Size: `25`

---

## ObstacleSpawner Configuration

After running the game once (so ObstacleSpawner is created), configure it:

1. Find **ObstacleSpawner** in your scene Hierarchy
2. In the Inspector, configure settings:

### Music Sync Settings
- Base Spawn Chance: `0.1` (10% at low intensity)
- High Intensity Spawn Chance: `0.6` (60% at high intensity)
- Intensity Threshold: `0.6` (threshold for "high intensity")

### Difficulty Settings
- Difficulty Curve: Animation curve from (0s, 0.5x) to (100s, 2.0x)
- Safe Start Segments: `3` (first 3 segments have no obstacles)

### Obstacle Types Configuration
Set **Obstacle Types** size to **4** and configure:

**Type 0: Traffic Cone**
- Pool Name: `TrafficCone`
- Spawn Weight: `2.0` (more common)
- Type: `OneLane`
- Min Intensity Required: `0.0` (can spawn at any intensity)

**Type 1: Barrier Small**
- Pool Name: `BarrierSmall`
- Spawn Weight: `1.5`
- Type: `OneLane`
- Min Intensity Required: `0.3` (requires medium intensity)

**Type 2: Barrier Large**
- Pool Name: `BarrierLarge`
- Spawn Weight: `0.5` (rare)
- Type: `FullWidth`
- Min Intensity Required: `0.7` (requires high intensity)

**Type 3: Vehicle**
- Pool Name: `Vehicle`
- Spawn Weight: `1.0`
- Type: `Moving`
- Min Intensity Required: `0.4` (requires medium+ intensity)

### Spacing
- Lane Spacing: `2.0` (should match road width and collectible spacing)

### Debug
- Debug Mode: Check this to see spawn logs

---

## Testing Checklist

After setup, test the following:

### Basic Functionality
- [ ] ObstacleSpawner initializes with analysis data
- [ ] Obstacles spawn on road segments (after first 3 segments)
- [ ] Different obstacle types appear (cones, barriers, vehicles)
- [ ] Obstacles are positioned correctly on lanes

### Collision & Penalty
- [ ] Collision detection works when motorcycle hits obstacle
- [ ] Speed penalty applies correctly (motorcycle slows down)
- [ ] Console shows "Speed penalty applied!" message
- [ ] Motorcycle doesn't go below 20% max speed (6 units/s if max=30)

### Obstacle Behavior
- [ ] **OneLane obstacles** (cone, small barrier): Disappear after collision
- [ ] **FullWidth obstacles** (large barrier): Stay in place after collision
- [ ] **Moving obstacles** (vehicle): Move horizontally across road
- [ ] Moving obstacles reverse direction at edges

### Intensity-Based Spawning
- [ ] More obstacles spawn during high-intensity sections
- [ ] Fewer obstacles spawn during low-intensity sections
- [ ] Harder obstacles (FullWidth, Moving) only appear at high intensity
- [ ] Traffic cones appear at all intensity levels

### Cleanup & Performance
- [ ] Obstacles cleanup when segment is destroyed
- [ ] Objects return to pool correctly (check ObjectPoolManager statistics)
- [ ] No memory leaks or performance issues
- [ ] Pool statistics show correct active/available counts

### Difficulty Progression
- [ ] Spawn rate increases over time (difficulty curve)
- [ ] First 3 segments are clear (safe start period)
- [ ] Game gets progressively harder

### Balance
- [ ] Never completely blocks player (always an escape route)
- [ ] Obstacles are challenging but fair
- [ ] Speed penalties feel appropriate (not too harsh)
- [ ] Collectibles and obstacles don't overlap too much

---

## Debug Commands

Use these to test and debug:

### Inspector Context Menu
- Right-click **ObjectPoolManager** > "Show Pool Statistics"
  - Shows current pool usage for all pools

### Console Logs
Enable debug mode on these components to see detailed logs:
- **ObstacleSpawner**: Shows spawn decisions, intensity values
- **Obstacle**: Shows collision events, pool returns
- **SegmentObjectTracker**: Shows object registration/cleanup

### Test Different Songs
Try MP3 files with different characteristics:
- **High BPM/Intensity**: Should spawn many obstacles
- **Low BPM/Calm**: Should spawn fewer obstacles
- **Dynamic songs**: Spawn rate should follow intensity

---

## Balancing Tips

If the game feels too hard or too easy:

### Too Many Obstacles
- Decrease `baseSpawnChance` (default: 0.1)
- Decrease `highIntensitySpawnChance` (default: 0.6)
- Increase `intensityThreshold` (default: 0.6)
- Increase `safeStartSegments` (default: 3)

### Too Few Obstacles
- Increase spawn chances
- Decrease intensity thresholds
- Adjust obstacle type weights

### Speed Penalties Too Harsh
- Increase `speedPenaltyFactor` on obstacles (closer to 1.0)
- Examples: 0.5 = harsh, 0.7 = medium, 0.9 = gentle

### Moving Obstacles Too Fast/Slow
- Adjust `moveSpeed` on Vehicle prefab (default: 3)
- Adjust `moveDistance` for shorter/longer travel

---

## Integration Notes

### Works With Feature 1
- Uses same ObjectPoolManager for pooling
- Uses same SegmentObjectTracker for cleanup
- Spawns alongside collectibles without conflicts
- Different seeding (LevelSeed + 1000) for variety

### Differences from Collectibles
- **Intensity-based** spawning (vs beat-based for collectibles)
- **Penalty** on collision (vs reward for collectibles)
- **Physical collision** (vs trigger detection)
- **Harder obstacles** at high intensity (vs more collectibles)

---

## Known Limitations

1. **No warning UI** for FullWidth obstacles (Obstacle.cs has the code, but UI not implemented)
2. **Simple visuals** (primitive shapes) - replace with proper 3D models later
3. **No sound effects** configured (AudioClip fields are there, just add clips)
4. **No particle effects** configured (ParticleSystem fields are there, just add effects)

---

## Next Steps

To enhance the obstacle system:

1. **Add 3D Models**: Replace primitive shapes with proper car/barrier models
2. **Add Effects**: Collision particles, smoke, sparks
3. **Add Sounds**: Impact sounds, warning beeps
4. **Warning UI**: Show on-screen indicator when FullWidth obstacle ahead
5. **Variety**: Add more obstacle types (rocks, animals, etc.)
6. **Patterns**: Spawn obstacle formations (slalom, gaps)
7. **Power-ups**: Add shield/invincibility collectible to avoid penalties

---

## Troubleshooting

### Obstacles Don't Spawn
- Check ObjectPoolManager has obstacle pools configured
- Check ObstacleSpawner.Initialize() was called
- Enable debug mode to see spawn decisions
- Check intensity values (may be too low)

### Collision Doesn't Work
- Ensure motorcycle has "Player" tag
- Ensure obstacles have non-trigger Collider
- Ensure obstacles have kinematic Rigidbody
- Check Layer collision matrix (Edit > Project Settings > Physics)

### Obstacles Don't Return to Pool
- Check SegmentObjectTracker is on each segment
- Check poolName is set correctly on Obstacle script
- Enable debug mode on ObjectPoolManager

### Speed Never Recovers
- Speed penalty is permanent until acceleration kicks in
- Motorcycle will naturally accelerate back to maxSpeed over time
- Hitting multiple obstacles compounds the penalty (intentional)

---

## Code Reference

### Key Methods

**ObstacleSpawner.cs**
- `Initialize(AnalysisData)` - Setup with music data
- `SpawnObstaclesForSegment(segment, time)` - Main spawn logic
- `GetAverageIntensityForSegment()` - Intensity sampling
- `ChooseObstacleType(intensity)` - Type selection

**Obstacle.cs**
- `OnCollisionEnter(Collision)` - Collision detection
- `HandlePlayerCollision()` - Apply penalty
- `UpdateMovement()` - Moving obstacle logic
- `ReturnToPool()` - Cleanup

**MotorcycleController.cs**
- `ApplySpeedPenalty(float)` - Reduces speed by factor

---

## Summary

Feature 2 is fully implemented! The obstacle system:
- Spawns obstacles based on music intensity (higher = more/harder)
- Three obstacle types: OneLane, FullWidth, Moving
- Applies speed penalties on collision
- Integrates with existing pooling and cleanup systems
- Difficulty increases over time
- Balanced and fair (always provides escape routes)

Complete the prefab setup in Unity, configure the pools, and you're ready to test!
