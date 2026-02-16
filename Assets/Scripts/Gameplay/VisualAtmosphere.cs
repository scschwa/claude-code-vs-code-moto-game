using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Controls the overall visual atmosphere of the game including sky color,
    /// fog, and ambient lighting to create the dramatic neon aesthetic.
    /// </summary>
    public class VisualAtmosphere : MonoBehaviour
    {
        [Header("Sky Configuration")]
        [Tooltip("Top color of sky gradient (horizon)")]
        public Color skyHorizonColor = new Color(1f, 0.3f, 0.2f); // Red/orange

        [Tooltip("Bottom color of sky gradient (zenith)")]
        public Color skyZenithColor = new Color(1f, 0.5f, 0.3f); // Lighter orange

        [Tooltip("Camera to apply background color to")]
        public Camera mainCamera;

        [Header("Fog Configuration")]
        [Tooltip("Enable distance fog")]
        public bool enableFog = true;

        [Tooltip("Fog color (should match horizon)")]
        public Color fogColor = new Color(1f, 0.3f, 0.2f); // Red/orange

        [Tooltip("Fog start distance")]
        public float fogStart = 50f;

        [Tooltip("Fog end distance")]
        public float fogEnd = 200f;

        [Header("Lighting")]
        [Tooltip("Ambient light color")]
        public Color ambientColor = new Color(0.3f, 0.15f, 0.1f); // Warm ambient

        [Tooltip("Directional light (sun/key light)")]
        public Light directionalLight;

        [Tooltip("Directional light color")]
        public Color lightColor = new Color(1f, 0.8f, 0.6f); // Warm light

        [Tooltip("Directional light intensity")]
        [Range(0f, 2f)]
        public float lightIntensity = 0.8f;

        void Start()
        {
            SetupAtmosphere();
        }

        /// <summary>
        /// Configures the visual atmosphere for dramatic neon aesthetic.
        /// </summary>
        private void SetupAtmosphere()
        {
            // Setup camera background
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = skyHorizonColor;
                Debug.Log($"VisualAtmosphere: Set camera background to {skyHorizonColor}");
            }

            // Setup fog
            RenderSettings.fog = enableFog;
            if (enableFog)
            {
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = fogStart;
                RenderSettings.fogEndDistance = fogEnd;
                Debug.Log($"VisualAtmosphere: Fog enabled - start:{fogStart} end:{fogEnd}");
            }

            // Setup ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            Debug.Log($"VisualAtmosphere: Ambient light set to {ambientColor}");

            // Setup directional light
            if (directionalLight == null)
            {
                // Find existing directional light
                Light[] lights = FindObjectsOfType<Light>();
                foreach (Light light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        directionalLight = light;
                        break;
                    }
                }

                // Create one if none exists
                if (directionalLight == null)
                {
                    GameObject lightObj = new GameObject("Directional Light");
                    directionalLight = lightObj.AddComponent<Light>();
                    directionalLight.type = LightType.Directional;
                    directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    Debug.Log("VisualAtmosphere: Created directional light");
                }
            }

            if (directionalLight != null)
            {
                directionalLight.color = lightColor;
                directionalLight.intensity = lightIntensity;
                directionalLight.shadows = LightShadows.None; // Disable for performance
                Debug.Log($"VisualAtmosphere: Directional light configured - color:{lightColor}, intensity:{lightIntensity}");
            }
        }

        /// <summary>
        /// Updates atmosphere in real-time if values change in inspector.
        /// </summary>
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                SetupAtmosphere();
            }
        }
    }
}
