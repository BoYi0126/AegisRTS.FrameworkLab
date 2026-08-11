using System;
using System.Collections.Generic;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Economy
{
    /// <summary>Atomic, world-neutral resource storage keyed only by content-authored IDs.</summary>
    public sealed class ResourceWallet
    {
        private readonly Dictionary<DefinitionId, double> _balances = new Dictionary<DefinitionId, double>();

        public double GetBalance(DefinitionId resourceId) =>
            resourceId.IsValid && _balances.TryGetValue(resourceId, out double value) ? value : 0d;

        public bool Deposit(DefinitionId resourceId, double amount)
        {
            if (!resourceId.IsValid || !IsFinite(amount) || amount < 0d) return false;
            _balances[resourceId] = GetBalance(resourceId) + amount;
            return true;
        }

        public bool CanAfford(IEnumerable<ResourceCost> costs)
        {
            if (costs == null) return false;
            var totals = new Dictionary<DefinitionId, double>();
            foreach (ResourceCost cost in costs)
            {
                if (!cost.ResourceId.IsValid || !IsFinite(cost.Amount) || cost.Amount < 0d) return false;
                totals.TryGetValue(cost.ResourceId, out double total);
                totals[cost.ResourceId] = total + cost.Amount;
            }
            foreach (KeyValuePair<DefinitionId, double> pair in totals)
                if (GetBalance(pair.Key) < pair.Value) return false;
            return true;
        }

        public bool TrySpend(IEnumerable<ResourceCost> costs)
        {
            var copy = new List<ResourceCost>(costs ?? Array.Empty<ResourceCost>());
            if (!CanAfford(copy)) return false;
            foreach (ResourceCost cost in copy)
                _balances[cost.ResourceId] = GetBalance(cost.ResourceId) - cost.Amount;
            return true;
        }

        public IReadOnlyDictionary<DefinitionId, double> Snapshot() =>
            new Dictionary<DefinitionId, double>(_balances);

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
