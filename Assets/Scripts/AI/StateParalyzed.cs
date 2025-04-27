using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateParalyzed", menuName = "State Machines/StateParalyzed")]
    public class StateParalyzed : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        protected override void EntryAction()
        {
            _agent.ChangeColor(_agent.ParalyzedColor);
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
    

