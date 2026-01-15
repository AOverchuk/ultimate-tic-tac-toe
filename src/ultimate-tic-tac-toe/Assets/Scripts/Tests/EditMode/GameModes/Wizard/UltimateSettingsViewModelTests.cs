using System;
using FluentAssertions;
using NUnit.Framework;
using Runtime.GameModes.Wizard;

namespace Tests.EditMode.GameModes.Wizard
{
    [TestFixture]
    [Category("Unit")]
    public class UltimateSettingsViewModelTests
    {
        [Test]
        public void WhenUltimateSettingsViewModelCreated_ThenConfigIsUltimateModeConfigAndIsValidTrue()
        {
            // Arrange
            using var sut = new UltimateSettingsViewModel();

            // Act
            var config = sut.Config.CurrentValue;
            var isValid = sut.IsValid.CurrentValue;

            // Assert
            config.Should().BeOfType<UltimateModeConfig>();
            isValid.Should().BeTrue();
        }

        [Test]
        public void WhenUltimateSettingsViewModelDisposeCalledMultipleTimes_ThenIsIdempotent()
        {
            // Arrange
            var sut = new UltimateSettingsViewModel();

            // Act
            Action act = () =>
            {
                sut.Dispose();
                sut.Dispose();
            };

            // Assert
            act.Should().NotThrow();
        }
    }
}
