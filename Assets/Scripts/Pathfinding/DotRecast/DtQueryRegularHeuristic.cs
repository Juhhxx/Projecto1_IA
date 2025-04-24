using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace Scripts.Pathfinding.DotRecast
{
    public class DtQueryRegularHeuristic : IDtQueryHeuristic
    {
        public float GetCost(RcVec3f neighbourPos, RcVec3f endPos)
        {
            return (neighbourPos - endPos).LengthSquared(); // simple euclidean heuristic
        }
    }
}