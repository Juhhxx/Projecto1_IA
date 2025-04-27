using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateDeath", menuName = "State Machines/StateDeath")]
    public class StateDeath : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        protected override void EntryAction()
        {
            _agent.Deactivate();

            // Debug.Log($"Start State {Name}");
        }
        protected override void StateAction()
        {
        }
        protected override void ExitAction()
        {
            // Debug.Log($"Exiting State {Name}");
        }
        public override void InstantiateState()
        {
            gameObject  = base.objectReference;
            _agent = gameObject.GetComponent<AgentStatsController>();

            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }

    }
}
    

