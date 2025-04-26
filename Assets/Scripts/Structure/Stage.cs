using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

namespace Scripts.Structure
{
    public class Stage : Structure<Stage>
    {
        [field:SerializeField] private Renderer _area;
        [SerializeField] private float _spacing = 3f;
        protected override void SetUpPoints()
        {
            Vector3 forward = transform.position + transform.forward * transform.localScale.z / 2f;
            Vector3 rightOffset = transform.right * transform.localScale.x / 2f;

            forward.z -= 1.2f;

            Vector3 edgeA = forward + rightOffset;
            Vector3 edgeB = forward - rightOffset;

            Debug.DrawLine(edgeA, edgeB, Color.yellow, float.MaxValue);

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

            foreach ( (RcVec3f, long) pos in samplePoints )
                _positions.Add( DRcHandle.ToUnityVec3(pos.Item1) );
        }
    }
}