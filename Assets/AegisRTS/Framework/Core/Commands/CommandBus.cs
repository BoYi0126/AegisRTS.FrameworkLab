using System;
using System.Collections.Generic;
using System.Threading;
using AegisRTS.Core.Diagnostics;

namespace AegisRTS.Core.Commands
{
    /// <summary>
    /// Validates and dispatches commands through one handler per command type.
    /// </summary>
    /// <remarks>Registration and dispatch are intended to run on the simulation thread.</remarks>
    public sealed class CommandBus
    {
        private const string DiagnosticCategory = "CommandBus";

        private readonly IDiagnosticSink _diagnostics;
        private readonly Dictionary<Type, HandlerRegistration> _handlers =
            new Dictionary<Type, HandlerRegistration>();
        private readonly Dictionary<Type, List<ValidatorRegistration>> _validators =
            new Dictionary<Type, List<ValidatorRegistration>>();
        private long _nextRegistrationId;

        /// <summary>Initializes a command bus with an optional diagnostics sink.</summary>
        public CommandBus(IDiagnosticSink diagnostics = null)
        {
            _diagnostics = diagnostics ?? NullDiagnosticSink.Instance;
        }

        /// <summary>Gets the number of command types with registered handlers.</summary>
        public int RegisteredHandlerCount => _handlers.Count;

        /// <summary>Gets the total number of registered validators.</summary>
        public int RegisteredValidatorCount
        {
            get
            {
                int count = 0;
                foreach (List<ValidatorRegistration> registrations in _validators.Values)
                {
                    count += registrations.Count;
                }

                return count;
            }
        }

        /// <summary>Registers the sole handler for a command type.</summary>
        public IDisposable RegisterHandler<TCommand>(Action<TCommand> handler)
            where TCommand : ICommand
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type commandType = typeof(TCommand);
            if (_handlers.ContainsKey(commandType))
            {
                throw new InvalidOperationException($"A handler is already registered for {commandType.FullName}.");
            }

            long registrationId = NextRegistrationId();
            _handlers.Add(commandType, new HandlerRegistration(registrationId, handler));
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Registered handler for {commandType.FullName}.");

            return new Subscription(() => RemoveHandler(commandType, registrationId));
        }

        /// <summary>Registers the sole handler for a command type.</summary>
        public IDisposable RegisterHandler<TCommand>(ICommandHandler<TCommand> handler)
            where TCommand : ICommand
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return RegisterHandler<TCommand>(handler.Handle);
        }

        /// <summary>Registers a validator. Validators run in registration order.</summary>
        public IDisposable RegisterValidator<TCommand>(Func<TCommand, CommandValidationResult> validator)
            where TCommand : ICommand
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            Type commandType = typeof(TCommand);
            if (!_validators.TryGetValue(commandType, out List<ValidatorRegistration> registrations))
            {
                registrations = new List<ValidatorRegistration>();
                _validators.Add(commandType, registrations);
            }

            long registrationId = NextRegistrationId();
            registrations.Add(new ValidatorRegistration(registrationId, validator));
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Registered validator for {commandType.FullName}.");

            return new Subscription(() => RemoveValidator(commandType, registrationId));
        }

        /// <summary>Registers a validator. Validators run in registration order.</summary>
        public IDisposable RegisterValidator<TCommand>(ICommandValidator<TCommand> validator)
            where TCommand : ICommand
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            return RegisterValidator<TCommand>(validator.Validate);
        }

        /// <summary>Validates and dispatches a command to its handler.</summary>
        public CommandDispatchResult Dispatch<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);
            if (command is null)
            {
                return Reject(commandType, "Command cannot be null.");
            }

            if (_validators.TryGetValue(commandType, out List<ValidatorRegistration> registrations))
            {
                ValidatorRegistration[] snapshot = registrations.ToArray();
                foreach (ValidatorRegistration registration in snapshot)
                {
                    var validator = (Func<TCommand, CommandValidationResult>)registration.Callback;
                    CommandValidationResult validation = validator(command);
                    if (!validation.IsValid)
                    {
                        string error = string.IsNullOrWhiteSpace(validation.Error)
                            ? "Command validation failed without a reason."
                            : validation.Error;
                        return Reject(commandType, error);
                    }
                }
            }

            if (!_handlers.TryGetValue(commandType, out HandlerRegistration handlerRegistration))
            {
                return Reject(commandType, $"No handler is registered for {commandType.FullName}.");
            }

            ((Action<TCommand>)handlerRegistration.Callback)(command);
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, $"Handled {commandType.FullName}.");
            return CommandDispatchResult.Handled();
        }

        /// <summary>Removes all handlers and validators.</summary>
        public void Clear()
        {
            _handlers.Clear();
            _validators.Clear();
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, "Cleared all registrations.");
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary() =>
            $"Handlers={RegisteredHandlerCount}, Validators={RegisteredValidatorCount}";

        private CommandDispatchResult Reject(Type commandType, string error)
        {
            _diagnostics.Record(DiagnosticSeverity.Warning, DiagnosticCategory, $"Rejected {commandType.FullName}: {error}");
            return CommandDispatchResult.Rejected(error);
        }

        private long NextRegistrationId()
        {
            if (_nextRegistrationId == long.MaxValue)
            {
                throw new InvalidOperationException("Command registration identifier space has been exhausted.");
            }

            return ++_nextRegistrationId;
        }

        private void RemoveHandler(Type commandType, long registrationId)
        {
            if (_handlers.TryGetValue(commandType, out HandlerRegistration registration) &&
                registration.Id == registrationId)
            {
                _handlers.Remove(commandType);
            }
        }

        private void RemoveValidator(Type commandType, long registrationId)
        {
            if (!_validators.TryGetValue(commandType, out List<ValidatorRegistration> registrations))
            {
                return;
            }

            registrations.RemoveAll(registration => registration.Id == registrationId);
            if (registrations.Count == 0)
            {
                _validators.Remove(commandType);
            }
        }

        private sealed class HandlerRegistration
        {
            public HandlerRegistration(long id, Delegate callback)
            {
                Id = id;
                Callback = callback;
            }

            public long Id { get; }

            public Delegate Callback { get; }
        }

        private sealed class ValidatorRegistration
        {
            public ValidatorRegistration(long id, Delegate callback)
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
