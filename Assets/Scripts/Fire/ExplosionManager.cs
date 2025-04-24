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
    public class ExplosionManager : Manager
    {
        [SerializeField] private DRcHandle _handle;
        [SerializeField] private GameObject _firePrefab;
        [SerializeField] private GameObject _explosionObject;
        [SerializeField] private float _radius = 20f, _radiusDeviation = 3f, _fireChance = 0.5f;
        [SerializeField] private int _FirePerUpdateBatch = 2;
        [SerializeField] private float _explosionChancePerFrame = 0.001f;

        private static Dictionary<long, GameObject> _firePolys = new();
        [SerializeField] private List<Fire> _firePolyList = new();
        public RcVec3f LatestExplosion { get; private set; }

        
        public static ISeedRandom Rand { get; private set; }

        internal protected override void AwakeOrdered()
        {
            _FirePerUpdateBatch = _firePolyList.Count / _FirePerUpdateBatch;

            if ( _handle == null )
                _handle = FindFirstObjectByType<DRcHandle>();

            Rand = new RcSeedRandom(gameObject);

            Debug.Log("Poly list count: " + _firePolyList.Count);
            foreach (Fire fire in _firePolyList)
            {
                _firePolys[fire.PolyRef] = fire.gameObject;
                // Debug.Log( "new fire go: " + _firePolys[fire.PolyRef] );
            }
            Debug.Log("Poly dict count: " + _firePolys.Count);
        }

        #if UNITY_EDITOR
        internal protected override void Bake()
        {
            _firePolys = new Dictionary<long, GameObject>();
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

        private void NewFire(long polyRef)
        {
            GameObject newFire = (GameObject) PrefabUtility.InstantiatePrefab(_firePrefab, transform); // keep hierarchy clean
            
            newFire.name = $"Fire_{polyRef}";

            newFire.transform.position =
                DRcHandle.ToUnityVec3( _handle.NavMeshData.GetPolyCenter(polyRef) ); // we should give it a bit of wiggle here

            _handle.NavMeshData.GetTileAndPolyByRef(polyRef, out DtMeshTile tile, out DtPoly poly);
            Fire fire = newFire.GetComponent<Fire>();

            IList<long> neighborRefs = new List<long>();

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

            fire.SetRefs(polyRef, neighborRefs.ToArray());
            
            // Debug.Log("polyref: " + fire.PolyRef);

            _firePolys[polyRef] = newFire;
            _firePolyList.Add(fire);

            foreach (long p in neighborRefs)
                NewFirePoly(p);

            newFire.SetActive(false);

            // Undo.RegisterCreatedObjectUndo(newFire, "Bake Fire");
        }

        private void NewFirePoly(long polyRef)
        {
            if (_firePolys.ContainsKey(polyRef))
                return;
            NewFire(polyRef);
        }
        #endif

        internal protected override void UpdateOrdered()
        {
            if (Time.frameCount % 3 != 0) // run every 3 frames
                return;

            Profiler.BeginSample("DRC ExplosionManager");
            // run fire update only every _RunFireEveryFrame frames

            if ( Rand.Range(0f, 1f) < _explosionChancePerFrame )
            {
                DRcHandle.NavQuery.FindRandomPoint(DRcHandle.Filter, Rand as IRcRand, out long polyRef, out RcVec3f centerPos);

                Debug.Log("Exploding " + polyRef + "!");

                float radius = _radius + Rand.Range(-_radiusDeviation, _radiusDeviation);;
                List<long> resultRefs = PolysInCircle( polyRef, centerPos, radius);
                LatestExplosion = centerPos;
                StartCoroutine(ExplodeAt(_firePolys[polyRef].transform.position, radius));

                foreach ( long fireRef in resultRefs )
                {
                    Debug.DrawLine(_firePolys[polyRef].transform.position, _firePolys[fireRef].transform.position, Color.yellow, 5f);
                    if ( Rand.Range(0f, 1f) < _fireChance )
                        SetFire( fireRef );
                }
            }

            int offset = Time.frameCount % _FirePerUpdateBatch;

            for (int i = offset; i < _firePolyList.Count; i += _FirePerUpdateBatch)
                if ( _firePolyList[i].gameObject.activeSelf )
                    _firePolyList[i].UpdateOrdered();
        }

        private void Update()
        {
            if (Time.frameCount % 3 != 0) // run every 3 frames
                return;
            
            Profiler.BeginSample("DRC ExplosionManager Graphics");

            _fireMatrices.Clear();

            foreach ( Fire fire in _firePolyList )
                if ( fire.gameObject.activeSelf )
                    _fireMatrices.Add(fire.Matrix);

            Graphics.DrawMeshInstanced(_fireMesh, 0, _fireMaterial, _fireMatrices);
        }

        [SerializeField] private Mesh _fireMesh;
        [SerializeField] private Material _fireMaterial;
        [SerializeField] private List<Matrix4x4> _fireMatrices = new();


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
        /// To be used by agents
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="fireSource"></param>
        /// <returns></returns>
        public bool LookForFire(RcVec3f position, float radius, out RcVec3f fireSource)
        {
            fireSource = RcVec3f.Zero;

            DtStatus status = DRcHandle.FindNearest(position, out long startRef, out RcVec3f nearest, out _);
            if ( status.Failed() )
                return false;

            List<long> resultRefs = PolysInCircle(startRef, nearest, radius);

            foreach ( long polyRef in resultRefs )
                if ( PolyHasFire(polyRef) )
                {
                    fireSource = _handle.NavMeshData.GetPolyCenter(polyRef);
                    return true;
                }

            return false;
        }

        private List<long> PolysInCircle(long startRef, RcVec3f nearest, float radius)
        {
            List<long> resultRefs = new List<long>();
            List<long> resultParents = new List<long>();
            List<float> resultCosts = new List<float>();

            DtStatus status = DRcHandle.NavQuery.FindPolysAroundCircle (
                startRef,
                nearest,
                radius,
                DRcHandle.Filter,
                ref resultRefs,
                ref resultParents,
                ref resultCosts
            );

            if ( status.Failed() )
                return null;

            return resultRefs;
        }

        /// <summary>
        /// To be used by tile filters
        /// </summary>
        /// <param name="polyRef"></param>
        /// <returns></returns>
        public bool PolyHasFire(long polyRef)
        {
            if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
            {
                if ( fire.activeSelf )
                    return true;
                return false;
            }

            Debug.LogWarning("Poly not already baked, polyRef: " + polyRef + " _firePolys count: " + _firePolys.Count);
            return false;
        }

        public static bool SetFire(long polyRef)
        {
            if ( _firePolys.TryGetValue(polyRef, out var fire) && fire != null )
            {
                if ( fire.activeSelf )
                    return false;
                
                fire.SetActive(true);
                return true;
            }

            
            Debug.LogWarning("Fire not already baked, polyRef: " + polyRef + " _firePolys count: " + _firePolys.Count);
            return false;
        }
    }
}