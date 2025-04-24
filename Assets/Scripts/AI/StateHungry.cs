using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateHungry", menuName = "State Machines/StateHungry")]
    public class StateHungry : StateAbstract
    {
        private GameObject  gameObject;
        private AgentStatsController _agent;

        protected override void EntryAction()
        {
            Debug.Log($"Start State {Name}");
            _agent.ChangeColor(_agent.HungryColor);
            _agent.StartDepletingEnergy();
        }
        protected override void StateAction()
        {
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
    

