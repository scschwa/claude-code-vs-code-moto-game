using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DesertRider.MP3;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Generates infinite procedural road terrain that reacts to music analysis data.
    /// Manages road segments, applies Perlin noise, and modulates based on intensity curve.
    /// Uses deterministic seeding for same song = same level.
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        #region Singleton Pattern
        public static TerrainGenerator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        [Header("Road Configuration")]
        [Tooltip("Prefab for road segments (must have RoadSegment component)")]
        public GameObject roadSegmentPrefab;

        [Tooltip("Material for road segments")]
        public Material roadMaterial;

        [Tooltip("Length of each road segment")]
        public float segmentLength = 20f;

        [Tooltip("Width of the road")]
        public float roadWidth = 10f;

        [Tooltip("Number of segments to keep active at once")]
        public int activeSegmentCount = 10;

        [Header("Music Reactivity")]
        [Tooltip("Multiplier for intensity-based height variation")]
        public float intensityHeightMultiplier = 5f;

        [Tooltip("Multiplier for curve variation")]
        public float curveMultiplier = 0.5f;

        [Tooltip("How many intensity samples per segment")]
        public int intensitySamplesPerSegment = 10;

        [Header("Perlin Noise Settings")]
        [Tooltip("Enable Perlin noise for organic variation")]
        public bool usePerlinNoise = true;

        [Tooltip("Scale of Perlin noise (smaller = more variation)")]
        public float perlinScale = 0.1f;

        [Tooltip("Strength of Perlin noise height offset")]
        public float perlinStrength = 2f;

        [Header("Gameplay")]
        [Tooltip("Estimated player speed for time-to-distance conversion")]
        public float estimatedSpeed = 20f;

        [Header("Enhanced Features")]
        [Tooltip("Configuration for terrain features (ramps, banking, colors)")]
        public TerrainFeatureConfig featureConfig = new TerrainFeatureConfig();

        [Header("Debug")]
        [Tooltip("Show generation debug info in console")]
        public bool debugMode = false;

        // Private state
        private List<RoadSegment> activeSegments = new List<RoadSegment>();
        private AnalysisData analysisData;
        private System.Random seededRandom;
        private int currentSegmentIndex = 0;
        private int intensityCurveIndex = 0;
        private Vector3 nextSegmentPosition = Vector3.zero;
        private Quaternion nextSegmentRotation = Quaternion.identity;
        private float[] previousSegmentEndHeights = null; // Store last row of heights from previous segment

        /// <summary>
        /// Initializes terrain generation with music analysis data.
        /// </summary>
        /// <param name="data">Analysis data from PreAnalyzer.</param>
        public void Initialize(AnalysisData data)
        {
            if (data == null)
            {
                Debug.LogError("TerrainGenerator: Cannot initialize with null analysis data!");
                return;
            }

            analysisData = data;
            seededRandom = new System.Random(data.LevelSeed);

            Debug.Log($"TerrainGenerator: Initialized with seed {data.LevelSeed}, {data.IntensityCurve.Count} intensity samples");

            // Clear existing segments
            ClearAllSegments();

            // Reset generation state
            currentSegmentIndex = 0;
            intensityCurveIndex = 0;
            nextSegmentPosition = Vector3.zero;
            nextSegmentRotation = Quaternion.identity;
            previousSegmentEndHeights = null;

            // Generate initial segments
            for (int i = 0; i < activeSegmentCount; i++)
            {
                GenerateNextSegment();
            }
        }

        /// <summary>
        /// Generates the next road segment based on current music data.
        /// </summary>
        public void GenerateNextSegment()
        {
            if (analysisData == null)
            {
                Debug.LogWarning("TerrainGenerator: No analysis data loaded!");
                return;
            }

            // Create segment GameObject
            GameObject segmentObj;
            if (roadSegmentPrefab != null)
            {
                segmentObj = Instantiate(roadSegmentPrefab, nextSegmentPosition, nextSegmentRotation, transform);
            }
            else
            {
                segmentObj = new GameObject($"RoadSegment_{currentSegmentIndex}");
                segmentObj.transform.SetParent(transform);
                segmentObj.transform.position = nextSegmentPosition;
                segmentObj.transform.rotation = nextSegmentRotation;
                segmentObj.AddComponent<MeshFilter>();
                segmentObj.AddComponent<MeshRenderer>();
                segmentObj.AddComponent<RoadSegment>();
            }

            RoadSegment segment = segmentObj.GetComponent<RoadSegment>();
            if (segment == null)
            {
                Debug.LogError("TerrainGenerator: Road segment prefab missing RoadSegment component!");
                Destroy(segmentObj);
                return;
            }

            // Configure segment
            segment.segmentLength = segmentLength;
            segment.roadWidth = roadWidth;
            segment.heightIntensity = intensityHeightMultiplier;
            segment.curveIntensity = curveMultiplier;

            // Get intensity values for this segment
            float[] intensityValues = GetIntensityValuesForSegment();

            // Calculate curve amount based on music or Perlin noise
            float curveAmount = CalculateCurveAmount();

            // Determine if this segment should have special features
            bool shouldHaveJump = ShouldSpawnJump(intensityValues);
            bool shouldHaveBanking = Mathf.Abs(curveAmount) > featureConfig.bankingCurveThreshold;

            // Configure banking
            if (featureConfig.enableBanking && shouldHaveBanking)
            {
                segment.enableBanking = true;
                segment.bankingAngle = featureConfig.maxBankAngle;
            }

            // Add ramp feature if conditions met
            if (featureConfig.enableJumps && shouldHaveJump && currentSegmentIndex > 3) // Not on first 3 segments
            {
                RampFeature ramp = segmentObj.AddComponent<RampFeature>();
                ramp.rampStartZ = 0.2f;
                ramp.rampEndZ = 0.2f + featureConfig.rampLengthFraction;
                ramp.rampHeight = featureConfig.rampHeight;
                ramp.rampCurve = featureConfig.rampProfile;
            }

            // Apply Perlin noise to base height if enabled
            if (usePerlinNoise)
            {
                float perlinOffset = Mathf.PerlinNoise(
                    currentSegmentIndex * perlinScale,
                    seededRandom.Next(0, 1000) * perlinScale
                ) * 2f - 1f; // Remap to -1 to 1

                segment.baseHeight = perlinOffset * perlinStrength;
            }

            // Generate the mesh with seamless connection to previous segment
            float[] endHeights = segment.GenerateMesh(intensityValues, curveAmount, previousSegmentEndHeights);

            // Store end heights for next segment
            previousSegmentEndHeights = endHeights;

            // Set material
            if (roadMaterial != null)
            {
                segment.SetMaterial(roadMaterial);
            }
            else
            {
                Debug.LogWarning($"TerrainGenerator: No road material assigned! Segment {currentSegmentIndex} will appear white/pink.");
            }

            // Apply color variation based on intensity
            if (featureConfig.enableColorVariation && intensityValues != null && intensityValues.Length > 0)
            {
                float avgIntensity = intensityValues.Average();
                Color segmentColor = featureConfig.intensityColorGradient.Evaluate(avgIntensity);
                segment.SetMaterialColor(segmentColor);
            }

            // Ensure segment is on correct layer for collision detection (Layer 0 = Default)
            segmentObj.layer = 0;

            // Add SegmentObjectTracker component for managing spawned objects
            SegmentObjectTracker tracker = segmentObj.AddComponent<SegmentObjectTracker>();

            // Calculate segment start time for spawning (used by both collectibles and obstacles)
            float segmentStartTime = currentSegmentIndex * (segmentLength / estimatedSpeed);

            // Spawn collectibles if spawner is available
            DesertRider.Gameplay.CollectibleSpawner spawner = DesertRider.Gameplay.CollectibleSpawner.Instance;
            if (spawner != null && analysisData != null)
            {
                spawner.SpawnCollectiblesForSegment(segment, segmentStartTime);
            }

            // Spawn obstacles if spawner is available
            DesertRider.Gameplay.ObstacleSpawner obstacleSpawner = DesertRider.Gameplay.ObstacleSpawner.Instance;
            if (obstacleSpawner != null && analysisData != null)
            {
                obstacleSpawner.SpawnObstaclesForSegment(segment, segmentStartTime);
            }

            // Add to active segments
            activeSegments.Add(segment);

            // Update next segment position and rotation
            nextSegmentPosition = segment.EndPosition;

            // Slight rotation based on curve
            float rotationAngle = curveAmount * 5f; // Degrees
            nextSegmentRotation *= Quaternion.Euler(0, rotationAngle, 0);

            currentSegmentIndex++;

            if (debugMode)
            {
                Debug.Log($"Generated segment {currentSegmentIndex} at {nextSegmentPosition}, curve={curveAmount:F2}");
            }

            // Remove oldest segment if we have too many
            if (activeSegments.Count > activeSegmentCount)
            {
                RemoveOldestSegment();
            }
        }

        /// <summary>
        /// Gets intensity values for the current segment from the analysis data.
        /// </summary>
        private float[] GetIntensityValuesForSegment()
        {
            float[] values = new float[intensitySamplesPerSegment];

            if (analysisData.IntensityCurve == null || analysisData.IntensityCurve.Count == 0)
            {
                // Return default values if no intensity curve
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = 0.5f;
                }
                return values;
            }

            // Sample from intensity curve
            for (int i = 0; i < intensitySamplesPerSegment; i++)
            {
                if (intensityCurveIndex < analysisData.IntensityCurve.Count)
                {
                    values[i] = analysisData.IntensityCurve[intensityCurveIndex];
                    intensityCurveIndex++;
                }
                else
                {
                    // Loop back to start if we run out
                    intensityCurveIndex = 0;
                    values[i] = analysisData.IntensityCurve[intensityCurveIndex];
                }
            }

            return values;
        }

        /// <summary>
        /// Calculates curve amount for this segment based on music and noise.
        /// </summary>
        private float CalculateCurveAmount()
        {
            // Use seeded random for deterministic curves
            float randomCurve = (float)(seededRandom.NextDouble() * 2.0 - 1.0); // -1 to 1

            // Optionally modulate by current intensity
            float currentIntensity = 0.5f;
            if (analysisData.IntensityCurve != null && intensityCurveIndex < analysisData.IntensityCurve.Count)
            {
                currentIntensity = analysisData.IntensityCurve[intensityCurveIndex];
            }

            // More intensity = more extreme curves
            float curve = randomCurve * Mathf.Lerp(0.3f, 1.0f, currentIntensity);

            return curve;
        }

        /// <summary>
        /// Determines if segment should have a jump based on intensity.
        /// </summary>
        /// <param name="intensityValues">Array of intensity values for the segment</param>
        /// <returns>True if jump should be spawned</returns>
        private bool ShouldSpawnJump(float[] intensityValues)
        {
            if (intensityValues == null || intensityValues.Length == 0)
                return false;

            // Calculate average intensity
            float avgIntensity = 0f;
            foreach (float val in intensityValues)
                avgIntensity += val;
            avgIntensity /= intensityValues.Length;

            // Spawn jump if intensity exceeds threshold
            return avgIntensity > featureConfig.jumpIntensityThreshold;
        }

        /// <summary>
        /// Removes the oldest road segment.
        /// </summary>
        private void RemoveOldestSegment()
        {
            if (activeSegments.Count > 0)
            {
                RoadSegment oldSegment = activeSegments[0];
                activeSegments.RemoveAt(0);
                Destroy(oldSegment.gameObject);
            }
        }

        /// <summary>
        /// Clears all active road segments.
        /// </summary>
        public void ClearAllSegments()
        {
            foreach (var segment in activeSegments)
            {
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }
            activeSegments.Clear();
        }

        /// <summary>
        /// Advances terrain generation (call this to generate more road ahead).
        /// </summary>
        public void AdvanceGeneration()
        {
            GenerateNextSegment();
        }

        void OnDestroy()
        {
            ClearAllSegments();
        }
    }
}
