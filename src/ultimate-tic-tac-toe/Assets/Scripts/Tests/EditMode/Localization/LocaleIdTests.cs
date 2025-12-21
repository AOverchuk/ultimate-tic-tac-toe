using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Runtime.Localization;

namespace Tests.EditMode.Localization
{
    [Category("Unit")]
    public class LocaleIdTests
    {
        [Test]
        public void WhenCreatedWithNull_ThenThrowsException()
        {
            // Act
            Action act = () => _ = new LocaleId(null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Locale code must be non-empty*");
        }

        [Test]
        public void WhenCreatedWithEmpty_ThenThrowsException()
        {
            // Act
            Action act = () => _ = new LocaleId(string.Empty);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Locale code must be non-empty*");
        }

        [Test]
        public void WhenCreatedWithWhitespace_ThenThrowsException()
        {
            // Act
            Action act = () => _ = new LocaleId("   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Locale code must be non-empty*");
        }

        [Test]
        public void WhenUsedAsKeyInDictionary_ThenWorksCorrectly()
        {
            // Arrange
            var dict = new Dictionary<LocaleId, string>();
            var locale1 = new LocaleId("en-US");
            var locale2 = new LocaleId("en-US");
            var locale3 = new LocaleId("ru-RU");

            // Act
            dict[locale1] = "English";
            dict[locale2] = "English Updated";
            dict[locale3] = "Russian";

            // Assert
            dict.Should().HaveCount(2);
            dict[locale1].Should().Be("English Updated");
            dict[locale2].Should().Be("English Updated");
            dict[locale3].Should().Be("Russian");
        }
    }
}
