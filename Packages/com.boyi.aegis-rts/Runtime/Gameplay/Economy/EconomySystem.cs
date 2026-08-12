using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Economy
{
    /// <summary>Owns resource wallets, data-driven income, and optional population accounting.</summary>
    public sealed class EconomySystem
    {
        private readonly Dictionary<EntityId, Account> _accounts = new Dictionary<EntityId, Account>();
        private readonly bool _populationEnabled;
        private readonly EventBus _events;
        private readonly IResourceBalanceSink _sink;

        public EconomySystem(bool populationEnabled, EventBus eventBus = null, IResourceBalanceSink sink = null)
        { _populationEnabled = populationEnabled; _events = eventBus; _sink = sink ?? NullResourceBalanceSink.Instance; }

        public void RegisterAccount(EntityId accountId, IEnumerable<ResourceCost> startingResources = null,
            double populationUsed = 0d, double populationCapacity = 0d)
        {
            if (!accountId.IsValid) throw new ArgumentException("Account ID must be valid.", nameof(accountId));
            if (_accounts.ContainsKey(accountId)) throw new InvalidOperationException($"Economy account {accountId} already exists.");
            if (!IsFiniteNonNegative(populationUsed) || !IsFiniteNonNegative(populationCapacity) || populationUsed > populationCapacity)
                throw new ArgumentOutOfRangeException(nameof(populationUsed));
            var account = new Account(accountId, populationUsed, populationCapacity);
            _accounts.Add(accountId, account);
            foreach (ResourceCost resource in startingResources ?? Array.Empty<ResourceCost>())
                AddResource(accountId, resource.ResourceId, resource.Amount);
        }

        public bool ContainsAccount(EntityId accountId) => _accounts.ContainsKey(accountId);

        public bool AddResource(EntityId accountId, DefinitionId resourceId, double amount)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || !account.Wallet.Deposit(resourceId, amount)) return false;
            _sink.ApplyResourceDelta(accountId, resourceId, amount);
            return true;
        }

        public bool TrySpend(EntityId accountId, IEnumerable<ResourceCost> costs)
        {
            if (!_accounts.TryGetValue(accountId, out Account account)) return false;
            var copy = new List<ResourceCost>(costs ?? Array.Empty<ResourceCost>());
            if (!account.Wallet.TrySpend(copy)) return false;
            foreach (ResourceCost cost in copy) _sink.ApplyResourceDelta(accountId, cost.ResourceId, -cost.Amount);
            return true;
        }

        public bool CanAfford(EntityId accountId, IEnumerable<ResourceCost> costs) =>
            _accounts.TryGetValue(accountId, out Account account) && account.Wallet.CanAfford(costs);

        public bool AddProduction(EntityId accountId, IEnumerable<ResourceProduction> production)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || production == null) return false;
            foreach (ResourceProduction item in production)
            {
                if (!item.ResourceId.IsValid || !IsFiniteNonNegative(item.AmountPerSecond)) return false;
                account.Production.TryGetValue(item.ResourceId, out double current);
                account.Production[item.ResourceId] = current + item.AmountPerSecond;
            }
            return true;
        }

        public bool TryReservePopulation(EntityId accountId, double amount)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || !IsFiniteNonNegative(amount)) return false;
            if (!_populationEnabled) return true;
            if (account.PopulationUsed + amount > account.PopulationCapacity) return false;
            account.PopulationUsed += amount;
            return true;
        }

        public bool CanReservePopulation(EntityId accountId, double amount)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || !IsFiniteNonNegative(amount)) return false;
            return !_populationEnabled || account.PopulationUsed + amount <= account.PopulationCapacity;
        }

        public bool ReleasePopulation(EntityId accountId, double amount)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || !IsFiniteNonNegative(amount)) return false;
            if (!_populationEnabled) return true;
            account.PopulationUsed = Math.Max(0d, account.PopulationUsed - amount);
            return true;
        }

        public bool AddPopulationCapacity(EntityId accountId, double amount)
        {
            if (!_accounts.TryGetValue(accountId, out Account account) || !IsFiniteNonNegative(amount)) return false;
            account.PopulationCapacity += amount;
            return true;
        }

        public void Tick(double deltaSeconds)
        {
            if (!IsFiniteNonNegative(deltaSeconds)) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (deltaSeconds == 0d) return;
            foreach (Account account in _accounts.Values)
            foreach (KeyValuePair<DefinitionId, double> item in account.Production)
            {
                double amount = item.Value * deltaSeconds;
                if (amount <= 0d) continue;
                account.Wallet.Deposit(item.Key, amount);
                _sink.ApplyResourceDelta(account.AccountId, item.Key, amount);
                _events?.Publish(new ResourceProducedEvent(account.AccountId, item.Key, amount));
            }
        }

        public bool TryGetState(EntityId accountId, out EconomyAccountSnapshot snapshot)
        {
            if (!_accounts.TryGetValue(accountId, out Account account)) { snapshot = default; return false; }
            snapshot = new EconomyAccountSnapshot(accountId, account.Wallet.Snapshot(), account.PopulationUsed,
                account.PopulationCapacity, new Dictionary<DefinitionId, double>(account.Production));
            return true;
        }

        public string GetDebugSummary() => $"Accounts={_accounts.Count}, PopulationRule={_populationEnabled}";

        private static bool IsFiniteNonNegative(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;

        private sealed class Account
        {
            public Account(EntityId id, double used, double capacity)
            { AccountId = id; PopulationUsed = used; PopulationCapacity = capacity; }
            public EntityId AccountId { get; }
            public ResourceWallet Wallet { get; } = new ResourceWallet();
            public Dictionary<DefinitionId, double> Production { get; } = new Dictionary<DefinitionId, double>();
            public double PopulationUsed { get; set; }
            public double PopulationCapacity { get; set; }
        }
    }
}
