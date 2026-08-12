using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.UI;
using AegisRTS.Presentation.Selection;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>Read-only HUD projection and command boundary for the playable product scene.</summary>
    public sealed class PrototypeHudAdapter : IHudQuery, IHudCommandSink
    {
        private readonly PrototypeSystemComposition _composition;
        private readonly ISelectionQuery _selection;

        public PrototypeHudAdapter(PrototypeSystemComposition composition, ISelectionQuery selection)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public HudSnapshot Query()
        {
            var panels = new List<HudPanelViewModel>
            {
                ResourcePanel(),
                SelectionPanel(),
                CommandPanel(),
                ArmyPanel(),
                SettlementPanel(),
                ProductionPanel(),
                new HudPanelViewModel(HudPanelId.Objective, "Objective", new[]
                {
                    new HudEntry("objective.current", "Current", _composition.GetObjectiveSummary()),
                }),
                new HudPanelViewModel(HudPanelId.Pause, "Session", new[]
                {
                    new HudEntry("session.clock", "Simulation", $"{_composition.ElapsedSeconds:0.0}s"),
                    new HudEntry("session.content", "Content", _composition.ContentPackId),
                    new HudEntry("session.scenario", "Scenario", _composition.ScenarioId),
                }),
            };
            return new HudSnapshot(panels, (long)Math.Round(_composition.ElapsedSeconds * 1000d));
        }

        public HudCommandResult Dispatch(HudCommand command)
        {
            try
            {
                CommandDispatchResult result;
                switch (command.CommandId)
                {
                    case "build.economy": result = _composition.Construct(new DefinitionId("building.economy")); break;
                    case "build.recruitment": result = _composition.Construct(new DefinitionId("building.recruitment")); break;
                    case "research.siege": result = _composition.Research(new DefinitionId("technology.siege")); break;
                    case "recruit.infantry": result = _composition.Recruit(new DefinitionId("unit.infantry")); break;
                    case "recruit.archer": result = _composition.Recruit(new DefinitionId("unit.archer")); break;
                    case "recruit.cavalry": result = _composition.Recruit(new DefinitionId("unit.cavalry")); break;
                    case "recruit.siege": result = _composition.Recruit(new DefinitionId("unit.siege")); break;
                    case "army.create": result = _composition.CreatePlayerArmy(); break;
                    case "army.add-selected": result = _composition.AddSelectedToPlayerArmy(_selection.SelectedIds); break;
                    case "army.split-selected": result = _composition.SplitSelectedFromPlayerArmy(_selection.SelectedIds); break;
                    case "army.merge": result = _composition.MergePlayerDetachment(); break;
                    case "army.commander": result = _composition.AssignPlayerLieutenantCommander(); break;
                    case "army.move": result = _composition.MovePlayerArmy(new WorldPoint(6d, 0d, 3d)); break;
                    case "army.attack": result = _composition.AttackWithPlayerArmy(_composition.FindFirstEnemyTarget()); break;
                    case "army.defend": result = _composition.DefendPlayerArmy(new WorldPoint(-4d, 0d, 0d)); break;
                    case "army.retreat": result = _composition.RetreatPlayerArmy(new WorldPoint(-12d, 0d, 0d)); break;
                    case "engagement.hold-ground": result = SetEngagementMode(UnitEngagementMode.HoldGround); break;
                    case "engagement.normal": result = SetEngagementMode(UnitEngagementMode.Normal); break;
                    case "engagement.aggressive": result = SetEngagementMode(UnitEngagementMode.Aggressive); break;
                    case "engagement.retaliate": result = SetEngagementMode(UnitEngagementMode.Retaliate); break;
                    case "siege.start": result = _composition.StartPlayerSiege(); break;
                    case "siege.breach": result = _composition.BreachGate(); break;
                    case "siege.enter": result = _composition.EnterFortress(); break;
                    case "siege.capture": result = _composition.CaptureFortress(); break;
                    default: return HudCommandResult.Failure($"Unknown HUD command '{command.CommandId}'.");
                }
                return result.WasHandled ? HudCommandResult.Success() : HudCommandResult.Failure(result.Error);
            }
            catch (Exception exception)
            {
                return HudCommandResult.Failure(exception.Message);
            }
        }

        private CommandDispatchResult SetEngagementMode(UnitEngagementMode mode)
        {
            EntityId[] actors = _selection.SelectedIds.Where(id =>
                _composition.Registry.TryGet(id, out PrototypeEntityRecord record) &&
                record.FactionId == PrototypeSystemComposition.PlayerFactionId).ToArray();
            return actors.Length == 0
                ? CommandDispatchResult.Rejected("Select at least one player unit.")
                : _composition.SetEngagementMode(actors, mode);
        }

        private HudPanelViewModel ResourcePanel()
        {
            _composition.TryGetPlayerEconomy(out EconomyAccountSnapshot economy);
            var entries = new List<HudEntry>();
            foreach (KeyValuePair<DefinitionId, double> resource in economy.Resources.OrderBy(value => value.Key.Value))
            {
                economy.Production.TryGetValue(resource.Key, out double rate);
                entries.Add(new HudEntry(resource.Key.Value, resource.Key.Value, $"{resource.Value:0.0}  (+{rate:0.0}/s)"));
            }
            entries.Add(new HudEntry("population", "Population", $"{economy.PopulationUsed:0}/{economy.PopulationCapacity:0}"));
            return new HudPanelViewModel(HudPanelId.ResourceBar, "Resources", entries);
        }

        private HudPanelViewModel SelectionPanel()
        {
            var entries = new List<HudEntry>();
            foreach (EntityId id in _selection.SelectedIds)
            {
                if (_composition.Registry.TryGet(id, out PrototypeEntityRecord record))
                {
                    _composition.Combat.TryGetState(id, out CombatantSnapshot combat);
                    string army = _composition.Armies.TryGetArmyForUnit(id, out EntityId armyId) ? armyId.ToString() : "none";
                    entries.Add(new HudEntry(id.ToString(), record.DefinitionId,
                        $"HP {combat.Health:0}/{combat.MaxHealth:0} | Army {army} | Stance {combat.EngagementMode} | Target {combat.TargetId}"));
                }
                else if (_composition.Settlements.TryGetState(id, out SettlementSnapshot settlement))
                {
                    entries.Add(new HudEntry(id.ToString(), settlement.Profile.DefinitionId,
                        $"Owner {settlement.OwnerId} | Defense {settlement.Defense:0}"));
                }
                else if (_selection.TryGetDescriptor(id, out SelectableDescriptor descriptor))
                {
                    entries.Add(new HudEntry(id.ToString(), descriptor.DefinitionId,
                        $"{descriptor.Kind} | {descriptor.Affiliation}"));
                }
            }
            if (entries.Count == 0) entries.Add(new HudEntry("selection.none", "Selected", "None"));
            return new HudPanelViewModel(HudPanelId.SelectionPanel, "Selection / Unit / Hero", entries);
        }

        private HudPanelViewModel CommandPanel()
        {
            string[] commands =
            {
                "build.economy", "build.recruitment", "research.siege", "recruit.infantry", "recruit.archer",
                "recruit.cavalry", "recruit.siege", "army.create", "army.add-selected", "army.split-selected",
                "army.merge", "army.commander", "army.move", "army.attack", "army.defend", "army.retreat",
                "engagement.hold-ground", "engagement.normal", "engagement.aggressive", "engagement.retaliate",
                "siege.start", "siege.breach", "siege.enter", "siege.capture",
            };
            return new HudPanelViewModel(HudPanelId.CommandPanel, "Commands",
                commands.Select(value => new HudEntry(value, value, enabled: true)));
        }

        private HudPanelViewModel ArmyPanel()
        {
            var entries = new List<HudEntry>();
            foreach (ArmySnapshot army in _composition.Armies.Snapshot())
                entries.Add(new HudEntry(army.ArmyId.ToString(), $"Army {army.ArmyId}",
                    $"Members {army.UnitCount} | Commander {army.CommanderId} | {army.Order.Type} | Morale {army.Morale:0} | Supply {army.Supply:0}"));
            return new HudPanelViewModel(HudPanelId.ArmyPanel, "Armies", entries);
        }

        private HudPanelViewModel SettlementPanel()
        {
            var entries = new List<HudEntry>();
            foreach (SettlementSnapshot settlement in _composition.Settlements.Snapshot())
                entries.Add(new HudEntry(settlement.SettlementId.ToString(), settlement.Profile.DefinitionId,
                    $"Owner {settlement.OwnerId} | Defense {settlement.Defense:0}"));
            return new HudPanelViewModel(HudPanelId.SettlementPanel, "Settlements", entries);
        }

        private HudPanelViewModel ProductionPanel()
        {
            var entries = new List<HudEntry>();
            entries.AddRange(_composition.Buildings.SnapshotQueue().Select(value => new HudEntry(
                $"building.{value.BuildingId}", "Build", $"{value.BuildingId} — {value.RemainingSeconds:0.0}s")));
            entries.AddRange(_composition.Technologies.SnapshotQueue().Select(value => new HudEntry(
                $"technology.{value.TechnologyId}", "Research", $"{value.TechnologyId} — {value.RemainingSeconds:0.0}s")));
            entries.AddRange(_composition.Recruitment.SnapshotQueue().Select(value => new HudEntry(
                $"recruit.{value.UnitId}", "Recruit", $"{value.UnitId} — {value.RemainingSeconds:0.0}s")));
            if (entries.Count == 0) entries.Add(new HudEntry("queue.empty", "Queues", "Empty"));
            return new HudPanelViewModel(HudPanelId.AbilityBar, "Production / Research / Recruitment", entries);
        }
    }
}
