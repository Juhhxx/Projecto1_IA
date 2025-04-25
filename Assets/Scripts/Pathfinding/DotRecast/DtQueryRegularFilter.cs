using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Scripts.Fire;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Here you have to assign which places are walkable or not, so for example here you would assign that agents cant walk in places with fire etc
    /// </summary>
    public class DtQueryRegularFilter : DtQueryFilter
    {
        private readonly float[] _areaCost = new float[10];
        public DtQueryRegularFilter(ExplosionManager explosionManager) : base(explosionManager)
        {
            for (int i = 0; i < 10; i++)
                _areaCost[i] = 1f;
        }
        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            float value = RcVec3f.Distance(pa, pb) * _areaCost[ curPoly.GetArea() ]; // need to assign area weights later through this
            
            if ( _explosion.PolyHasFire(curRef) )
                value *= 10;
            
            return value; // basic cost in distance, perhaps we should have a mono behavior child of the interface of DtMeshTile so we manually set these costs?
        }
    }
}