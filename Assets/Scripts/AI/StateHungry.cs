using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;
using Scripts.Structure;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateHungry", menuName = "State Machines/StateHungry")]
    public class StateHungry : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        private Structure<FoodArea> _food;

        protected override void EntryAction()
        {
            _food = FoodArea.FindNearest(_agent.ID.npos);
            _agent.Crowd.SetTarget(_agent.ID, _food.Ref, _food.Position);


            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.HungryColor);
            _agent.StartDepletingEnergy();
        }
        protected override void StateAction()
        {
            if ( StateNavHelper.FindSpot(_agent, ref _food) )
                _agent.ReturnToNormal();
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
            _agent.StopDepletingEnergy();
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

