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

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;

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

            // Add/update MeshCollider for collision detection
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false; // Better for static terrain

            // Ensure GameObject is on Default layer (0) for collision detection
            gameObject.layer = 0;

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

        void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
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
