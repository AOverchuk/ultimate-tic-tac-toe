using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Runtime.Infrastructure.States;

namespace Tests.EditMode
{
    [TestFixture]
    public class GameStateMachineTests
    {
        private IStateFactory _stateFactory;

        [SetUp]
        public void SetUp()
        {
            _stateFactory = Substitute.For<IStateFactory>();
        }

        [Test]
        public void WhenConstructor_ThenSetsCurrentStateToNull()
        {
            // Arrange & Act
            var stateMachine = new GameStateMachine(_stateFactory);

            // Assert
            stateMachine.CurrentState.Should().BeNull();
        }
    }
}

