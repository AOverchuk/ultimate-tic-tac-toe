using UnityEngine;

namespace Infrastructure.States
{
    public class MainMenuState : IState
    {
        private readonly IGameStateMachine _stateMachine;

        public MainMenuState(IGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            Debug.Log("[MainMenuState] Entered MainMenu");
        }

        public void Exit()
        {
            Debug.Log("[MainMenuState] Exiting MainMenu");
        }

        public void OnPlayButtonClicked()
        {
            _stateMachine.Enter<LoadGameplayState>();
        }
    }
}

