using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateTired", menuName = "State Machines/StateTired")]
    public class StateTired : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<GreenSpace> _green;

        protected override void EntryAction()
        {
            _green = GreenSpace.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _green.Ref, _green.Position);

            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.TiredColor);
            _agent.StartDepletingHunger();
        }
        protected override void StateAction()
        {
            if ( StateNavHelper.FindSpot(_agent, ref _green) )
                _agent.ReturnToNormal();
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingHunger();
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

