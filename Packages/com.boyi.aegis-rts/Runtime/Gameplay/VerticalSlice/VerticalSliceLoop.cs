using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AegisRTS.Gameplay.VerticalSlice
{
    public enum VerticalSliceStage
    { Start, Income, Recruit, Army, Move, FieldBattle, Siege, BreakGate, Enter, Capture, Victory, Defeat }

    public enum VerticalSliceStepStatus { Completed, Waiting, Defeated }

    public readonly struct VerticalSliceStepResult
    {
        private VerticalSliceStepResult(VerticalSliceStepStatus status, string message)
        { Status = status; Message = message ?? string.Empty; }
        public VerticalSliceStepStatus Status { get; }
        public string Message { get; }
        public static VerticalSliceStepResult Completed(string message = "") => new VerticalSliceStepResult(VerticalSliceStepStatus.Completed, message);
        public static VerticalSliceStepResult Waiting(string message = "") => new VerticalSliceStepResult(VerticalSliceStepStatus.Waiting, message);
        public static VerticalSliceStepResult Defeated(string message) => new VerticalSliceStepResult(VerticalSliceStepStatus.Defeated, message);
    }

    public interface IVerticalSliceStepExecutor
    { VerticalSliceStepResult Execute(VerticalSliceStage stage); }

    /// <summary>Deterministic framework loop; world content is supplied by definition bindings.</summary>
    public sealed class VerticalSliceLoop
    {
        private readonly IVerticalSliceStepExecutor _executor;
        private readonly List<VerticalSliceStage> _history = new List<VerticalSliceStage>();
        public VerticalSliceLoop(IVerticalSliceStepExecutor executor) => _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        public VerticalSliceStage CurrentStage { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsDefeated { get; private set; }
        public string LastMessage { get; private set; } = string.Empty;
        public IReadOnlyList<VerticalSliceStage> History => new ReadOnlyCollection<VerticalSliceStage>(_history);

        public void Begin()
        { _history.Clear(); CurrentStage = VerticalSliceStage.Start; IsRunning = true; IsCompleted = false; IsDefeated = false; LastMessage = string.Empty; }

        public VerticalSliceStepResult Tick()
        {
            if (!IsRunning) return VerticalSliceStepResult.Waiting("Loop is not running.");
            VerticalSliceStepResult result = _executor.Execute(CurrentStage); LastMessage = result.Message;
            if (result.Status == VerticalSliceStepStatus.Waiting) return result;
            if (result.Status == VerticalSliceStepStatus.Defeated)
            { CurrentStage = VerticalSliceStage.Defeat; _history.Add(CurrentStage); IsRunning = false; IsDefeated = true; return result; }
            _history.Add(CurrentStage);
            if (CurrentStage == VerticalSliceStage.Victory)
            { IsRunning = false; IsCompleted = true; return result; }
            CurrentStage = (VerticalSliceStage)((int)CurrentStage + 1);
            return result;
        }

        public bool RunToCompletion(int maximumTicks = 64)
        {
            for (int i = 0; i < maximumTicks && IsRunning; i++) Tick();
            return IsCompleted;
        }
    }
}
