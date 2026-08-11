using System;

namespace AegisRTS.Core.Commands
{
    /// <summary>Describes whether a command reached its registered handler.</summary>
    public readonly struct CommandDispatchResult
    {
        private CommandDispatchResult(bool wasHandled, string error)
        {
            WasHandled = wasHandled;
            Error = error;
        }

        /// <summary>Gets whether the command was accepted and handled.</summary>
        public bool WasHandled { get; }

        /// <summary>Gets the rejection reason, or an empty string when handled.</summary>
        public string Error { get; }

        /// <summary>Creates a successful dispatch result.</summary>
        public static CommandDispatchResult Handled() => new CommandDispatchResult(true, string.Empty);

        /// <summary>Creates a rejected dispatch result.</summary>
        public static CommandDispatchResult Rejected(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("A rejection reason is required.", nameof(error));
            }

            return new CommandDispatchResult(false, error);
        }
    }
}
