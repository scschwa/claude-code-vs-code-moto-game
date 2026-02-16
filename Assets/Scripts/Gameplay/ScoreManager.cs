using UnityEngine;
using UnityEngine.Events;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Manages player score, combo system, and scoring events.
    /// Implements combo multiplier that increases with consecutive collections.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static ScoreManager Instance { get; private set; }

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

        [Header("Score Settings")]
        [Tooltip("Current total score")]
        public int currentScore = 0;

        [Tooltip("Total number of coins collected")]
        public int coinsCollected = 0;

        [Header("Combo System")]
        [Tooltip("Current combo count (consecutive collections)")]
        public int currentCombo = 0;

        [Tooltip("Highest combo achieved this session")]
        public int maxCombo = 0;

        [Tooltip("Time in seconds before combo resets")]
        public float comboTimeout = 3f;

        [Tooltip("Combo multiplier formula: 1.0 + (currentCombo / comboStep) * comboMultiplierStep")]
        public int comboStep = 10;

        [Tooltip("Multiplier increase per combo step")]
        public float comboMultiplierStep = 0.5f;

        [Header("Events")]
        [Tooltip("Invoked when score changes (score, combo)")]
        public UnityEvent<int, int> OnScoreChanged;

        [Tooltip("Invoked when combo resets")]
        public UnityEvent OnComboReset;

        [Header("Debug")]
        [Tooltip("Show debug logs for scoring events")]
        public bool debugMode = false;

        private float lastCollectionTime = 0f;
        private bool comboActive = false;

        /// <summary>
        /// Current score multiplier based on combo.
        /// </summary>
        public float CurrentMultiplier
        {
            get
            {
                if (currentCombo == 0)
                {
                    return 1.0f;
                }
                return 1.0f + (currentCombo / (float)comboStep) * comboMultiplierStep;
            }
        }

        void Update()
        {
            // Check for combo timeout
            if (comboActive && Time.time - lastCollectionTime > comboTimeout)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// Adds score with combo multiplier applied.
        /// </summary>
        /// <param name="baseScore">Base score value before multiplier</param>
        public void AddScore(int baseScore)
        {
            // Update combo
            currentCombo++;
            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
            }

            // Calculate actual score with multiplier
            float multiplier = CurrentMultiplier;
            int actualScore = Mathf.RoundToInt(baseScore * multiplier);

            // Add to total
            currentScore += actualScore;
            coinsCollected++;

            // Update timing
            lastCollectionTime = Time.time;
            comboActive = true;

            if (debugMode)
            {
                Debug.Log($"ScoreManager: +{actualScore} (base={baseScore}, multiplier={multiplier:F2}, combo={currentCombo})");
            }

            // Invoke event
            OnScoreChanged?.Invoke(currentScore, currentCombo);
        }

        /// <summary>
        /// Resets the combo counter.
        /// </summary>
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"ScoreManager: Combo reset (was {currentCombo})");
                }

                currentCombo = 0;
                comboActive = false;

                // Invoke reset event
                OnComboReset?.Invoke();
            }
        }

        /// <summary>
        /// Resets all score data (for new game).
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            coinsCollected = 0;
            currentCombo = 0;
            maxCombo = 0;
            comboActive = false;

            if (debugMode)
            {
                Debug.Log("ScoreManager: Score reset");
            }

            OnScoreChanged?.Invoke(currentScore, currentCombo);
        }

        /// <summary>
        /// Gets current score statistics.
        /// </summary>
        public void GetStats(out int score, out int coins, out int combo, out int maxComboValue, out float multiplier)
        {
            score = currentScore;
            coins = coinsCollected;
            combo = currentCombo;
            maxComboValue = maxCombo;
            multiplier = CurrentMultiplier;
        }

        /// <summary>
        /// Debug method to display score statistics.
        /// </summary>
        [ContextMenu("Show Score Statistics")]
        public void ShowScoreStatistics()
        {
            Debug.Log("=== Score Statistics ===");
            Debug.Log($"Current Score: {currentScore}");
            Debug.Log($"Coins Collected: {coinsCollected}");
            Debug.Log($"Current Combo: {currentCombo} (x{CurrentMultiplier:F2})");
            Debug.Log($"Max Combo: {maxCombo}");
        }
    }
}
