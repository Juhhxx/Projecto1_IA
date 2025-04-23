using UnityEngine;
using NaughtyAttributes;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public class StateMachineRunner : MonoBehaviour
    {
        [Expandable][SerializeField] private StateMachineCreator _stateMachineModel;
        private StateMachineCreator _stateMachine;

        private void Start()
        {
            _stateMachine = _stateMachineModel.CreateStateMachine();
            
            _stateMachine.SetObjectReference(gameObject);
            _stateMachine.InstantiateStateMachine();
        }
        private void Update()
        {
            _stateMachine.Run();
        }
    }
}
