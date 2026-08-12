using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Siege
{
    /// <summary>Routes all siege intent through the shared validation and command flow.</summary>
    public sealed class SiegeCommandRouter : IDisposable
    {
        private readonly IDisposable[] _registrations;
        public SiegeCommandRouter(CommandBus commands, SiegeSystem sieges)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (sieges == null) throw new ArgumentNullException(nameof(sieges));
            _registrations = new IDisposable[]
            {
                commands.RegisterValidator<StartSiegeCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<StartSiegeCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<AttackDefenseStructureCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<AttackDefenseStructureCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<RepairDefenseStructureCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<RepairDefenseStructureCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<SetGateStateCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<SetGateStateCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<EnterSiegeAreaCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<EnterSiegeAreaCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<ReportSiegeConditionCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<ReportSiegeConditionCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<CompleteSiegeWaveCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<CompleteSiegeWaveCommand>(value => LastResult = sieges.Execute(value)),
                commands.RegisterValidator<CaptureSiegeCommand>(value => Result(sieges.Validate(value))),
                commands.RegisterHandler<CaptureSiegeCommand>(value => LastResult = sieges.Execute(value)),
            };
        }
        public SiegeActionResult LastResult { get; private set; }
        public void Dispose() { for (int i = _registrations.Length - 1; i >= 0; i--) _registrations[i].Dispose(); }
        private static CommandValidationResult Result(SiegeActionResult value) => value.Succeeded
            ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(value.Error);
    }
}
