using UnityEngine;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public class StateMachineRunner : MonoBehaviour
    {
        [SerializeField] private StateMachineCreator _stateMachine;

        private void Start()
        {
            _stateMachine.SetObjectReference(gameObject);
            _stateMachine.InstantiateStateMachine();
        }
        private void Update()
        {
            _stateMachine.Run();
        }
    }
}
