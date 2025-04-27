using System.Collections.Generic;
using System.Linq;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using UnityEngine;
using System.Collections;
using DotRecast.Core;
using Scripts.Pathfinding;
using Scripts.Random;
using UnityEditor;
using UnityEngine.Profiling;

namespace Scripts.Fire
{
    /// <summary>
    /// Manages fire propagation, explosion effects, and coordination with the crowd system.
    /// </summary>
    public class ExplosionManager : Manager
    {
        [SerializeField] private DRcHandle _handle;
        [SerializeField] private DRCrowdManager _crowd;
        [SerializeField] private GameObject _firePrefab;
        [SerializeField] private GameObject _explosionObject;
        [SerializeField] private float _deathRadius = 20f, _fearRadius = 30f, _panicRadius = 40f;
        [SerializeField] private Vector2 _radiusDeviation = Vector2.one;
        [SerializeField] private float _fireChance = 0.5f;
        [SerializeField] private int _FirePerUpdateBatch = 2;
        [SerializeField] private int _runEveryframe = 2;
        [SerializeField] private float _explosionChancePerFrame = 0.001f;

        private static Dictionary<long, Fire> _firePolys = new();
        [SerializeField] private List<Fire> _firePolyList = new();
        private IList<Fire> _activeFires;
        private IList<Fire> _inactiveFires;
        public RcVec3f LatestExplosion { get; private set; }
        public static int PolyNum { get; private set; }

        
        public ISeedRandom Rand { get; private set; }

        /// <summary>
        /// Initializes fire pools and poly references.
        /// </summary>
        internal protected override void AwakeOrdered()
        {
            _activeFires = new List<Fire>();
            _inactiveFires = _firePolyList.ToList();

            PolyNum = _firePolyList.Count;

            if (  _handle == null )
                _handle = FindFirstObjectByType<DRcHandle>();
            if ( _crowd == null )
                _crowd = FindFirstObjectByType<DRCrowdManager>();

            Rand = new RcSeedRandom(gameObject);

            Debug.Log("Poly list count: " + _firePolyList.Count);
            foreach (Fire fire in _firePolyList)
            {
                _firePolys[fire.PolyRef] = fire;
                // Debug.Log( "new fire go: " + _firePolys[fire.PolyRef] );
            }
            Debug.Log("Poly dict count: " + _firePolys.Count);
        }

        internal protected override void StartOrdered(){}

        #if UNITY_EDITOR
        /// <summary>
        /// Editor only method to bake fire objects on all navmesh polys.
        /// </summary>
        internal protected override void Bake()
        {
            _firePolys = new Dictionary<long, Fire>();
            _firePolyList = new List<Fire>();

            /*Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Bake All Fire Objects");
            int group = Undo.GetCurrentGroup();

            Fire[] fireChildren = GetComponentsInChildren<Fire>(true); // true for include inactive

            foreach(Fire fire in fireChildren)
                Undo.DestroyObjectImmediate(fire.gameObject);*/

            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0));

            Debug.Log("Init tile count: " + _handle.NavMeshData.GetTileCount());

            for (int tileIndex = 0; tileIndex < _handle.NavMeshData.GetTileCount(); tileIndex++)
            {
                DtMeshTile tile = _handle.NavMeshData.GetTile(tileIndex);
                if (tile == null || tile.data == null) continue;

                for (int polyIndex = 0; polyIndex < tile.data.header.polyCount; polyIndex++)
                {
                    long polyRef = _handle.NavMeshData.GetPolyRefBase(tile) | (uint)polyIndex;
                    NewFirePoly(polyRef);
                }
            }

            Debug.Log("Final poly count: " + _firePolyList.Count);

            // Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// Instantiates a new Fire object and sets up its neighbor references.
        /// </summary>
        private void NewFire(long polyRef)
        {
            GameObject newFire = (GameObject) PrefabUtility.InstantiatePrefab(_firePrefab, transform); // keep hierarchy clean
            
            newFire.name = $"Fire_{polyRef}";

            newFire.transform.position =
                DRcHandle.ToUnityVec3( _handle.NavMeshData.GetPolyCenter(polyRef) ); // we should give it a bit of wiggle here

            _handle.NavMeshData.GetTileAndPolyByRef(polyRef, out DtMeshTile tile, out DtPoly poly);
            Fire fire = newFire.GetComponent<Fire>();

            IList<long> neighborRefs = new List<long>();

            // Collect neis - neighbors
            for (int edge = 0; edge < poly.vertCount; edge++)
            {
                int nei = poly.neis[edge];

                if (nei != 0 && (nei & 0x8000) == 0) // internal neis
                {
                    int neighborIndex = nei - 1;
                    long neighborRef = _handle.NavMeshData.GetPolyRefBase(tile) | (uint)neighborIndex;

                    neighborRefs.Add(neighborRef);
                }
                else if (nei != 0) // external neis
                {
                    for (int linkIdx = poly.firstLink; linkIdx != -1; linkIdx = tile.links[linkIdx].next)
                    {
                        long neighborRef = tile.links[linkIdx].refs;
                        if ( neighborRef != 0 )
                            neighborRefs.Add(neighborRef);
                    }
                }
            }

            fire.SetRefs( this, polyRef, neighborRefs.ToArray() );
            
            // Debug.Log("polyref: " + fire.PolyRef);

            _firePolys[polyRef] = fire;
            _firePolyList.Add(fire);

            foreach (long p in neighborRefs)
                NewFirePoly(p);

            newFire.SetActive(false);

            // Undo.RegisterCreatedObjectUndo(newFire, "Bake Fire");
        }

