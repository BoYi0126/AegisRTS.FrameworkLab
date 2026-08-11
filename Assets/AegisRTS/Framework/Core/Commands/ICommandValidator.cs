namespace AegisRTS.Core.Commands
{
    /// <summary>Validates a command before it reaches its handler.</summary>
    public interface ICommandValidator<in TCommand>
        where TCommand : ICommand
    {
        /// <summary>Returns whether the command is currently valid.</summary>
        CommandValidationResult Validate(TCommand command);
    }
}
