using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "StateMove", menuName = "State Machines/StateMove")]
    public class StateMove : StateAbstract
    {
        public string Name;
        public GameObject gameObject;
        protected override void EntryAction()
        {
            Debug.Log($"Start State {Name}");
        }
        protected override void StateAction()
        {
            Debug.Log($"Doing State {gameObject.transform.position}");
        }
        protected override void ExitAction()
        {
            Debug.Log($"Exiting State {Name}");
        }
        public override void InstantiateState()
        {
            base.state = new State(Name,EntryAction,StateAction,ExitAction);
        }
    }
}
    

