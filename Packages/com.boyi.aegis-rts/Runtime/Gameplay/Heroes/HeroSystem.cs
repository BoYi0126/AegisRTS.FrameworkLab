using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;

namespace AegisRTS.Gameplay.Heroes
{
    /// <summary>Stores hero and leadership components without duplicating unit movement or combat state.</summary>
    public sealed class HeroSystem : IHeroQuery
    {
        private readonly Dictionary<EntityId, HeroRecord> _heroes = new Dictionary<EntityId, HeroRecord>();

        public int HeroCount => _heroes.Count;

        public void Register(EntityId entityId, HeroProfile profile)
        {
            if (!entityId.IsValid) throw new ArgumentException("Hero entity ID must be valid.", nameof(entityId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (_heroes.ContainsKey(entityId)) throw new InvalidOperationException($"Hero {entityId} is already registered.");
            _heroes.Add(entityId, new HeroRecord(entityId, profile));
        }

        public bool Unregister(EntityId entityId)
        {
            if (!_heroes.TryGetValue(entityId, out HeroRecord hero) || hero.ArmyId.IsValid) return false;
            return _heroes.Remove(entityId);
        }

        public bool IsHero(EntityId entityId) => _heroes.ContainsKey(entityId);

        public bool CanCommand(EntityId entityId, EntityId factionId) =>
            _heroes.TryGetValue(entityId, out HeroRecord hero) && hero.Profile.FactionId == factionId;

        public bool AssignArmy(EntityId entityId, EntityId armyId)
        {
            if (!_heroes.TryGetValue(entityId, out HeroRecord hero)) return false;
            hero.ArmyId = armyId;
            return true;
        }

        public bool TryGetState(EntityId entityId, out HeroSnapshot snapshot)
        {
            if (!_heroes.TryGetValue(entityId, out HeroRecord hero))
            {
                snapshot = default;
                return false;
            }
            snapshot = new HeroSnapshot(hero.EntityId, hero.Profile, hero.ArmyId);
            return true;
        }

        public IReadOnlyList<HeroSnapshot> Snapshot()
        {
            var result = new List<HeroSnapshot>(_heroes.Count);
            foreach (HeroRecord hero in _heroes.Values) result.Add(new HeroSnapshot(hero.EntityId, hero.Profile, hero.ArmyId));
            result.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int assigned = 0;
            foreach (HeroRecord hero in _heroes.Values) if (hero.ArmyId.IsValid) assigned++;
            return $"Heroes={_heroes.Count}, Assigned={assigned}, Unassigned={_heroes.Count - assigned}";
        }

        private sealed class HeroRecord
        {
            public HeroRecord(EntityId entityId, HeroProfile profile) { EntityId = entityId; Profile = profile; }
            public EntityId EntityId { get; }
            public HeroProfile Profile { get; }
            public EntityId ArmyId { get; set; }
        }
    }
}
