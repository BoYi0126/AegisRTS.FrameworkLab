using System;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Content-neutral capability or classification tag.</summary>
    public readonly struct ContentTag : IEquatable<ContentTag>, IComparable<ContentTag>
    {
        /// <summary>Initializes a normalized content tag.</summary>
        public ContentTag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A content tag is required.", nameof(value));
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
                        "Content tags may only contain letters, numbers, '.', '-' and '_'.",
                        nameof(value));
                }
            }

            Value = normalized;
        }

        /// <summary>Gets the normalized lowercase value.</summary>
        public string Value { get; }

        /// <summary>Gets whether this value was initialized.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        /// <inheritdoc />
        public bool Equals(ContentTag other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ContentTag other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        /// <inheritdoc />
        public int CompareTo(ContentTag other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override string ToString() => IsValid ? Value : "Invalid";

        public static bool operator ==(ContentTag left, ContentTag right) => left.Equals(right);

        public static bool operator !=(ContentTag left, ContentTag right) => !left.Equals(right);
    }
}
