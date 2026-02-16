using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Sets up post-processing effects for polished neon aesthetic.
    /// Includes bloom, color grading, vignette for professional look.
    /// Works with Unity Post Processing Stack v2 or URP Volume system.
    /// </summary>
    public class PostProcessingSetup : MonoBehaviour
    {
        [Header("Post-Processing Configuration")]
        [Tooltip("Enable bloom effect for glowing neon aesthetic")]
        public bool enableBloom = true;

        [Tooltip("Bloom intensity (higher = more glow)")]
        [Range(0f, 10f)]
        public float bloomIntensity = 5f;

        [Tooltip("Bloom threshold (lower = more things glow)")]
        [Range(0f, 2f)]
        public float bloomThreshold = 0.8f;

        [Tooltip("Enable color grading for warm tones")]
        public bool enableColorGrading = true;

        [Tooltip("Temperature shift (positive = warmer/orange)")]
        [Range(-100f, 100f)]
        public float temperature = 20f;

        [Tooltip("Saturation boost")]
        [Range(-100f, 100f)]
        public float saturation = 15f;

        [Tooltip("Enable vignette (darkens screen edges)")]
        public bool enableVignette = true;

        [Tooltip("Vignette intensity")]
        [Range(0f, 1f)]
        public float vignetteIntensity = 0.3f;

        [Header("Camera Settings")]
        [Tooltip("Enable HDR for better bloom")]
        public bool enableHDR = true;

        [Tooltip("Enable MSAA anti-aliasing (if available)")]
        public bool enableMSAA = true;

        private Camera mainCamera;

#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessVolume volume;
        private PostProcessLayer layer;
#endif

        void Start()
        {
            SetupPostProcessing();
        }

        /// <summary>
        /// Sets up post-processing for polished visual quality.
        /// </summary>
        private void SetupPostProcessing()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("PostProcessingSetup: No main camera found!");
                return;
            }

            // Setup camera for HDR and better quality
            SetupCamera();

            // Try to setup post-processing (requires Post Processing Stack)
#if UNITY_POST_PROCESSING_STACK_V2
            SetupPostProcessingStack();
#else
            // Try URP Volume system
            SetupURPVolume();
#endif
        }

        /// <summary>
        /// Configures camera for HDR and quality rendering.
        /// </summary>
        private void SetupCamera()
        {
            if (enableHDR)
            {
                mainCamera.allowHDR = true;
                Debug.Log("PostProcessingSetup: HDR enabled on camera");
            }

            if (enableMSAA)
            {
                mainCamera.allowMSAA = true;
                Debug.Log("PostProcessingSetup: MSAA enabled on camera");
            }

            // Set rendering path for better quality
            mainCamera.renderingPath = RenderingPath.UsePlayerSettings;

            Debug.Log($"PostProcessingSetup: Camera configured - HDR:{enableHDR}, MSAA:{enableMSAA}");
        }

#if UNITY_POST_PROCESSING_STACK_V2
        /// <summary>
        /// Sets up Post Processing Stack v2 (Built-in Render Pipeline).
        /// </summary>
        private void SetupPostProcessingStack()
        {
            // Add PostProcessLayer to camera if not present
            layer = mainCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.volumeTrigger = mainCamera.transform;
                layer.volumeLayer = LayerMask.GetMask("PostProcessing");
                Debug.Log("PostProcessingSetup: Added PostProcessLayer to camera");
            }

            // Create PostProcessVolume
            GameObject volumeObj = new GameObject("PostProcessVolume");
            volumeObj.layer = LayerMask.NameToLayer("PostProcessing");
            volume = volumeObj.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 1;

            // Create profile
            PostProcessProfile profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            volume.profile = profile;

            // Add Bloom
            if (enableBloom)
            {
                var bloom = profile.AddSettings<Bloom>();
                bloom.enabled.Override(true);
                bloom.intensity.Override(bloomIntensity);
                bloom.threshold.Override(bloomThreshold);
                bloom.softKnee.Override(0.5f);
                bloom.diffusion.Override(7f);
                Debug.Log($"PostProcessingSetup: Bloom added - intensity:{bloomIntensity}, threshold:{bloomThreshold}");
            }

            // Add Color Grading
            if (enableColorGrading)
            {
                var grading = profile.AddSettings<ColorGrading>();
                grading.enabled.Override(true);
                grading.tonemapper.Override(Tonemapper.ACES);
                grading.temperature.Override(temperature);
                grading.saturation.Override(saturation);
                Debug.Log($"PostProcessingSetup: Color grading added - temp:{temperature}, sat:{saturation}");
            }

            // Add Vignette
            if (enableVignette)
            {
                var vignette = profile.AddSettings<Vignette>();
                vignette.enabled.Override(true);
                vignette.intensity.Override(vignetteIntensity);
                vignette.smoothness.Override(0.4f);
                vignette.roundness.Override(1f);
                Debug.Log($"PostProcessingSetup: Vignette added - intensity:{vignetteIntensity}");
            }

            Debug.Log("✅ Post Processing Stack v2 configured successfully!");
        }
#endif

        /// <summary>
        /// Sets up URP Volume system (Universal Render Pipeline).
        /// </summary>
        private void SetupURPVolume()
        {
            // Try to find or create URP Volume
            var volumeType = System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            if (volumeType == null)
            {
                Debug.LogWarning("PostProcessingSetup: Neither Post Processing Stack v2 nor URP found.");
                Debug.LogWarning("To enable post-processing effects:");
                Debug.LogWarning("1. Install 'Post Processing' package from Package Manager");
                Debug.LogWarning("2. Add PostProcessLayer component to Main Camera");
                Debug.LogWarning("3. This script will automatically configure effects on next run");
                return;
            }

            // URP Volume setup (requires manual configuration in Unity Editor)
            Debug.Log("PostProcessingSetup: URP detected. Please configure Volume manually:");
            Debug.Log("1. Create GameObject → Volume → Global Volume");
            Debug.Log("2. Add Volume Profile with Bloom, Tonemapping, Color Adjustments");
            Debug.Log("3. Set Bloom intensity to " + bloomIntensity);
            Debug.Log("4. Enable ACES Tonemapping");
        }

        /// <summary>
        /// Update values in real-time when changed in inspector.
        /// </summary>
        void OnValidate()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (Application.isPlaying && volume != null && volume.profile != null)
            {
                // Update bloom
                if (volume.profile.TryGetSettings(out Bloom bloom))
                {
                    bloom.intensity.Override(bloomIntensity);
                    bloom.threshold.Override(bloomThreshold);
                }

                // Update color grading
                if (volume.profile.TryGetSettings(out ColorGrading grading))
                {
                    grading.temperature.Override(temperature);
                    grading.saturation.Override(saturation);
                }

                // Update vignette
                if (volume.profile.TryGetSettings(out Vignette vignette))
                {
                    vignette.intensity.Override(vignetteIntensity);
                }
            }
#endif
        }
    }
}
