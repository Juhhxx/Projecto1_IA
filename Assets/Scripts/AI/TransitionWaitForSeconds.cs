using UnityEngine;
using Scripts.AI.FSMs.BaseFiles;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
{
    [CreateAssetMenu(fileName = "TransitionWaitForSeconds", menuName = "State Machines/TransitionWaitForSeconds")]
    public class TransitionWaitForSeconds : TransitionAbstract
    {
        public  float _seconds;
        private float _timeElapsed;
        private GameObject gameObject;

        protected override void Action()
        {
            Debug.Log($"{_seconds} seconds passed.");
        }
        protected override bool Condition()
        {
            _timeElapsed += Time.deltaTime;
            return _timeElapsed == _seconds;
        }
        public override void InstantiateTransition()
        {
            gameObject = base.objectReference;

            base.transition = new Transition(base.Name,Condition,base.ToState.State,Action);
        }
    }
}
