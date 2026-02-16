using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesertRider.MP3;
using DesertRider.Audio;

namespace DesertRider.Gameplay
{
    /// <summary>
    /// Manages Tron-style motorcycle visuals with music-reactive effects.
    /// Handles geometry creation, trail rendering, boost effects, and beat pulses.
    /// </summary>
    public class MotorcycleVisualController : MonoBehaviour
    {
        #region Configuration
        [Header("Visual Configuration")]
        [Tooltip("Enable debug logging")]
        public bool debugMode = true; // Enabled to verify music reactivity

        [Header("Color Palette")]
        [Tooltip("Primary cyan glow color")]
        public Color primaryCyan = new Color(0f, 0.9f, 1f);

        [Tooltip("Accent blue color")]
        public Color accentBlue = new Color(0.2f, 0.6f, 1f);

        [Tooltip("Hot pink boost color")]
        public Color hotPink = new Color(1f, 0f, 0.5f);

        [Header("Trail Configuration")]
        [Tooltip("Minimum trail time (quiet music)")]
        [Range(0.5f, 2f)]
        public float minTrailTime = 0.8f;

        [Tooltip("Maximum trail time (loud music/boost)")]
        [Range(1f, 3f)]
        public float maxTrailTime = 2.0f;

        [Tooltip("Minimum trail width")]
        [Range(0.1f, 1f)]
        public float minTrailWidth = 0.5f;

        [Tooltip("Maximum trail width")]
        [Range(0.5f, 2f)]
        public float maxTrailWidth = 1.2f;

        [Header("Beat Detection")]
        [Tooltip("Minimum beat strength for glow pulse")]
        [Range(0f, 1f)]
        public float beatPulseThreshold = 0.7f;

        [Tooltip("Beat strength for camera shake")]
        [Range(0f, 1f)]
        public float beatShakeThreshold = 10.0f; // DISABLED - was causing uncontrollable camera shake (set to 10.0 so it never triggers)

        [Tooltip("Beat detection window (seconds)")]
        public float beatDetectionWindow = 0.15f;

        [Header("Speed Lines")]
        [Tooltip("Speed threshold for speed lines (0-1)")]
        [Range(0f, 1f)]
        public float speedLineThreshold = 0.8f;

        [Tooltip("Enable speed lines effect")]
        public bool enableSpeedLines = true;
        #endregion

        #region References
        private AnalysisData analysisData;
        private MusicPlayer musicPlayer;
        private MotorcycleController motorcycle;
        private CameraFollow cameraFollow;
        private BoostSystem boostSystem;
        #endregion

        #region Visual Components
        private GameObject tronBikeContainer;
        private TrailRenderer mainTrail;
        private ParticleSystem boostParticles;
        private ParticleSystem speedLinesParticles;

        private List<Renderer> glowRenderers = new List<Renderer>();
        private List<Material> materialInstances = new List<Material>();
        #endregion

        #region State
        private bool isBoosting = false;
        private float currentIntensity = 0.3f;
        private float targetIntensity = 0.3f;
        private int lastBeatIndex = -1;
        private bool isInitialized = false;

        private float originalShakeIntensity = 0.2f;
        private Coroutine currentShakeCoroutine = null;
        private Coroutine currentGlowPulseCoroutine = null;
        #endregion

