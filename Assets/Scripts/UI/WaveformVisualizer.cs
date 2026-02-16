using UnityEngine;

namespace DesertRider.UI
{
    /// <summary>
    /// Visualizes audio waveform data in Unity's immediate mode GUI.
    /// Displays loaded MP3 waveform for debugging and analysis.
    /// </summary>
    public class WaveformVisualizer : MonoBehaviour
    {
        [Header("Waveform Data")]
        [Tooltip("Audio samples to visualize (-1.0 to 1.0)")]
        public float[] samples;

        [Tooltip("Sample rate of the audio (e.g., 44100 Hz)")]
        public int sampleRate = 44100;

        [Header("Display Settings")]
        [Tooltip("Screen position (x, y, width, height)")]
        public Rect displayRect = new Rect(10, 10, 800, 200);

        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [Tooltip("Waveform color")]
        public Color waveformColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        [Tooltip("Grid line color")]
        public Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Tooltip("Downsample factor (1 = all samples, 10 = every 10th sample)")]
        public int downsampleFactor = 100;

        private Texture2D backgroundTexture;
        private Texture2D waveformTexture;
        private GUIStyle labelStyle;

        void Start()
        {
            // Create textures for drawing
            backgroundTexture = MakeTex(2, 2, backgroundColor);
            waveformTexture = MakeTex(2, 2, waveformColor);

            // Create label style
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 12;
        }

        void OnGUI()
        {
            if (samples == null || samples.Length == 0)
            {
                // Draw placeholder
                GUI.Box(displayRect, "No waveform data loaded", labelStyle);
                return;
            }

            // Draw background
            GUI.DrawTexture(displayRect, backgroundTexture);

            // Draw grid lines
            DrawGrid();

            // Draw waveform
            DrawWaveform();

            // Draw info text
            DrawInfoText();
        }

        void DrawGrid()
        {
            // Center line
            float centerY = displayRect.y + displayRect.height / 2f;
            DrawLine(
                new Vector2(displayRect.x, centerY),
                new Vector2(displayRect.x + displayRect.width, centerY),
                gridColor
            );

            // Top/bottom lines
            DrawLine(
                new Vector2(displayRect.x, displayRect.y),
                new Vector2(displayRect.x + displayRect.width, displayRect.y),
                gridColor
            );
            DrawLine(
                new Vector2(displayRect.x, displayRect.y + displayRect.height),
                new Vector2(displayRect.x + displayRect.width, displayRect.y + displayRect.height),
                gridColor
            );
        }

        void DrawWaveform()
        {
            float width = displayRect.width;
            float height = displayRect.height;
            float centerY = displayRect.y + height / 2f;

            // Downsample for performance
            int step = Mathf.Max(1, samples.Length / (int)width);
            step = Mathf.Max(step, downsampleFactor);

            Vector2? previousPoint = null;

            for (int i = 0; i < samples.Length; i += step)
            {
                float normalizedX = (float)i / samples.Length;
                float x = displayRect.x + normalizedX * width;

                float sample = Mathf.Clamp(samples[i], -1f, 1f);
                float y = centerY - (sample * height / 2f);

                Vector2 currentPoint = new Vector2(x, y);

                if (previousPoint.HasValue)
                {
                    DrawLine(previousPoint.Value, currentPoint, waveformColor);
                }

                previousPoint = currentPoint;
            }
        }

        void DrawInfoText()
        {
            float duration = (float)samples.Length / sampleRate;

            string info = $"Samples: {samples.Length:N0} | Sample Rate: {sampleRate} Hz | Duration: {duration:F2}s";

            Vector2 infoPosition = new Vector2(displayRect.x + 5, displayRect.y + displayRect.height + 5);
            GUI.Label(new Rect(infoPosition.x, infoPosition.y, 600, 20), info, labelStyle);
        }

        void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            // Simple line drawing using GUI.DrawTexture with rotation
            Vector2 diff = end - start;
            float length = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(angle, start);
            GUI.color = color;
            GUI.DrawTexture(new Rect(start.x, start.y - 0.5f, length, 1f), waveformTexture);
            GUIUtility.RotateAroundPivot(-angle, start);
            GUI.color = Color.white;
        }

        Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Load waveform data from MP3LoadTest component
        /// </summary>
        public void LoadFromTest(float[] waveformData, int sampleRateHz)
        {
            samples = waveformData;
            sampleRate = sampleRateHz;
            Debug.Log($"WaveformVisualizer: Loaded {samples.Length} samples at {sampleRate} Hz");
        }
    }
}
