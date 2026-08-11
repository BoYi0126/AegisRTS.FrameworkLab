using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Technology
{
    public sealed class TechnologyCommandRouter : IDisposable
    {
        private readonly IDisposable _validator, _handler;
        public TechnologyCommandRouter(CommandBus commands, TechnologySystem technologies)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (technologies == null) throw new ArgumentNullException(nameof(technologies));
            _validator = commands.RegisterValidator<ResearchTechnologyCommand>(command =>
            { TechnologyRequestResult result = technologies.Validate(command); return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error); });
            _handler = commands.RegisterHandler<ResearchTechnologyCommand>(command => LastResult = technologies.Request(command));
        }
        public TechnologyRequestResult LastResult { get; private set; }
        public void Dispose() { _handler.Dispose(); _validator.Dispose(); }
    }
}
