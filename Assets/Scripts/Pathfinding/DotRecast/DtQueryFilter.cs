using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Here you have to assign which places are walkable or not, so for example here you would assign that agents cant walk in places with fire etc
    /// </summary>
    public class DtQueryFilter : IDtQueryFilter
    {
        public float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            return (pa - pb).LengthSquared(); // basic cost in distance, perhaps we should have a mono behavior child of the interface of DtMeshTile so we manually set these costs?

            // for patterns, shouldnt the lpa dstar.. etc pathfinders have saved paths and
            // form preferences on their own or do we apply weighted preferences to places that are more open etc, and then remove that weight when they enter panic?
            // if we remove and add weight, we need to have one query for normal behavior and another for panic behavior
        }

        public bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return poly.GetPolyType() == DtPolyTypes.DT_POLYTYPE_GROUND;
        }
    }
}