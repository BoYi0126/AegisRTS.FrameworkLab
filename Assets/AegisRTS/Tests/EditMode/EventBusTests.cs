using AegisRTS.Core.Events;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class EventBusTests
    {
        [Test]
        public void SubscribeAndDispose_ControlEventDelivery()
        {
            var bus = new EventBus();
            int receivedValue = 0;
            var subscription = bus.Subscribe<TestEvent>(eventData => receivedValue += eventData.Value);

            bus.Publish(new TestEvent(3));
            subscription.Dispose();
            subscription.Dispose();
            bus.Publish(new TestEvent(5));

            Assert.That(receivedValue, Is.EqualTo(3));
            Assert.That(bus.GetSubscriberCount<TestEvent>(), Is.Zero);
        }

        [Test]
        public void Publish_UsesSnapshotWhenSubscriberUnsubscribes()
        {
            var bus = new EventBus();
            int calls = 0;
            System.IDisposable first = null;
            first = bus.Subscribe<TestEvent>(_ =>
            {
                calls++;
                first.Dispose();
            });
            using (bus.Subscribe<TestEvent>(_ => calls++))
            {
                bus.Publish(new TestEvent(1));
                bus.Publish(new TestEvent(1));
            }

            Assert.That(calls, Is.EqualTo(3));
        }

        private readonly struct TestEvent : IEvent
        {
            public TestEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
