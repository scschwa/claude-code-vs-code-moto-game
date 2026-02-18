# Terrain Enhancements - Quick Configuration Reference

## Default Values (Recommended Starting Point)

### TerrainGenerator > Feature Config

#### Ramps & Jumps
```
Enable Jumps:              true
Jump Intensity Threshold:  0.7
Ramp Height:              3.0
Ramp Length Fraction:     0.4
Ramp Profile:             EaseInOut(0,0,1,1)
```

#### Road Banking
```
Enable Banking:           true
Max Bank Angle:           15.0°
Banking Curve Threshold:  0.3
```

#### Visual Variety
```
Enable Color Variation:   true
Intensity Color Gradient:
  - 0.0 → Gray   (low intensity)
  - 0.5 → Blue   (medium)
  - 1.0 → Red    (high intensity)
```

### MotorcycleController > Jump Physics
```
Jump Boost Force:         10.0
Air Control Multiplier:   0.5
```

---

## Configuration Presets

### Preset 1: Subtle (Minimal Terrain Features)
**Use Case:** Slower songs, beginner difficulty

```
Jump Intensity Threshold:  0.85  (fewer jumps)
Ramp Height:              2.0   (lower jumps)
Jump Boost Force:         8.0   (less air time)
Max Bank Angle:           10.0° (subtle banking)
Banking Curve Threshold:  0.4   (banking only on sharp curves)
```

### Preset 2: Standard (Balanced)
**Use Case:** Most songs, normal difficulty

```
Jump Intensity Threshold:  0.7   (moderate jumps)
Ramp Height:              3.0   (standard height)
Jump Boost Force:         10.0  (good air time)
Max Bank Angle:           15.0° (noticeable banking)
Banking Curve Threshold:  0.3   (banking on most curves)
```

### Preset 3: Extreme (Maximum Features)
**Use Case:** Fast/intense songs, expert difficulty

```
Jump Intensity Threshold:  0.5   (frequent jumps)
Ramp Height:              5.0   (high jumps)
Jump Boost Force:         18.0  (long air time)
Max Bank Angle:           25.0° (dramatic banking)
Banking Curve Threshold:  0.2   (banking on all curves)
Air Control Multiplier:   0.3   (very limited air control)
```

### Preset 4: Rollercoaster (Maximum Drama)
**Use Case:** Showcasing features, cinematic mode

```
Jump Intensity Threshold:  0.4   (very frequent jumps)
Ramp Height:              6.0   (very high jumps)
Ramp Length Fraction:     0.6   (long ramps)
Jump Boost Force:         20.0  (extreme air time)
Max Bank Angle:           30.0° (extreme banking)
Banking Curve Threshold:  0.1   (banking everywhere)
```

---

## Parameter Relationships

### Jump Height = Ramp Height + Jump Boost Force
- **Ramp Height:** Physical terrain elevation
- **Jump Boost Force:** Additional upward impulse
- **Combined Effect:** Total jump height

**Examples:**
- Low: Ramp 2 + Boost 8 = ~3 units height
- Medium: Ramp 3 + Boost 10 = ~5 units height
- High: Ramp 5 + Boost 15 = ~8 units height

### Jump Frequency = Intensity Threshold
- **0.9:** Rare jumps (only on extreme peaks)
- **0.7:** Moderate jumps (on high intensity sections)
- **0.5:** Frequent jumps (on above-average intensity)
- **0.3:** Very frequent jumps (most segments)

### Banking Visibility = Max Angle × Curve Amount
- **10° × 0.5 curve:** Subtle (5° visible bank)
- **15° × 0.5 curve:** Noticeable (7.5° visible bank)
- **25° × 0.5 curve:** Dramatic (12.5° visible bank)

---

## Troubleshooting Quick Reference

| Issue | Likely Cause | Solution |
|-------|--------------|----------|
| No ramps appearing | Threshold too high | Lower to 0.5 or 0.6 |
| Too many ramps | Threshold too low | Raise to 0.8 or 0.9 |
| Jumps too low | Forces too weak | Increase ramp height + boost |
| Jumps too high | Forces too strong | Decrease ramp height + boost |
| Can't control in air | Multiplier too low | Increase from 0.5 to 0.7 |
| Too much air control | Multiplier too high | Decrease from 0.5 to 0.3 |
| Landing too harsh | Dampening too weak | Check OnLanding() is working |
| Banking not visible | Angle too low | Increase to 20-25° |
| Banking too extreme | Angle too high | Decrease to 10-12° |
| Wrong colors | Gradient misconfigured | Check gradient in Inspector |

---

## Color Gradient Presets

### Default (Gray → Blue → Red)
```
0.0 → RGB(0.5, 0.5, 0.5)  // Gray
0.5 → RGB(0.0, 0.0, 1.0)  // Blue
1.0 → RGB(1.0, 0.0, 0.0)  // Red
```

