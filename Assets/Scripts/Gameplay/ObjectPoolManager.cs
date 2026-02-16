using System.Collections.Generic;
using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Manages object pools for all spawned gameplay objects (coins, obstacles, etc.).
    /// Implements efficient pooling to avoid constant instantiation and destruction.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static ObjectPoolManager Instance { get; private set; }

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
        /// Configuration for a single object pool.
        /// </summary>
        [System.Serializable]
        public class PoolConfig
        {
            [Tooltip("Unique name identifier for this pool")]
            public string poolName;

            [Tooltip("Prefab to pool")]
            public GameObject prefab;

            [Tooltip("Initial number of objects to create")]
            public int initialSize = 50;

            [Tooltip("Maximum pool size (prevents infinite growth)")]
            public int maxSize = 200;
        }

        [Header("Pool Configuration")]
        [Tooltip("List of all object pools to create")]
        public List<PoolConfig> poolConfigs = new List<PoolConfig>();

        [Header("Debug")]
        [Tooltip("Show debug logs for pool operations")]
        public bool debugMode = false;

        // Internal pool storage
        private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();

        /// <summary>
        /// Internal class representing a single object pool.
        /// </summary>
        private class Pool
        {
            public GameObject prefab;
            public int maxSize;
            public Queue<GameObject> availableObjects = new Queue<GameObject>();
            public HashSet<GameObject> activeObjects = new HashSet<GameObject>();

            public int TotalCount => availableObjects.Count + activeObjects.Count;
        }

        void Start()
        {
            InitializePools();
        }

        /// <summary>
        /// Creates all configured pools with initial objects.
        /// </summary>
        private void InitializePools()
        {
            foreach (var config in poolConfigs)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning($"ObjectPoolManager: Pool '{config.poolName}' has null prefab, skipping.");
                    continue;
                }

                Pool pool = new Pool
                {
                    prefab = config.prefab,
                    maxSize = config.maxSize
                };

                // Pre-instantiate initial objects
                for (int i = 0; i < config.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(config.prefab, config.poolName);
                    obj.SetActive(false);
                    pool.availableObjects.Enqueue(obj);
                }

                pools[config.poolName] = pool;

                if (debugMode)
                {
                    Debug.Log($"ObjectPoolManager: Initialized pool '{config.poolName}' with {config.initialSize} objects");
                }
            }
        }

        /// <summary>
        /// Gets an object from the specified pool.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <returns>GameObject from pool, or null if pool doesn't exist</returns>
        public GameObject Get(string poolName)
        {
            if (!pools.ContainsKey(poolName))
            {
                Debug.LogError($"ObjectPoolManager: Pool '{poolName}' does not exist!");
                return null;
            }

            Pool pool = pools[poolName];
            GameObject obj;

            // Try to get from available objects
            if (pool.availableObjects.Count > 0)
            {
                obj = pool.availableObjects.Dequeue();
            }
            else
            {
                // Pool is empty, check if we can expand
                if (pool.TotalCount >= pool.maxSize)
                {
                    Debug.LogWarning($"ObjectPoolManager: Pool '{poolName}' has reached max size ({pool.maxSize}), recycling oldest object");
                    // In a production system, you might recycle the oldest active object here
                    // For now, just return null to prevent overflow
                    return null;
                }

                // Create new object to expand pool
                obj = CreateNewObject(pool.prefab, poolName);

                if (debugMode)
                {
                    Debug.Log($"ObjectPoolManager: Expanding pool '{poolName}' (now {pool.TotalCount + 1} total)");
                }
            }

            // Activate and track
            obj.SetActive(true);
            pool.activeObjects.Add(obj);

            return obj;
        }

        /// <summary>
        /// Returns an object to its pool.
        /// </summary>
        /// <param name="poolName">Name of the pool to return to</param>
        /// <param name="obj">Object to return</param>
        public void Return(string poolName, GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("ObjectPoolManager: Tried to return null object");
                return;
            }

            if (!pools.ContainsKey(poolName))
            {
                Debug.LogError($"ObjectPoolManager: Pool '{poolName}' does not exist!");
                Destroy(obj);
                return;
            }

            Pool pool = pools[poolName];

            // Remove from active tracking
            if (pool.activeObjects.Contains(obj))
            {
                pool.activeObjects.Remove(obj);
            }

            // Deactivate and return to pool
            obj.SetActive(false);
            pool.availableObjects.Enqueue(obj);

            if (debugMode)
            {
                Debug.Log($"ObjectPoolManager: Returned object to pool '{poolName}' (available: {pool.availableObjects.Count})");
            }
        }

        /// <summary>
        /// Creates a new pooled object instance.
        /// </summary>
        private GameObject CreateNewObject(GameObject prefab, string poolName)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.name = $"{poolName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            return obj;
        }

        /// <summary>
        /// Gets statistics about a pool.
        /// </summary>
        public void GetPoolStats(string poolName, out int available, out int active, out int total)
        {
            available = 0;
            active = 0;
            total = 0;

            if (pools.ContainsKey(poolName))
            {
                Pool pool = pools[poolName];
                available = pool.availableObjects.Count;
                active = pool.activeObjects.Count;
                total = pool.TotalCount;
            }
        }

        /// <summary>
        /// Clears all pools and destroys all objects.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var kvp in pools)
            {
                Pool pool = kvp.Value;

                // Destroy all available objects
                while (pool.availableObjects.Count > 0)
                {
                    GameObject obj = pool.availableObjects.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }

                // Destroy all active objects
                foreach (GameObject obj in pool.activeObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
                pool.activeObjects.Clear();
            }

            pools.Clear();
            Debug.Log("ObjectPoolManager: All pools cleared");
        }

        void OnDestroy()
        {
            ClearAllPools();
        }

        /// <summary>
        /// Debug method to display pool statistics.
        /// </summary>
        [ContextMenu("Show Pool Statistics")]
        public void ShowPoolStatistics()
        {
            Debug.Log("=== Object Pool Statistics ===");
            foreach (var kvp in pools)
            {
                Pool pool = kvp.Value;
                Debug.Log($"Pool '{kvp.Key}': Available={pool.availableObjects.Count}, Active={pool.activeObjects.Count}, Total={pool.TotalCount}, Max={pool.maxSize}");
            }
        }
    }
}
