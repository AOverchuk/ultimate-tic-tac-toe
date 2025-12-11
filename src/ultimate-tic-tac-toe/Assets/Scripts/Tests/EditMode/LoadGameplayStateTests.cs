using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Runtime.Infrastructure.GameStateMachine;
using Runtime.Infrastructure.GameStateMachine.States;
using Runtime.Services.Scenes;
using Runtime.Services.UI;

namespace Tests.EditMode
{
    [TestFixture]
    public class LoadGameplayStateTests
    {
        private IGameStateMachine _stateMachine;
        private ISceneLoaderService _sceneLoader;
        private IUIService _uiService;
        private LoadGameplayState _sut;

        [SetUp]
        public void SetUp()
        {
            _stateMachine = Substitute.For<IGameStateMachine>();
            _sceneLoader = Substitute.For<ISceneLoaderService>();
            _uiService = Substitute.For<IUIService>();

            _sut = new LoadGameplayState(_stateMachine, _sceneLoader, _uiService);
        }

        [Test]
        public void WhenEnter_ThenClearsPoolsAndLoadsGameplayScene_InOrder()
        {
            Action capturedCallback = null;

            _sceneLoader
                .When(x => x.LoadSceneAsync(SceneNames.Gameplay, Arg.Any<Action>()))
                .Do(callInfo => capturedCallback = callInfo.Arg<Action>());

            _sut.Enter();

            _uiService.Received(1).ClearViewModelPools();
            _sceneLoader.Received(1).LoadSceneAsync(SceneNames.Gameplay, Arg.Any<Action>());
            capturedCallback.Should().NotBeNull("Callback должен быть передан в LoadSceneAsync");

            Received.InOrder(() =>
            {
                _uiService.ClearViewModelPools();
                _sceneLoader.LoadSceneAsync(SceneNames.Gameplay, Arg.Any<Action>());
            });
        }

        [Test]
        public void WhenSceneLoaded_ThenTransitionsToGameplayState()
        {
            Action capturedCallback = null;

            _sceneLoader
                .When(x => x.LoadSceneAsync(SceneNames.Gameplay, Arg.Any<Action>()))
                .Do(callInfo => capturedCallback = callInfo.Arg<Action>());

            _sut.Enter();

            capturedCallback.Should().NotBeNull("Callback должен быть захвачен для проверки перехода состояния");
            _stateMachine.DidNotReceive().Enter<GameplayState>();

            capturedCallback.Invoke();

            _stateMachine.Received(1).Enter<GameplayState>();
        }

        [Test]
        public void WhenExit_ThenCompletesWithoutError()
        {
            Action act = () => _sut.Exit();

            act.Should().NotThrow();
        }
    }
}