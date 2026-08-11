namespace AegisRTS.Core.StateMachine
{
    /// <summary>Represents one state operating on a caller-owned context.</summary>
    public interface IState<in TContext>
    {
        /// <summary>Runs immediately after the state becomes active.</summary>
        void Enter(TContext context);

        /// <summary>Runs immediately before the state stops being active.</summary>
        void Exit(TContext context);

        /// <summary>Advances the active state by scaled simulation time.</summary>
        void Tick(TContext context, double deltaSeconds);
    }
}
