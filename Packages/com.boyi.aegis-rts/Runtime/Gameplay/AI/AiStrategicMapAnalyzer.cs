using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Territory;

namespace AegisRTS.Gameplay.AI
{
    /// <summary>Deterministic target selection and breadth-first territory routing for AI blackboards.</summary>
    public sealed class AiStrategicMapAnalyzer
    {
        public EntityId SelectEnemySettlement(EntityId factionId, ISettlementQuery settlements, ITerritoryQuery territories)
        {
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (settlements == null || territories == null) throw new ArgumentNullException(nameof(settlements));
            EntityId selected = default; double selectedValue = double.MinValue;
            foreach (SettlementSnapshot settlement in settlements.Snapshot())
            {
                if (settlement.OwnerId == factionId || !territories.TryGetTerritoryForSettlement(settlement.SettlementId, out EntityId territoryId) ||
                    !territories.TryGetState(territoryId, out TerritorySnapshot territory)) continue;
                if (territory.Profile.Value > selectedValue || territory.Profile.Value == selectedValue && settlement.SettlementId < selected)
                { selected = settlement.SettlementId; selectedValue = territory.Profile.Value; }
            }
            return selected;
        }

        public IReadOnlyList<EntityId> FindRoute(ITerritoryQuery territories, EntityId startId, EntityId targetId)
        {
            if (territories == null) throw new ArgumentNullException(nameof(territories));
            if (!startId.IsValid || !targetId.IsValid || !territories.TryGetState(startId, out _) || !territories.TryGetState(targetId, out _))
                return Array.Empty<EntityId>();
            var queue = new Queue<EntityId>(); var previous = new Dictionary<EntityId, EntityId>();
            queue.Enqueue(startId); previous[startId] = default;
            while (queue.Count > 0)
            {
                EntityId current = queue.Dequeue(); if (current == targetId) break;
                territories.TryGetState(current, out TerritorySnapshot state);
                var connections = new List<EntityId>(state.ConnectionIds); connections.Sort();
                foreach (EntityId next in connections) if (!previous.ContainsKey(next)) { previous[next] = current; queue.Enqueue(next); }
            }
            if (!previous.ContainsKey(targetId)) return Array.Empty<EntityId>();
            var route = new List<EntityId>();
            for (EntityId current = targetId; current.IsValid; current = previous[current]) route.Add(current);
            route.Reverse(); return route.AsReadOnly();
        }
    }
}
