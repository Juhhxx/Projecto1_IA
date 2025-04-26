using System.Collections.Generic;
using DotRecast.Core.Numerics;
using Scripts.Pathfinding;
using UnityEngine;

public abstract class Structure<T> : MonoBehaviour where T : Structure<T>
{
    [SerializeField] private float _radius = 10f;
    [SerializeField] private Transform _pivot;
    [SerializeField] private int _tooManyAgents = 5;

    // one structure list per subclass
    protected static List<T> _structures = new List<T>();
    protected (RcVec3f, long)[] _places;
    private Dictionary<long, int> _placeDict;

    public RcVec3f Position { get; private set; }
    public long Ref { get; private set; }

    public static void AwakeOrdered()
    {
        foreach (T structure in _structures)
            structure.StartStructure();
    }

    private void Awake()
    {
        if (this is T type)
            _structures.Add(type);
    }
    private void StartStructure()
    {
        _positions = new HashSet<Vector3>();
        _placeDict = new Dictionary<long, int>();

        DRcHandle.FindNearest(_pivot.position, out long nearestRef, out RcVec3f nearestPt, out _);
        Ref = nearestRef;
        Position = nearestPt;

        SetUpPoints();

        foreach ( (RcVec3f, long) y in _places )
            _placeDict[y.Item2] = 0;
    }

    public static T FindNearest(RcVec3f pos)
    {
        if (_structures.Count == 0)
        {
            Debug.LogWarning("No structures of this type yet. ");
            return null;
        }

        T chosen = _structures[0];
        float minDist = RcVec3f.Distance(pos, chosen.Position);
        float dist;

        foreach (T structure in _structures)
        {
            dist = RcVec3f.Distance(pos, structure.Position);
            if (dist < minDist)
            {
                minDist = dist;
                chosen = structure;
            }
        }

        return chosen;
    }

    protected abstract void SetUpPoints();

    public bool EnteredArea(RcVec3f pos)
    {
        Debug.Log("Entered area? dis: " + RcVec3f.Distance(pos, Position) + " rad " + _radius);
        return RcVec3f.Distance(pos, Position) < _radius;
    }

    /// <summary>
    /// check if a near random place from _places is good spot until it chooses one
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public (RcVec3f, long) GetBestSpot(RcVec3f pos, int random)
    {
        Debug.DrawLine(DRcHandle.ToUnityVec3(pos), DRcHandle.ToUnityVec3(_places[ random % _places.Length ].Item1));
        return _places[ random % _places.Length ];
        /*int len = _places.Length/2;
        int start = random % len;

        (RcVec3f, long) fallback = _places[0];
        float minDist = RcVec3f.Distance(pos, fallback.Item1);

        int index;
        (RcVec3f, long) next;

        for (int i = 0; i < len; i++)
        {
            index = (start + i * 73) % len;
            next = _places[index];

            if ( IsGoodSpot(next.Item2) )
                return next;

            float dist = RcVec3f.Distance(pos, next.Item1);
            if (dist < minDist)
            {
                fallback = next;
                minDist = dist;
            }
        }

        return fallback;*/
    }

    public void StayInSpot(long placeRef) => _placeDict[placeRef]++;
    public void LeaveSpot(long placeRef) => _placeDict[placeRef]--;

    /// <summary>
    /// Check crowd to see if number of agents in tile is or poly is above _tooManyAgents
    /// </summary>
    /// <param name="polyId"> The poly reference </param>
    /// <returns> If its a good spot </returns>
    public bool IsGoodSpot(long polyRef)
    {
        return _placeDict.TryGetValue(polyRef, out int count) && count < _tooManyAgents;
    }


    protected HashSet<Vector3> _positions;

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(_pivot.position, _radius);

        if ( _positions != null )
            foreach (Vector3 pos in _positions)
                Gizmos.DrawSphere(pos, 1f);
    }
    #endif

    public override string ToString()
    {
        return $"Structure {gameObject.name}";
    }
}
