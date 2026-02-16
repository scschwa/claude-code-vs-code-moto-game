using UnityEngine;
using DesertRider.MP3;
using DesertRider.Terrain;
using System.Collections.Generic;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Spawns obstacles on road segments based on music intensity data.
    /// Uses intensity curve to determine spawn rates and obstacle types.
    /// Higher intensity = more and harder obstacles.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        #region Singleton Pattern
        public static ObstacleSpawner Instance { get; private set; }

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

        /// <summary>
        /// Types of obstacles that can be spawned.
        /// </summary>
        public enum ObstacleType
        {
            OneLane,    // Blocks one lane (cone, barrier)
            FullWidth,  // Blocks all lanes (large barrier)
            Moving      // Vehicle moving across
        }

        /// <summary>
        /// Configuration for each obstacle type.
        /// </summary>
        [System.Serializable]
        public class ObstacleConfig
        {
            [Tooltip("Pool name for this obstacle type")]
            public string poolName;

            [Tooltip("Weight for random selection (higher = more likely)")]
            public float spawnWeight = 1f;

            [Tooltip("Obstacle type")]
            public ObstacleType type;

            [Tooltip("Minimum intensity required to spawn this type (0-1)")]
            [Range(0f, 1f)]
            public float minIntensityRequired = 0f;
        }

        [Header("Music Sync")]
        [Tooltip("Base spawn chance at low intensity (0-1)")]
        [Range(0f, 1f)]
        public float baseSpawnChance = 0.1f;

        [Tooltip("High spawn chance at high intensity (0-1)")]
        [Range(0f, 1f)]
        public float highIntensitySpawnChance = 0.6f;

        [Tooltip("Intensity threshold for 'high intensity' (0-1)")]
        [Range(0f, 1f)]
        public float intensityThreshold = 0.6f;

        [Header("Difficulty")]
        [Tooltip("Difficulty curve over time (x=time in seconds, y=difficulty multiplier)")]
        public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 0.5f, 100, 2.0f);

        [Tooltip("Number of initial segments to skip (give player time to start)")]
        public int safeStartSegments = 3;

        [Header("Obstacle Types")]
        [Tooltip("List of obstacle types that can be spawned")]
        public List<ObstacleConfig> obstacleTypes = new List<ObstacleConfig>();

        [Header("Spacing")]
        [Tooltip("Distance between lanes")]
        public float laneSpacing = 2f;

        [Header("Debug")]
        [Tooltip("Show debug logs for spawn operations")]
        public bool debugMode = false;

        private AnalysisData analysisData;
        private System.Random seededRandom;
        private int segmentsGenerated = 0;

        /// <summary>
        /// Initializes spawner with analysis data.
        /// </summary>
        public void Initialize(AnalysisData data)
        {
            analysisData = data;
            segmentsGenerated = 0;

            if (data != null)
            {
                seededRandom = new System.Random(data.LevelSeed + 1000); // Different seed than collectibles
                if (debugMode)
                {
                    Debug.Log($"ObstacleSpawner: Initialized with seed {data.LevelSeed + 1000}");
                }
            }

            // Setup default obstacle configurations if empty
            if (obstacleTypes.Count == 0)
            {
                Debug.LogWarning("ObstacleSpawner: No obstacle types configured! Add them in the Inspector.");
            }
        }

        /// <summary>
        /// Spawns obstacles for a specific road segment based on intensity data.
        /// </summary>
        /// <param name="segment">Road segment to spawn on</param>
        /// <param name="segmentStartTime">Time in song when this segment starts (seconds)</param>
        public void SpawnObstaclesForSegment(RoadSegment segment, float segmentStartTime)
        {
            if (segment == null)
            {
                Debug.LogWarning("ObstacleSpawner: Cannot spawn on null segment");
                return;
            }

            // Skip initial segments for safety
            if (segmentsGenerated < safeStartSegments)
            {
                segmentsGenerated++;
                if (debugMode)
                {
                    Debug.Log($"ObstacleSpawner: Skipping segment {segmentsGenerated} (safe start period)");
                }
                return;
            }

            if (analysisData == null || analysisData.IntensityCurve == null || analysisData.IntensityCurve.Count == 0)
            {
                if (debugMode)
                {
                    Debug.LogWarning("ObstacleSpawner: No analysis data or intensity curve available");
                }
                return;
            }

            if (ObjectPoolManager.Instance == null)
            {
                Debug.LogError("ObstacleSpawner: ObjectPoolManager not found!");
                return;
            }

            if (obstacleTypes.Count == 0)
            {
                if (debugMode)
                {
                    Debug.LogWarning("ObstacleSpawner: No obstacle types configured");
                }
                return;
            }

            // Initialize random if needed
            if (seededRandom == null)
            {
                seededRandom = new System.Random(analysisData.LevelSeed + 1000);
            }

            // Get segment tracker for cleanup
            SegmentObjectTracker tracker = segment.GetComponent<SegmentObjectTracker>();
            if (tracker == null)
            {
                Debug.LogWarning($"ObstacleSpawner: Segment {segment.name} missing SegmentObjectTracker");
                return;
            }

            // Get average intensity for this segment
            float avgIntensity = GetAverageIntensityForSegment(segmentStartTime, segment.segmentLength);

            // Calculate spawn chance based on intensity
            float spawnChance = Mathf.Lerp(baseSpawnChance, highIntensitySpawnChance, avgIntensity);

            // Apply difficulty curve based on time
            float difficultyMultiplier = difficultyCurve.Evaluate(segmentStartTime);
            spawnChance *= difficultyMultiplier;

            // Clamp to reasonable values
            spawnChance = Mathf.Clamp01(spawnChance);

            if (debugMode)
            {
                Debug.Log($"ObstacleSpawner: Segment at time {segmentStartTime:F2}s, Intensity={avgIntensity:F2}, SpawnChance={spawnChance:F2}, Difficulty={difficultyMultiplier:F2}");
            }

            // Roll for spawn
            float roll = (float)seededRandom.NextDouble();
            if (roll > spawnChance)
            {
                // No obstacle this segment
                segmentsGenerated++;
                return;
            }

            // Choose obstacle type based on intensity
            ObstacleConfig chosenConfig = ChooseObstacleType(avgIntensity);
            if (chosenConfig == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning("ObstacleSpawner: No valid obstacle type for current intensity");
                }
                segmentsGenerated++;
                return;
            }

            // Calculate spawn position
            Vector3 spawnPosition = CalculateObstaclePosition(segment, chosenConfig.type);

            // Spawn the obstacle
            GameObject obstacle = ObjectPoolManager.Instance.Get(chosenConfig.poolName);
            if (obstacle == null)
            {
                Debug.LogWarning($"ObstacleSpawner: Failed to get obstacle from pool '{chosenConfig.poolName}'");
                segmentsGenerated++;
                return;
            }

            // Position and configure obstacle
            obstacle.transform.position = spawnPosition;
            obstacle.transform.rotation = Quaternion.identity;

            // Set obstacle type
            Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
            if (obstacleComponent != null)
            {
                obstacleComponent.obstacleType = chosenConfig.type;
                obstacleComponent.poolName = chosenConfig.poolName;
            }

            // Register with tracker for cleanup
            tracker.RegisterObject(obstacle);

            if (debugMode)
            {
                Debug.Log($"ObstacleSpawner: Spawned {chosenConfig.poolName} at {spawnPosition}");
            }

            segmentsGenerated++;
        }

        /// <summary>
        /// Gets average intensity for a segment's time range.
        /// </summary>
        private float GetAverageIntensityForSegment(float startTime, float segmentLength)
        {
            if (analysisData == null || analysisData.IntensityCurve == null || analysisData.IntensityCurve.Count == 0)
            {
                return 0.5f; // Default medium intensity
            }

            // Calculate time range
            float endTime = startTime + (segmentLength / 20f); // Assume 20 units/sec speed

            // Sample intensity curve
            float totalIntensity = 0f;
            int sampleCount = 0;

            // Sample rate: one sample per 0.1 seconds
            float sampleInterval = 0.1f;
            for (float t = startTime; t < endTime; t += sampleInterval)
            {
                int index = Mathf.RoundToInt(t * 10f); // IntensityCurve is sampled at 10 Hz
                if (index >= 0 && index < analysisData.IntensityCurve.Count)
                {
                    totalIntensity += analysisData.IntensityCurve[index];
                    sampleCount++;
                }
            }

            if (sampleCount > 0)
            {
                return totalIntensity / sampleCount;
            }

            return 0.5f; // Fallback
        }

        /// <summary>
        /// Chooses an obstacle type based on intensity and weights.
        /// </summary>
        private ObstacleConfig ChooseObstacleType(float intensity)
        {
            // Filter by minimum intensity
            List<ObstacleConfig> validConfigs = new List<ObstacleConfig>();
            foreach (var config in obstacleTypes)
            {
                if (intensity >= config.minIntensityRequired)
                {
                    validConfigs.Add(config);
                }
            }

            if (validConfigs.Count == 0)
            {
                return null;
            }

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var config in validConfigs)
            {
                totalWeight += config.spawnWeight;
            }

            if (totalWeight <= 0f)
            {
                totalWeight = 1f;
            }

            // Random selection based on weights
            float roll = (float)seededRandom.NextDouble() * totalWeight;
            float currentWeight = 0f;

            foreach (var config in validConfigs)
            {
                currentWeight += config.spawnWeight;
                if (roll <= currentWeight)
                {
                    return config;
                }
            }

            // Fallback to first valid config
            return validConfigs[0];
        }

        /// <summary>
        /// Calculates spawn position for an obstacle on the segment.
        /// </summary>
        private Vector3 CalculateObstaclePosition(RoadSegment segment, ObstacleType type)
        {
            // Random position along segment (normalized 0-1)
            float zNormalized = 0.3f + (float)seededRandom.NextDouble() * 0.4f; // Middle 40% of segment

            float xOffset = 0f;

            switch (type)
            {
                case ObstacleType.OneLane:
                    // Random lane (-1, 0, or 1)
                    int lane = seededRandom.Next(-1, 2);
                    xOffset = lane * laneSpacing;
                    break;

                case ObstacleType.FullWidth:
                    // Center of road
                    xOffset = 0f;
                    break;

                case ObstacleType.Moving:
                    // Start at road edge
                    xOffset = (seededRandom.NextDouble() < 0.5 ? -1f : 1f) * laneSpacing * 1.5f;
                    break;
            }

            // Get position on segment
            Vector3 position = segment.GetPositionOnSegment(zNormalized, xOffset);

            return position;
        }

        /// <summary>
        /// Resets the spawner state (call when restarting game).
        /// </summary>
        public void Reset()
        {
            segmentsGenerated = 0;
            if (analysisData != null)
            {
                seededRandom = new System.Random(analysisData.LevelSeed + 1000);
            }
        }
    }
}
