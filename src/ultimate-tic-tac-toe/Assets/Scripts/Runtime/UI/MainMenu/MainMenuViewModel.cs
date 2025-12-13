using R3;
using Runtime.UI.Core;

namespace Runtime.UI.MainMenu
{
    public class MainMenuViewModel : BaseViewModel
    {
        private readonly ReactiveProperty<string> _title = new("Ultimate Tic-Tac-Toe");
        private readonly ReactiveProperty<string> _startButtonText = new("Start Game");
        private readonly ReactiveProperty<string> _exitButtonText = new("Exit");
        private readonly ReactiveProperty<bool> _isInteractable = new(true);
        private readonly Subject<Unit> _startGameRequested = new();
        private readonly Subject<Unit> _exitRequested = new();

        public ReadOnlyReactiveProperty<string> Title => _title;
        public ReadOnlyReactiveProperty<string> StartButtonText => _startButtonText;
        public ReadOnlyReactiveProperty<string> ExitButtonText => _exitButtonText;
        public ReadOnlyReactiveProperty<bool> IsInteractable => _isInteractable;
        public Observable<Unit> StartGameRequested => _startGameRequested;
        public Observable<Unit> ExitRequested => _exitRequested;

        public void SetInteractable(bool isInteractable) => _isInteractable.Value = isInteractable;

        public void RequestStartGame() => _startGameRequested.OnNext(Unit.Default);

        public void RequestExit() => _exitRequested.OnNext(Unit.Default);

        protected override void OnDispose()
        {
            _startGameRequested?.Dispose();
            _exitRequested?.Dispose();
            _title?.Dispose();
            _startButtonText?.Dispose();
            _exitButtonText?.Dispose();
            _isInteractable?.Dispose();
        }
    }
}