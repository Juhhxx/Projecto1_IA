using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Scripts.Fire;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Here you have to assign which places are walkable or not, so for example here you would assign that agents cant walk in places with fire etc
    /// </summary>
    public class DtQueryPanicFilter : DtQueryFilter
    {
        protected IDtQueryHeuristic _heuristic;
        public DtQueryPanicFilter(ExplosionManager explosionManager) : base(explosionManager)
        {
            _heuristic = new DtQueryPanicHeuristic();
        }
        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            float value = RcVec3f.Distance(pa, pb);

            float expl = RcVec3f.Distance(_explosion.LatestExplosion, pa);

            if ( expl > value && expl < 120f ) // only avoid explosion if objective is farther and is near it
                expl = (120f - expl) * 2f; // stronger penalty the closer it is
            
            if ( _explosion.PolyHasFire(curRef) )
                value *= 10;
            
            return value + expl;
        }
    }
}