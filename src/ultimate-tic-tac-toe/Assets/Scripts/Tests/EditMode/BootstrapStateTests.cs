using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using R3;
using Runtime.Infrastructure.GameStateMachine;
using Runtime.Infrastructure.GameStateMachine.States;
using Runtime.Localization;
using UnityEngine.TestTools;

namespace Tests.EditMode
{
    [TestFixture]
    public class BootstrapStateTests
    {
        private IGameStateMachine _stateMachineMock;
        private ILocalizationService _localizationMock;
        private BootstrapState _sut;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            _stateMachineMock = Substitute.For<IGameStateMachine>();
            _localizationMock = Substitute.For<ILocalizationService>();
            
            // Setup default behavior for localization
            _localizationMock.InitializeAsync(Arg.Any<CancellationToken>()).Returns(UniTask.CompletedTask);
            
            var currentLocale = new ReactiveProperty<LocaleId>(LocaleId.EnglishUs);
            _localizationMock.CurrentLocale.Returns(currentLocale);
            
            _sut = new BootstrapState(_stateMachineMock, _localizationMock);
            _cancellationToken = CancellationToken.None;
        }

        [Test]
        public async Task WhenEnter_ThenInitializesLocalizationService()
        {
            // Arrange
            _stateMachineMock.EnterAsync<LoadMainMenuState>(Arg.Any<CancellationToken>()).Returns(UniTask.CompletedTask);

            // Act
            await _sut.EnterAsync(_cancellationToken);

            // Assert
            await _localizationMock.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task WhenEnter_ThenTransitionsToLoadMainMenuState()
        {
            // Arrange
            _stateMachineMock.EnterAsync<LoadMainMenuState>(Arg.Any<CancellationToken>()).Returns(UniTask.CompletedTask);

            // Act
            await _sut.EnterAsync(_cancellationToken);

            // Assert
            await _stateMachineMock.Received(1).EnterAsync<LoadMainMenuState>(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task WhenEnterAndLocalizationFails_ThenThrowsException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Localization failed");
            _localizationMock.InitializeAsync(Arg.Any<CancellationToken>()).Returns(UniTask.FromException(expectedException));
            
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex(@"\[BootstrapState\] Failed to initialize localization: System\.InvalidOperationException: Localization failed"));

            // Act
            Func<Task> act = async () => await _sut.EnterAsync(_cancellationToken);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Localization failed");
        }

        [Test]
        public void WhenExit_ThenCompletesWithoutError()
        {
            // Arrange
            Action act = () => _sut.Exit();

            // Assert
            act.Should().NotThrow();
        }
    }
}