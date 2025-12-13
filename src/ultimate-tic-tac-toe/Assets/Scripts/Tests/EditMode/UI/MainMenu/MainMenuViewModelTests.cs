using System;
using FluentAssertions;
using NUnit.Framework;
using R3;
using Runtime.UI.MainMenu;

namespace Tests.EditMode.UI.MainMenu
{
    [TestFixture]
    public class MainMenuViewModelTests
    {
        [Test]
        public void WhenInitialized_ThenHasCorrectDefaults()
        {
            // Arrange
            var sut = new MainMenuViewModel();

            // Assert
            sut.Title.CurrentValue.Should().Be("Ultimate Tic-Tac-Toe");
            sut.StartButtonText.CurrentValue.Should().Be("Start Game");
            sut.ExitButtonText.CurrentValue.Should().Be("Exit");
            sut.IsInteractable.CurrentValue.Should().BeTrue();
            sut.StartGameRequested.Should().NotBeNull();
            sut.ExitRequested.Should().NotBeNull();
        }

        [Test]
        public void WhenSetInteractable_ThenUpdatesIsInteractable()
        {
            // Arrange
            var sut = new MainMenuViewModel();

            // Act
            sut.SetInteractable(false);

            // Assert
            sut.IsInteractable.CurrentValue.Should().BeFalse();

            // Act again
            sut.SetInteractable(true);

            // Assert again
            sut.IsInteractable.CurrentValue.Should().BeTrue();
        }

        [Test]
        public void WhenDisposed_ThenObservablesThrowObjectDisposedException()
        {
            // Arrange
            var sut = new MainMenuViewModel();
            var valueEmitted = false;
            sut.StartGameRequested.Subscribe(_ => valueEmitted = true);

            sut.RequestStartGame();
            valueEmitted.Should().BeTrue("Subject should work before dispose");

            // Act
            sut.Dispose();

            // Assert
            Action actSubject = () => sut.RequestStartGame();
            actSubject.Should().Throw<ObjectDisposedException>();

            Action actProperty = () => sut.Title.Subscribe(_ => { });
            actProperty.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenDisposedMultipleTimes_ThenDoesNotThrow()
        {
            // Arrange
            var sut = new MainMenuViewModel();

            // Act
            sut.Dispose();
            Action act = () => sut.Dispose();

            // Assert
            act.Should().NotThrow();
        }
    }
}