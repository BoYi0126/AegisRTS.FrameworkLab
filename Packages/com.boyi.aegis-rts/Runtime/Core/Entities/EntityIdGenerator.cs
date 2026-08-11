using System;

namespace AegisRTS.Core.Entities
{
    /// <summary>
    /// Produces deterministic, monotonically increasing entity identifiers.
    /// </summary>
    /// <remarks>This type is intended for single-threaded simulation ownership.</remarks>
    public sealed class EntityIdGenerator
    {
        private ulong _nextValue;

        /// <summary>
        /// Initializes a generator.
        /// </summary>
        /// <param name="firstValue">The first non-zero value to issue.</param>
        public EntityIdGenerator(ulong firstValue = 1)
        {
            if (firstValue == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstValue), "Entity identifiers must be non-zero.");
            }

            _nextValue = firstValue;
        }

        /// <summary>
        /// Gets the value that will be issued next, or zero after the identifier space is exhausted.
        /// </summary>
        public ulong NextValue => _nextValue;

        /// <summary>
        /// Issues the next identifier.
        /// </summary>
        /// <exception cref="InvalidOperationException">The identifier space has been exhausted.</exception>
        public EntityId Next()
        {
            if (_nextValue == 0)
            {
                throw new InvalidOperationException("The entity identifier space has been exhausted.");
            }

            var result = new EntityId(_nextValue);
            _nextValue = unchecked(_nextValue + 1);
            return result;
        }

        /// <summary>
        /// Resets the sequence, primarily for deterministic scenario and test setup.
        /// </summary>
        public void Reset(ulong firstValue = 1)
        {
            if (firstValue == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstValue), "Entity identifiers must be non-zero.");
            }

            _nextValue = firstValue;
        }
    }
}
