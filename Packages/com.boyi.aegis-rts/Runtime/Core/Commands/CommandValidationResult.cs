using System;

namespace AegisRTS.Core.Commands
{
    /// <summary>Represents the result of validating a command.</summary>
    public readonly struct CommandValidationResult
    {
        private CommandValidationResult(bool isValid, string error)
        {
            IsValid = isValid;
            Error = error;
        }

        /// <summary>Gets whether validation succeeded.</summary>
        public bool IsValid { get; }

        /// <summary>Gets the rejection reason, or an empty string when valid.</summary>
        public string Error { get; }

        /// <summary>Creates a successful validation result.</summary>
        public static CommandValidationResult Valid() => new CommandValidationResult(true, string.Empty);

        /// <summary>Creates a rejected validation result.</summary>
        public static CommandValidationResult Invalid(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("A validation error is required.", nameof(error));
            }

            return new CommandValidationResult(false, error);
        }
    }
}
