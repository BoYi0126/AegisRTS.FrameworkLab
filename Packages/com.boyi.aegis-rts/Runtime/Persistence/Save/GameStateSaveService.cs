using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AegisRTS.Persistence.Save
{
    public sealed class SaveLoadException : Exception
    {
        public SaveLoadException(string message) : base(message) { }
        public SaveLoadException(string message, Exception inner) : base(message, inner) { }
    }

    public interface IGameStateCaptureSource { GameStateDocument CaptureGameState(); }
    public interface IGameStateRestoreSink { void RestoreGameState(GameStateDocument state); }

    public sealed class GameStateSaveService
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = true };
        private readonly string _saveVersion, _frameworkVersion, _contentVersion;

        public GameStateSaveService(string saveVersion, string frameworkVersion, string contentVersion)
        {
            _saveVersion = Required(saveVersion, nameof(saveVersion));
            _frameworkVersion = Required(frameworkVersion, nameof(frameworkVersion));
            _contentVersion = Required(contentVersion, nameof(contentVersion));
        }

        public SaveMetadata CreateMetadata(string scenarioId, DateTimeOffset? timestamp = null) => new SaveMetadata
        {
            SaveVersion = _saveVersion, FrameworkVersion = _frameworkVersion, ContentVersion = _contentVersion,
            ScenarioId = Required(scenarioId, nameof(scenarioId)), Timestamp = timestamp ?? DateTimeOffset.UtcNow,
        };

        public string Serialize(GameStateDocument state, SaveMetadata metadata)
        {
            if (state == null) throw new ArgumentNullException(nameof(state)); if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            ValidateCompatibility(metadata);
            var envelope = new SaveEnvelope { Metadata = metadata, State = state };
            envelope.Checksum = ComputeChecksum(metadata, state);
            return JsonSerializer.Serialize(envelope, Options);
        }

        public SaveEnvelope Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Save JSON is required.", nameof(json));
            try
            {
                SaveEnvelope envelope = JsonSerializer.Deserialize<SaveEnvelope>(json, Options);
                if (envelope?.Metadata == null || envelope.State == null) throw new SaveLoadException("Save envelope is incomplete.");
                ValidateEnvelope(envelope); return envelope;
            }
            catch (SaveLoadException) { throw; }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException)
            { throw new SaveLoadException("Save JSON is invalid.", exception); }
        }

        public void ValidateEnvelope(SaveEnvelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            ValidateCompatibility(envelope.Metadata);
            string expected = ComputeChecksum(envelope.Metadata, envelope.State);
            if (!string.Equals(expected, envelope.Checksum, StringComparison.OrdinalIgnoreCase))
                throw new SaveLoadException("Save checksum validation failed.");
        }

        public string Fingerprint(GameStateDocument state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return Hash(JsonSerializer.Serialize(state, Options));
        }

        private void ValidateCompatibility(SaveMetadata metadata)
        {
            if (metadata == null) throw new SaveLoadException("Save metadata is missing.");
            if (!string.Equals(metadata.SaveVersion, _saveVersion, StringComparison.Ordinal))
                throw new SaveLoadException($"Unsupported save version '{metadata.SaveVersion}'. Expected '{_saveVersion}'.");
            if (!string.Equals(metadata.FrameworkVersion, _frameworkVersion, StringComparison.Ordinal))
                throw new SaveLoadException($"Framework version '{metadata.FrameworkVersion}' is incompatible.");
            if (!string.Equals(metadata.ContentVersion, _contentVersion, StringComparison.Ordinal))
                throw new SaveLoadException($"Content version '{metadata.ContentVersion}' is incompatible.");
            if (string.IsNullOrWhiteSpace(metadata.ScenarioId)) throw new SaveLoadException("Scenario ID is required.");
        }

        private static string ComputeChecksum(SaveMetadata metadata, GameStateDocument state) =>
            Hash(JsonSerializer.Serialize(new IntegrityPayload { Metadata = metadata, State = state }, Options));

        private static string Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", string.Empty);
        }
        private static string Required(string value, string name)
        { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", name); return value.Trim(); }
        private sealed class IntegrityPayload { public SaveMetadata Metadata { get; set; } public GameStateDocument State { get; set; } }
    }

    public sealed class GameStateCoordinator
    {
        private readonly GameStateSaveService _service;
        public GameStateCoordinator(GameStateSaveService service) { _service = service ?? throw new ArgumentNullException(nameof(service)); }
        public string Save(IGameStateCaptureSource source, SaveMetadata metadata) =>
            _service.Serialize((source ?? throw new ArgumentNullException(nameof(source))).CaptureGameState(), metadata);
        public SaveEnvelope Load(string json, IGameStateRestoreSink sink)
        {
            SaveEnvelope envelope = _service.Deserialize(json);
            (sink ?? throw new ArgumentNullException(nameof(sink))).RestoreGameState(envelope.State); return envelope;
        }
    }

    public interface ISaveStore { void Write(string slotId, string json); bool TryRead(string slotId, out string json); }
    public sealed class MemorySaveStore : ISaveStore
    {
        private readonly System.Collections.Generic.Dictionary<string, string> _slots = new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);
        public void Write(string slotId, string json) { _slots[RequiredSlot(slotId)] = json ?? throw new ArgumentNullException(nameof(json)); }
        public bool TryRead(string slotId, out string json) => _slots.TryGetValue(RequiredSlot(slotId), out json);
        private static string RequiredSlot(string value) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Slot ID is required.", nameof(value)); return value.Trim(); }
    }

    public sealed class FileSaveStore : ISaveStore
    {
        private readonly string _directory;
        public FileSaveStore(string directory) { _directory = Path.GetFullPath(directory ?? throw new ArgumentNullException(nameof(directory))); Directory.CreateDirectory(_directory); }
        public void Write(string slotId, string json)
        {
            string path = PathFor(slotId), temporary = path + ".tmp"; File.WriteAllText(temporary, json ?? throw new ArgumentNullException(nameof(json)), Encoding.UTF8);
            if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
        }
        public bool TryRead(string slotId, out string json)
        { string path = PathFor(slotId); if (!File.Exists(path)) { json = null; return false; } json = File.ReadAllText(path, Encoding.UTF8); return true; }
        private string PathFor(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) throw new ArgumentException("Slot ID is required.", nameof(slotId));
            foreach (char value in slotId) if (!char.IsLetterOrDigit(value) && value != '-' && value != '_') throw new ArgumentException("Slot ID contains invalid characters.", nameof(slotId));
            return Path.Combine(_directory, slotId + ".json");
        }
    }
}
