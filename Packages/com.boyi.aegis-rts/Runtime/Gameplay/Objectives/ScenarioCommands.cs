using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Objectives
{
    public sealed class StartScenarioCommand : ICommand
    {
        public StartScenarioCommand(ScenarioDefinition definition) { Definition = definition; }
        public ScenarioDefinition Definition { get; }
    }

    public sealed class AddScenarioFactCommand : ICommand
    {
        public AddScenarioFactCommand(string factId, double amount) { FactId = factId; Amount = amount; }
        public string FactId { get; }
        public double Amount { get; }
    }

    public sealed class SetScenarioFactCommand : ICommand
    {
        public SetScenarioFactCommand(string factId, double value) { FactId = factId; Value = value; }
        public string FactId { get; }
        public double Value { get; }
    }

    /// <summary>Routes Player, AI, Scenario, and Test intent through the shared CommandBus.</summary>
    public sealed class ScenarioCommandRouter : IDisposable
    {
        private readonly IDisposable[] _registrations;
        public ScenarioCommandRouter(CommandBus commands, ScenarioSystem scenarios)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (scenarios == null) throw new ArgumentNullException(nameof(scenarios));
            _registrations = new[]
            {
                commands.RegisterValidator<StartScenarioCommand>(command =>
                    command?.Definition == null ? CommandValidationResult.Invalid("Scenario definition is required.") :
                    scenarios.Status == ScenarioStatus.Running ? CommandValidationResult.Invalid("A scenario is already running.") : CommandValidationResult.Valid()),
                commands.RegisterHandler<StartScenarioCommand>(command => scenarios.Start(command.Definition)),
                commands.RegisterValidator<AddScenarioFactCommand>(command => ValidateFact(command?.FactId, command?.Amount ?? double.NaN, scenarios)),
                commands.RegisterHandler<AddScenarioFactCommand>(command => scenarios.AddFact(command.FactId, command.Amount)),
                commands.RegisterValidator<SetScenarioFactCommand>(command => ValidateFact(command?.FactId, command?.Value ?? double.NaN, scenarios)),
                commands.RegisterHandler<SetScenarioFactCommand>(command => scenarios.SetFact(command.FactId, command.Value)),
            };
        }

        public void Dispose() { foreach (IDisposable registration in _registrations) registration.Dispose(); }

        private static CommandValidationResult ValidateFact(string factId, double value, ScenarioSystem scenarios)
        {
            if (scenarios.Status != ScenarioStatus.Running) return CommandValidationResult.Invalid("No scenario is running.");
            if (string.IsNullOrWhiteSpace(factId)) return CommandValidationResult.Invalid("Fact ID is required.");
            if (double.IsNaN(value) || double.IsInfinity(value)) return CommandValidationResult.Invalid("Fact value must be finite.");
            return CommandValidationResult.Valid();
        }
    }
}
