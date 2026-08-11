using System;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Stable, content-authored identity for a definition.</summary>
    public readonly struct DefinitionId : IEquatable<DefinitionId>, IComparable<DefinitionId>
    {
        /// <summary>Initializes a normalized definition identifier.</summary>
        public DefinitionId(string value)
        {
            Value = Normalize(value, nameof(value));
        }

        /// <summary>Gets the normalized lowercase value.</summary>
        public string Value { get; }

        /// <summary>Gets whether this value was initialized.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        /// <inheritdoc />
        public bool Equals(DefinitionId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is DefinitionId other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        /// <inheritdoc />
        public int CompareTo(DefinitionId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override string ToString() => IsValid ? Value : "Invalid";

        public static bool operator ==(DefinitionId left, DefinitionId right) => left.Equals(right);

        public static bool operator !=(DefinitionId left, DefinitionId right) => !left.Equals(right);

        private static string Normalize(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A definition ID is required.", parameterName);
            }

            string normalized = value.Trim().ToLowerInvariant();
            for (int index = 0; index < normalized.Length; index++)
            {
                char character = normalized[index];
                bool valid = character >= 'a' && character <= 'z' ||
                             character >= '0' && character <= '9' ||
                             character == '.' || character == '-' || character == '_';
                if (!valid)
                {
                    throw new ArgumentException(
                        "Definition IDs may only contain letters, numbers, '.', '-' and '_'.",
                        parameterName);
                }
            }

            return normalized;
        }
    }
}
