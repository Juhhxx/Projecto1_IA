using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    /// <summary>
    /// Stage is a concert hall, so points should be set at the front of it.
    /// </summary>
    public class Stage : Structure<Stage>
    {
        [SerializeField] private float _spacing = 3f;

        /// <summary>
        /// Stage sets up its points in a line in its forward direction.
        /// </summary>
        protected override void SetUpPoints()
        {
            // Find the center of the front edge (halfway along the local Z axis, forward direction)
            Vector3 forward = transform.position + transform.forward * transform.localScale.z / 2f;
            Vector3 rightOffset = transform.right * transform.localScale.x / 2f;

            forward.z -= Mathf.Sign(forward.z) * 4f * _tooManyAgents;

            // start and end of the line to get points at. In world position.
            Vector3 edgeA = forward + rightOffset;
            Vector3 edgeB = forward - rightOffset;

            Debug.DrawLine(edgeA, edgeB, Color.yellow, float.MaxValue);

            // Spacing, space between each sample, sets the amount of samples aloud 
            int samples = Mathf.Max(1, Mathf.RoundToInt(Vector3.Distance(edgeA, edgeB) / _spacing));

            HashSet<(RcVec3f, long)> samplePoints = new();

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 newPos = Vector3.Lerp(edgeA, edgeB, t);

                if ( DRcHandle.FindNearest( newPos , out long polyRef, out RcVec3f polyPos, out _ ).Succeeded() &&  polyRef  != 0 )
                    samplePoints.Add((polyPos, polyRef));
            }

            _places = samplePoints.ToArray();
        }
    }
}