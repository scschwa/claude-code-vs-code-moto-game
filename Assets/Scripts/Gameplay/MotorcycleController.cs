using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Arcade-style motorcycle controller for Desert Rider.
    /// Non-realistic physics optimized for fun, responsive gameplay.
    /// Inspired by racing games like Sayonara Wild Hearts.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MotorcycleController : MonoBehaviour
    {
        #region Configuration

        [Header("Movement")]
        [Tooltip("Maximum forward speed")]
        public float maxSpeed = 30f;

        [Tooltip("Acceleration rate")]
        public float acceleration = 10f;

        [Tooltip("Deceleration rate when not accelerating")]
        public float deceleration = 5f;

        [Tooltip("Brake strength")]
        public float brakeStrength = 15f;

        [Header("Steering")]
        [Tooltip("How fast the motorcycle turns (degrees per second)")]
        public float turnSpeed = 120f;

        [Tooltip("Maximum lean angle when turning (degrees)")]
        public float maxLeanAngle = 30f;

        [Tooltip("How quickly the motorcycle leans into turns")]
        public float leanSpeed = 5f;

        [Header("Lane Movement")]
        [Tooltip("Enable lane-based movement (left/right snap to lanes)")]
        public bool useLaneSystem = false;

        [Tooltip("Distance between lanes")]
        public float laneWidth = 3f;

        [Tooltip("Speed of lane switching")]
        public float laneSwitchSpeed = 10f;

        [Tooltip("Number of lanes (-1 = left, 0 = center, 1 = right, etc.)")]
        public int currentLane = 0;

        [Header("Physics")]
        [Tooltip("Gravity multiplier for snappy feel")]
        public float gravityMultiplier = 2f;

        [Tooltip("Height above ground to maintain")]
        public float hoverHeight = 0.5f;

        [Tooltip("Strength of ground snapping")]
        public float hoverForce = 50f;

        [Tooltip("Layer mask for ground detection")]
        public LayerMask groundLayer = 1; // Default layer

        [Header("Input")]
        [Tooltip("Use controller input (if available)")]
        public bool useController = true;

        [Tooltip("Keyboard horizontal axis sensitivity")]
        public float keyboardSensitivity = 1f;

        #endregion

        #region Private State

        private Rigidbody rb;
        private float currentSpeed = 0f;
        private float targetLanePosition = 0f;
        private float currentLeanAngle = 0f;
        private Vector3 moveDirection = Vector3.forward;

        // Input values
        private float steerInput = 0f;
        private float accelerateInput = 0f;
        private bool brakeInput = false;

        #endregion

        #region Properties

        /// <summary>
        /// Current forward speed of the motorcycle.
        /// </summary>
        public float CurrentSpeed => currentSpeed;

        /// <summary>
        /// Normalized speed (0-1).
        /// </summary>
        public float NormalizedSpeed => currentSpeed / maxSpeed;

        /// <summary>
        /// Is the motorcycle on the ground?
        /// </summary>
        public bool IsGrounded { get; private set; }

        #endregion

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // Configure rigidbody for arcade physics
            rb.useGravity = true;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        void Update()
        {
            // Gather input
            GatherInput();

            // Debug: Log input when detected (helps diagnose input issues)
            if (Mathf.Abs(steerInput) > 0.01f || brakeInput)
            {
                Debug.Log($"[MotorcycleController] Input - Steer: {steerInput:F2}, Brake: {brakeInput}, Speed: {currentSpeed:F1}, Grounded: {IsGrounded}");
            }

            // Update lean angle for visual feedback
            UpdateLeanAngle();
        }

        void FixedUpdate()
        {
            // Check if grounded
            CheckGround();

            // Apply movement
            if (!useLaneSystem)
            {
                ApplyFreeMovement();
            }
            else
            {
                ApplyLaneMovement();
            }

            // Apply hover force to stay above ground
            ApplyHoverForce();

            // Apply extra gravity
            rb.AddForce(Vector3.down * gravityMultiplier * 9.81f, ForceMode.Acceleration);
        }

        /// <summary>
        /// Gathers input from keyboard/controller.
        /// </summary>
        private void GatherInput()
        {
            // Horizontal steering input
            if (useController && Input.GetJoystickNames().Length > 0)
            {
                steerInput = Input.GetAxis("Horizontal");
            }
            else
            {
                // Keyboard input
                steerInput = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    steerInput = -1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    steerInput = 1f;

                steerInput *= keyboardSensitivity;
            }

            // Acceleration input (forward is always on by default, or use W key)
            accelerateInput = 1f; // Always accelerating in arcade mode

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                accelerateInput = 1f;

            // Brake input
            brakeInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            // Lane switching (if using lane system)
            if (useLaneSystem)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ChangeLane(-1);
                }
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ChangeLane(1);
                }
            }
        }

        /// <summary>
        /// Applies free steering movement (continuous left/right).
        /// </summary>
        private void ApplyFreeMovement()
        {
            // Update speed
            if (brakeInput)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - brakeStrength * Time.fixedDeltaTime);
            }
            else if (accelerateInput > 0)
            {
                currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.fixedDeltaTime);
            }

            // Apply forward movement
            moveDirection = transform.forward;
            rb.linearVelocity = new Vector3(
                moveDirection.x * currentSpeed,
                rb.linearVelocity.y, // Preserve vertical velocity
                moveDirection.z * currentSpeed
            );

            // Apply steering rotation
            if (Mathf.Abs(steerInput) > 0.01f && currentSpeed > 1f)
            {
                float turnAmount = steerInput * turnSpeed * Time.fixedDeltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }
        }

        /// <summary>
        /// Applies lane-based movement (snap to discrete lanes).
        /// </summary>
        private void ApplyLaneMovement()
        {
            // Update speed
            if (brakeInput)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - brakeStrength * Time.fixedDeltaTime);
            }
            else if (accelerateInput > 0)
            {
                currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.fixedDeltaTime);
            }

            // Calculate target lane position
            targetLanePosition = currentLane * laneWidth;

            // Smoothly move to target lane position
            Vector3 targetPosition = transform.position;
            targetPosition.x = Mathf.Lerp(transform.position.x, targetLanePosition, laneSwitchSpeed * Time.fixedDeltaTime);

            // Apply movement
            Vector3 velocity = Vector3.forward * currentSpeed;
            velocity.x = (targetPosition.x - transform.position.x) / Time.fixedDeltaTime;
            velocity.y = rb.linearVelocity.y; // Preserve vertical velocity

            rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Changes the current lane.
        /// </summary>
        private void ChangeLane(int direction)
        {
            currentLane += direction;
            currentLane = Mathf.Clamp(currentLane, -2, 2); // Max 5 lanes
            Debug.Log($"Switching to lane {currentLane}");
        }

        /// <summary>
        /// Checks if motorcycle is on the ground.
        /// </summary>
        private void CheckGround()
        {
            RaycastHit hit;
            IsGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                out hit,
                hoverHeight + 1f,
                groundLayer
            );
        }

        /// <summary>
        /// Applies hover force to maintain height above ground.
        /// </summary>
        private void ApplyHoverForce()
        {
            if (!IsGrounded)
                return;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, hoverHeight + 1f, groundLayer))
            {
                float distanceToGround = hit.distance;
                float hoverError = hoverHeight - distanceToGround;

                // Apply proportional hover force
                if (hoverError > 0)
                {
                    rb.AddForce(Vector3.up * hoverForce * hoverError, ForceMode.Acceleration);
                }
            }
        }

        /// <summary>
        /// Updates visual lean angle based on steering input.
        /// </summary>
        private void UpdateLeanAngle()
        {
            // Target lean based on steering
            float targetLean = -steerInput * maxLeanAngle;

            // Smoothly interpolate to target
            currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLean, leanSpeed * Time.deltaTime);

            // Apply lean to visual model (if exists)
            // Note: This rotates the entire GameObject - in a real game, you'd rotate a child mesh
            // For now, we'll skip visual lean to avoid affecting physics
            // transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, currentLeanAngle);
        }

        /// <summary>
        /// Visualizes hover raycast in Scene view.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (hoverHeight + 1f));
        }

        #region Public Methods

        /// <summary>
        /// Resets motorcycle to a specific position and rotation.
        /// </summary>
        public void ResetPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = 0f;
            currentLane = 0;
        }

        /// <summary>
        /// Applies a boost to the motorcycle.
        /// </summary>
        public void ApplyBoost(float boostAmount)
        {
            currentSpeed = Mathf.Min(maxSpeed * 1.5f, currentSpeed + boostAmount);
        }

        #endregion
    }
}