### Cool (Blue → Cyan → White)
```
0.0 → RGB(0.0, 0.2, 0.4)  // Dark Blue
0.5 → RGB(0.0, 0.8, 0.8)  // Cyan
1.0 → RGB(1.0, 1.0, 1.0)  // White
```

### Warm (Yellow → Orange → Red)
```
0.0 → RGB(0.8, 0.8, 0.2)  // Yellow
0.5 → RGB(1.0, 0.5, 0.0)  // Orange
1.0 → RGB(1.0, 0.0, 0.0)  // Red
```

### Neon (Purple → Cyan → Yellow)
```
0.0 → RGB(0.5, 0.0, 0.8)  // Purple
0.5 → RGB(0.0, 1.0, 1.0)  // Cyan
1.0 → RGB(1.0, 1.0, 0.0)  // Yellow
```

### Monochrome (Black → Gray → White)
```
0.0 → RGB(0.2, 0.2, 0.2)  // Dark Gray
0.5 → RGB(0.5, 0.5, 0.5)  // Gray
1.0 → RGB(0.9, 0.9, 0.9)  // Light Gray
```

---

## Ramp Profile Curve Presets

### Linear (Straight Ramp)
```
Key 0: Time 0.0, Value 0.0
Key 1: Time 1.0, Value 1.0
Tangents: Linear
```

### EaseInOut (Default - Smooth)
```
Key 0: Time 0.0, Value 0.0, Out Tangent: 0
Key 1: Time 1.0, Value 1.0, In Tangent: 0
```

### EaseIn (Slow Start, Fast End)
```
Key 0: Time 0.0, Value 0.0, Out Tangent: 0
Key 1: Time 1.0, Value 1.0, In Tangent: 2
```

### EaseOut (Fast Start, Slow End)
```
Key 0: Time 0.0, Value 0.0, Out Tangent: 2
Key 1: Time 1.0, Value 1.0, In Tangent: 0
```

### Curved (Ski Jump - Aggressive Launch)
```
Key 0: Time 0.0, Value 0.0
Key 1: Time 0.7, Value 0.3
Key 2: Time 1.0, Value 1.0
```

---

## Performance Optimization

### If Frame Rate Drops:
1. Disable Color Variation (saves material instances)
2. Reduce Ramp Length Fraction (0.4 → 0.3)
3. Increase Jump Intensity Threshold (fewer ramps)
4. Lower Mesh Subdivisions in RoadSegment

### If Memory Usage High:
1. Reduce Active Segment Count (10 → 8)
2. Disable Color Variation
3. Use simpler Ramp Profile curves

---

## Unity Inspector Quick Setup (5 Minutes)

1. **Select TerrainGenerator**
   - Scroll to "Enhanced Features"
   - Expand "Feature Config"
   - Copy values from "Standard" preset above

2. **Select Motorcycle**
   - Scroll to "Jump Physics"
   - Set Jump Boost Force: 10
   - Set Air Control Multiplier: 0.5

3. **Play Test**
   - Select a song with varying intensity
   - Watch for ramps on intense sections
   - Test jump feel and landing

4. **Adjust to Taste**
   - Too few jumps? Lower threshold
   - Too many jumps? Raise threshold
   - Jump too low? Increase forces
   - Jump too high? Decrease forces

---

## Advanced: Runtime Configuration

If you want to change values at runtime (e.g., difficulty settings):

```csharp
// Example: Make game harder at runtime
TerrainGenerator.Instance.featureConfig.jumpIntensityThreshold = 0.5f;
TerrainGenerator.Instance.featureConfig.rampHeight = 5f;

MotorcycleController moto = FindObjectOfType<MotorcycleController>();
moto.jumpBoostForce = 15f;
moto.airControlMultiplier = 0.3f;
```

---

## Testing Recommendations

### Test Sequence:
1. ✓ Load game with Standard preset
2. ✓ Play 30 seconds, verify ramps appear
3. ✓ Switch to Subtle preset
4. ✓ Play 30 seconds, verify fewer ramps
5. ✓ Switch to Extreme preset
6. ✓ Play 30 seconds, verify frequent ramps
7. ✓ Choose your preferred balance

### Metrics to Track:
- Ramps per minute (target: 3-5 for Standard)
- Average jump height (target: 3-5 units)
- Air time per jump (target: 1-2 seconds)
- Banking frequency (target: 50% of curves)
- Color variation visibility (target: obvious differences)

---

## Support

For detailed information, see:
- `TERRAIN_ENHANCEMENTS_GUIDE.md` - Full feature guide

For code reference, see:
- `Assets/Scripts/Terrain/TerrainFeatureConfig.cs`
- `Assets/Scripts/Terrain/RampFeature.cs`
