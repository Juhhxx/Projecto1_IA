using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Scripts.Fire;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Panic-specific query filter that penalizes paths near explosions and fire.
    /// Used when agents are in a panic state to simulate avoidance behavior.
    /// </summary>
    public class DtQueryPanicFilter : DtQueryFilter
    {
        // protected IDtQueryHeuristic _heuristic;

        /// <summary>
        /// Constructs a panic behavior query filter.
        /// </summary>
        /// <param name="explosionManager">Explosion manager that tracks active fires and explosions.</param>
        public DtQueryPanicFilter(ExplosionManager explosionManager) : base(explosionManager)
        {
            // _heuristic = new DtQueryPanicHeuristic();
        }

        /// <summary>
        /// Computes movement cost, heavily penalizing proximity to explosions and fire.
        /// </summary>
        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            float value = RcVec3f.Distance(pa, pb);

            float expl = RcVec3f.Distance(_explosion.LatestExplosion, pa);

            // If the explosion is closer than the target and within 150 units
            if (expl > value && expl < 150f)
                // Stronger penalty the closer the point is to explosion center
                expl = (150f - expl) * 2f;
            else
                expl = 0;
            
            if ( _explosion.PolyHasFire(curRef) )
                value *= 10;
            
            return value + expl;
        }
    }
}