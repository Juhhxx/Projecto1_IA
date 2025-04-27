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
            if ( StateNavHelper.FindSpot(_agent, ref _exit) )
                _agent.Deactivate();
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
    

