namespace AegisRTS.Core.Commands
{
    /// <summary>Handles validated commands of one type.</summary>
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        /// <summary>Executes a validated command.</summary>
        void Handle(TCommand command);
    }
}
