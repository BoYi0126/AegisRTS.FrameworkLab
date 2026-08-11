using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using AegisRTS.Persistence.Save;

namespace AegisRTS.Persistence.Replay
{
    public sealed class ReplayCommandRecord
    {
        public ulong Tick { get; set; } public ulong Sequence { get; set; }
        public string CommandId { get; set; } = ""; public string PayloadJson { get; set; } = "{}";
    }
    public sealed class ReplayDocument
    {
        public SaveEnvelope InitialState { get; set; } = new SaveEnvelope();
        public int Seed { get; set; }
        public ReplayCommandRecord[] Commands { get; set; } = Array.Empty<ReplayCommandRecord>();
    }
    public interface IReplayCommandSink { void Execute(ReplayCommandRecord command); }

    public sealed class ReplayRecorder
    {
        private readonly SaveEnvelope _initialState; private readonly int _seed;
        private readonly List<ReplayCommandRecord> _commands = new List<ReplayCommandRecord>(); private ulong _sequence;
        public ReplayRecorder(SaveEnvelope initialState, int seed) { _initialState = initialState ?? throw new ArgumentNullException(nameof(initialState)); _seed = seed; }
        public void Record(ulong tick, string commandId, string payloadJson = "{}")
        {
            if (string.IsNullOrWhiteSpace(commandId)) throw new ArgumentException("Replay command ID is required.", nameof(commandId));
            if (_commands.Count > 0 && tick < _commands[_commands.Count - 1].Tick) throw new InvalidOperationException("Replay ticks must be recorded in non-decreasing order.");
            using (JsonDocument.Parse(payloadJson ?? "{}")) { }
            _commands.Add(new ReplayCommandRecord { Tick = tick, Sequence = _sequence++, CommandId = commandId.Trim(), PayloadJson = payloadJson ?? "{}" });
        }
        public ReplayDocument Build() => new ReplayDocument { InitialState = _initialState, Seed = _seed, Commands = _commands.ToArray() };
    }

    public sealed class ReplayPlayer
    {
        private readonly IReadOnlyList<ReplayCommandRecord> _commands; private readonly IReplayCommandSink _sink; private int _index;
        public ReplayPlayer(ReplayDocument replay, IReplayCommandSink sink)
        {
            if (replay == null) throw new ArgumentNullException(nameof(replay)); _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _commands = new ReadOnlyCollection<ReplayCommandRecord>((replay.Commands ?? Array.Empty<ReplayCommandRecord>())
                .OrderBy(item => item.Tick).ThenBy(item => item.Sequence).ToList()); Seed = replay.Seed; InitialState = replay.InitialState;
        }
        public int Seed { get; }
        public SaveEnvelope InitialState { get; }
        public ulong CurrentTick { get; private set; }
        public bool IsComplete => _index >= _commands.Count;
        public int AdvanceTo(ulong tick)
        {
            if (tick < CurrentTick) throw new InvalidOperationException("Replay cannot move backwards."); int executed = 0;
            while (_index < _commands.Count && _commands[_index].Tick <= tick) { _sink.Execute(_commands[_index++]); executed++; }
            CurrentTick = tick; return executed;
        }
    }

    public sealed class ReplayJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = true };
        public string Serialize(ReplayDocument replay) => JsonSerializer.Serialize(replay ?? throw new ArgumentNullException(nameof(replay)), Options);
        public ReplayDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Replay JSON is required.", nameof(json));
            ReplayDocument value = JsonSerializer.Deserialize<ReplayDocument>(json, Options);
            if (value?.InitialState == null) throw new FormatException("Replay initial state is required.");
            return value;
        }
    }
}
