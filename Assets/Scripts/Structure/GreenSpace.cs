using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    /// <summary>
    /// GreenSpace is an open area, points are set within it.
    /// </summary>
    public class GreenSpace : Structure<GreenSpace>
    {
        [SerializeField] private float _spacing = 8f;

        /// <summary>
        /// Sets up points inside the area of the green space.
        /// </summary>
        protected override void SetUpPoints()
        {
            // Calculate the size of the GreenSpace based on its lossy (world) scale.
            // Multiplying by 10 assumes a base 10x10 area scaled by the object's transform.
            Vector3 size = new Vector3(10 * transform.lossyScale.x, 0, 10 * transform.lossyScale.z);

            // Determine how many points fit horizontally (X) and vertically (Z) based on spacing
            int stepsX = Mathf.FloorToInt(size.x / _spacing);
            int stepsZ = Mathf.FloorToInt(size.z / _spacing);

            Vector3 start =
                transform.position - (transform.right * (stepsX * _spacing * 0.5f))
                    - (transform.forward * (stepsZ * _spacing * 0.5f));

            HashSet<(RcVec3f, long)> samplePoints = new();

            Vector3 worldPos;

            for (int x = 0; x <= stepsX; x++)
            {
                for (int z = 0; z <= stepsZ; z++)
                {
                    // The point in world position to be used by navmesh
                    worldPos = start
                        + (transform.right * (x * _spacing))
                            + (transform.forward * (z * _spacing));

                    if ( DRcHandle.FindNearest(worldPos, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                        samplePoints.Add((polyPos, polyRef));
                }
            }

            _places = samplePoints.ToArray();
        }
    }
}