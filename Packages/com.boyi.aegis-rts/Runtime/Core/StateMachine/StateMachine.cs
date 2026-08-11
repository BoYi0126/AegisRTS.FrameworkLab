using System;
using AegisRTS.Core.Diagnostics;

namespace AegisRTS.Core.StateMachine
{
    /// <summary>Coordinates explicit lifecycle transitions between states.</summary>
    public sealed class StateMachine<TContext>
    {
        private const string DiagnosticCategory = "StateMachine";
        private readonly IDiagnosticSink _diagnostics;

        /// <summary>Initializes a state machine around a caller-owned context.</summary>
        public StateMachine(TContext context, IDiagnosticSink diagnostics = null)
        {
            Context = context;
            _diagnostics = diagnostics ?? NullDiagnosticSink.Instance;
        }

        /// <summary>Gets the context supplied to every state callback.</summary>
        public TContext Context { get; }

        /// <summary>Gets the active state, or null before start and after stop.</summary>
        public IState<TContext> CurrentState { get; private set; }

        /// <summary>Gets whether a state is active.</summary>
        public bool IsRunning => CurrentState != null;

        /// <summary>Starts the machine with its initial state.</summary>
        public void Start(IState<TContext> initialState)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            if (IsRunning)
            {
                throw new InvalidOperationException("The state machine has already started.");
            }

            CurrentState = initialState;
            CurrentState.Enter(Context);
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Started {GetStateName(initialState)}.");
        }

        /// <summary>Exits the current state and enters <paramref name="nextState"/>.</summary>
        public void TransitionTo(IState<TContext> nextState)
        {
            if (nextState == null)
            {
                throw new ArgumentNullException(nameof(nextState));
            }

            if (!IsRunning)
            {
                throw new InvalidOperationException("The state machine must be started before transitioning.");
            }

            IState<TContext> previousState = CurrentState;
            previousState.Exit(Context);
            CurrentState = nextState;
            CurrentState.Enter(Context);
            _diagnostics.Record(
                DiagnosticSeverity.Trace,
                DiagnosticCategory,
                $"Transitioned {GetStateName(previousState)} -> {GetStateName(nextState)}.");
        }

        /// <summary>Advances the active state.</summary>
        public void Tick(double deltaSeconds)
        {
            if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be finite and non-negative.");
            }

            if (!IsRunning)
            {
                throw new InvalidOperationException("The state machine must be started before ticking.");
            }

            CurrentState.Tick(Context, deltaSeconds);
        }

        /// <summary>Exits the active state and stops the machine.</summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            IState<TContext> previousState = CurrentState;
            previousState.Exit(Context);
            CurrentState = null;
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Stopped {GetStateName(previousState)}.");
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary() => IsRunning
            ? $"Running=True, State={GetStateName(CurrentState)}"
            : "Running=False, State=None";

        private static string GetStateName(IState<TContext> state) => state.GetType().FullName;
    }
}
