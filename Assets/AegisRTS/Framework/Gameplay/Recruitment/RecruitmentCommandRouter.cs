using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Recruitment
{
    public sealed class RecruitmentCommandRouter : IDisposable
    {
        private readonly IDisposable _validator, _handler;
        public RecruitmentCommandRouter(CommandBus commands, RecruitmentSystem recruitment)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (recruitment == null) throw new ArgumentNullException(nameof(recruitment));
            _validator = commands.RegisterValidator<RecruitUnitCommand>(command =>
            { RecruitmentRequestResult result = recruitment.Validate(command); return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error); });
            _handler = commands.RegisterHandler<RecruitUnitCommand>(command => LastResult = recruitment.Request(command));
        }
        public RecruitmentRequestResult LastResult { get; private set; }
        public void Dispose() { _handler.Dispose(); _validator.Dispose(); }
    }
}
