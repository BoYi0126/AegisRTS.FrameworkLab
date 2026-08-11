using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Settlements;

namespace AegisRTS.Gameplay.Siege
{
    /// <summary>Adapts ordinary Combat unit profiles to siege attack queries; no special siege-unit class exists.</summary>
    public sealed class CombatSiegeAttackerQuery : ISiegeAttackerQuery
    {
        private readonly CombatSystem _combat;
        public CombatSiegeAttackerQuery(CombatSystem combat) => _combat = combat ?? throw new ArgumentNullException(nameof(combat));
        public bool TryGetAttacker(EntityId entityId, out SiegeAttackerSnapshot attacker)
        {
            if (_combat.TryGetState(entityId, out CombatantSnapshot state) && state.IsAlive &&
                _combat.TryGetProfile(entityId, out CombatantProfile profile))
            { attacker = new SiegeAttackerSnapshot(state.FactionId, profile.Attack, profile.Tags); return true; }
            attacker = default; return false;
        }
    }

    /// <summary>Uses the existing Settlement capture transaction for siege ownership changes.</summary>
    public sealed class SettlementSiegeCaptureSink : ISiegeCaptureSink
    {
        private readonly SettlementSystem _settlements;
        public SettlementSiegeCaptureSink(SettlementSystem settlements) => _settlements = settlements ?? throw new ArgumentNullException(nameof(settlements));
        public SiegeActionResult Capture(EntityId settlementId, EntityId newOwnerId, CaptureCondition conditions, EntityId capturingArmyId)
        {
            SettlementCommandResult result = _settlements.Execute(new CaptureSettlementCommand(settlementId, newOwnerId, conditions, capturingArmyId));
            return result.Succeeded ? SiegeActionResult.Success() : SiegeActionResult.Failure(result.Error);
        }
    }

    /// <summary>Projects Combat deaths into clear-defender and kill-commander siege conditions.</summary>
    public sealed class SiegeCombatEventBridge : IDisposable
    {
        private readonly IDisposable _subscription;
        public SiegeCombatEventBridge(EventBus events, SiegeSystem sieges)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (sieges == null) throw new ArgumentNullException(nameof(sieges));
            _subscription = events.Subscribe<UnitDiedEvent>(value => sieges.NotifyUnitDied(value.EntityId));
        }
        public void Dispose() => _subscription.Dispose();
    }

    public sealed class RecordingSiegeNavigationSink : ISiegeNavigationSink
    {
        private readonly List<EntityId> _openedStructures = new List<EntityId>();
        public int RefreshCount => _openedStructures.Count;
        public IReadOnlyList<EntityId> OpenedStructures => _openedStructures.AsReadOnly();
        public void RefreshAfterBreach(EntityId siegeId, EntityId structureId, SiegeArea openedArea) => _openedStructures.Add(structureId);
    }
}
