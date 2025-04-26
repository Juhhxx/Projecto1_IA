using DotRecast.Core.Numerics;
using DotRecast.Detour;

namespace Scripts.Pathfinding.DotRecast
{
    /// <summary>
    /// Panic-specific heuristic for navigation queries.
    /// Encourages agents to move away from fire while moving toward their destination.
    /// </summary>
    public class DtQueryPanicHeuristic : IDtQueryHeuristic
    {
        private RcVec3f _firePos;
        public float GetCost(RcVec3f neighbourPos, RcVec3f endPos)
        {
            return (_firePos - neighbourPos).LengthSquared() - (endPos - neighbourPos).LengthSquared(); // simple euclidean heuristic
        }
    }
}