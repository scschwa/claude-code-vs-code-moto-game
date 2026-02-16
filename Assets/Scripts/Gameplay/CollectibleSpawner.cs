using UnityEngine;
using DesertRider.MP3;
using DesertRider.Terrain;
using System.Collections.Generic;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Spawns collectibles (coins) on road segments based on music beat data.
    /// Uses beat strength to determine spawn patterns and positions.
    /// </summary>
    public class CollectibleSpawner : MonoBehaviour
    {
        #region Singleton Pattern
        public static CollectibleSpawner Instance { get; private set; }

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
        /// Spawn pattern types for collectibles.
        /// </summary>
        public enum SpawnPattern
        {
            Line,      // Straight line of coins
            Arc,       // Arc/curve pattern
            Zigzag     // Zigzag pattern
        }

        [Header("Music Reactivity")]
        [Tooltip("Reference to analysis data (usually set by TerrainGenerator)")]
        public AnalysisData analysisData;

        [Tooltip("Minimum beat strength to spawn collectibles (0-1)")]
        [Range(0f, 1f)]
        public float beatStrengthThreshold = 0.5f;

        [Header("Spawn Configuration")]
        [Tooltip("Height above road surface to spawn coins")]
        public float spawnHeight = 1.5f;

        [Tooltip("Spacing between lanes (road width / 3 typically)")]
        public float laneSpacing = 2f;

        [Tooltip("Number of coins to spawn per beat")]
        [Range(3, 7)]
        public int coinsPerBeat = 5;

        [Tooltip("Spacing between coins in a pattern (along Z axis)")]
        public float coinSpacingZ = 2f;

        [Header("Pattern Weights")]
        [Tooltip("Probability of Line pattern")]
        [Range(0f, 1f)]
        public float linePatternWeight = 0.4f;

        [Tooltip("Probability of Arc pattern")]
        [Range(0f, 1f)]
        public float arcPatternWeight = 0.3f;

        [Tooltip("Probability of Zigzag pattern")]
        [Range(0f, 1f)]
        public float zigzagPatternWeight = 0.3f;

        [Header("Debug")]
        [Tooltip("Show debug logs for spawn operations")]
        public bool debugMode = false;

        private System.Random seededRandom;
        private bool hasWarnedObjectPoolMissing = false;

        /// <summary>
        /// Initializes spawner with analysis data.
        /// </summary>
        public void Initialize(AnalysisData data)
        {
            analysisData = data;
            if (data != null)
            {
                seededRandom = new System.Random(data.LevelSeed);
                if (debugMode)
                {
                    Debug.Log($"CollectibleSpawner: Initialized with seed {data.LevelSeed}");
                }
            }
        }

        /// <summary>
        /// Spawns collectibles for a specific road segment based on beat data.
        /// </summary>
        /// <param name="segment">Road segment to spawn on</param>
        /// <param name="segmentStartTime">Time in song when this segment starts (seconds)</param>
        public void SpawnCollectiblesForSegment(RoadSegment segment, float segmentStartTime)
        {
            if (segment == null)
            {
                Debug.LogWarning("CollectibleSpawner: Cannot spawn on null segment");
                return;
            }

            if (analysisData == null || analysisData.Beats == null || analysisData.Beats.Count == 0)
            {
                if (debugMode)
                {
                    Debug.LogWarning("CollectibleSpawner: No analysis data or beats available");
                }
                return;
            }

            // ObjectPoolManager is optional - we can spawn without it
            if (ObjectPoolManager.Instance == null && !hasWarnedObjectPoolMissing)
            {
                Debug.LogWarning("CollectibleSpawner: ObjectPoolManager not found, using direct instantiation (less efficient)");
                hasWarnedObjectPoolMissing = true;
            }

            // Initialize random if needed
            if (seededRandom == null)
            {
                seededRandom = new System.Random(analysisData.LevelSeed);
            }

            // Get segment tracker for cleanup
            SegmentObjectTracker tracker = segment.GetComponent<SegmentObjectTracker>();
            if (tracker == null)
            {
                Debug.LogWarning($"CollectibleSpawner: Segment {segment.name} missing SegmentObjectTracker");
                return;
            }

            // Calculate time range for this segment
            float segmentEndTime = segmentStartTime + (segment.segmentLength / 20f); // Assume 20 units/sec speed

            // Find beats that fall within this segment
            List<BeatEvent> relevantBeats = new List<BeatEvent>();
            foreach (var beat in analysisData.Beats)
            {
                if (beat.Time >= segmentStartTime && beat.Time < segmentEndTime && beat.Strength >= beatStrengthThreshold)
                {
                    relevantBeats.Add(beat);
                }
            }

            if (debugMode)
            {
                Debug.Log($"CollectibleSpawner: Found {relevantBeats.Count} beats for segment (time {segmentStartTime:F2}-{segmentEndTime:F2})");
            }

            // Spawn coins for each beat
            int totalCoinsSpawned = 0;
            foreach (var beat in relevantBeats)
            {
                // Calculate Z position within segment (0 to segmentLength)
                float normalizedTime = (beat.Time - segmentStartTime) / (segmentEndTime - segmentStartTime);
                normalizedTime = Mathf.Clamp01(normalizedTime);

                // Choose spawn pattern based on beat strength and random weights
                SpawnPattern pattern = ChooseSpawnPattern(beat.Strength);

                // Spawn coins in pattern
                int spawned = SpawnPattern_Execute(segment, tracker, normalizedTime, pattern, beat.Strength);
                totalCoinsSpawned += spawned;
            }

            if (debugMode && totalCoinsSpawned > 0)
            {
                Debug.Log($"CollectibleSpawner: Spawned {totalCoinsSpawned} coins on segment {segment.name}");
            }
        }

        /// <summary>
        /// Chooses a spawn pattern based on beat strength and random weights.
        /// </summary>
        private SpawnPattern ChooseSpawnPattern(float beatStrength)
        {
            // Normalize weights
            float totalWeight = linePatternWeight + arcPatternWeight + zigzagPatternWeight;
            if (totalWeight == 0f)
            {
                totalWeight = 1f;
            }

            float normalizedLine = linePatternWeight / totalWeight;
            float normalizedArc = arcPatternWeight / totalWeight;
            float normalizedZigzag = zigzagPatternWeight / totalWeight;

            // Random selection based on weights
            float random = (float)seededRandom.NextDouble();

            if (random < normalizedLine)
            {
                return SpawnPattern.Line;
            }
            else if (random < normalizedLine + normalizedArc)
            {
                return SpawnPattern.Arc;
            }
            else
            {
                return SpawnPattern.Zigzag;
            }
        }

        /// <summary>
        /// Executes a spawn pattern and returns number of coins spawned.
        /// </summary>
        private int SpawnPattern_Execute(RoadSegment segment, SegmentObjectTracker tracker, float zNormalized, SpawnPattern pattern, float beatStrength)
        {
            int coinsSpawned = 0;

            switch (pattern)
            {
                case SpawnPattern.Line:
                    coinsSpawned = SpawnLine(segment, tracker, zNormalized);
                    break;

                case SpawnPattern.Arc:
                    coinsSpawned = SpawnArc(segment, tracker, zNormalized);
                    break;

                case SpawnPattern.Zigzag:
                    coinsSpawned = SpawnZigzag(segment, tracker, zNormalized);
                    break;
            }

            return coinsSpawned;
        }

        /// <summary>
        /// Spawns coins in a straight line pattern.
        /// </summary>
        private int SpawnLine(RoadSegment segment, SegmentObjectTracker tracker, float zNormalized)
        {
            // Random lane position (-laneSpacing, 0, or +laneSpacing)
            int laneChoice = seededRandom.Next(-1, 2); // -1, 0, or 1
            float xOffset = laneChoice * laneSpacing;

            int spawned = 0;
            for (int i = 0; i < coinsPerBeat; i++)
            {
                float zOffset = i * coinSpacingZ / segment.segmentLength;
                float finalZ = Mathf.Clamp01(zNormalized + zOffset);

                Vector3 spawnPos = segment.GetPositionOnSegment(finalZ, xOffset);
                GameObject coin = SpawnCoin(spawnPos, tracker);
                if (coin != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        /// <summary>
        /// Spawns coins in an arc pattern.
        /// </summary>
        private int SpawnArc(RoadSegment segment, SegmentObjectTracker tracker, float zNormalized)
        {
            int spawned = 0;

            // Arc from left to right or right to left
            float startX = (seededRandom.NextDouble() < 0.5f ? -1f : 1f) * laneSpacing;
            float endX = -startX;

            for (int i = 0; i < coinsPerBeat; i++)
            {
                float t = i / (float)(coinsPerBeat - 1);
                float xOffset = Mathf.Lerp(startX, endX, t);
                float zOffset = i * coinSpacingZ / segment.segmentLength;
                float finalZ = Mathf.Clamp01(zNormalized + zOffset);

                Vector3 spawnPos = segment.GetPositionOnSegment(finalZ, xOffset);
                GameObject coin = SpawnCoin(spawnPos, tracker);
                if (coin != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        /// <summary>
        /// Spawns coins in a zigzag pattern.
        /// </summary>
        private int SpawnZigzag(RoadSegment segment, SegmentObjectTracker tracker, float zNormalized)
        {
            int spawned = 0;

            for (int i = 0; i < coinsPerBeat; i++)
            {
                // Alternate between left and right
                float xOffset = ((i % 2 == 0) ? -1f : 1f) * laneSpacing;
                float zOffset = i * coinSpacingZ / segment.segmentLength;
                float finalZ = Mathf.Clamp01(zNormalized + zOffset);

                Vector3 spawnPos = segment.GetPositionOnSegment(finalZ, xOffset);
                GameObject coin = SpawnCoin(spawnPos, tracker);
                if (coin != null)
                {
                    spawned++;
                }
            }

            return spawned;
        }

        /// <summary>
        /// Spawns a single coin at the specified position.
        /// </summary>
        private GameObject SpawnCoin(Vector3 position, SegmentObjectTracker tracker)
        {
            GameObject coin;

            // Try to use ObjectPoolManager if available
            if (ObjectPoolManager.Instance != null)
            {
                coin = ObjectPoolManager.Instance.Get("Coin");
                if (coin == null)
                {
                    Debug.LogWarning("CollectibleSpawner: Failed to get coin from pool");
                    return null;
                }
            }
            else
            {
                // Fallback: Create a simple sphere as a coin
                coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coin.name = "Coin";
                coin.transform.localScale = Vector3.one * 0.5f;
                // Skip tag - "Collectible" tag doesn't exist in project
                coin.layer = 0; // Default layer

                // Add gold emissive material with HDR emission for intense bloom glow
                Renderer renderer = coin.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material goldMaterial = new Material(Shader.Find("Standard"));

                    // Gold color
                    Color goldColor = new Color(1f, 0.84f, 0f); // Rich gold

                    // Dark base color (emission will provide the glow)
                    goldMaterial.SetColor("_Color", goldColor * 0.3f);

                    // HDR Emission for intense bloom (values > 1.0 bloom with post-processing)
                    goldMaterial.EnableKeyword("_EMISSION");
                    Color hdrEmission = goldColor * 4f; // HDR intensity for strong bloom
                    goldMaterial.SetColor("_EmissionColor", hdrEmission);
                    goldMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                    // Metallic properties for gold appearance
                    goldMaterial.SetFloat("_Metallic", 0.9f); // Very metallic
                    goldMaterial.SetFloat("_Glossiness", 1f); // Maximum glossiness

                    renderer.material = goldMaterial;
                }

                // Add trigger collider
                SphereCollider collider = coin.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }

                // Add Collectible component if it exists
                var collectibleType = System.Type.GetType("DesertRider.Gameplay.Collectible");
                if (collectibleType != null)
                {
                    coin.AddComponent(collectibleType);
                }
            }

            coin.transform.position = position;
            coin.transform.rotation = Quaternion.identity;

            // Register with tracker for cleanup
            tracker.RegisterObject(coin);

            return coin;
        }
    }
}
