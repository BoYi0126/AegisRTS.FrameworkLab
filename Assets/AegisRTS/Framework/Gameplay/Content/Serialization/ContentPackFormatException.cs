using System;

namespace AegisRTS.Gameplay.Content.Serialization
{
    /// <summary>Indicates that a JSON document cannot be converted into a content pack.</summary>
    public sealed class ContentPackFormatException : FormatException
    {
        public ContentPackFormatException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
