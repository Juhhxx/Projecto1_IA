using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Scripts.Fire;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Regular query filter.
    /// Agents avoid fire areas by increasing the cost but otherwise treat all ground normally.
    /// Area types can later be weighted using the _areaCost array.
    /// </summary>
    public class DtQueryRegularFilter : DtQueryFilter
    {
        private readonly float[] _areaCost = new float[10];

        /// <summary>
        /// Constructs a regular behavior query filter.
        /// Initializes all area types with a default cost of 1.
        /// </summary>
        /// <param name="explosionManager">Explosion manager for detecting fire zones.</param>
        public DtQueryRegularFilter(ExplosionManager explosionManager) : base(explosionManager)
        {
            for (int i = 0; i < 10; i++)
                _areaCost[i] = i+1f;
        }

        /// <summary>
        /// Computes movement cost based on distance, polygon area type, and fire presence.
        /// </summary>
        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            float value = RcVec3f.Distance(pa, pb) * _areaCost[ curPoly.GetArea() ]; // need to assign area weights later through this
            
            // if ( _explosion.PolyHasFire(curRef) )
            //     value *= 10;
            //if ( curPoly.GetArea() != 0 ) Debug.Log(" poly area is " + curPoly.GetArea() );
            
            return value; // basic cost in distance, perhaps we should have a mono behavior child of the interface of DtMeshTile so we manually set these costs?
        }
    }
}