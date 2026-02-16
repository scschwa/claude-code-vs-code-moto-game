using UnityEngine;
using UnityEngine.Events;
using DesertRider.MP3;
using DesertRider.Audio;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Manages boost mechanics with beat-synchronized charge refilling.
    /// Players can activate speed bursts that consume charges, which refill on strong beats.
    /// </summary>
    public class BoostSystem : MonoBehaviour
    {
        #region Singleton Pattern
        public static BoostSystem Instance { get; private set; }

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

        [Header("Boost Configuration")]
        [Tooltip("Maximum number of boost charges")]
        public int maxCharges = 3;

        [Tooltip("Duration of boost effect in seconds")]
        public float boostDuration = 2f;

        [Tooltip("Speed multiplier during boost")]
        public float boostSpeedMultiplier = 1.5f;

        [Header("Beat Detection")]
        [Tooltip("Time window for detecting beats (seconds)")]
        public float beatDetectionWindow = 0.15f;

        [Tooltip("Minimum beat strength to grant a charge")]
        [Range(0f, 1f)]
        public float strongBeatThreshold = 0.7f;

        [Header("Input")]
        [Tooltip("Key to activate boost")]
        public KeyCode boostKey = KeyCode.Space;

        [Header("Debug")]
        [Tooltip("Show beat detection debug logs")]
        public bool debugMode = false;

        // State
        private int currentCharges = 0;
        private bool isBoosting = false;
        private float boostEndTime = 0f;
        private int lastBeatIndex = -1;

        // References
        private AnalysisData analysisData;
        private MotorcycleController motorcycle;
        private MusicPlayer musicPlayer;

        // Events for UI feedback
        [System.Serializable]
        public class ChargesChangedEvent : UnityEvent<int> { }
        public ChargesChangedEvent OnChargesChanged = new ChargesChangedEvent();
        public UnityEvent OnBoostActivated = new UnityEvent();
        public UnityEvent OnBoostEnded = new UnityEvent();
        public UnityEvent OnChargeGained = new UnityEvent();

        // Public accessors
        public int CurrentCharges => currentCharges;
        public int MaxCharges => maxCharges;
        public bool IsBoosting => isBoosting;

        /// <summary>
        /// Initializes the boost system with references to game systems.
        /// </summary>
        public void Initialize(AnalysisData data, MotorcycleController motorcycleController, MusicPlayer player)
        {
            analysisData = data;
            motorcycle = motorcycleController;
            musicPlayer = player;

            // Reset state
            currentCharges = 0;
            isBoosting = false;
            lastBeatIndex = -1;
            boostEndTime = 0f;

            if (analysisData == null || analysisData.Beats == null || analysisData.Beats.Count == 0)
            {
                Debug.LogWarning("BoostSystem: No beat data available - boost charging disabled");
            }
            else
            {
                Debug.Log($"BoostSystem: Initialized with {analysisData.Beats.Count} beats");
            }
        }

        void Update()
        {
            // Beat detection and charge granting
            DetectBeats();

            // Handle boost input
            HandleBoostInput();

            // Update boost timer
            UpdateBoostTimer();
        }

        /// <summary>
        /// Detects beats during gameplay and grants charges on strong beats.
        /// </summary>
        private void DetectBeats()
        {
            // Skip if music isn't playing or data is missing
            if (musicPlayer == null || !musicPlayer.IsPlaying || analysisData == null || analysisData.Beats == null)
                return;

            float currentTime = musicPlayer.CurrentTime;

            // Iterate through beats starting from lastBeatIndex + 1
            for (int i = lastBeatIndex + 1; i < analysisData.Beats.Count; i++)
            {
                BeatEvent beat = analysisData.Beats[i];
                float timeDiff = beat.Time - currentTime;

                // Haven't reached this beat yet
                if (timeDiff > beatDetectionWindow)
                    break;

                // Beat is within detection window
                if (timeDiff >= 0f && timeDiff <= beatDetectionWindow)
                {
                    OnBeatDetected(beat);
                    lastBeatIndex = i;
                }
            }
        }

        /// <summary>
        /// Called when a beat is detected. Grants charge if beat is strong enough.
        /// </summary>
        private void OnBeatDetected(BeatEvent beat)
        {
            // Only grant charge on strong beats
            if (beat.Strength >= strongBeatThreshold)
            {
                if (currentCharges < maxCharges)
                {
                    currentCharges++;
                    OnChargesChanged?.Invoke(currentCharges);
                    OnChargeGained?.Invoke();

                    if (debugMode)
                    {
                        Debug.Log($"BoostSystem: Charge gained! ({currentCharges}/{maxCharges}) - Beat strength: {beat.Strength:F2}");
                    }
                }
            }
        }

        /// <summary>
        /// Handles boost activation input.
        /// </summary>
        private void HandleBoostInput()
        {
            // Check keyboard input
            if (Input.GetKeyDown(boostKey))
            {
                TryActivateBoost();
            }

            // Check controller input (A button on Xbox, Cross on PlayStation)
            if (Input.GetButtonDown("Jump")) // Unity's default "Jump" button maps to controller A/Cross
            {
                TryActivateBoost();
            }
        }

        /// <summary>
        /// Attempts to activate boost if conditions are met.
        /// </summary>
        public void TryActivateBoost()
        {
            // Can only boost if we have charges and aren't already boosting
            if (currentCharges > 0 && !isBoosting && motorcycle != null)
            {
                // Consume a charge
                currentCharges--;
                OnChargesChanged?.Invoke(currentCharges);

                // Activate boost
                isBoosting = true;
                boostEndTime = Time.time + boostDuration;

                // Apply speed boost
                motorcycle.maxSpeed *= boostSpeedMultiplier;

                OnBoostActivated?.Invoke();

                if (debugMode)
                {
                    Debug.Log($"BoostSystem: Boost activated! New speed: {motorcycle.maxSpeed:F1}");
                }
            }
            else if (debugMode && currentCharges == 0)
            {
                Debug.Log("BoostSystem: Cannot boost - no charges available");
            }
        }

        /// <summary>
        /// Updates boost timer and ends boost when duration expires.
        /// </summary>
        private void UpdateBoostTimer()
        {
            if (isBoosting && Time.time >= boostEndTime)
            {
                EndBoost();
            }
        }

        /// <summary>
        /// Ends the boost effect and restores normal speed.
        /// </summary>
        private void EndBoost()
        {
            if (!isBoosting)
                return;

            isBoosting = false;

            // Restore original maxSpeed
            if (motorcycle != null)
            {
                motorcycle.maxSpeed /= boostSpeedMultiplier;

                if (debugMode)
                {
                    Debug.Log($"BoostSystem: Boost ended. Speed restored to: {motorcycle.maxSpeed:F1}");
                }
            }

            OnBoostEnded?.Invoke();
        }

        /// <summary>
        /// Manually grant a charge (for testing or special events).
        /// </summary>
        public void GrantCharge()
        {
            if (currentCharges < maxCharges)
            {
                currentCharges++;
                OnChargesChanged?.Invoke(currentCharges);
                OnChargeGained?.Invoke();
            }
        }

        /// <summary>
        /// Reset boost state (useful for level restart).
        /// </summary>
        public void ResetBoost()
        {
            if (isBoosting && motorcycle != null)
            {
                motorcycle.maxSpeed /= boostSpeedMultiplier;
            }

            currentCharges = 0;
            isBoosting = false;
            lastBeatIndex = -1;
            boostEndTime = 0f;

            OnChargesChanged?.Invoke(currentCharges);
        }

        void OnDestroy()
        {
            // Cleanup: restore speed if boost was active
            if (isBoosting && motorcycle != null)
            {
                motorcycle.maxSpeed /= boostSpeedMultiplier;
            }
        }
    }
}
