using UnityEngine;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Configuration for special terrain features like ramps, banking, and color variation.
    /// Attached as a nested configuration within TerrainGenerator.
    /// </summary>
    [System.Serializable]
    public class TerrainFeatureConfig
    {
        [Header("Ramps & Jumps")]
        [Tooltip("Enable ramp/jump generation")]
        public bool enableJumps = true;

        [Tooltip("Intensity threshold for spawning jumps (0.7 = high intensity only)")]
        public float jumpIntensityThreshold = 0.7f;

        [Tooltip("Height of ramp launch")]
        public float rampHeight = 3f;

        [Tooltip("Length of ramp as fraction of segment (0.4 = 40% of segment)")]
        public float rampLengthFraction = 0.4f;

        [Tooltip("Ramp profile curve (0-1 input, 0-1 output for height)")]
        public AnimationCurve rampProfile = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Road Banking")]
        [Tooltip("Enable road banking on curves")]
        public bool enableBanking = true;

        [Tooltip("Maximum bank angle in degrees")]
        public float maxBankAngle = 15f;

        [Tooltip("Curve amount threshold for banking (0.3 = moderate curves)")]
        public float bankingCurveThreshold = 0.3f;

        [Header("Visual Variety")]
        [Tooltip("Enable color variation based on intensity")]
        public bool enableColorVariation = true;

        [Tooltip("Color gradient mapped to intensity (0=low, 1=high)")]
        public Gradient intensityColorGradient = CreateDefaultGradient();

        /// <summary>
        /// Creates default color gradient for intensity visualization.
        /// </summary>
        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.gray, 0.0f);     // Low intensity
            colorKeys[1] = new GradientColorKey(Color.blue, 0.5f);     // Medium
            colorKeys[2] = new GradientColorKey(Color.red, 1.0f);      // High intensity

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
    }
}
