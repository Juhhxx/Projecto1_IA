using Unity.VisualScripting;
using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "TransitionKeyPressed", menuName = "State Machines/TransitionKeyPressed")]
    public class TransitionKeyPressed : TransitionAbstract
    {
        public KeyCode Key;

        protected override void Action()
        {
            Debug.Log($"The key {Key} was pressed");
        }
        protected override bool Condition()
        {
            return Input.GetKeyDown(Key);
        }
        public override void IntantiateTransition()
        {
            base.transition = new Transition(base.Name,Condition,base.ToState.State,Action);
        }
    }
}
