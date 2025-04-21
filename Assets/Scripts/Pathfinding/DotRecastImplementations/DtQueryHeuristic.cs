using DotRecast.Core.Numerics;
using DotRecast.Detour;

public class DtQueryHeuristic : IDtQueryHeuristic
{
    public float GetCost(RcVec3f neighbourPos, RcVec3f endPos)
    {
        return (neighbourPos - endPos).LengthSquared(); // simple euclidean heuristic
    }
}
