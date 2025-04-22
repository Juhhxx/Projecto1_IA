using UnityEngine;

namespace Scripts.AI.FSMs.UnityIntegration
{
    public class StateMachineRunner : MonoBehaviour
    {
        [SerializeField] private StateMachineCreator stateMachine;

        private void Start()
        {
            stateMachine.InstantiateStateMachine();
        }
        private void Update()
        {
            stateMachine.Run();
        }
    }
}
