using System;
using FluentAssertions;
using NUnit.Framework;
using NSubstitute;
using Runtime.GameModes.Wizard;

namespace Tests.EditMode.GameModes.Wizard
{
    [TestFixture]
    [Category("Unit")]
    public class ModeSettingsPresentationTests
    {
        [Test]
        public void WhenModeSettingsPresentationCreatedWithNullUxmlAssetKey_ThenThrowsArgumentException()
        {
            // Arrange
            var vm = Substitute.For<ISpecificModeSettingsViewModel>();

            // Act
            Action act = () => _ = new ModeSettingsPresentation(uxmlAssetKey: null, viewModel: vm);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WhenModeSettingsPresentationCreatedWithWhitespaceUxmlAssetKey_ThenThrowsArgumentException()
        {
            // Arrange
            var vm = Substitute.For<ISpecificModeSettingsViewModel>();

            // Act
            Action act = () => _ = new ModeSettingsPresentation(uxmlAssetKey: " ", viewModel: vm);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WhenModeSettingsPresentationCreatedWithNullViewModel_ThenThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Action act = () => _ = new ModeSettingsPresentation(uxmlAssetKey: "ui/key", viewModel: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
