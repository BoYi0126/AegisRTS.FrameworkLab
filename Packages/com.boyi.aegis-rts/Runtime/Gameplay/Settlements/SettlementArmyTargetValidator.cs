using System;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Factions;

namespace AegisRTS.Gameplay.Settlements
{
    /// <summary>Validates that AttackSettlement targets exist, are foreign, and diplomacy permits combat.</summary>
    public sealed class SettlementArmyTargetValidator : IArmySettlementTargetValidator
    {
        private readonly ISettlementQuery _settlements;
        private readonly IFactionQuery _factions;

        public SettlementArmyTargetValidator(ISettlementQuery settlements, IFactionQuery factions)
        { _settlements = settlements ?? throw new ArgumentNullException(nameof(settlements)); _factions = factions ?? throw new ArgumentNullException(nameof(factions)); }

        public bool Validate(EntityId settlementId, EntityId attackerFactionId, out string error)
        {
            if (!_settlements.TryGetState(settlementId, out SettlementSnapshot settlement))
            { error = "Settlement target does not exist."; return false; }
            if (settlement.OwnerId == attackerFactionId)
            { error = "An army cannot attack its own settlement."; return false; }
            if (!_factions.TryGetState(attackerFactionId, out FactionSnapshot faction))
            { error = "Attacker faction does not exist."; return false; }
            DiplomacyStatus status = faction.Diplomacy.TryGetValue(settlement.OwnerId, out DiplomacyStatus configured)
                ? configured : DiplomacyStatus.Neutral;
            if (status != DiplomacyStatus.Hostile && status != DiplomacyStatus.War)
            { error = "Diplomacy does not permit attacking this settlement."; return false; }
            error = string.Empty; return true;
        }
    }
}
