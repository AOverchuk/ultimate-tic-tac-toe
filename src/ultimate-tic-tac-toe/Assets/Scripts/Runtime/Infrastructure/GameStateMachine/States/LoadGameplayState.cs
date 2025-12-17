using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Infrastructure.Logging;
using Runtime.Services.Assets;
using Runtime.Services.Scenes;
using Runtime.Services.UI;
using StripLog;

namespace Runtime.Infrastructure.GameStateMachine.States
{
    public class LoadGameplayState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly IUIService _uiService;
        private readonly IAssetProvider _assets;

        public LoadGameplayState(
            IGameStateMachine stateMachine,
            ISceneLoaderService sceneLoader,
            IUIService uiService,
            IAssetProvider assets)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _uiService = uiService;
            _assets = assets;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Log.Debug(LogTags.Scenes, "[LoadGameplayState] Loading Gameplay scene...");
            _uiService.ClearViewModelPools();
            _assets.Cleanup();
            await _sceneLoader.LoadSceneAsync(SceneNames.Gameplay, cancellationToken);
            Log.Debug(LogTags.Scenes, "[LoadGameplayState] Gameplay scene loaded");
            await _stateMachine.EnterAsync<GameplayState>(cancellationToken);
        }

        public void Exit() => Log.Debug(LogTags.Scenes, "[LoadGameplayState] Exiting...");
    }
}