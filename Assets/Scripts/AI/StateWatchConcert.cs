using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;
using DotRecast.Core.Numerics;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateWatchConcert", menuName = "State Machines/StateWatchConcert")]
    public class StateWatchConcert : StateAbstract
    {
        private GameObject gameObject;
        private AgentStatsController _agent;

        private Structure<Stage> _stage;

        protected override void EntryAction()
        {
            _stage = Stage.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _stage.Ref, _stage.Position);

            // Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.NormalColor);
            _agent.StartDepletingHunger();
            _agent.StartDepletingEnergy();
        }
        protected override void StateAction()
        {
            StateNavHelper.FindSpot(_agent, ref _stage);
            _agent.UpdateStats();
        }
        protected override void ExitAction()
        {
            // Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingHunger();
            _agent.StopDepletingEnergy();
        }
        public override void InstantiateState()
        {
            gameObject = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

