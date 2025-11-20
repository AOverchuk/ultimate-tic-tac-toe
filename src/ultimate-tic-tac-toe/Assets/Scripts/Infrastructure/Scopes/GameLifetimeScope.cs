using Infrastructure.EntryPoint;
using Infrastructure.States;
using Services.Scenes;
using VContainer;
using VContainer.Unity;

namespace Infrastructure.Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameEntryPoint>();
            
            builder.Register<ISceneLoaderService, SceneLoaderService>(Lifetime.Singleton);
        
            builder.Register<IStateFactory, StateFactory>(Lifetime.Singleton);
            builder.Register<IGameStateMachine, GameStateMachine>(Lifetime.Singleton);
        
            builder.Register<BootstrapState>(Lifetime.Transient);
            builder.Register<LoadMainMenuState>(Lifetime.Transient);
            builder.Register<MainMenuState>(Lifetime.Transient);
            builder.Register<LoadGameplayState>(Lifetime.Transient);
            builder.Register<GameplayState>(Lifetime.Transient);
        }
    }
}
