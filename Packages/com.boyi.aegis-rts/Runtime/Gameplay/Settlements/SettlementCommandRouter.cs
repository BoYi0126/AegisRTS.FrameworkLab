using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Settlements
{
    public sealed class SettlementCommandRouter : IDisposable
    {
        private readonly IDisposable _validator;
        private readonly IDisposable _handler;
        private readonly SettlementSystem _settlements;

        public SettlementCommandRouter(CommandBus commands, SettlementSystem settlements)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            _settlements = settlements ?? throw new ArgumentNullException(nameof(settlements));
            _validator = commands.RegisterValidator<CaptureSettlementCommand>(command =>
            {
                SettlementCommandResult result = _settlements.Validate(command);
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            });
            _handler = commands.RegisterHandler<CaptureSettlementCommand>(command => LastResult = _settlements.Execute(command));
        }

        public SettlementCommandResult LastResult { get; private set; }
        public void Dispose() { _handler.Dispose(); _validator.Dispose(); }
    }
}
