using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Infrastructure.Logging;
using Runtime.Services.Assets;
using Runtime.Services.UI;
using Runtime.UI.MainMenu;
using StripLog;

namespace Runtime.Infrastructure.GameStateMachine.States
{
    public class MainMenuState : IState
    {
        private readonly IUIService _uiService;
        private readonly IMainMenuCoordinator _coordinator;
        private readonly IAssetProvider _assets;
        private readonly AssetLibrary _assetLibrary;
        private bool _isExited;

        public MainMenuState(
            IUIService uiService, 
            IMainMenuCoordinator coordinator,
            IAssetProvider assets,
            AssetLibrary assetLibrary)
        {
            _uiService = uiService;
            _coordinator = coordinator;
            _assets = assets;
            _assetLibrary = assetLibrary;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _isExited = false;
            Log.Debug(LogTags.Scenes, "[MainMenuState] Entered MainMenu");
            var mainMenuPrefab = await _assets.LoadAsync<UnityEngine.GameObject>(_assetLibrary.MainMenuPrefab, cancellationToken);
            _uiService.RegisterWindowPrefab<MainMenuView>(mainMenuPrefab);
            var view = _uiService.Open<MainMenuView, MainMenuViewModel>();
            
            if (view == null)
            {
                Log.Error(LogTags.UI, "[MainMenuState] Failed to open MainMenuView!");
                return;
            }
            
            var viewModel = view.GetViewModel();
            _coordinator.Initialize(viewModel);
        }

        public void Exit()
        {
            if (_isExited)
                return;
            
            _isExited = true;
            Log.Debug(LogTags.Scenes, "[MainMenuState] Exiting MainMenu");
            _uiService.Close<MainMenuView>();
            _coordinator.Dispose();
        }
    }
}

