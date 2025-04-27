using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StatePanic", menuName = "State Machines/StatePanic")]
    public class StatePanic : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<Exit> _exit;

        protected override void EntryAction()
        {
            _exit = Exit.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _exit.Ref, _exit.Position);
            _agent.ChangeColor(_agent.PanicColor);

            Debug.Log($"Start State {Name}");
        }
        protected override void StateAction()
        {
            // If inside structure area
            if (_exit.EnteredArea(_agent.ID.npos))
            {
                // If currently targeting the center (structure.Ref) or wrong target, pick best spot
                if ( _agent.NextRef.Ref != _agent.ID.targetRef )
                {
                    (RcVec3f, long) nextRef = Structure<Exit>.GetBestSpot(_agent.ID.npos, _exit, out _);

                    _agent.NextRef = nextRef;
                    _agent.Crowd.SetTarget(_agent.ID, _agent.NextRef.Ref, _agent.NextRef.Pos);
                }
                // If close enough to the chosen good spot
                else if ( RcVec3f.Distance(_agent.ID.npos, _agent.NextRef.Pos) <= _agent.AcceptedDist )
                    _agent.Deactivate();
            }
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

