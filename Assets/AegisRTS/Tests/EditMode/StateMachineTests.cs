using System.Collections.Generic;
using System.Globalization;
using AegisRTS.Core.StateMachine;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class StateMachineTests
    {
        [Test]
        public void Transition_ExitsPreviousBeforeEnteringNext()
        {
            var calls = new List<string>();
            var machine = new StateMachine<List<string>>(calls);
            var idle = new RecordingState("Idle");
            var active = new RecordingState("Active");

            machine.Start(idle);
            machine.Tick(0.25d);
            machine.TransitionTo(active);
            machine.Stop();

            Assert.That(calls, Is.EqualTo(new[]
            {
                "Idle.Enter",
                "Idle.Tick:0.25",
                "Idle.Exit",
                "Active.Enter",
                "Active.Exit",
            }));
            Assert.That(machine.IsRunning, Is.False);
        }

        private sealed class RecordingState : IState<List<string>>
        {
            private readonly string _name;

            public RecordingState(string name)
            {
                _name = name;
            }

            public void Enter(List<string> context) => context.Add($"{_name}.Enter");

            public void Exit(List<string> context) => context.Add($"{_name}.Exit");

            public void Tick(List<string> context, double deltaSeconds) =>
                context.Add($"{_name}.Tick:{deltaSeconds.ToString("0.##", CultureInfo.InvariantCulture)}");
        }
    }
}
