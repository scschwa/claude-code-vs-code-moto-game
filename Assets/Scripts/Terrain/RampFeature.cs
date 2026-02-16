using UnityEngine;

namespace DesertRider.Terrain
{
    /// <summary>
    /// Component attached to road segments that have ramps/jumps.
    /// Calculates height modifications at specific positions along the segment.
    /// </summary>
    public class RampFeature : MonoBehaviour
    {
        [Header("Ramp Configuration")]
        [Tooltip("Normalized position where ramp starts (0-1)")]
        public float rampStartZ = 0.3f;

        [Tooltip("Normalized position where ramp ends (0-1)")]
        public float rampEndZ = 0.7f;

        [Tooltip("Maximum height of ramp")]
        public float rampHeight = 3f;

        [Tooltip("Ramp profile curve")]
        public AnimationCurve rampCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Gets height modification at normalized position along segment.
        /// </summary>
        /// <param name="zNormalized">Normalized Z position along segment (0 = start, 1 = end)</param>
        /// <returns>Height offset to apply at this position</returns>
        public float GetHeightModificationAt(float zNormalized)
        {
            // Only apply within ramp range
            if (zNormalized < rampStartZ || zNormalized > rampEndZ)
                return 0f;

            // Calculate progress along ramp (0 at start, 1 at end)
            float rampProgress = (zNormalized - rampStartZ) / (rampEndZ - rampStartZ);

            // Evaluate curve and apply height
            float curveValue = rampCurve.Evaluate(rampProgress);
            return curveValue * rampHeight;
        }

        /// <summary>
        /// Visualizes ramp in Scene view for debugging.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Visualize ramp in scene view
            RoadSegment segment = GetComponent<RoadSegment>();
            if (segment != null)
            {
                Gizmos.color = Color.yellow;

                float segmentLength = segment.segmentLength;
                Vector3 startPos = transform.position + Vector3.forward * (rampStartZ * segmentLength);
                Vector3 endPos = transform.position + Vector3.forward * (rampEndZ * segmentLength);

                startPos.y += GetHeightModificationAt(rampStartZ);
                endPos.y += GetHeightModificationAt(rampEndZ);

                Gizmos.DrawLine(startPos, endPos);
                Gizmos.DrawWireSphere(endPos, 1f); // Mark jump takeoff point
            }
        }
    }
}