        /// <summary>
        /// Creates a fire object only if not already present.
        /// </summary>
        private void NewFirePoly(long polyRef)
        {
            if (_firePolys.ContainsKey(polyRef))
                return;
            NewFire(polyRef);
        }
        #endif

        private int _updateCursor = 0;
        /// <summary>
        /// Updates fire propagation and handles random explosions.
        /// </summary>
        internal protected override void UpdateOrdered()
        {
            if (Time.frameCount % _runEveryframe != 0) // run every _runEveryframe frames
                return;

            Profiler.BeginSample("DRC ExplosionManager");

            // Randomly trigger an explosion
            if ( Rand.Range(0f, 1f) < _explosionChancePerFrame )
            {
                DRcHandle.NavQuery.FindRandomPoint(DRcHandle.Filter, Rand as IRcRand, out long polyRef, out RcVec3f centerPos);

                float deviation = Rand.Range(_radiusDeviation.x, _radiusDeviation.y);
                float death = _deathRadius * deviation;

                List<long> resultRefs = _handle.PolysInCircle( polyRef, centerPos, death);

                LatestExplosion = centerPos;

                _crowd.ExplosionAt(centerPos, death, _fearRadius * deviation, _panicRadius * deviation);

                StartCoroutine(ExplodeAt(_firePolys[polyRef].transform.position, death));

                foreach ( long fireRef in resultRefs )
                {
                    Debug.DrawLine(_firePolys[polyRef].transform.position, _firePolys[fireRef].transform.position, Color.yellow, 5f);
                    if ( Rand.Range(0f, 1f) < _fireChance )
                        SetFire( fireRef );
                }
            }

            // Update active fires batch by batch
            for (int i = 0; i < _FirePerUpdateBatch && _activeFires.Count > 0; i++)
            {
                int safeIndex = _updateCursor % _activeFires.Count;
                _activeFires[safeIndex].UpdateOrdered();
                _updateCursor++;
            }
        }

        /// <summary>
        /// Plays explosion visual effect by growing and shrinking the explosion object.
        /// </summary>
        private IEnumerator ExplodeAt(Vector3 pos, float radius)
        {
            _explosionObject.SetActive(true);
            _explosionObject.transform.position = pos;
            radius *= 2;

            YieldInstruction wfs = new WaitForSeconds(0.005f);

            for ( int i = 0; i <= radius ; i++ )
            {
                _explosionObject.transform.localScale = Vector3.one * i;
                yield return wfs;
            }

            for ( int i = 0; i < radius ; i++ )
            {
                _explosionObject.transform.localScale = Vector3.one * (radius - i);
                yield return null;
            }

            _explosionObject.SetActive(false);
        }

        /// <summary>
        /// Checks if a polygon currently has active fire.
        /// </summary>
        public bool PolyHasFire(long polyRef)
        {
            if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
            {
                if ( _activeFires.Contains(fire) )
                    return true;
                return false;
            }

            Debug.LogWarning("Poly not already baked, polyRef: " + polyRef + " _firePolys count: " + _firePolys.Count);
            return false;
        }

        /// <summary>
        /// Activates fire on a given polygon, if not already active.
        /// </summary>
        public bool SetFire(long polyRef)
        {
            if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
            {
                if ( _activeFires.Contains(fire) )
                    return false;
                
                _inactiveFires.Remove(fire);
                _activeFires.Add(fire);
                fire.gameObject.SetActive(true);
                return true;
            }
            
            Debug.LogWarning("Fire not already baked, polyRef: " + polyRef + " _firePolys count: " + _firePolys.Count);
            return false;
        }

        /// <summary>
        /// Deactivates a fire and returns it to the inactive pool.
        /// </summary>
        public bool UnSetFire(Fire fire)
        {
            if ( _inactiveFires.Contains(fire) )
                return false;

            _activeFires.Remove(fire);
            _inactiveFires.Add(fire);
            
            // Debug.LogWarning("Fire already extinguished: " + " " + _firePolys.Count);
            return false;
        }
    }
}