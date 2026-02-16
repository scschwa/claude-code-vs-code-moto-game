using UnityEngine;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Creates glowing edges along road segments for dramatic visual effect.
    /// Instantiates emissive edge lights at segment creation.
    /// </summary>
    public class RoadEdgeRenderer : MonoBehaviour
    {
        [Header("Edge Configuration")]
        [Tooltip("Enable glowing road edges")]
        public bool enableEdges = true;

        [Tooltip("Edge glow color")]
        public Color edgeColor = new Color(1f, 0.8f, 0f); // Gold/yellow

        [Tooltip("Edge emission intensity")]
        [Range(0f, 5f)]
        public float emissionIntensity = 2.5f;

        [Tooltip("Edge width")]
        public float edgeWidth = 0.2f;

        [Tooltip("Edge height above road surface")]
        public float edgeHeight = 0.05f;

        private Material edgeMaterial;

        void Awake()
        {
            CreateEdgeMaterial();
        }

        /// <summary>
        /// Creates emissive material for road edges with HDR emission for bloom.
        /// </summary>
        private void CreateEdgeMaterial()
        {
            edgeMaterial = new Material(Shader.Find("Standard"));

            // Base color (very dark, almost black)
            edgeMaterial.SetColor("_Color", Color.black);

            // HDR Emission for intense bloom glow (values > 1.0 create bloom with post-processing)
            edgeMaterial.EnableKeyword("_EMISSION");
            Color hdrEmission = edgeColor * emissionIntensity * 3f; // Multiply by 3 for HDR intensity
            edgeMaterial.SetColor("_EmissionColor", hdrEmission);
            edgeMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Material properties optimized for glowing neon look
            edgeMaterial.SetFloat("_Metallic", 0f); // Non-metallic for pure emission
            edgeMaterial.SetFloat("_Glossiness", 1f); // Max glossiness for sharp highlights

            Debug.Log($"RoadEdgeRenderer: Created HDR edge material - emission RGB: ({hdrEmission.r:F1}, {hdrEmission.g:F1}, {hdrEmission.b:F1})");
        }

        /// <summary>
        /// Adds glowing edges to a road segment.
        /// </summary>
        public void AddEdgesToSegment(GameObject segmentObj, float segmentLength, float roadWidth)
        {
            if (!enableEdges || segmentObj == null)
                return;

            float halfWidth = roadWidth / 2f;

            // Left edge
            CreateEdgeLine(segmentObj.transform, new Vector3(-halfWidth, edgeHeight, 0f), segmentLength, "EdgeLeft");

            // Right edge
            CreateEdgeLine(segmentObj.transform, new Vector3(halfWidth, edgeHeight, 0f), segmentLength, "EdgeRight");
        }

        /// <summary>
        /// Creates a single glowing edge line.
        /// </summary>
        private void CreateEdgeLine(Transform parent, Vector3 startPosition, float length, string name)
        {
            // Create edge object (thin cube stretched along Z axis)
            GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = name;
            edge.transform.SetParent(parent);
            edge.transform.localPosition = startPosition + new Vector3(0f, 0f, length / 2f);
            edge.transform.localRotation = Quaternion.identity;
            edge.transform.localScale = new Vector3(edgeWidth, edgeHeight * 2f, length);

            // Remove collider (edges are purely visual)
            Collider edgeCollider = edge.GetComponent<Collider>();
            if (edgeCollider != null)
                Destroy(edgeCollider);

            // Apply emissive material
            MeshRenderer renderer = edge.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = edgeMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        void OnDestroy()
        {
            // Cleanup material instance
            if (edgeMaterial != null)
                Destroy(edgeMaterial);
        }
    }
}
