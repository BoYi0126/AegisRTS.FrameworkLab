using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.VerticalSlice
{
    /// <summary>One reusable orchestration layer over the existing RTS domain systems.</summary>
    public sealed class VerticalSliceSimulation : IVerticalSliceStepExecutor, IUnitSpawnSink,
        IAiWorldQuery, IAiActionExecutor, IDisposable
    {
        public static readonly EntityId PlayerFactionId = new EntityId(15001);
        public static readonly EntityId EnemyFactionId = new EntityId(15002);
        public static readonly EntityId PlayerCityEntityId = new EntityId(15010);
        public static readonly EntityId VillageEntityId = new EntityId(15011);
        public static readonly EntityId EnemyFortressEntityId = new EntityId(15012);
        public static readonly EntityId PlayerArmyId = new EntityId(15040);
        public static readonly EntityId EnemyArmyId = new EntityId(15041);
        public static readonly EntityId SiegeEntityId = new EntityId(15070);
        public static readonly EntityId GateEntityId = new EntityId(15071);

        private readonly ContentPack _pack;
        private readonly VerticalSliceDefinition _definition;
        private readonly EventBus _events = new EventBus();
        private readonly List<EntityId> _playerUnits = new List<EntityId>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly EntityId _playerHeroId = new EntityId(15030);
        private readonly EntityId _enemyHeroId = new EntityId(15031);
        private readonly EntityId _fieldEnemyId = new EntityId(15060);
        private ulong _nextUnitId = 15100;
        private EntityId _siegeUnitId;
        private bool _recruitmentRequested;

        public VerticalSliceSimulation(ContentPack pack, VerticalSliceDefinition definition)
        {
            _pack = pack ?? throw new ArgumentNullException(nameof(pack));
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            VerticalSliceValidationResult validation = new VerticalSliceValidator().Validate(definition, pack);
            if (!validation.IsValid) throw new ArgumentException(string.Join("; ", validation.Issues), nameof(definition));

            Factions = new FactionSystem(_events);
            Territories = new TerritorySystem(Factions, _events);
            Settlements = new SettlementSystem(Factions, Territories, _events);
            Economy = new EconomySystem(pack.Rules == null || pack.Rules.PopulationEnabled, _events);
            Combat = new CombatSystem(_events);
            Heroes = new HeroSystem();
            Armies = new ArmySystem(Heroes, eventBus: _events,
                membershipSink: new CombatArmyMembershipSink(Combat),
                settlementTargetValidator: new SettlementArmyTargetValidator(Settlements, Factions));
            Recruitment = new RecruitmentSystem(pack.Units, Economy, sink: this, eventBus: _events);
            Sieges = new SiegeSystem(new CombatSiegeAttackerQuery(Combat), new RecordingSiegeNavigationSink(),
                new SettlementSiegeCaptureSink(Settlements), eventBus: _events);
            AI = new AiSystem(eventBus: _events);
            _subscriptions.Add(new FactionArmyEventBridge(_events, Factions, Armies));
            ConfigureWorld();
        }

        public FactionSystem Factions { get; }
        public TerritorySystem Territories { get; }
        public SettlementSystem Settlements { get; }
        public EconomySystem Economy { get; }
        public RecruitmentSystem Recruitment { get; }
        public HeroSystem Heroes { get; }
        public ArmySystem Armies { get; }
        public CombatSystem Combat { get; }
        public SiegeSystem Sieges { get; }
        public AiSystem AI { get; }
        public int RecruitedUnitCount => _playerUnits.Count;
        public bool FieldBattleWon { get; private set; }
        public bool CounterattackIssued { get; private set; }
        public bool FortressCaptured => Settlements.TryGetState(EnemyFortressEntityId, out SettlementSnapshot value) && value.OwnerId == PlayerFactionId;
        public string WorldId => _definition.WorldId;

        public VerticalSliceStepResult Execute(VerticalSliceStage stage)
        {
            switch (stage)
            {
                case VerticalSliceStage.Start:
                    AI.Tick(1d);
                    return CounterattackIssued ? Done("AI counterattack issued against the player city.") : Fail("AI counterattack was not issued.");
                case VerticalSliceStage.Income:
                    Economy.Tick(1d);
                    return HasIncome() ? Done("Two resources produced.") : Fail("Income was not produced.");
                case VerticalSliceStage.Recruit:
                    return Recruit();
                case VerticalSliceStage.Army:
                    return CreatePlayerArmy();
                case VerticalSliceStage.Move:
                    return Armies.Execute(new MoveArmyCommand(PlayerArmyId, new WorldPoint(0d, 0d, 4d))).Succeeded
                        ? Done("Army moved through the village.") : Fail("Army move failed.");
                case VerticalSliceStage.FieldBattle:
                    return ResolveFieldBattle();
                case VerticalSliceStage.Siege:
                    return StartSiege();
                case VerticalSliceStage.BreakGate:
                    return BreakGate();
                case VerticalSliceStage.Enter:
                    return EnterFortress();
                case VerticalSliceStage.Capture:
                    return Sieges.Execute(new CaptureSiegeCommand(SiegeEntityId)).Succeeded
                        ? Done("Enemy fortress captured.") : Fail("Fortress capture failed.");
                case VerticalSliceStage.Victory:
                    return FortressCaptured ? Done("Victory.") : Fail("Victory condition was not met.");
                default:
                    return VerticalSliceStepResult.Waiting();
            }
        }

        public void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId)
        {
            UnitDefinition definition = Find(_pack.Units, unitId);
            EntityId entityId = new EntityId(_nextUnitId++);
            _playerUnits.Add(entityId); Armies.RegisterMember(entityId, factionId);
            bool siege = HasTag(definition.Tags, "siege-unit");
            Combat.Register(entityId, new CombatantProfile(definition.Id.Value, factionId, definition.MaxHealth,
                new AttackProfile(siege ? 250d : 90d, DamageType.Physical, 3d, 0d, 0d,
                    targetTags: siege ? new[] { "structure" } : null), tags: siege ? new[] { "unit", "siege" } : new[] { "unit" }),
                new WorldPoint(-4d + _playerUnits.Count, 0d, 0d));
            if (siege) _siegeUnitId = entityId;
        }

        public AiWorldSnapshot Observe(EntityId factionId) => new AiWorldSnapshot(factionId, 500d, 20d,
            4, 1, 1, 1, 80d, 20d, PlayerCityEntityId,
            new[] { new EntityId(15022), new EntityId(15021), new EntityId(15020) }, true, false, true,
            CounterattackIssued, false, false, false, false);

        public AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world)
        {
            if (action == AiActionType.MoveToTarget)
            {
                ArmyCommandResult result = Armies.Execute(new AttackSettlementArmyCommand(EnemyArmyId, PlayerCityEntityId));
                CounterattackIssued = result.Succeeded;
                return result.Succeeded ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }
            return AiActionResult.Waiting();
        }

        public void Dispose()
        { for (int i = _subscriptions.Count - 1; i >= 0; i--) _subscriptions[i].Dispose(); _subscriptions.Clear(); }

        private void ConfigureWorld()
        {
            Factions.Register(PlayerFactionId, new FactionProfile("faction.player"));
            Factions.Register(EnemyFactionId, new FactionProfile("faction.enemy", _definition.AiProfileId.Value));
            Factions.SetDiplomacy(PlayerFactionId, EnemyFactionId, DiplomacyStatus.War);
            Territories.RegisterNode(new EntityId(15020), new TerritoryNodeProfile("territory.player-city", 100, PlayerCityEntityId), PlayerFactionId);
            Territories.RegisterNode(new EntityId(15021), new TerritoryNodeProfile("territory.village", 50, VillageEntityId), PlayerFactionId);
            Territories.RegisterNode(new EntityId(15022), new TerritoryNodeProfile("territory.enemy-fortress", 120, EnemyFortressEntityId), EnemyFactionId);
            Territories.Connect(new EntityId(15020), new EntityId(15021)); Territories.Connect(new EntityId(15021), new EntityId(15022));
            Settlements.Register(PlayerCityEntityId, SettlementProfile.FromDefinition(Find(_pack.Settlements, _definition.PlayerCityId)), PlayerFactionId);
            Settlements.Register(VillageEntityId, SettlementProfile.FromDefinition(Find(_pack.Settlements, _definition.VillageId)), PlayerFactionId);
            Settlements.Register(EnemyFortressEntityId, SettlementProfile.FromDefinition(Find(_pack.Settlements, _definition.EnemyFortressId)), EnemyFactionId);
            Settlements.AddBuilding(PlayerCityEntityId, _definition.EconomyBuildingId.Value);
            Settlements.AddBuilding(PlayerCityEntityId, _definition.RecruitmentBuildingId.Value);
            Economy.RegisterAccount(PlayerCityEntityId, populationCapacity: 30d);
            Economy.AddProduction(PlayerCityEntityId, Find(_pack.Buildings, _definition.EconomyBuildingId).Production);

            HeroDefinition playerHero = Find(_pack.Heroes, _definition.HeroIds[0]);
            HeroDefinition enemyHero = Find(_pack.Heroes, _definition.HeroIds[1]);
            Heroes.Register(_playerHeroId, HeroProfile.FromDefinition(playerHero, PlayerFactionId));
            Heroes.Register(_enemyHeroId, HeroProfile.FromDefinition(enemyHero, EnemyFactionId));
            RegisterHeroCombat(_playerHeroId, playerHero, PlayerFactionId, 500d);
            RegisterHeroCombat(_enemyHeroId, enemyHero, EnemyFactionId, 80d);
            Armies.RegisterMember(_playerHeroId, PlayerFactionId); Armies.RegisterMember(_enemyHeroId, EnemyFactionId);
            Armies.Execute(new CreateArmyCommand(EnemyArmyId, EnemyFactionId, new[] { _enemyHeroId }, _enemyHeroId));
            AI.Register(EnemyFactionId, AiProfile.FromDefinition(Find(_pack.AiProfiles, _definition.AiProfileId)), this, this);
        }

        private VerticalSliceStepResult Recruit()
        {
            if (!_recruitmentRequested)
            {
                foreach (VerticalSliceUnitRole role in _definition.UnitRoles)
                {
                    RecruitmentRequestResult request = Recruitment.Request(new RecruitUnitCommand(PlayerCityEntityId, PlayerFactionId, role.UnitId));
                    if (!request.Succeeded) return Fail(request.Error);
                }
                _recruitmentRequested = true;
            }
            Recruitment.Tick(1d);
            return _playerUnits.Count == 4 ? Done("Four unit roles recruited.") : VerticalSliceStepResult.Waiting("Recruitment in progress.");
        }

        private VerticalSliceStepResult CreatePlayerArmy()
        {
            var members = new List<EntityId> { _playerHeroId }; members.AddRange(_playerUnits);
            ArmyCommandResult result = Armies.Execute(new CreateArmyCommand(PlayerArmyId, PlayerFactionId, members, _playerHeroId));
            return result.Succeeded ? Done("Hero-led army assembled.") : Fail(result.Error);
        }

        private VerticalSliceStepResult ResolveFieldBattle()
        {
            if (!Combat.TryGetState(_fieldEnemyId, out _))
                Combat.Register(_fieldEnemyId, new CombatantProfile("field-defender", EnemyFactionId, 100d,
                    new AttackProfile(20d, DamageType.Physical, 2d, 1d, 0d), tags: new[] { "unit" }), new WorldPoint(0d, 0d, 4d));
            Combat.UpdatePosition(_playerHeroId, new WorldPoint(0d, 0d, 4d));
            Combat.IssueAttack(new AttackTargetCommand(new[] { _playerHeroId }, _fieldEnemyId)); Combat.Tick(0.1d);
            Combat.TryGetState(_fieldEnemyId, out CombatantSnapshot target); FieldBattleWon = !target.IsAlive;
            return FieldBattleWon ? Done("Field battle won through CombatSystem.") : VerticalSliceStepResult.Waiting("Field battle continues.");
        }

        private VerticalSliceStepResult StartSiege()
        {
            Sieges.Register(SiegeEntityId, new SiegeProfile(EnemyFortressEntityId, PlayerFactionId, EnemyFactionId,
                SiegeMode.Assault, PlayerArmyId));
            Sieges.RegisterStructure(SiegeEntityId, GateEntityId,
                DefenseStructureProfile.FromDefinition(Find(_pack.DefenseStructures, _definition.GateId), EnemyFactionId));
            SiegeActionResult result = Sieges.Execute(new StartSiegeCommand(SiegeEntityId));
            return result.Succeeded ? Done("Siege started.") : Fail(result.Error);
        }

        private VerticalSliceStepResult BreakGate()
        {
            if (!_siegeUnitId.IsValid) return Fail("Siege unit is missing.");
            SiegeActionResult result = Sieges.Execute(new AttackDefenseStructureCommand(SiegeEntityId, _siegeUnitId, GateEntityId));
            if (!result.Succeeded) return Fail(result.Error);
            Sieges.TryGetState(SiegeEntityId, out SiegeSnapshot siege);
            return siege.State == SiegeState.Breached ? Done("Gate breached through SiegeSystem.") : VerticalSliceStepResult.Waiting("Gate attack continues.");
        }

        private VerticalSliceStepResult EnterFortress()
        {
            SiegeActionResult inner = Sieges.Execute(new EnterSiegeAreaCommand(SiegeEntityId, SiegeArea.InnerArea));
            if (!inner.Succeeded) return Fail(inner.Error);
            SiegeActionResult objective = Sieges.Execute(new EnterSiegeAreaCommand(SiegeEntityId, SiegeArea.CaptureObjective));
            return objective.Succeeded ? Done("Army entered the capture objective.") : Fail(objective.Error);
        }

        private bool HasIncome()
        {
            if (!Economy.TryGetState(PlayerCityEntityId, out EconomyAccountSnapshot account)) return false;
            foreach (DefinitionId id in _definition.ResourceIds)
                if (!account.Resources.TryGetValue(id, out double value) || value <= 0d) return false;
            return true;
        }

        private void RegisterHeroCombat(EntityId id, HeroDefinition hero, EntityId faction, double damage) =>
            Combat.Register(id, new CombatantProfile(hero.Id.Value, faction, hero.MaxHealth,
                new AttackProfile(damage, DamageType.Physical, 4d, 0d, 0d), tags: new[] { "unit", "hero" }), new WorldPoint(0d, 0d, 0d));

        private static T Find<T>(IReadOnlyList<T> values, DefinitionId id) where T : IDefinition
        { foreach (T value in values) if (value.Id == id) return value; throw new InvalidOperationException($"Definition '{id}' was not found."); }
        private static bool HasTag(IReadOnlyList<ContentTag> tags, string value)
        { foreach (ContentTag tag in tags) if (tag.Value == value) return true; return false; }
        private static VerticalSliceStepResult Done(string message) => VerticalSliceStepResult.Completed(message);
        private static VerticalSliceStepResult Fail(string message) => VerticalSliceStepResult.Defeated(message);
    }
}
