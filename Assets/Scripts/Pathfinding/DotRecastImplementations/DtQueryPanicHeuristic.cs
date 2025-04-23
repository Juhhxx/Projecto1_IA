using DotRecast.Core.Numerics;
using DotRecast.Detour;

public class DtQueryPanicHeuristic : IDtQueryHeuristic
{
    private RcVec3f _firePos;
    public DtQueryPanicHeuristic (RcVec3f fireSource) : base()
    {
        _firePos = fireSource;
    }
    public float GetCost(RcVec3f neighbourPos, RcVec3f endPos)
    {
        return (_firePos - neighbourPos).LengthSquared() - (endPos - neighbourPos).LengthSquared(); // simple euclidean heuristic
    }
}
