using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Represents a single collectible item (coin, power-up, etc.).
    /// Handles collection detection and visual animation.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        /// <summary>
        /// Type of collectible.
        /// </summary>
        public enum CollectibleType
        {
            Coin,              // Standard score collectible
            ScoreMultiplier,   // Temporary score multiplier boost
            SpeedBoost         // Temporary speed boost
        }

        [Header("Collectible Properties")]
        [Tooltip("Type of this collectible")]
        public CollectibleType collectibleType = CollectibleType.Coin;

        [Tooltip("Base score value when collected")]
        public int scoreValue = 10;

        [Header("Animation")]
        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 180f;

        [Tooltip("Enable bobbing animation")]
        public bool enableBobbing = false;

        [Tooltip("Bobbing height offset")]
        public float bobbingHeight = 0.2f;

        [Tooltip("Bobbing speed")]
        public float bobbingSpeed = 2f;

        [Header("Debug")]
        [Tooltip("Show debug logs for collection events")]
        public bool debugMode = false;

        private Vector3 initialPosition;
        private float bobbingTime = 0f;
        private bool isCollected = false;

        void OnEnable()
        {
            // Reset state when object is enabled from pool
            isCollected = false;
            initialPosition = transform.position;
            bobbingTime = 0f;

            // Ensure collider is trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        void Update()
        {
            // Rotate around Y axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Optional bobbing animation
            if (enableBobbing)
            {
                bobbingTime += Time.deltaTime * bobbingSpeed;
                float bobOffset = Mathf.Sin(bobbingTime) * bobbingHeight;
                transform.position = initialPosition + new Vector3(0, bobOffset, 0);
            }
        }

        /// <summary>
        /// Handles collection when player touches collectible.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            // Prevent double collection
            if (isCollected)
            {
                return;
            }

            // Check if player collected it
            if (other.CompareTag("Player"))
            {
                CollectItem(other.gameObject);
            }
        }

        /// <summary>
        /// Executes collection logic.
        /// </summary>
        private void CollectItem(GameObject player)
        {
            isCollected = true;

            if (debugMode)
            {
                Debug.Log($"Collectible: {gameObject.name} collected by {player.name}");
            }

            // Add score through ScoreManager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue);
            }
            else
            {
                Debug.LogWarning("Collectible: ScoreManager not found, score not added");
            }

            // Handle special collectible types
            switch (collectibleType)
            {
                case CollectibleType.ScoreMultiplier:
                    // TODO: Implement multiplier power-up
                    Debug.Log("ScoreMultiplier collected (not yet implemented)");
                    break;

                case CollectibleType.SpeedBoost:
                    // Apply speed boost to motorcycle
                    MotorcycleController motorcycle = player.GetComponent<MotorcycleController>();
                    if (motorcycle != null)
                    {
                        motorcycle.ApplyBoost(10f); // Boost by 10 units
                        if (debugMode)
                        {
                            Debug.Log("Speed boost applied!");
                        }
                    }
                    break;
            }

            // TODO: Play collection sound effect
            // TODO: Play collection particle effect

            // Return to pool
            ReturnToPool();
        }

        /// <summary>
        /// Returns this collectible to the object pool.
        /// </summary>
        private void ReturnToPool()
        {
            // Determine pool name based on type
            string poolName = GetPoolName();

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Return(poolName, gameObject);

                if (debugMode)
                {
                    Debug.Log($"Collectible: Returned {gameObject.name} to pool '{poolName}'");
                }
            }
            else
            {
                // Fallback: just deactivate if no pool manager
                gameObject.SetActive(false);
                Debug.LogWarning("Collectible: ObjectPoolManager not found, deactivating instead");
            }
        }

        /// <summary>
        /// Gets the pool name for this collectible type.
        /// </summary>
        private string GetPoolName()
        {
            // For now, all types use "Coin" pool
            // In future, could have separate pools per type
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                    return "Coin";
                case CollectibleType.ScoreMultiplier:
                    return "Coin"; // TODO: Create separate pool
                case CollectibleType.SpeedBoost:
                    return "Coin"; // TODO: Create separate pool
                default:
                    return "Coin";
            }
        }

        /// <summary>
        /// Visualizes collection radius in Scene view.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f); // Show collection radius
        }
    }
}
