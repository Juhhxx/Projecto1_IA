using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Regular heuristic for navigation queries.
    /// Calculates cost based on squared Euclidean distance between a neighbor and the goal.
    /// </summary>
    public class DtQueryRegularHeuristic : IDtQueryHeuristic
    {
        public float GetCost(RcVec3f neighbourPos, RcVec3f endPos)
        {
            return (neighbourPos - endPos).LengthSquared(); // simple euclidean heuristic
        }
    }
}