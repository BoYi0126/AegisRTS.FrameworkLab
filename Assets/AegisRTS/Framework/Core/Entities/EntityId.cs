using System;
using System.Globalization;

namespace AegisRTS.Core.Entities
{
    /// <summary>
    /// Identifies a runtime entity independently from any Unity object or content-specific name.
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>, IComparable<EntityId>
    {
        /// <summary>
        /// Represents an entity identifier that has not been assigned.
        /// </summary>
        public static readonly EntityId Invalid = default;

        /// <summary>
        /// Initializes an identifier from its persistent numeric value.
        /// </summary>
        /// <param name="value">The identifier value. Zero represents <see cref="Invalid"/>.</param>
        public EntityId(ulong value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the numeric value suitable for serialization.
        /// </summary>
        public ulong Value { get; }

        /// <summary>
        /// Gets whether this identifier has been assigned.
        /// </summary>
        public bool IsValid => Value != 0;

        /// <inheritdoc />
        public bool Equals(EntityId other) => Value == other.Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is EntityId other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(EntityId other) => Value.CompareTo(other.Value);

        /// <inheritdoc />
        public override string ToString() => IsValid
            ? Value.ToString(CultureInfo.InvariantCulture)
            : "Invalid";

        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

        public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);

        public static bool operator <(EntityId left, EntityId right) => left.Value < right.Value;

        public static bool operator >(EntityId left, EntityId right) => left.Value > right.Value;
    }
}
