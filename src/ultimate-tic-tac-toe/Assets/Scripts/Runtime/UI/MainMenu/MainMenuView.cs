using R3;
using Runtime.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runtime.UI.MainMenu
{
    public class MainMenuView : UIView<MainMenuViewModel>
    {
        [Core.UxmlElementAttribute("Title")] 
        private Label _titleLabel;
        [Core.UxmlElementAttribute("StartButton")] 
        private Button _startButton;
        [Core.UxmlElementAttribute("ExitButton")] 
        private Button _exitButton;

        protected override void BindViewModel()
        {
            BindText(ViewModel.Title, _titleLabel);
            BindText(ViewModel.StartButtonText, _startButton);
            BindText(ViewModel.ExitButtonText, _exitButton);

            BindEnabled(ViewModel.IsInteractable, _startButton);
            BindEnabled(ViewModel.IsInteractable, _exitButton);

            _startButton.clicked += OnStartButtonClicked;
            _exitButton.clicked += OnExitButtonClicked;
        }

        internal void OnStartButtonClicked()
        {
            Debug.Log("[MainMenuView] Start button clicked");
            ViewModel.OnStartGameClicked.OnNext(Unit.Default);
        }

        internal void OnExitButtonClicked()
        {
            Debug.Log("[MainMenuView] Exit button clicked");
            ViewModel.OnExitClicked.OnNext(Unit.Default);
        }

        protected override void OnDestroy()
        {
            if (_startButton != null)
                _startButton.clicked -= OnStartButtonClicked;
            
            if (_exitButton != null)
                _exitButton.clicked -= OnExitButtonClicked;

            base.OnDestroy();
        }
    }
}