        /// <summary>
        /// Initializes the visual controller with all necessary references.
        /// </summary>
        public void Initialize(AnalysisData data, MusicPlayer player, MotorcycleController motorcycleController,
            CameraFollow camera, BoostSystem boost)
        {
            if (isInitialized)
            {
                Debug.LogWarning("MotorcycleVisualController: Already initialized");
                return;
            }

            analysisData = data;
            musicPlayer = player;
            motorcycle = motorcycleController;
            cameraFollow = camera;
            boostSystem = boost;

            if (cameraFollow != null)
            {
                originalShakeIntensity = cameraFollow.shakeIntensity;
            }

            // Hide original motorcycle primitive (capsule/mesh) to prevent double rendering
            MeshRenderer originalRenderer = GetComponent<MeshRenderer>();
            if (originalRenderer != null)
            {
                originalRenderer.enabled = false;
                if (debugMode)
                {
                    Debug.Log("MotorcycleVisualController: Disabled original capsule renderer");
                }
            }

            // Build visual components
            BuildTronMotorcycle();
            SetupTrailRenderer();
            SetupBoostParticles();

            if (enableSpeedLines)
            {
                SetupSpeedLines();
            }

            // Subscribe to boost events
            if (boostSystem != null)
            {
                boostSystem.OnBoostActivated.AddListener(OnBoostActivated);
                boostSystem.OnBoostEnded.AddListener(OnBoostEnded);
            }

            isInitialized = true;

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Initialized with " +
                    $"{glowRenderers.Count} glow renderers, " +
                    $"{analysisData?.Beats?.Count ?? 0} beats");
            }
        }

        void Update()
        {
            // ALWAYS log once at start to verify Update is being called
            if (Time.frameCount == 100)
            {
                Debug.LogError($"[DIAGNOSTIC] VisualController Update IS BEING CALLED - isInit:{isInitialized}, player:{musicPlayer != null}, playing:{musicPlayer?.IsPlaying ?? false}, debugMode:{debugMode}");
            }

            if (!isInitialized || musicPlayer == null || !musicPlayer.IsPlaying)
            {
                if (Time.frameCount % 60 == 0) // ALWAYS log, ignore debugMode
                {
                    Debug.LogWarning($"[VisualController] Update BLOCKED - isInit:{isInitialized}, player:{musicPlayer != null}, playing:{musicPlayer?.IsPlaying ?? false}, Time:{Time.time:F2}s");
                }
                return;
            }

            if (Time.frameCount % 60 == 0) // ALWAYS log, ignore debugMode
            {
                Debug.Log($"[VisualController] Update RUNNING - Time:{musicPlayer.CurrentTime:F2}s, Intensity:{currentIntensity:F2}");
            }

            // Update music-reactive intensity
            UpdateIntensity();

            // Update trail properties based on intensity
            UpdateTrailProperties();

            // Detect beats for pulse effects
            DetectBeats();

            // Update speed lines
            if (enableSpeedLines && speedLinesParticles != null)
            {
                UpdateSpeedLines();
            }
        }

        #region Tron Geometry Construction
        /// <summary>
        /// Builds the Tron-style motorcycle geometry using Unity primitives.
        /// </summary>
        private void BuildTronMotorcycle()
        {
            // Create container for visual geometry
            tronBikeContainer = new GameObject("TronBike");
            tronBikeContainer.transform.SetParent(transform);
            tronBikeContainer.transform.localPosition = Vector3.zero;
            tronBikeContainer.transform.localRotation = Quaternion.identity;

            // Create emissive materials
            Material bodyMaterial = CreateEmissiveMaterial(primaryCyan, 2f);
            Material wheelMaterial = CreateEmissiveMaterial(accentBlue, 1.5f);
            Material accentMaterial = CreateEmissiveMaterial(primaryCyan, 3f);

            // Main body (angular chassis)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(tronBikeContainer.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.8f, 0.3f, 1.5f);
            Destroy(body.GetComponent<Collider>()); // Remove collider (physics on parent)

            Renderer bodyRenderer = body.GetComponent<Renderer>();
            bodyRenderer.material = bodyMaterial;
            glowRenderers.Add(bodyRenderer);

            // Front wheel
            GameObject frontWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frontWheel.name = "FrontWheel";
            frontWheel.transform.SetParent(tronBikeContainer.transform);
            frontWheel.transform.localPosition = new Vector3(0f, -0.2f, 0.6f);
            frontWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            frontWheel.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            Destroy(frontWheel.GetComponent<Collider>());

            Renderer frontWheelRenderer = frontWheel.GetComponent<Renderer>();
            frontWheelRenderer.material = wheelMaterial;
            glowRenderers.Add(frontWheelRenderer);

            // Rear wheel
            GameObject rearWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rearWheel.name = "RearWheel";
            rearWheel.transform.SetParent(tronBikeContainer.transform);
            rearWheel.transform.localPosition = new Vector3(0f, -0.2f, -0.6f);
            rearWheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rearWheel.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            Destroy(rearWheel.GetComponent<Collider>());

            Renderer rearWheelRenderer = rearWheel.GetComponent<Renderer>();
            rearWheelRenderer.material = wheelMaterial;
            glowRenderers.Add(rearWheelRenderer);

            // Accent lines (3 thin light strips)
            for (int i = 0; i < 3; i++)
            {
                GameObject accentLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                accentLine.name = $"AccentLine{i + 1}";
                accentLine.transform.SetParent(tronBikeContainer.transform);

                float xOffset = (i - 1) * 0.3f; // Spread: -0.3, 0, 0.3
                accentLine.transform.localPosition = new Vector3(xOffset, 0.2f, 0f);
                accentLine.transform.localScale = new Vector3(0.05f, 0.05f, 1.4f);
                Destroy(accentLine.GetComponent<Collider>());

                Renderer accentRenderer = accentLine.GetComponent<Renderer>();
                accentRenderer.material = accentMaterial;
                glowRenderers.Add(accentRenderer);
            }

            if (debugMode)
            {
                Debug.Log($"MotorcycleVisualController: Built Tron geometry with {glowRenderers.Count} renderers");
            }
        }

        /// <summary>
        /// Creates an emissive material for neon glow effect.
        /// </summary>
        private Material CreateEmissiveMaterial(Color glowColor, float intensity)
        {
            Material mat = new Material(Shader.Find("Standard"));

            // Base color (darker)
            Color baseColor = glowColor * 0.5f;
            baseColor.a = 1f;
            mat.SetColor("_Color", baseColor);

            // Emission (bright glow)
            mat.EnableKeyword("_EMISSION");
            Color emissionColor = glowColor * intensity;
            mat.SetColor("_EmissionColor", emissionColor);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Material properties
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Glossiness", 0.8f);

            materialInstances.Add(mat);
            return mat;
        }
        #endregion

        #region Trail System
        /// <summary>
        /// Sets up the main trail renderer with base configuration.
        /// </summary>
        private void SetupTrailRenderer()
        {
            mainTrail = gameObject.AddComponent<TrailRenderer>();

            // Base properties
            mainTrail.time = minTrailTime;
            mainTrail.startWidth = minTrailWidth;
            mainTrail.endWidth = 0.1f;
            mainTrail.minVertexDistance = 0.1f;
            mainTrail.alignment = LineAlignment.TransformZ;
            mainTrail.numCornerVertices = 5;
            mainTrail.numCapVertices = 5;

            // Create trail material
            Material trailMaterial = CreateTrailMaterial(primaryCyan, 0.5f);
            mainTrail.material = trailMaterial;

            // Color gradient (will be updated based on boost state)
            UpdateTrailColor(primaryCyan, 0.5f);

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Trail renderer configured");
            }
        }

        /// <summary>
        /// Creates a material for the trail renderer.
        /// </summary>
        private Material CreateTrailMaterial(Color color, float alpha)
        {
            Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
            mat.SetColor("_Color", new Color(color.r, color.g, color.b, alpha));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);

            // Enable transparency
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            materialInstances.Add(mat);
            return mat;
        }

        /// <summary>
        /// Updates trail color gradient.
        /// </summary>
        private void UpdateTrailColor(Color color, float alpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(alpha, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            mainTrail.colorGradient = gradient;
        }
        #endregion

        #region Music Reactivity
        /// <summary>
        /// Updates intensity based on current music playback.
        /// </summary>
        private void UpdateIntensity()
        {
            if (analysisData == null || analysisData.IntensityCurve == null || analysisData.IntensityCurve.Count == 0)
            {
                if (Time.frameCount % 120 == 0) // Log every 2 seconds
                {
                    Debug.LogError($"[VisualController] UpdateIntensity BLOCKED - analysisData:{analysisData != null}, curve:{analysisData?.IntensityCurve != null}, count:{analysisData?.IntensityCurve?.Count ?? 0}");
                }
                targetIntensity = 0.3f;
                return;
            }

            float currentTime = musicPlayer.CurrentTime;
            float intensity = GetIntensityAtTime(currentTime);

            // Apply boost multiplier
            float boostMult = isBoosting ? 1.5f : 1.0f;

            // Map intensity to 0.3-1.0 range
            targetIntensity = Mathf.Lerp(0.3f, 1.0f, intensity) * boostMult;

            // Smooth lerp to target
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 5f);

            // Log intensity values periodically
            if (Time.frameCount % 120 == 0) // Log every 2 seconds
            {
                Debug.Log($"[VisualController] Intensity - raw:{intensity:F3}, target:{targetIntensity:F3}, current:{currentIntensity:F3}, time:{currentTime:F2}s, duration:{analysisData.Duration:F2}s");
            }
        }

        /// <summary>
        /// Samples intensity from AnalysisData at specific time.
        /// </summary>
        private float GetIntensityAtTime(float currentTime)
        {
            if (analysisData == null || analysisData.IntensityCurve == null || analysisData.IntensityCurve.Count == 0)
                return 0.3f;

            // Normalize time to 0-1
            float normalizedTime = Mathf.Clamp01(currentTime / analysisData.Duration);

            // Map to intensity curve index
            int index = (int)(normalizedTime * (analysisData.IntensityCurve.Count - 1));
            index = Mathf.Clamp(index, 0, analysisData.IntensityCurve.Count - 1);

            return analysisData.IntensityCurve[index];
        }

        /// <summary>
        /// Updates trail properties based on current intensity.
        /// </summary>
        private void UpdateTrailProperties()
        {
            if (mainTrail == null)
                return;

            // Modulate trail time
            mainTrail.time = Mathf.Lerp(minTrailTime, maxTrailTime, currentIntensity);

            // Modulate trail width
            mainTrail.startWidth = Mathf.Lerp(minTrailWidth, maxTrailWidth, currentIntensity);

            // Update alpha based on intensity (unless boosting)
            if (!isBoosting)
            {
                float alpha = Mathf.Lerp(0.5f, 1.0f, currentIntensity);
                UpdateTrailColor(primaryCyan, alpha);
            }
        }
        #endregion

        #region Boost Integration
        /// <summary>
        /// Sets up the boost particle burst effect.
        /// </summary>
        private void SetupBoostParticles()
        {
            GameObject particlesGO = new GameObject("BoostParticles");
            particlesGO.transform.SetParent(transform);
            particlesGO.transform.localPosition = new Vector3(0f, 0f, -0.5f); // Behind motorcycle

            boostParticles = particlesGO.AddComponent<ParticleSystem>();

            var main = boostParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.2f;
            main.maxParticles = 50;
            main.loop = false;
            main.playOnAwake = false;

            var emission = boostParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 30)
            });

            var shape = boostParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.rotation = new Vector3(90f, 0f, 0f); // Point backward

            var colorOverLifetime = boostParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(hotPink, 0f),
                    new GradientColorKey(primaryCyan, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            // Create emissive material for particles
            var renderer = boostParticles.GetComponent<ParticleSystemRenderer>();
            Material particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
            particleMat.EnableKeyword("_EMISSION");
            particleMat.SetColor("_EmissionColor", hotPink * 2f);
            renderer.material = particleMat;
            materialInstances.Add(particleMat);

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Boost particles configured");
            }
        }

        /// <summary>
        /// Called when boost is activated.
        /// </summary>
        private void OnBoostActivated()
        {
            isBoosting = true;

            // Play particle burst
            if (boostParticles != null)
            {
                boostParticles.Play();
            }

            // Change trail to hot pink
            if (mainTrail != null)
            {
                UpdateTrailColor(hotPink, 1.0f);
            }

            // Camera shake
            if (cameraFollow != null)
            {
                if (currentShakeCoroutine != null)
                {
                    StopCoroutine(currentShakeCoroutine);
                }
                currentShakeCoroutine = StartCoroutine(BeatShake(0.3f, 0.8f));
            }

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Boost activated - visual effects triggered");
            }
        }

        /// <summary>
        /// Called when boost ends.
        /// </summary>
        private void OnBoostEnded()
        {
            isBoosting = false;

            // Restore trail to cyan
            if (mainTrail != null)
            {
                float alpha = Mathf.Lerp(0.5f, 1.0f, currentIntensity);
                UpdateTrailColor(primaryCyan, alpha);
            }

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Boost ended - trail restored");
            }
        }
        #endregion

        #region Beat Reactivity
        /// <summary>
        /// Detects beats during gameplay and triggers visual effects.
        /// </summary>
        private void DetectBeats()
        {
            if (analysisData == null || analysisData.Beats == null || analysisData.Beats.Count == 0)
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
        /// Called when a beat is detected.
        /// </summary>
        private void OnBeatDetected(BeatEvent beat)
        {
            // Only trigger effects on strong beats
            if (beat.Strength >= beatPulseThreshold)
            {
                // Glow pulse effect
                if (currentGlowPulseCoroutine != null)
                {
                    StopCoroutine(currentGlowPulseCoroutine);
                }
                currentGlowPulseCoroutine = StartCoroutine(GlowPulse(beat.Strength));

                // Camera shake on very strong beats
                if (beat.Strength >= beatShakeThreshold && cameraFollow != null)
                {
                    if (currentShakeCoroutine != null)
                    {
                        StopCoroutine(currentShakeCoroutine);
                    }
                    currentShakeCoroutine = StartCoroutine(BeatShake(0.15f, beat.Strength * 0.5f));
                }

                if (debugMode)
                {
                    Debug.Log($"MotorcycleVisualController: Beat detected (strength: {beat.Strength:F2})");
                }
            }
        }

        /// <summary>
        /// Pulses the glow intensity on all renderers.
        /// </summary>
        private IEnumerator GlowPulse(float strength)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            float pulseMultiplier = 2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentPulse = Mathf.Lerp(pulseMultiplier, 1f, t);

                // Apply pulse to all glow renderers
                foreach (Renderer renderer in glowRenderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        Color baseEmission = renderer.material.GetColor("_EmissionColor");
                        Color pulsedEmission = baseEmission * currentPulse;
                        renderer.material.SetColor("_EmissionColor", pulsedEmission);
                    }
                }

                yield return null;
            }

            // Restore original emission
            foreach (Renderer renderer in glowRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    // Restore based on material type
                    if (renderer.gameObject.name.Contains("Wheel"))
                    {
                        renderer.material.SetColor("_EmissionColor", accentBlue * 1.5f);
                    }
                    else if (renderer.gameObject.name.Contains("AccentLine"))
                    {
                        renderer.material.SetColor("_EmissionColor", primaryCyan * 3f);
                    }
                    else
                    {
                        renderer.material.SetColor("_EmissionColor", primaryCyan * 2f);
                    }
                }
            }
        }

        /// <summary>
        /// Triggers camera shake effect.
        /// </summary>
        private IEnumerator BeatShake(float duration, float intensity)
        {
            if (cameraFollow == null)
                yield break;

            float originalValue = originalShakeIntensity;
            cameraFollow.shakeIntensity = originalValue + intensity;

            yield return new WaitForSeconds(duration);

            cameraFollow.shakeIntensity = originalValue;
        }
        #endregion

        #region Speed Lines (Optional)
        /// <summary>
        /// Sets up the speed lines particle system.
        /// </summary>
        private void SetupSpeedLines()
        {
            GameObject speedLinesGO = new GameObject("SpeedLines");
            speedLinesGO.transform.SetParent(transform);
            speedLinesGO.transform.localPosition = new Vector3(0f, 0f, -1f);

            speedLinesParticles = speedLinesGO.AddComponent<ParticleSystem>();

            var main = speedLinesParticles.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.maxParticles = 100;
            main.loop = true;

            var emission = speedLinesParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f; // Controlled manually

            var shape = speedLinesParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var colorOverLifetime = speedLinesParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(primaryCyan, 0f),
                    new GradientColorKey(primaryCyan, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var renderer = speedLinesParticles.GetComponent<ParticleSystemRenderer>();
            Material speedMat = new Material(Shader.Find("Particles/Standard Unlit"));
            speedMat.SetColor("_Color", new Color(primaryCyan.r, primaryCyan.g, primaryCyan.b, 0.5f));
            renderer.material = speedMat;
            materialInstances.Add(speedMat);

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Speed lines configured");
            }
        }

        /// <summary>
        /// Updates speed lines emission based on motorcycle speed.
        /// </summary>
        private void UpdateSpeedLines()
        {
            if (motorcycle == null || speedLinesParticles == null)
                return;

            // Get normalized speed (0-1) from motorcycle
            float speedRatio = motorcycle.NormalizedSpeed;

            var emission = speedLinesParticles.emission;

            if (speedRatio >= speedLineThreshold)
            {
                // Map speed to emission rate (0-40 particles/sec)
                float normalizedSpeed = (speedRatio - speedLineThreshold) / (1f - speedLineThreshold);
                emission.rateOverTime = Mathf.Lerp(0f, 40f, normalizedSpeed);
            }
            else
            {
                emission.rateOverTime = 0f;
            }
        }
        #endregion

        #region Cleanup
        void OnDestroy()
        {
            // Unsubscribe from events
            if (boostSystem != null)
            {
                boostSystem.OnBoostActivated.RemoveListener(OnBoostActivated);
                boostSystem.OnBoostEnded.RemoveListener(OnBoostEnded);
            }

            // Destroy all material instances to prevent memory leaks
            foreach (Material mat in materialInstances)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
            materialInstances.Clear();

            if (debugMode)
            {
                Debug.Log("MotorcycleVisualController: Cleaned up materials");
            }
        }
        #endregion
    }
}
