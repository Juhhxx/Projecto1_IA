using DotRecast.Core.Numerics;
using DotRecast.Detour.Crowd;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Pathfinding
{
    public class DRAgent : MonoBehaviour
    {
        [SerializeField] private DRCrowdManager _crowdManager;
        [field:SerializeField] public AgentStatsController AgentState { get; private set; }
        [SerializeField] private float _acceptedDistToGoal = 2f;
        private DtCrowdAgent _agentID;
        public DtCrowdAgent ID => _agentID;

        [SerializeField] private Structure<Stage> _stage;
        private Structure<Exit> _exit;
        private (RcVec3f, long)  _lastSpot;
        private (RcVec3f, long)  _curSpot;
        private (RcVec3f, long) _nextSpot;

        public void Activate()
        {
            _exit = Exit.GetRandomGoodExit( _crowdManager.Rand.Range(0, 32), out (RcVec3f, long) pos);
            _exit.StayInSpot(pos.Item2);
            _lastSpot = pos;

            transform.position = _crowdManager.SnapToNavMesh(pos.Item1);;
            
            _agentID = _crowdManager.AddAgent(transform.position, false);
            _crowdManager.SwitchToNormal(_agentID);

            // testing purposes, remove later
            _stage = Stage.FindNearest(_agentID.npos);
            _crowdManager.SetTarget(_agentID, _stage.Ref, _stage.Position);

            // Debug.Log("Set agent target as " + _stage + " in pos " + transform.position + " spawned at exit " + pos.Item1);
        }

        public void Deactivate()
        {
            _crowdManager.RemoveAgent(_agentID);
            _agentID = null;
            
            gameObject.SetActive(false);
        }

        public void UpdateOrdered() // garbage collector is having a hard time here
        {
            Profiler.BeginSample("DRC Agent");

            if ( _exit != null && RcVec3f.Distance(_agentID.npos, _lastSpot.Item1) > _acceptedDistToGoal && _stage.EnteredArea(_agentID.npos) )
            {
                _exit.LeaveSpot( _lastSpot.Item2 ); // only supposed to run one time

                Debug.Log("Exited spot with distance " + RcVec3f.Distance(_agentID.npos, _lastSpot.Item1));

                if ( _stage.EnteredArea(_agentID.npos) )
                {
                    Debug.Log("Exited exit " + RcVec3f.Distance(_agentID.npos, _lastSpot.Item1));
                    _nextSpot = _stage.GetBestSpot(_agentID.npos);
                    _crowdManager.SetTarget(_agentID, _nextSpot.Item2, _nextSpot.Item1);
                    _exit = null;
                }
            }
            else if ( _stage != null )
            {
                Debug.Log("Done Entered! New place: " + _nextSpot.Item1.X + " " + _nextSpot.Item1.Z);

                if ( _agentID.vel.Length() < 0.1f )
                {
                    if ( RcVec3f.Distance(_agentID.npos, _nextSpot.Item1) < _acceptedDistToGoal )
                    {
                        _stage.StayInSpot(_nextSpot.Item2);
                        _stage = null;
                        return;
                    }

                    _nextSpot = _stage.GetBestSpot(_agentID.npos);
                    _crowdManager.SetTarget(_agentID, _nextSpot.Item2, _nextSpot.Item1);
                }
                
            }

            // use DtCrowdNeighbour to alarm other agents of panic

            if ( _agentID != null )
            {
                transform.position = DRcHandle.ToUnityVec3(_agentID.npos);
                if ( _agentID.vel.Length() > 0.1f )
                    transform.rotation = DRcHandle.ToDotQuat(_agentID.vel);
            }
        }

        private void Update()
        {

        }


        /*// these are testing methods
        public void StartOrdered()
        {
            transform.position = _crowdManager.SnapToNavMesh(transform.position);

            _agentID = _crowdManager.AddAgent(transform.position, false);

            _stage = Stage.FindNearest(_agentID.npos);
            _crowdManager.SetTarget(_agentID, _stage.Ref, _stage.Position);
            Debug.Log("Set agent target as " + _stage);
        }
        private Vector3 GetRandomPos()
        {
           Bounds values = _crowdManager.BoundBox.bounds;

            Vector3 randomEnd = new Vector3(
                    _crowdManager.Rand.Range(values.min.x, values.max.x),
                    transform.position.y, // Random.Range(values.min.y, values.max.y), // this wont be needed later i assume
                    _crowdManager.Rand.Range(values.min.z, values.max.z)
                );
            return _crowdManager.SnapToNavMesh(randomEnd);
        }
        // these are testing methods*/




        #if UNITY_EDITOR
        public void SetRefs(DRCrowdManager manager)
        {
            _crowdManager = manager;
        }
        #endif

        public override string ToString()
        {
            if (_agentID == null)
                return $"null";

            return $"[DRAgent] Pos: {_agentID.npos} | Target: {_agentID.targetPos} | Vel: {_agentID.vel} | State: {_agentID.state} | TargetState: {_agentID.targetState} | Speed: {_agentID.desiredSpeed }";
        }
    }
}