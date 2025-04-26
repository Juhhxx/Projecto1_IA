using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    public class GreenSpace : Structure<GreenSpace>
    {
        [field:SerializeField] private Renderer _area;
        [SerializeField] private float _spacing = 8f;
        protected override void SetUpPoints()
        {
            Vector3 size = new Vector3(10 * transform.lossyScale.x, 0, 10 * transform.lossyScale.z);

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
                    worldPos = start
                        + (transform.right * (x * _spacing))
                            + (transform.forward * (z * _spacing));

                    if ( DRcHandle.FindNearest(worldPos, out long polyRef, out RcVec3f polyPos, out _).Succeeded() &&  polyRef  != 0 )
                        samplePoints.Add((polyPos, polyRef));
                }
            }

            _places = samplePoints.ToArray();

            foreach ((RcVec3f, long) pos in samplePoints)
                _positions.Add(DRcHandle.ToUnityVec3(pos.Item1));
        }
    }
}