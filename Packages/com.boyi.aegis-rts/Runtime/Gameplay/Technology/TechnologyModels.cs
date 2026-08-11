using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Technology
{
    public sealed class ResearchTechnologyCommand : ICommand
    {
        public ResearchTechnologyCommand(EntityId factionId, DefinitionId technologyId)
            : this(factionId, factionId, technologyId) { }
        public ResearchTechnologyCommand(EntityId accountId, EntityId factionId, DefinitionId technologyId)
        { if (!accountId.IsValid || !factionId.IsValid || !technologyId.IsValid) throw new ArgumentException("Account, faction, and technology IDs must be valid."); AccountId = accountId; FactionId = factionId; TechnologyId = technologyId; }
        public EntityId AccountId { get; }
        public EntityId FactionId { get; }
        public DefinitionId TechnologyId { get; }
    }

    public readonly struct TechnologyRequestResult
    {
        private TechnologyRequestResult(bool succeeded, string error) { Succeeded = succeeded; Error = error ?? string.Empty; }
        public bool Succeeded { get; }
        public string Error { get; }
        public static TechnologyRequestResult Success() => new TechnologyRequestResult(true, string.Empty);
        public static TechnologyRequestResult Failure(string error) => new TechnologyRequestResult(false, error);
    }

    public interface ITechnologyStatusQuery
    {
        bool IsResearched(EntityId factionId, DefinitionId technologyId);
    }

    public interface ITechnologyCompletionSink
    {
        void TechnologyCompleted(EntityId factionId, DefinitionId technologyId);
    }

    public sealed class TechnologyCompletedEvent : IEvent
    {
        public TechnologyCompletedEvent(EntityId factionId, DefinitionId technologyId)
        { FactionId = factionId; TechnologyId = technologyId; }
        public EntityId FactionId { get; }
        public DefinitionId TechnologyId { get; }
    }

    public readonly struct TechnologyModifierSnapshot
    {
        public TechnologyModifierSnapshot(double additive, double multiplier) { Additive = additive; Multiplier = multiplier; }
        public double Additive { get; }
        public double Multiplier { get; }
        public double Apply(double baseValue) => (baseValue + Additive) * Multiplier;
    }

    public sealed class TechnologyModifierRegistry
    {
        private readonly Dictionary<EntityId, Dictionary<string, TechnologyModifierSnapshot>> _values =
            new Dictionary<EntityId, Dictionary<string, TechnologyModifierSnapshot>>();

        public void Apply(EntityId factionId, IEnumerable<TechnologyModifier> modifiers)
        {
            if (!_values.TryGetValue(factionId, out Dictionary<string, TechnologyModifierSnapshot> stats))
            { stats = new Dictionary<string, TechnologyModifierSnapshot>(StringComparer.Ordinal); _values.Add(factionId, stats); }
            foreach (TechnologyModifier modifier in modifiers)
            {
                stats.TryGetValue(modifier.StatId, out TechnologyModifierSnapshot current);
                double multiplier = current.Multiplier == 0d ? 1d : current.Multiplier;
                stats[modifier.StatId] = new TechnologyModifierSnapshot(
                    current.Additive + modifier.Additive, multiplier * modifier.Multiplier);
            }
        }

        public TechnologyModifierSnapshot Get(EntityId factionId, string statId)
        {
            if (_values.TryGetValue(factionId, out Dictionary<string, TechnologyModifierSnapshot> stats) &&
                !string.IsNullOrWhiteSpace(statId) && stats.TryGetValue(statId.Trim().ToLowerInvariant(), out TechnologyModifierSnapshot value)) return value;
            return new TechnologyModifierSnapshot(0d, 1d);
        }
    }
}
