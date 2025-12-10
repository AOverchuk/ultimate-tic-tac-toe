using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Runtime.Infrastructure.GameStateMachine;
using Runtime.Infrastructure.GameStateMachine.States;

namespace Tests.EditMode
{
    [TestFixture]
    public class BootstrapStateTests
    {
        private IGameStateMachine _stateMachineMock;
        private BootstrapState _sut;

        [SetUp]
        public void SetUp()
        {
            _stateMachineMock = Substitute.For<IGameStateMachine>();
            _sut = new BootstrapState(_stateMachineMock);
        }

        [Test]
        public void WhenEnter_ThenTransitionsToLoadMainMenuState()
        {
            // Act
            _sut.Enter();

            // Assert
            _stateMachineMock.Received(1).Enter<LoadMainMenuState>();
        }

        [Test]
        public void WhenExit_ThenCompletesWithoutError()
        {
            // Act
            Action act = () => _sut.Exit();

            // Assert
            act.Should().NotThrow();
        }
    }
}