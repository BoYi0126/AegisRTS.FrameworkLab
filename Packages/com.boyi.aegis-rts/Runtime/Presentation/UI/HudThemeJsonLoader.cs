using System;
using System.Text.Json;

namespace AegisRTS.Presentation.UI
{
    public sealed class HudThemeJsonLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip };

        public HudThemeDefinition Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("HUD theme JSON is required.", nameof(json));
            try
            {
                ThemeDocument value = JsonSerializer.Deserialize<ThemeDocument>(json, Options);
                if (value == null) throw new FormatException("HUD theme document is empty.");
                return new HudThemeDefinition(value.Id, value.DisplayName, value.Background, value.Panel,
                    value.Primary, value.Accent, value.Text, value.MutedText, value.Scale, value.PanelOpacity);
            }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException || exception is FormatException)
            { throw new FormatException("HUD theme JSON is invalid.", exception); }
        }

        public sealed class ThemeDocument
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Background { get; set; }
            public string Panel { get; set; }
            public string Primary { get; set; }
            public string Accent { get; set; }
            public string Text { get; set; }
            public string MutedText { get; set; }
            public double Scale { get; set; } = 1d;
            public double PanelOpacity { get; set; } = 0.92d;
        }
    }
}
