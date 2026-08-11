using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AegisRTS.Gameplay.Debugging
{
    public enum DebugCommandType
    {
        Spawn, Kill, Damage, GiveResource, Capture, SetSpeed, ToggleAi, ShowPath, ShowThreat,
    }

    public sealed class DebugCommandRequest
    {
        public DebugCommandRequest(DebugCommandType type, IEnumerable<string> arguments)
        { Type = type; Arguments = new ReadOnlyCollection<string>(new List<string>(arguments ?? Array.Empty<string>())); }
        public DebugCommandType Type { get; }
        public IReadOnlyList<string> Arguments { get; }
    }

    public readonly struct DebugCommandResult
    {
        private DebugCommandResult(bool succeeded, string output) { Succeeded = succeeded; Output = output ?? string.Empty; }
        public bool Succeeded { get; }
        public string Output { get; }
        public static DebugCommandResult Success(string output = "OK") => new DebugCommandResult(true, output);
        public static DebugCommandResult Failure(string error) => new DebugCommandResult(false, string.IsNullOrWhiteSpace(error) ? "Debug command failed." : error);
    }

    public interface IDebugCommandExecutor { DebugCommandResult Execute(DebugCommandRequest request); }

    /// <summary>Parses development-only commands and delegates all mutations to an injected executor.</summary>
    public sealed class DebugConsole
    {
        private static readonly Dictionary<string, CommandSpec> Specs = new Dictionary<string, CommandSpec>(StringComparer.OrdinalIgnoreCase)
        {
            { "spawn", new CommandSpec(DebugCommandType.Spawn, 1, 4) }, { "kill", new CommandSpec(DebugCommandType.Kill, 1, 1) },
            { "damage", new CommandSpec(DebugCommandType.Damage, 2, 2) }, { "give_resource", new CommandSpec(DebugCommandType.GiveResource, 3, 3) },
            { "capture", new CommandSpec(DebugCommandType.Capture, 2, 2) }, { "set_speed", new CommandSpec(DebugCommandType.SetSpeed, 1, 1) },
            { "toggle_ai", new CommandSpec(DebugCommandType.ToggleAi, 1, 1) }, { "show_path", new CommandSpec(DebugCommandType.ShowPath, 1, 1) },
            { "show_threat", new CommandSpec(DebugCommandType.ShowThreat, 1, 1) },
        };
        private readonly IDebugCommandExecutor _executor;
        public DebugConsole(IDebugCommandExecutor executor, bool enabled = false) { _executor = executor ?? throw new ArgumentNullException(nameof(executor)); Enabled = enabled; }
        public bool Enabled { get; set; }
        public IReadOnlyList<string> CommandNames => new ReadOnlyCollection<string>(new List<string>(Specs.Keys));

        public DebugCommandResult Execute(string input)
        {
            if (!Enabled) return DebugCommandResult.Failure("Debug console is disabled.");
            IReadOnlyList<string> tokens;
            try { tokens = Tokenize(input); } catch (FormatException exception) { return DebugCommandResult.Failure(exception.Message); }
            if (tokens.Count == 0) return DebugCommandResult.Failure("Command is required.");
            if (!Specs.TryGetValue(tokens[0], out CommandSpec spec)) return DebugCommandResult.Failure($"Unknown debug command '{tokens[0]}'.");
            int count = tokens.Count - 1;
            if (count < spec.MinimumArguments || count > spec.MaximumArguments)
                return DebugCommandResult.Failure($"Command '{tokens[0]}' expects {spec.MinimumArguments}..{spec.MaximumArguments} argument(s).");
            var arguments = new string[count]; for (int i = 0; i < count; i++) arguments[i] = tokens[i + 1];
            return _executor.Execute(new DebugCommandRequest(spec.Type, arguments));
        }

        private static IReadOnlyList<string> Tokenize(string input)
        {
            var result = new List<string>(); if (string.IsNullOrWhiteSpace(input)) return result;
            var current = new System.Text.StringBuilder(); bool quoted = false;
            for (int i = 0; i < input.Length; i++)
            {
                char value = input[i];
                if (value == '"') { quoted = !quoted; continue; }
                if (char.IsWhiteSpace(value) && !quoted)
                { if (current.Length > 0) { result.Add(current.ToString()); current.Clear(); } continue; }
                current.Append(value);
            }
            if (quoted) throw new FormatException("Debug command contains an unterminated quote.");
            if (current.Length > 0) result.Add(current.ToString()); return result;
        }

        private readonly struct CommandSpec
        {
            public CommandSpec(DebugCommandType type, int min, int max) { Type = type; MinimumArguments = min; MaximumArguments = max; }
            public DebugCommandType Type { get; }
            public int MinimumArguments { get; }
            public int MaximumArguments { get; }
        }
    }
}
