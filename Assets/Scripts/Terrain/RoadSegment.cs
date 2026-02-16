using UnityEngine;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Represents a single segment of procedurally generated road.
    /// Handles mesh generation with configurable width, length, and height modulation.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadSegment : MonoBehaviour
    {
        [Header("Segment Dimensions")]
        [Tooltip("Length of this road segment in world units")]
        public float segmentLength = 20f;

        [Tooltip("Width of the road in world units")]
        public float roadWidth = 10f;

        [Tooltip("Number of subdivisions along the length (more = smoother curves)")]
        public int lengthSubdivisions = 10;

        [Tooltip("Number of subdivisions across the width")]
        public int widthSubdivisions = 5;

        [Header("Height Modulation")]
        [Tooltip("Base height of the road segment")]
        public float baseHeight = 0f;

        [Tooltip("Height variation based on intensity (0-1)")]
        public float heightIntensity = 5f;

        [Header("Curve Parameters")]
        [Tooltip("Horizontal curve amount (-1 to 1, negative = left, positive = right)")]
        public float curveAmount = 0f;

        [Tooltip("Curve intensity multiplier")]
        public float curveIntensity = 5f;

        [Header("Enhanced Features")]
        [Tooltip("Enable road banking on curves")]
        public bool enableBanking = false;

        [Tooltip("Banking angle in degrees")]
        public float bankingAngle = 0f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private RampFeature rampFeature;

        /// <summary>
        /// Gets the world-space end position of this segment.
        /// </summary>
        public Vector3 EndPosition => transform.position + transform.forward * segmentLength;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Generates the road mesh with specified parameters.
        /// </summary>
        /// <param name="intensityValues">Array of intensity values (0-1) for height modulation.</param>
        /// <param name="curveValue">Curve amount for this segment (-1 to 1).</param>
        /// <param name="previousEndHeights">Heights from the end of the previous segment for seamless connection.</param>
        /// <returns>Heights at the end of this segment for the next segment.</returns>
        public float[] GenerateMesh(float[] intensityValues = null, float curveValue = 0f, float[] previousEndHeights = null)
        {
            curveAmount = curveValue;

            // Create new mesh
            mesh = new Mesh();
            mesh.name = $"RoadSegment_{gameObject.GetInstanceID()}";

            // Calculate vertex count
            int vertsX = widthSubdivisions + 1;
            int vertsZ = lengthSubdivisions + 1;
            int vertCount = vertsX * vertsZ;

            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[widthSubdivisions * lengthSubdivisions * 6];

            // Array to store end heights for next segment
            float[] endHeights = new float[vertsX];

            // Generate vertices
            for (int z = 0; z < vertsZ; z++)
            {
                float zPos = (float)z / lengthSubdivisions * segmentLength;
                float zNormalized = (float)z / lengthSubdivisions;

                // Get intensity for this z position
                float intensity = 0f;
                if (intensityValues != null && intensityValues.Length > 0)
                {
                    int intensityIndex = Mathf.FloorToInt(zNormalized * (intensityValues.Length - 1));
                    intensityIndex = Mathf.Clamp(intensityIndex, 0, intensityValues.Length - 1);
                    intensity = intensityValues[intensityIndex];
                }

                // Calculate height from intensity
                float height = baseHeight + intensity * heightIntensity;

                // Apply ramp feature if present
                rampFeature = GetComponent<RampFeature>();
                if (rampFeature != null)
                {
                    float rampHeightMod = rampFeature.GetHeightModificationAt(zNormalized);
                    height += rampHeightMod;
                }

                // Calculate curve offset (more curve as we go further along segment)
                float curveOffset = curveAmount * curveIntensity * zNormalized * zNormalized;

                for (int x = 0; x < vertsX; x++)
                {
                    float xPos = ((float)x / widthSubdivisions - 0.5f) * roadWidth;

                    // Use previous segment's end height for seamless connection at z=0
                    float finalHeight = height;
                    if (z == 0 && previousEndHeights != null && x < previousEndHeights.Length)
                    {
                        finalHeight = previousEndHeights[x];
                    }

                    // Apply banking to vertex positions (rotate around forward axis)
                    if (enableBanking && Mathf.Abs(curveAmount) > 0.01f)
                    {
                        float bankAngle = curveAmount * bankingAngle * Mathf.Deg2Rad;

                        // Banking creates height variation across width
                        float bankHeightOffset = Mathf.Sin(bankAngle) * xPos;
                        finalHeight += bankHeightOffset;
                    }

                    // Store end heights for next segment
                    if (z == vertsZ - 1)
                    {
                        endHeights[x] = height;
                    }

                    int vertIndex = z * vertsX + x;
                    vertices[vertIndex] = new Vector3(xPos + curveOffset, finalHeight, zPos);
                    uvs[vertIndex] = new Vector2((float)x / widthSubdivisions, zNormalized);
                }
            }

            // Generate triangles
            int triIndex = 0;
            for (int z = 0; z < lengthSubdivisions; z++)
            {
                for (int x = 0; x < widthSubdivisions; x++)
                {
                    int bottomLeft = z * vertsX + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = (z + 1) * vertsX + x;
                    int topRight = topLeft + 1;

                    // First triangle
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomRight;

                    // Second triangle
                    triangles[triIndex++] = bottomRight;
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = topRight;
                }
            }

            // Assign mesh data
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // Recalculate normals for lighting
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            // WORKAROUND: Use BoxCollider instead of MeshCollider (Unity 6 MeshCollider bug)
            // Remove any existing MeshCollider
            MeshCollider oldMeshCollider = GetComponent<MeshCollider>();
            if (oldMeshCollider != null)
            {
                Destroy(oldMeshCollider);
            }

            // Add/update BoxCollider for collision detection
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
                Debug.Log($"RoadSegment: Added BoxCollider to {gameObject.name}");
            }

            // Set BoxCollider to match mesh bounds
            Bounds meshBounds = mesh.bounds;
            boxCollider.center = meshBounds.center;

            // CRITICAL FIX: Ensure BoxCollider has minimum thickness in Y direction
            // Flat colliders (Y=0) can miss raycasts due to floating-point precision
            Vector3 size = meshBounds.size;
            size.y = Mathf.Max(size.y, 1.0f); // Minimum 1 meter thickness
            boxCollider.size = size;

            // Ensure GameObject is on Default layer (0) for collision detection
            gameObject.layer = 0;

            // CRITICAL FIX: Force physics system to register this collider immediately
            Physics.SyncTransforms();

            // CRITICAL DEBUG: Verify BoxCollider is actually working
            bool colliderEnabled = boxCollider.enabled;
            bool goActive = gameObject.activeInHierarchy;

            Debug.Log($"RoadSegment {gameObject.name}: BoxCollider VERIFICATION - " +
                $"Center: {boxCollider.center}, Size: {boxCollider.size}, " +
                $"ColliderEnabled: {colliderEnabled}, GameObjectActive: {goActive}, " +
                $"Layer: {gameObject.layer}, Bounds: {boxCollider.bounds}");

            // Return end heights for next segment
            return endHeights;
        }

        /// <summary>
        /// Updates the segment's material.
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
        }

        /// <summary>
        /// Sets material color for visual variety.
        /// </summary>
        /// <param name="color">Color to apply to the material</param>
        public void SetMaterialColor(Color color)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                // Create material instance to avoid affecting other segments
                renderer.material = new Material(renderer.material);
                renderer.material.color = color;
            }
        }

        void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
        }

        /// <summary>
        /// Gets world position for a normalized position on this segment.
        /// </summary>
        /// <param name="zNormalized">Normalized Z position (0 = start, 1 = end)</param>
        /// <param name="xOffset">Lateral offset from center (negative = left, positive = right)</param>
        /// <returns>World position on this segment</returns>
        public Vector3 GetPositionOnSegment(float zNormalized, float xOffset)
        {
            float zLocal = zNormalized * segmentLength;

            // Sample height at this position using raycast
            float height = SampleHeightAtPosition(zNormalized);

            Vector3 localPos = new Vector3(xOffset, height + 1.5f, zLocal);
            return transform.TransformPoint(localPos);
        }

        /// <summary>
        /// Samples the height of the road at a normalized position.
        /// </summary>
        private float SampleHeightAtPosition(float zNormalized)
        {
            // Cast ray from above to find road surface
            Vector3 testPos = transform.TransformPoint(new Vector3(0, 100f, zNormalized * segmentLength));

            RaycastHit hit;
            if (Physics.Raycast(testPos, Vector3.down, out hit, 200f))
            {
                // Convert hit point back to local space to get height
                return transform.InverseTransformPoint(hit.point).y;
            }

            // Fallback to base height if raycast fails
            return baseHeight;
        }

        /// <summary>
        /// Visualizes the segment bounds in Scene view.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 start = transform.position;
            Vector3 end = EndPosition;

            // Draw segment bounds
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireCube((start + end) / 2f, new Vector3(roadWidth, 0.1f, segmentLength));
        }
    }
}
