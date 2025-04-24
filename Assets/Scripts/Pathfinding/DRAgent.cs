using DotRecast.Detour.Crowd;
using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Pathfinding
{
    public class DRAgent : MonoBehaviour
    {
        [SerializeField] private DRCrowdManager _crowdManager;
        [SerializeField] private float _acceptedDistToGoal = 2f;
        private DtCrowdAgent _agentID;

        public bool IsActive { get; private set; }

        public void Activate(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
            IsActive = true;

            _agentID = _crowdManager.AddAgent(position, false);
        }

        public void Deactivate()
        {
            _crowdManager.RemoveAgent(_agentID);
            _agentID = null;
            
            IsActive = false;
            gameObject.SetActive(false);
        }



        // these are testing methods
        public void StartOrdered()
        {
            Vector3 newRandomPos = GetRandomPos();

            transform.position = _crowdManager.SnapToNavMesh(transform.position);

            _agentID = _crowdManager.AddAgent(transform.position, false);
            _crowdManager.SetTarget(_agentID, newRandomPos);
        }
        public void UpdateOrdered()
        {
            Profiler.BeginSample("DRC Agent");

            transform.SetPositionAndRotation(
                DRcHandle.ToUnityVec3(_agentID.npos),
                DRcHandle.ToDotQuat(_agentID.vel));

            if ( _agentID.GetDistanceToGoal(_acceptedDistToGoal) < _acceptedDistToGoal)
            {
                _crowdManager.SetTarget(_agentID, GetRandomPos());
            }
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
        // these are testing methods




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