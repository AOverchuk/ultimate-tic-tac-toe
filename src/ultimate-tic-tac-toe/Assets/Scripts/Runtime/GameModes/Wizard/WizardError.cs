#nullable enable

using System;

namespace Runtime.GameModes.Wizard
{
    public enum ErrorDisplayType
    {
        Inline = 0,
        Toast = 1,
        Modal = 2,
    }

    /// <summary>
    /// User-facing wizard error representation.
    /// Kept minimal in Phase 3; can be extended in later phases.
    /// </summary>
    public sealed class WizardError
    {
        public string Code { get; }
        public string Message { get; }
        public bool IsBlocking { get; }
        public ErrorDisplayType DisplayType { get; }

        public WizardError(string code, string message, bool isBlocking, ErrorDisplayType displayType)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(code));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));

            Code = code;
            Message = message;
            IsBlocking = isBlocking;
            DisplayType = displayType;
        }

        public static WizardError FromException(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            // Phase 3: keep user-facing message generic and log full exception elsewhere.
            // Later phases can map exception types to localized messages.
            const string fallbackMessage = "Произошла ошибка. Попробуйте ещё раз.";

            return new WizardError(
                code: "wizard.unhandled_exception",
                message: fallbackMessage,
                isBlocking: true,
                displayType: ErrorDisplayType.Modal);
        }
    }
}
