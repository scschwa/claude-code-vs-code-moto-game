using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Third-person camera controller that follows the motorcycle.
    /// Provides smooth following, rotation, and look-ahead offset.
    /// Arcade-style camera with customizable positioning.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Transform to follow (motorcycle)")]
        public Transform target;

        [Header("Position")]
        [Tooltip("Distance behind the target")]
        public float followDistance = 10f;

        [Tooltip("Height above the target")]
        public float followHeight = 5f;

        [Tooltip("Look-ahead offset (how far ahead to focus)")]
        public float lookAheadDistance = 5f;

        [Header("Smoothing")]
        [Tooltip("Position follow smoothness (lower = smoother, higher = snappier)")]
        public float positionSmoothSpeed = 5f;

        [Tooltip("Rotation follow smoothness")]
        public float rotationSmoothSpeed = 5f;

        [Header("Dynamic Camera")]
        [Tooltip("Adjust FOV based on speed")]
        public bool dynamicFOV = true;

        [Tooltip("Base field of view")]
        public float baseFOV = 60f;

        [Tooltip("Max FOV when at max speed")]
        public float maxSpeedFOV = 75f;

        [Header("Shake")]
        [Tooltip("Enable camera shake at high speeds")]
        public bool enableShake = false; // Disabled due to accumulation issue

        [Tooltip("Max shake intensity")]
        public float shakeIntensity = 0.02f; // Reduced from 0.2 to prevent excessive shake

        private Camera cam;
        private Vector3 velocity = Vector3.zero;
        private MotorcycleController motorcycleController;

        void Start()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (target != null)
            {
                motorcycleController = target.GetComponent<MotorcycleController>();
            }

            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("CameraFollow: No target assigned!");
                return;
            }

            // Calculate desired position
            Vector3 desiredPosition = CalculateDesiredPosition();

            // Smoothly move to desired position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                1f / positionSmoothSpeed
            );

            // Calculate look-at point (ahead of target)
            Vector3 lookAtPoint = target.position + target.forward * lookAheadDistance;

            // Smoothly rotate to look at point
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );

            // Update dynamic FOV
            if (dynamicFOV && motorcycleController != null && cam != null)
            {
                float speedRatio = motorcycleController.NormalizedSpeed;
                float targetFOV = Mathf.Lerp(baseFOV, maxSpeedFOV, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 2f);
            }

            // Apply camera shake
            if (enableShake && motorcycleController != null)
            {
                ApplyCameraShake();
            }
        }

        /// <summary>
        /// Calculates the desired camera position based on target.
        /// </summary>
        private Vector3 CalculateDesiredPosition()
        {
            // Position behind and above the target
            Vector3 offset = -target.forward * followDistance + Vector3.up * followHeight;
            return target.position + offset;
        }

        /// <summary>
        /// Applies subtle camera shake at high speeds.
        /// </summary>
        private void ApplyCameraShake()
        {
            if (motorcycleController == null || cam == null)
                return;

            float speedRatio = motorcycleController.NormalizedSpeed;

            if (speedRatio > 0.8f)
            {
                float shake = (speedRatio - 0.8f) * 5f * shakeIntensity;
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-shake, shake),
                    Random.Range(-shake, shake),
                    0f
                );

                transform.position += shakeOffset;
            }
        }

        /// <summary>
        /// Sets the camera target.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            motorcycleController = target != null ? target.GetComponent<MotorcycleController>() : null;
        }

        /// <summary>
        /// Instantly snaps camera to target (useful for teleports/resets).
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
                return;

            transform.position = CalculateDesiredPosition();
            Vector3 lookAtPoint = target.position + target.forward * lookAheadDistance;
            transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);
            velocity = Vector3.zero;
        }

        /// <summary>
        /// Visualizes camera target in Scene view.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (target == null)
                return;

            // Draw desired position
            Vector3 desiredPos = CalculateDesiredPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(desiredPos, 0.5f);

            // Draw line to target
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);

            // Draw look-ahead point
            Vector3 lookAhead = target.position + target.forward * lookAheadDistance;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lookAhead, 0.3f);
            Gizmos.DrawLine(transform.position, lookAhead);
        }
    }
}
