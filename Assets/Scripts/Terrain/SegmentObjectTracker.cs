using System.Collections.Generic;
using UnityEngine;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Tracks all spawned objects (collectibles, obstacles) associated with a specific road segment.
    /// Handles cleanup when the segment is destroyed.
    /// </summary>
    public class SegmentObjectTracker : MonoBehaviour
    {
        [Header("Tracked Objects")]
        [Tooltip("List of all objects spawned on this segment")]
        public List<GameObject> trackedObjects = new List<GameObject>();

        [Header("Debug")]
        [Tooltip("Show debug logs for tracking operations")]
        public bool debugMode = false;

        /// <summary>
        /// Registers an object as belonging to this segment.
        /// </summary>
        /// <param name="obj">GameObject to track</param>
        public void RegisterObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("SegmentObjectTracker: Attempted to register null object");
                return;
            }

            if (!trackedObjects.Contains(obj))
            {
                trackedObjects.Add(obj);

                if (debugMode)
                {
                    Debug.Log($"SegmentObjectTracker [{gameObject.name}]: Registered {obj.name} (total: {trackedObjects.Count})");
                }
            }
        }

        /// <summary>
        /// Unregisters an object from this segment (e.g., when collected).
        /// </summary>
        /// <param name="obj">GameObject to untrack</param>
        public void UnregisterObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (trackedObjects.Contains(obj))
            {
                trackedObjects.Remove(obj);

                if (debugMode)
                {
                    Debug.Log($"SegmentObjectTracker [{gameObject.name}]: Unregistered {obj.name} (remaining: {trackedObjects.Count})");
                }
            }
        }

        /// <summary>
        /// Cleans up all tracked objects by returning them to the pool.
        /// Called when the segment is destroyed.
        /// </summary>
        public void CleanupAllObjects()
        {
            if (trackedObjects.Count == 0)
            {
                return;
            }

            if (debugMode)
            {
                Debug.Log($"SegmentObjectTracker [{gameObject.name}]: Cleaning up {trackedObjects.Count} objects");
            }

            // Return all objects to their pools
            foreach (GameObject obj in trackedObjects)
            {
                if (obj != null && obj.activeSelf)
                {
                    // Try to get the collectible component to determine pool name
                    var collectible = obj.GetComponent<DesertRider.Gameplay.Collectible>();
                    if (collectible != null)
                    {
                        // Return to appropriate pool based on type
                        string poolName = GetPoolNameForCollectible(collectible);
                        if (DesertRider.Gameplay.ObjectPoolManager.Instance != null)
                        {
                            DesertRider.Gameplay.ObjectPoolManager.Instance.Return(poolName, obj);
                        }
                        else
                        {
                            // Fallback: just deactivate if pool manager doesn't exist
                            obj.SetActive(false);
                        }
                    }
                    else
                    {
                        // Try to get the obstacle component
                        var obstacle = obj.GetComponent<DesertRider.Gameplay.Obstacle>();
                        if (obstacle != null)
                        {
                            // Return to appropriate pool based on obstacle's pool name
                            string poolName = obstacle.poolName;
                            if (DesertRider.Gameplay.ObjectPoolManager.Instance != null && !string.IsNullOrEmpty(poolName))
                            {
                                DesertRider.Gameplay.ObjectPoolManager.Instance.Return(poolName, obj);
                            }
                            else
                            {
                                // Fallback: just deactivate if pool manager doesn't exist
                                obj.SetActive(false);
                            }
                        }
                        else
                        {
                            // Unknown object type, just deactivate
                            obj.SetActive(false);
                        }
                    }
                }
            }

            trackedObjects.Clear();
        }

        /// <summary>
        /// Determines the pool name for a collectible based on its type.
        /// </summary>
        private string GetPoolNameForCollectible(DesertRider.Gameplay.Collectible collectible)
        {
            // For now, all collectibles use "Coin" pool
            // In future, this could check collectible.collectibleType and return different pool names
            return "Coin";
        }

        /// <summary>
        /// Cleanup when segment is destroyed.
        /// </summary>
        void OnDestroy()
        {
            Debug.LogError($"[SegmentObjectTracker] OnDestroy() CALLED on {gameObject.name}! StackTrace: {UnityEngine.StackTraceUtility.ExtractStackTrace()}");
            CleanupAllObjects();
        }
    }
}
