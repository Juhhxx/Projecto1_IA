using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Scripts.Fire;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Query filter for customizing navigation mesh behavior.
    /// Decides which areas are walkable and how much it costs to move through them.
    /// Avoid areas with fire, prefer safer paths.
    /// </summary>
    public class DtQueryFilter : IDtQueryFilter
    {
        protected ExplosionManager _explosion;

        /// <summary>
        /// Constructs a basic query filter.
        /// </summary>
        /// <param name="explosionManager">ExplosionManager that tracks fire or hazard areas.</param>
        public DtQueryFilter(ExplosionManager explosionManager) : base()
        {
            _explosion = explosionManager;
        }

        /// <summary>
        /// Computes the cost of moving from one point to another.
        /// Currently returns distance squared.
        /// </summary>
        /// <param name="pa">Current agent position.</param>
        /// <param name="pb">Next point being considered.</param>
        /// <param name="prevRef">Previous polygon reference.</param>
        /// <param name="prevTile">Previous navmesh tile.</param>
        /// <param name="prevPoly">Previous polygon.</param>
        /// <param name="curRef">Current polygon reference.</param>
        /// <param name="curTile">Current navmesh tile.</param>
        /// <param name="curPoly">Current polygon.</param>
        /// <param name="nextRef">Next polygon reference.</param>
        /// <param name="nextTile">Next navmesh tile.</param>
        /// <param name="nextPoly">Next polygon.</param>
        /// <returns>The cost of moving from pa to pb, based on distance.</returns>
        public virtual float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            return (pa - pb).LengthSquared(); // basic cost in distance, perhaps we should have a mono behavior child of the interface of DtMeshTile so we manually set these costs?

            // for patterns, shouldnt the lpa dstar.. etc pathfinders have saved paths and
            // form preferences on their own or do we apply weighted preferences to places that are more open etc, and then remove that weight when they enter panic?
            // if we remove and add weight, we need to have one query for normal behavior and another for panic behavior
        }

        /// <summary>
        /// Determines whether a polygon is walkable.
        /// </summary>
        /// <param name="refs">Polygon reference ID.</param>
        /// <param name="tile">Tile that contains the polygon.</param>
        /// <param name="poly">Polygon to check.</param>
        /// <returns>True if the polygon can be walked on; otherwise, false.</returns>
        public bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return poly.GetPolyType() == DtPolyTypes.DT_POLYTYPE_GROUND;
        }
    }
}