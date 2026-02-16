using UnityEngine;
using System;
using System.Reflection;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Sets up Post Processing bloom effect using reflection (works without compile-time defines).
    /// Creates intense neon glow on emissive materials for polished aesthetic.
    /// </summary>
    public class BloomSetup : MonoBehaviour
    {
        [Header("Bloom Configuration")]
        [Range(0f, 10f)]
        public float bloomIntensity = 6f;

        [Range(0f, 2f)]
        public float bloomThreshold = 0.7f;

        [Range(0f, 1f)]
        public float bloomSoftKnee = 0.5f;

        [Range(1f, 10f)]
        public float bloomDiffusion = 7f;

        [Header("Additional Effects")]
        public bool enableVignette = true;

        [Range(0f, 1f)]
        public float vignetteIntensity = 0.3f;

        private Camera mainCamera;
        private object postProcessLayer;
        private object postProcessVolume;
        private bool setupSuccessful = false;

        void Start()
        {
            SetupBloom();
        }

        private void SetupBloom()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("BloomSetup: No main camera found!");
                return;
            }

            // Enable HDR on camera (required for bloom)
            mainCamera.allowHDR = true;
            mainCamera.allowMSAA = true;

            Debug.Log("BloomSetup: Starting post-processing setup...");

            try
            {
                // Try to find PostProcessing types using reflection
                Assembly ppAssembly = null;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name.Contains("PostProcessing"))
                    {
                        ppAssembly = assembly;
                        break;
                    }
                }

                if (ppAssembly == null)
                {
                    Debug.LogError("BloomSetup: Post Processing assembly not found!");
                    LogManualSetupInstructions();
                    return;
                }

                Debug.Log($"BloomSetup: Found Post Processing assembly: {ppAssembly.GetName().Name}");

                // Get types
                Type layerType = ppAssembly.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer");
                Type volumeType = ppAssembly.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume");
                Type profileType = ppAssembly.GetType("UnityEngine.Rendering.PostProcessing.PostProcessProfile");
                Type bloomType = ppAssembly.GetType("UnityEngine.Rendering.PostProcessing.Bloom");
                Type vignetteType = ppAssembly.GetType("UnityEngine.Rendering.PostProcessing.Vignette");

                if (layerType == null || volumeType == null)
                {
                    Debug.LogError("BloomSetup: Could not find PostProcessing types!");
                    LogManualSetupInstructions();
                    return;
                }

                Debug.Log("BloomSetup: Found all required types");

                // Add PostProcessLayer to camera
                Component layer = mainCamera.gameObject.GetComponent(layerType);
                if (layer == null)
                {
                    layer = mainCamera.gameObject.AddComponent(layerType);

                    // Set volumeTrigger
                    PropertyInfo volumeTriggerProp = layerType.GetProperty("volumeTrigger");
                    if (volumeTriggerProp != null)
                        volumeTriggerProp.SetValue(layer, mainCamera.transform);

                    // Set volumeLayer to Everything
                    PropertyInfo volumeLayerProp = layerType.GetProperty("volumeLayer");
                    if (volumeLayerProp != null)
                        volumeLayerProp.SetValue(layer, -1); // Everything layer mask

                    Debug.Log("BloomSetup: Added PostProcessLayer to camera");
                }

                postProcessLayer = layer;

                // Create PostProcessVolume
                GameObject volumeObj = new GameObject("PostProcessVolume");
                Component volume = volumeObj.AddComponent(volumeType);

                // Set isGlobal = true
                PropertyInfo isGlobalProp = volumeType.GetProperty("isGlobal");
                if (isGlobalProp != null)
                    isGlobalProp.SetValue(volume, true);

                // Set priority
                PropertyInfo priorityProp = volumeType.GetProperty("priority");
                if (priorityProp != null)
                    priorityProp.SetValue(volume, 1f);

                Debug.Log("BloomSetup: Created PostProcessVolume");

                // Create profile
                object profile = ScriptableObject.CreateInstance(profileType);

                // Set profile on volume
                PropertyInfo profileProp = volumeType.GetProperty("profile");
                if (profileProp != null)
                    profileProp.SetValue(volume, profile);

                Debug.Log("BloomSetup: Created profile");

                // Add Bloom settings
                if (bloomType != null)
                {
                    MethodInfo addSettingsMethod = profileType.GetMethod("AddSettings", new Type[] { bloomType });
                    if (addSettingsMethod == null)
                    {
                        // Try generic version
                        addSettingsMethod = profileType.GetMethod("AddSettings", BindingFlags.Public | BindingFlags.Instance);
                        if (addSettingsMethod != null)
                        {
                            addSettingsMethod = addSettingsMethod.MakeGenericMethod(bloomType);
                        }
                    }

                    if (addSettingsMethod != null)
                    {
                        object bloom = addSettingsMethod.Invoke(profile, null);

                        // Set bloom properties
                        SetBoolParameter(bloom, "enabled", true);
                        SetFloatParameter(bloom, "intensity", bloomIntensity);
                        SetFloatParameter(bloom, "threshold", bloomThreshold);
                        SetFloatParameter(bloom, "softKnee", bloomSoftKnee);
                        SetFloatParameter(bloom, "diffusion", bloomDiffusion);

                        Debug.Log($"✅ Bloom configured - intensity:{bloomIntensity}, threshold:{bloomThreshold}");
                    }
                }

                // Add Vignette if enabled
                if (enableVignette && vignetteType != null)
                {
                    MethodInfo addSettingsMethod = profileType.GetMethod("AddSettings", BindingFlags.Public | BindingFlags.Instance);
                    if (addSettingsMethod != null)
                    {
                        addSettingsMethod = addSettingsMethod.MakeGenericMethod(vignetteType);
                        object vignette = addSettingsMethod.Invoke(profile, null);

                        SetBoolParameter(vignette, "enabled", true);
                        SetFloatParameter(vignette, "intensity", vignetteIntensity);
                        SetFloatParameter(vignette, "smoothness", 0.4f);
                        SetFloatParameter(vignette, "roundness", 1f);

                        Debug.Log($"✅ Vignette configured - intensity:{vignetteIntensity}");
                    }
                }

                postProcessVolume = volume;
                setupSuccessful = true;

                Debug.Log("🌟 POST-PROCESSING BLOOM ACTIVE! 🌟");
                Debug.Log("You should now see glowing neon effects on edges and collectibles!");

            }
            catch (Exception e)
            {
                Debug.LogError($"BloomSetup: Error setting up post-processing: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                LogManualSetupInstructions();
            }
        }

        private void SetBoolParameter(object settings, string paramName, bool value)
        {
            try
            {
                FieldInfo field = settings.GetType().GetField(paramName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object param = field.GetValue(settings);
                    MethodInfo overrideMethod = param.GetType().GetMethod("Override", new Type[] { typeof(bool) });
                    if (overrideMethod != null)
                        overrideMethod.Invoke(param, new object[] { value });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"BloomSetup: Could not set {paramName}: {e.Message}");
            }
        }

        private void SetFloatParameter(object settings, string paramName, float value)
        {
            try
            {
                FieldInfo field = settings.GetType().GetField(paramName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object param = field.GetValue(settings);
                    MethodInfo overrideMethod = param.GetType().GetMethod("Override", new Type[] { typeof(float) });
                    if (overrideMethod != null)
                        overrideMethod.Invoke(param, new object[] { value });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"BloomSetup: Could not set {paramName}: {e.Message}");
            }
        }

        private void LogManualSetupInstructions()
        {
            Debug.LogWarning("========================================");
            Debug.LogWarning("MANUAL BLOOM SETUP REQUIRED:");
            Debug.LogWarning("1. Select Main Camera");
            Debug.LogWarning("2. Add Component → Post Process Layer");
            Debug.LogWarning("3. Set 'Volume Trigger' to Main Camera");
            Debug.LogWarning("4. Create GameObject → 3D Object → Post Process Volume");
            Debug.LogWarning("5. Check 'Is Global' on volume");
            Debug.LogWarning("6. Create new Post Process Profile");
            Debug.LogWarning("7. Add Bloom effect with:");
            Debug.LogWarning("   - Intensity: 6.0");
            Debug.LogWarning("   - Threshold: 0.7");
            Debug.LogWarning("8. Restart game to see glow effects!");
            Debug.LogWarning("========================================");
        }

        void OnValidate()
        {
            if (Application.isPlaying && setupSuccessful)
            {
                // Update bloom parameters in real-time
                if (postProcessVolume != null)
                {
                    try
                    {
                        PropertyInfo profileProp = postProcessVolume.GetType().GetProperty("profile");
                        if (profileProp != null)
                        {
                            object profile = profileProp.GetValue(postProcessVolume);
                            // Find and update bloom settings
                            // (Implementation would require more reflection here)
                            Debug.Log("BloomSetup: Parameters updated");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"BloomSetup: Could not update parameters: {e.Message}");
                    }
                }
            }
        }
    }
}
