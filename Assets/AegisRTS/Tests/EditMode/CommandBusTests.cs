using AegisRTS.Core.Commands;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class CommandBusTests
    {
        [Test]
        public void Dispatch_InvalidCommand_DoesNotReachHandler()
        {
            var bus = new CommandBus();
            int handledValue = 0;
            using (bus.RegisterValidator<TestCommand>(command => command.Value > 0
                       ? CommandValidationResult.Valid()
                       : CommandValidationResult.Invalid("Value must be positive.")))
            using (bus.RegisterHandler<TestCommand>(command => handledValue = command.Value))
            {
                CommandDispatchResult rejected = bus.Dispatch(new TestCommand(-1));
                Assert.That(rejected.WasHandled, Is.False);
                Assert.That(rejected.Error, Is.EqualTo("Value must be positive."));
                Assert.That(handledValue, Is.Zero);

                CommandDispatchResult accepted = bus.Dispatch(new TestCommand(7));
                Assert.That(accepted.WasHandled, Is.True);
                Assert.That(handledValue, Is.EqualTo(7));
            }
        }

        [Test]
        public void DisposedHandler_IsNoLongerUsed()
        {
            var bus = new CommandBus();
            var subscription = bus.RegisterHandler<TestCommand>(_ => { });
            subscription.Dispose();
            subscription.Dispose();

            CommandDispatchResult result = bus.Dispatch(new TestCommand(1));

            Assert.That(result.WasHandled, Is.False);
            Assert.That(bus.RegisteredHandlerCount, Is.Zero);
        }

        private readonly struct TestCommand : ICommand
        {
            public TestCommand(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
