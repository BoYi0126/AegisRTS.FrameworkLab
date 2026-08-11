using System;
using AegisRTS.Core.Commands;

namespace AegisRTS.Gameplay.Buildings
{
    public sealed class BuildingCommandRouter : IDisposable
    {
        private readonly IDisposable _validator, _handler;
        public BuildingCommandRouter(CommandBus commands, BuildingSystem buildings)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (buildings == null) throw new ArgumentNullException(nameof(buildings));
            _validator = commands.RegisterValidator<ConstructBuildingCommand>(command =>
            { BuildingRequestResult result = buildings.Validate(command); return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error); });
            _handler = commands.RegisterHandler<ConstructBuildingCommand>(command => LastResult = buildings.Request(command));
        }
        public BuildingRequestResult LastResult { get; private set; }
        public void Dispose() { _handler.Dispose(); _validator.Dispose(); }
    }
}
