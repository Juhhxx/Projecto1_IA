using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateExample", menuName = "State Machines/StateExample")]
    public class StateExample : StateAbstract
    {
        public string Name;
        private void EntryAction()
        {
            Debug.Log($"Start State {Name}");
        }
        private void StateAction()
        {
            Debug.Log($"Doing State {Name}");
        }
        private void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
        }
        public override void InstantiateState()
        {
            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

