using System;
using System.Collections.Generic;
using System.Threading;
using AegisRTS.Core.Diagnostics;

namespace AegisRTS.Core.Events
{
    /// <summary>Publishes immutable simulation events to subscribers of the exact event type.</summary>
    /// <remarks>Subscription and publishing are intended to run on the simulation thread.</remarks>
    public sealed class EventBus
    {
        private const string DiagnosticCategory = "EventBus";

        private readonly IDiagnosticSink _diagnostics;
        private readonly Dictionary<Type, List<SubscriberRegistration>> _subscribers =
            new Dictionary<Type, List<SubscriberRegistration>>();
        private long _nextRegistrationId;

        /// <summary>Initializes an event bus with an optional diagnostics sink.</summary>
        public EventBus(IDiagnosticSink diagnostics = null)
        {
            _diagnostics = diagnostics ?? NullDiagnosticSink.Instance;
        }

        /// <summary>Gets the total number of active subscriptions.</summary>
        public int SubscriberCount
        {
            get
            {
                int count = 0;
                foreach (List<SubscriberRegistration> registrations in _subscribers.Values)
                {
                    count += registrations.Count;
                }

                return count;
            }
        }

        /// <summary>Gets the active subscription count for an event type.</summary>
        public int GetSubscriberCount<TEvent>()
            where TEvent : IEvent
        {
            return _subscribers.TryGetValue(typeof(TEvent), out List<SubscriberRegistration> registrations)
                ? registrations.Count
                : 0;
        }

        /// <summary>Subscribes to an exact event type.</summary>
        public IDisposable Subscribe<TEvent>(Action<TEvent> subscriber)
            where TEvent : IEvent
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            Type eventType = typeof(TEvent);
            if (!_subscribers.TryGetValue(eventType, out List<SubscriberRegistration> registrations))
            {
                registrations = new List<SubscriberRegistration>();
                _subscribers.Add(eventType, registrations);
            }

            long registrationId = NextRegistrationId();
            registrations.Add(new SubscriberRegistration(registrationId, subscriber));
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Subscribed to {eventType.FullName}.");
            return new Subscription(() => Unsubscribe(eventType, registrationId));
        }

        /// <summary>
        /// Publishes an event to a snapshot of current subscribers in registration order.
        /// </summary>
        public void Publish<TEvent>(TEvent eventData)
            where TEvent : IEvent
        {
            if (eventData is null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            Type eventType = typeof(TEvent);
            if (!_subscribers.TryGetValue(eventType, out List<SubscriberRegistration> registrations))
            {
                _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Published {eventType.FullName} to 0 subscribers.");
                return;
            }

            SubscriberRegistration[] snapshot = registrations.ToArray();
            foreach (SubscriberRegistration registration in snapshot)
            {
                ((Action<TEvent>)registration.Callback)(eventData);
            }

            _diagnostics.Record(
                DiagnosticSeverity.Trace,
                DiagnosticCategory,
                $"Published {eventType.FullName} to {snapshot.Length} subscriber(s).");
        }

        /// <summary>Removes every active subscription.</summary>
        public void Clear()
        {
            _subscribers.Clear();
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, "Cleared all subscriptions.");
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary() => $"Subscribers={SubscriberCount}, EventTypes={_subscribers.Count}";

        private long NextRegistrationId()
        {
            if (_nextRegistrationId == long.MaxValue)
            {
                throw new InvalidOperationException("Event subscription identifier space has been exhausted.");
            }

            return ++_nextRegistrationId;
        }

        private void Unsubscribe(Type eventType, long registrationId)
        {
            if (!_subscribers.TryGetValue(eventType, out List<SubscriberRegistration> registrations))
            {
                return;
            }

            registrations.RemoveAll(registration => registration.Id == registrationId);
            if (registrations.Count == 0)
            {
                _subscribers.Remove(eventType);
            }
        }

        private sealed class SubscriberRegistration
        {
            public SubscriberRegistration(long id, Delegate callback)
            {
                Id = id;
                Callback = callback;
            }

            public long Id { get; }

            public Delegate Callback { get; }
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                Action dispose = Interlocked.Exchange(ref _dispose, null);
                dispose?.Invoke();
            }
        }
    }
}
