using UnityEngine;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// EMERGENCY BLOOM SETUP - Add this directly to Main Camera in Unity Editor.
    /// Forces bloom configuration even if automated setup fails.
    /// </summary>
    [ExecuteAlways]
    public class ForceBloomSetup : MonoBehaviour
    {
        [Header("INSTRUCTIONS")]
        [TextArea(3, 5)]
        public string instructions = "1. Add this script to Main Camera\n2. Click 'Force Setup Bloom' button below\n3. Check console for success message\n4. Play the game!";

        [Header("Bloom Settings")]
        [Range(0f, 10f)]
        public float intensity = 6f;

        [Range(0f, 2f)]
        public float threshold = 0.7f;

        [ContextMenu("Force Setup Bloom")]
        public void SetupBloom()
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("ForceBloomSetup: No camera found!");
                return;
            }

            // Enable HDR
            cam.allowHDR = true;
            cam.allowMSAA = true;

            // Change clear flags to solid color for red sky
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(1f, 0.3f, 0.2f); // Red/orange

            Debug.Log("ForceBloomSetup: Camera configured for HDR + red background");

            // Try to add PostProcessLayer using string type name
            System.Type layerType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer, Unity.Postprocessing.Runtime");

            if (layerType == null)
            {
                // Try alternative assembly name
                layerType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer, PostProcessing");
            }

            if (layerType != null)
            {
                Component existingLayer = GetComponent(layerType);
                if (existingLayer == null)
                {
                    Component layer = gameObject.AddComponent(layerType);

                    // Set properties using reflection
                    var volumeTrigger = layerType.GetProperty("volumeTrigger");
                    if (volumeTrigger != null)
                        volumeTrigger.SetValue(layer, transform);

                    var volumeLayer = layerType.GetProperty("volumeLayer");
                    if (volumeLayer != null)
                        volumeLayer.SetValue(layer, -1); // Everything

                    Debug.Log("✅ PostProcessLayer added to camera!");
                }
                else
                {
                    Debug.Log("✅ PostProcessLayer already exists");
                }

                // Now create the volume
                CreatePostProcessVolume();
            }
            else
            {
                Debug.LogError("❌ PostProcessLayer type not found! Post Processing package may not be imported correctly.");
                Debug.LogError("MANUAL SETUP REQUIRED:");
                Debug.LogError("1. Window → Package Manager");
                Debug.LogError("2. Search for 'Post Processing'");
                Debug.LogError("3. Ensure it's installed (version 3.5.1+)");
                Debug.LogError("4. Restart Unity");
                Debug.LogError("5. Run this setup again");
            }
        }

        private void CreatePostProcessVolume()
        {
            // Check if volume already exists
            System.Type volumeType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume, Unity.Postprocessing.Runtime");
            if (volumeType == null)
            {
                volumeType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume, PostProcessing");
            }

            if (volumeType == null)
            {
                Debug.LogError("PostProcessVolume type not found!");
                return;
            }

            // Find existing volume
            Object existingVolume = FindFirstObjectByType(volumeType);
            if (existingVolume != null)
            {
                Debug.Log("✅ PostProcessVolume already exists in scene");
                return;
            }

            // Create new volume
            GameObject volumeObj = new GameObject("GlobalPostProcessVolume");
            Component volume = volumeObj.AddComponent(volumeType);

            // Set to global
            var isGlobal = volumeType.GetProperty("isGlobal");
            if (isGlobal != null)
                isGlobal.SetValue(volume, true);

            var priority = volumeType.GetProperty("priority");
            if (priority != null)
                priority.SetValue(volume, 1f);

            // Create profile with bloom
            System.Type profileType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessProfile, Unity.Postprocessing.Runtime");
            if (profileType == null)
            {
                profileType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.PostProcessProfile, PostProcessing");
            }

            if (profileType != null)
            {
                var profile = ScriptableObject.CreateInstance(profileType);

                var profileProp = volumeType.GetProperty("profile");
                if (profileProp != null)
                    profileProp.SetValue(volume, profile);

                // Add bloom
                AddBloomToProfile(profile);

                Debug.Log("✅ PostProcessVolume created with Bloom!");
                Debug.Log("🌟 BLOOM SHOULD NOW BE ACTIVE! 🌟");
                Debug.Log("Play the game and look for glowing edges and coins!");
            }
        }

        private void AddBloomToProfile(object profile)
        {
            System.Type bloomType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.Bloom, Unity.Postprocessing.Runtime");
            if (bloomType == null)
            {
                bloomType = System.Type.GetType("UnityEngine.Rendering.PostProcessing.Bloom, PostProcessing");
            }

            if (bloomType == null)
            {
                Debug.LogError("Bloom type not found!");
                return;
            }

            // Add bloom settings
            var addMethod = profile.GetType().GetMethod("AddSettings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (addMethod != null)
            {
                addMethod = addMethod.MakeGenericMethod(bloomType);
                var bloom = addMethod.Invoke(profile, null);

                // Enable and configure bloom
                SetParameter(bloom, "enabled", true);
                SetParameter(bloom, "intensity", intensity);
                SetParameter(bloom, "threshold", threshold);
                SetParameter(bloom, "softKnee", 0.5f);
                SetParameter(bloom, "diffusion", 7f);

                Debug.Log($"✅ Bloom configured: intensity={intensity}, threshold={threshold}");
            }
        }

        private void SetParameter(object settings, string name, object value)
        {
            try
            {
                var field = settings.GetType().GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var param = field.GetValue(settings);
                    var overrideMethod = param.GetType().GetMethod("Override", new System.Type[] { value.GetType() });
                    if (overrideMethod != null)
                        overrideMethod.Invoke(param, new object[] { value });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set {name}: {e.Message}");
            }
        }

        void OnEnable()
        {
            // Auto-setup when added to camera
            if (!Application.isPlaying)
            {
                Debug.Log("ForceBloomSetup: Right-click this component and select 'Force Setup Bloom' to configure bloom!");
            }
        }
    }
}
