using UnityEngine;
using Scripts.AI.FSMs.UnityIntegration;

namespace Scripts.AI
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
