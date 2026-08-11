using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Heroes
{
    /// <summary>Hero-only configuration layered onto an existing unit entity.</summary>
    public sealed class HeroProfile
    {
        public HeroProfile(string definitionId, EntityId factionId, double leadership, IEnumerable<string> abilityIds = null)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (leadership < 0d || double.IsNaN(leadership) || double.IsInfinity(leadership))
                throw new ArgumentOutOfRangeException(nameof(leadership));
            DefinitionId = definitionId.Trim();
            FactionId = factionId;
            Leadership = leadership;
            var abilities = new List<string>();
            foreach (string abilityId in abilityIds ?? Array.Empty<string>())
                if (!string.IsNullOrWhiteSpace(abilityId)) abilities.Add(abilityId.Trim());
            AbilityIds = abilities.AsReadOnly();
        }

        public string DefinitionId { get; }
        public EntityId FactionId { get; }
        public double Leadership { get; }
        public IReadOnlyList<string> AbilityIds { get; }

        public static HeroProfile FromDefinition(HeroDefinition definition, EntityId factionId)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var abilities = new List<string>(definition.AbilityIds.Count);
            foreach (DefinitionId abilityId in definition.AbilityIds) abilities.Add(abilityId.ToString());
            return new HeroProfile(definition.Id.ToString(), factionId, definition.Leadership, abilities);
        }
    }

    public readonly struct HeroSnapshot
    {
        public HeroSnapshot(EntityId entityId, HeroProfile profile, EntityId armyId)
        { EntityId = entityId; Profile = profile; ArmyId = armyId; }
        public EntityId EntityId { get; }
        public HeroProfile Profile { get; }
        public EntityId ArmyId { get; }
        public bool IsAssigned => ArmyId.IsValid;
    }

    public interface IHeroQuery
    {
        bool IsHero(EntityId entityId);
        bool TryGetState(EntityId entityId, out HeroSnapshot snapshot);
        IReadOnlyList<HeroSnapshot> Snapshot();
    }
}
