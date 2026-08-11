using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Armies
{
    /// <summary>Registers ArmySystem validation and handlers on the shared CommandBus.</summary>
    public sealed class ArmyCommandRouter : IDisposable
    {
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private readonly ArmySystem _armies;

        public ArmyCommandRouter(CommandBus commands, ArmySystem armies)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            _armies = armies ?? throw new ArgumentNullException(nameof(armies));
            Register(commands, (CreateArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (MergeArmiesCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (SplitArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (AssignArmyCommanderCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (MoveArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (AttackArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (AttackSettlementArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (DefendArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
            Register(commands, (RetreatArmyCommand command) => _armies.Validate(command), command => _armies.Execute(command));
        }

        public ArmyCommandResult LastResult { get; private set; }

        public void Dispose()
        {
            foreach (IDisposable registration in _registrations) registration.Dispose();
            _registrations.Clear();
        }

        private void Register<TCommand>(CommandBus commands, Func<TCommand, ArmyCommandResult> validate,
            Func<TCommand, ArmyCommandResult> execute) where TCommand : ICommand
        {
            _registrations.Add(commands.RegisterValidator<TCommand>(command => ToValidation(validate(command))));
            _registrations.Add(commands.RegisterHandler<TCommand>(command => LastResult = execute(command)));
        }

        private static CommandValidationResult ToValidation(ArmyCommandResult result) => result.Succeeded
            ? CommandValidationResult.Valid()
            : CommandValidationResult.Invalid(result.Error);
    }
}
