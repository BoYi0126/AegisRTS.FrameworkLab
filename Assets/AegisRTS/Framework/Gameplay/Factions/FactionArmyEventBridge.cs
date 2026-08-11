using System;
using System.Collections.Generic;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Armies;

namespace AegisRTS.Gameplay.Factions
{
    /// <summary>Maintains the faction army index from authoritative army lifecycle events.</summary>
    public sealed class FactionArmyEventBridge : IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly FactionSystem _factions;
        private readonly IArmyQuery _armies;

        public FactionArmyEventBridge(EventBus events, FactionSystem factions, IArmyQuery armies)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            _factions = factions ?? throw new ArgumentNullException(nameof(factions));
            _armies = armies ?? throw new ArgumentNullException(nameof(armies));
            _subscriptions.Add(events.Subscribe<ArmyCreatedEvent>(value => _factions.AssignArmy(value.ArmyId, value.FactionId)));
            _subscriptions.Add(events.Subscribe<ArmySplitEvent>(OnSplit));
            _subscriptions.Add(events.Subscribe<ArmiesMergedEvent>(value => _factions.RemoveArmy(value.AbsorbedArmyId)));
        }

        public void Dispose() { foreach (IDisposable item in _subscriptions) item.Dispose(); _subscriptions.Clear(); }

        private void OnSplit(ArmySplitEvent value)
        {
            if (_armies.TryGetState(value.NewArmyId, out ArmySnapshot army)) _factions.AssignArmy(army.ArmyId, army.FactionId);
        }
    }
}
