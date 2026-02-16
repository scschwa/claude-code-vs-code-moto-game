using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Individual obstacle behavior and collision handling.
    /// Handles collision with player, applies speed penalty, and manages movement for Moving obstacles.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Configuration")]
        [Tooltip("Type of obstacle (set by spawner)")]
        public ObstacleSpawner.ObstacleType obstacleType = ObstacleSpawner.ObstacleType.OneLane;

        [Tooltip("Speed penalty factor (0.5 = reduce to 50% speed)")]
        [Range(0.1f, 1f)]
        public float speedPenaltyFactor = 0.5f;

        [Tooltip("Pool name for returning to pool")]
        public string poolName = "TrafficCone";

        [Header("Movement (for Moving obstacles)")]
        [Tooltip("Is this obstacle moving?")]
        public bool isMoving = false;

        [Tooltip("Movement speed (units per second)")]
        public float moveSpeed = 3f;

        [Tooltip("Movement direction (1 = right, -1 = left)")]
        public float moveDirection = 1f;

        [Tooltip("Distance to move before reversing direction")]
        public float moveDistance = 6f;

        [Header("Warning System")]
        [Tooltip("Distance at which to show warning indicator")]
        public float warningDistance = 30f;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect to play on collision (optional)")]
        public ParticleSystem collisionParticles;

        [Tooltip("Audio clip to play on collision (optional)")]
        public AudioClip collisionSound;

        [Header("Debug")]
        [Tooltip("Show debug logs for collision events")]
        public bool debugMode = false;

        private bool hasCollided = false;
        private Vector3 startPosition;
        private float distanceMoved = 0f;
        private Rigidbody rb;
        private Collider col;

        void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            // Configure rigidbody for proper physics
            if (rb != null)
            {
                rb.isKinematic = true; // Kinematic so it doesn't fall due to gravity
                rb.useGravity = false;
            }

            // Ensure collider is not a trigger (we want physical collision)
            if (col != null)
            {
                col.isTrigger = false;
            }
        }

        void OnEnable()
        {
            // Reset state when spawned from pool
            hasCollided = false;
            startPosition = transform.position;
            distanceMoved = 0f;

            // Set movement direction randomly if this is a Moving obstacle
            if (obstacleType == ObstacleSpawner.ObstacleType.Moving)
            {
                isMoving = true;
                moveDirection = (transform.position.x > 0) ? -1f : 1f; // Move towards center
            }
            else
            {
                isMoving = false;
            }
        }

        void Update()
        {
            // Handle movement for Moving obstacles
            if (isMoving && obstacleType == ObstacleSpawner.ObstacleType.Moving)
            {
                UpdateMovement();
            }

            // Check for nearby player to show warning (optional - can be implemented later)
            // CheckPlayerDistance();
        }

        /// <summary>
        /// Updates movement for Moving obstacles.
        /// </summary>
        private void UpdateMovement()
        {
            // Move horizontally
            float movement = moveSpeed * moveDirection * Time.deltaTime;
            transform.position += new Vector3(movement, 0f, 0f);
            distanceMoved += Mathf.Abs(movement);

            // Reverse direction if moved too far
            if (distanceMoved >= moveDistance)
            {
                moveDirection *= -1f;
                distanceMoved = 0f;

                if (debugMode)
                {
                    Debug.Log($"Obstacle {name}: Reversed direction");
                }
            }
        }

        /// <summary>
        /// Called when this obstacle collides with something.
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            // Ignore if already collided (prevents multiple penalty applications)
            if (hasCollided)
            {
                return;
            }

            // Check if collision is with player
            if (collision.gameObject.CompareTag("Player"))
            {
                if (debugMode)
                {
                    Debug.Log($"Obstacle {name} collided with player!");
                }

                HandlePlayerCollision(collision.gameObject);
            }
        }

        /// <summary>
        /// Handles collision with the player motorcycle.
        /// </summary>
        private void HandlePlayerCollision(GameObject player)
        {
            // Mark as collided
            hasCollided = true;

            // Get motorcycle controller
            MotorcycleController motorcycle = player.GetComponent<MotorcycleController>();
            if (motorcycle != null)
            {
                // Apply speed penalty
                motorcycle.ApplySpeedPenalty(speedPenaltyFactor);

                if (debugMode)
                {
                    Debug.Log($"Applied speed penalty: {speedPenaltyFactor:F2}x to {player.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Obstacle: Player object '{player.name}' missing MotorcycleController component!");
            }

            // Visual feedback
            PlayCollisionEffects();

            // Handle post-collision behavior based on obstacle type
            HandlePostCollisionBehavior();
        }

        /// <summary>
        /// Plays visual and audio feedback on collision.
        /// </summary>
        private void PlayCollisionEffects()
        {
            // Play particle effect if available
            if (collisionParticles != null)
            {
                collisionParticles.Play();
            }

            // Play sound effect if available
            if (collisionSound != null)
            {
                AudioSource.PlayClipAtPoint(collisionSound, transform.position);
            }
        }

        /// <summary>
        /// Handles behavior after collision based on obstacle type.
        /// </summary>
        private void HandlePostCollisionBehavior()
        {
            switch (obstacleType)
            {
                case ObstacleSpawner.ObstacleType.OneLane:
                    // Small obstacles are destroyed/returned to pool after collision
                    ReturnToPool(0.1f);
                    break;

                case ObstacleSpawner.ObstacleType.FullWidth:
                    // Large obstacles stay in place (act as permanent barriers)
                    // Do nothing - obstacle remains active
                    if (debugMode)
                    {
                        Debug.Log($"FullWidth obstacle {name} stays in place after collision");
                    }
                    break;

                case ObstacleSpawner.ObstacleType.Moving:
                    // Moving obstacles continue moving after collision
                    // Reset collision flag after short delay
                    Invoke("ResetCollision", 1f);
                    break;
            }
        }

        /// <summary>
        /// Returns obstacle to pool after a delay.
        /// </summary>
        private void ReturnToPool(float delay)
        {
            if (delay > 0f)
            {
                Invoke("ReturnToPoolImmediate", delay);
            }
            else
            {
                ReturnToPoolImmediate();
            }
        }

        /// <summary>
        /// Immediately returns obstacle to pool.
        /// </summary>
        private void ReturnToPoolImmediate()
        {
            if (ObjectPoolManager.Instance != null && !string.IsNullOrEmpty(poolName))
            {
                ObjectPoolManager.Instance.Return(poolName, gameObject);

                if (debugMode)
                {
                    Debug.Log($"Obstacle {name} returned to pool '{poolName}'");
                }
            }
            else
            {
                // Fallback: just deactivate
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Resets collision flag (for Moving obstacles).
        /// </summary>
        private void ResetCollision()
        {
            hasCollided = false;

            if (debugMode)
            {
                Debug.Log($"Obstacle {name}: Collision flag reset");
            }
        }

        /// <summary>
        /// Visualizes obstacle bounds in Scene view.
        /// </summary>
        void OnDrawGizmos()
        {
            // Draw obstacle bounds
            Gizmos.color = Color.red;
            if (col != null)
            {
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }

            // Draw movement range for Moving obstacles
            if (isMoving && obstacleType == ObstacleSpawner.ObstacleType.Moving)
            {
                Gizmos.color = Color.yellow;
                Vector3 leftEnd = startPosition + Vector3.left * moveDistance;
                Vector3 rightEnd = startPosition + Vector3.right * moveDistance;
                Gizmos.DrawLine(leftEnd, rightEnd);
            }
        }
    }
}
