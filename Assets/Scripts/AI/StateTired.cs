using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateTired", menuName = "State Machines/StateTired")]
    public class StateTired : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        protected override void EntryAction()
        {
            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.TiredColor);
            _agent.StartDepletingHunger();
        }
        protected override void StateAction()
        {
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
    

