using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Core.Random;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Content.Serialization;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Objectives;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Technology;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Demo.PlayablePrototype
{
    public sealed class PrototypeStartSiegeCommand : ICommand { }
    public sealed class PrototypeBreachGateCommand : ICommand { }
    public sealed class PrototypeEnterFortressCommand : ICommand { }
    public sealed class PrototypeCaptureFortressCommand : ICommand { }

    /// <summary>
    /// Manual playable-product composition. It owns orchestration only; authoritative state remains in framework systems.
    /// </summary>
    public sealed class PrototypeSystemComposition : IUnitSpawnSink, IBuildingCompletionSink,
        ITechnologyCompletionSink, IAiWorldQuery, IAiActionExecutor, IDisposable
    {
        public const double EnemyAttackGraceSeconds = 90d;
        public static readonly EntityId PlayerFactionId = new EntityId(1001);
        public static readonly EntityId EnemyFactionId = new EntityId(1002);
        public static readonly EntityId PlayerCityId = new EntityId(1101);
        public static readonly EntityId VillageId = new EntityId(1102);
        public static readonly EntityId EnemyFortressId = new EntityId(1103);
        public static readonly EntityId PlayerHeroId = new EntityId(1201);
        public static readonly EntityId EnemyHeroId = new EntityId(1202);
        public static readonly EntityId PlayerLieutenantId = new EntityId(1203);
        public static readonly EntityId PlayerArmyId = new EntityId(1301);
        public static readonly EntityId EnemyArmyId = new EntityId(1302);
        public static readonly EntityId PlayerSiegeId = new EntityId(1401);
        public static readonly EntityId FortressGateId = new EntityId(1402);
        public static readonly EntityId FortressStrongholdId = new EntityId(1403);
        public const double GateRepairDelaySeconds = 8d;
        public const double GateRepairAmount = 45d;

        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private readonly List<EntityId> _pendingDeaths = new List<EntityId>();
        private readonly List<string> _notifications = new List<string>();
        private readonly ContentPack _pack;
        private readonly ScenarioDefinition _scenarioDefinition;
        private readonly PrototypeSiegeNavigationSink _siegeNavigation = new PrototypeSiegeNavigationSink();
        private ulong _nextEntityId = 2000;
        private bool _disposed;
        private bool _playerSiegeRegistered;
        private bool _aiDeployed;
        private bool _aiCounterattackIssued;
        private double _gateRepairRemainingSeconds;

        public PrototypeSystemComposition(string contentJson, string scenarioJson, IPrototypeNavigationRuntime navigation = null)
        {
            _pack = new ContentPackJsonLoader().Load(contentJson);
            _scenarioDefinition = new ScenarioJsonLoader().Load(scenarioJson);
            Events = new EventBus();
            Commands = new CommandBus();
            RandomSource = new SeededRandom(20260811);
            Registry = new PrototypeEntityRegistry();
            Navigation = navigation ?? new PrototypeNavigationAdapter();
            Movement = new MovementSystem(Navigation);
            Combat = new CombatSystem(Events);
            CombatMovement = new CombatMovementCoordinator(Combat, Movement);
            Factions = new FactionSystem(Events);
            Territories = new TerritorySystem(Factions, Events);
            Settlements = new SettlementSystem(Factions, Territories, Events);
            Economy = new EconomySystem(_pack.Rules == null || _pack.Rules.PopulationEnabled, Events);
            Technologies = new TechnologySystem(_pack.Technologies, Economy, sink: this, eventBus: Events);
            Buildings = new BuildingSystem(_pack.Buildings, Economy, Technologies, this, Events);
            Heroes = new HeroSystem();
            Armies = new ArmySystem(Heroes, ArmyRuleOptions.From(_pack.Rules),
                new PrototypeArmyOrderExecutor(Movement, Combat, CombatMovement), new CombatArmyMembershipSink(Combat),
                Events, new SettlementArmyTargetValidator(Settlements, Factions));
            _registrations.Add(new FactionArmyEventBridge(Events, Factions, Armies));
            Recruitment = new RecruitmentSystem(_pack.Units, Economy, Buildings, Technologies, this, Events);
            Sieges = new SiegeSystem(new CombatSiegeAttackerQuery(Combat), _siegeNavigation,
                new SettlementSiegeCaptureSink(Settlements), new FortifiedCitySiegeRule(), Events);
            _registrations.Add(new SiegeCommandRouter(Commands, Sieges));
            Scenario = new ScenarioSystem(Events);
            AI = new AiSystem(eventBus: Events);

            RegisterEvents();
            RegisterCommands();
            ConfigureWorld();
        }

        public EventBus Events { get; }
        public CommandBus Commands { get; }
        public SeededRandom RandomSource { get; private set; }
        public PrototypeEntityRegistry Registry { get; }
        public IPrototypeNavigationRuntime Navigation { get; }
        public MovementSystem Movement { get; }
        public CombatSystem Combat { get; }
        public CombatMovementCoordinator CombatMovement { get; }
        public FactionSystem Factions { get; }
        public TerritorySystem Territories { get; }
        public SettlementSystem Settlements { get; }
        public EconomySystem Economy { get; }
        public BuildingSystem Buildings { get; }
        public TechnologySystem Technologies { get; }
        public RecruitmentSystem Recruitment { get; }
        public HeroSystem Heroes { get; }
        public ArmySystem Armies { get; }
        public SiegeSystem Sieges { get; }
        public ScenarioSystem Scenario { get; }
        public AiSystem AI { get; }
        public double ElapsedSeconds { get; private set; }
        public string LastCommandSummary { get; private set; } = "New game ready.";
        public bool GateNavigationRefreshed => _siegeNavigation.RefreshCount > 0;
        public double GateRepairRemainingSeconds => Math.Max(0d, _gateRepairRemainingSeconds);
        public bool AiCounterattackIssued => _aiCounterattackIssued;
        public double EnemyAttackRemainingSeconds => Math.Max(0d, EnemyAttackGraceSeconds - ElapsedSeconds);
        public bool IsVictory => Scenario.Status == ScenarioStatus.Victory;
        public bool IsDefeat => Scenario.Status == ScenarioStatus.Defeat;
        public string ContentPackId => _pack.Id.Value;
        public string ScenarioId => _scenarioDefinition.Id;
        public IReadOnlyList<string> Notifications => _notifications.AsReadOnly();
        public event Action<PrototypeEntityRecord> EntitySpawned;
        public event Action<EntityId> EntityRemoved;
        public event Action FortressGateBreached;
        public event Action FortressGateRepaired;
        public event Action FortressCaptured;

        public CommandDispatchResult Move(IReadOnlyList<EntityId> actors, WorldPoint destination, bool queue = false) =>
            Dispatch(new MoveUnitsCommand(actors, destination, queue));

        public CommandDispatchResult Attack(IReadOnlyList<EntityId> actors, EntityId targetId, bool queue = false) =>
            Dispatch(new AttackTargetCommand(actors, targetId, queue));

        public CommandDispatchResult SetEngagementMode(IReadOnlyList<EntityId> actors, UnitEngagementMode mode) =>
            Dispatch(new SetUnitEngagementModeCommand(actors, mode));

        public CommandDispatchResult Construct(DefinitionId buildingId) =>
            Dispatch(new ConstructBuildingCommand(PlayerCityId, PlayerFactionId, buildingId));

        public CommandDispatchResult Research(DefinitionId technologyId) =>
            Dispatch(new ResearchTechnologyCommand(PlayerCityId, PlayerFactionId, technologyId));

        public CommandDispatchResult Recruit(DefinitionId unitId) =>
            Dispatch(new RecruitUnitCommand(PlayerCityId, PlayerFactionId, unitId));

        public CommandDispatchResult CreatePlayerArmy()
        {
            IReadOnlyList<EntityId> members = Registry.GetFactionEntities(PlayerFactionId);
            return Dispatch(new CreateArmyCommand(PlayerArmyId, PlayerFactionId, members, PlayerHeroId));
        }

        public CommandDispatchResult MovePlayerArmy(WorldPoint destination) => Dispatch(new MoveArmyCommand(PlayerArmyId, destination));
        public CommandDispatchResult AttackWithPlayerArmy(EntityId targetId) => Dispatch(new AttackArmyCommand(PlayerArmyId, targetId));
        public CommandDispatchResult DefendPlayerArmy(WorldPoint position) => Dispatch(new DefendArmyCommand(PlayerArmyId, position));
        public CommandDispatchResult RetreatPlayerArmy(WorldPoint destination) => Dispatch(new RetreatArmyCommand(PlayerArmyId, destination));

        public CommandDispatchResult SplitSelectedFromPlayerArmy(IReadOnlyList<EntityId> selected)
        {
            if (!Armies.TryGetState(PlayerArmyId, out ArmySnapshot source)) return Reject("Create the player army first.");
            EntityId[] members = (selected ?? Array.Empty<EntityId>()).Where(value =>
                value != source.CommanderId && source.UnitIds.Contains(value)).Distinct().OrderBy(value => value).ToArray();
            if (members.Length == 0) return Reject("Select at least one non-commander member to split.");
            if (members.Length >= source.UnitCount) return Reject("A split must leave at least one unit in the source army.");
            EntityId commander = members.Contains(PlayerLieutenantId) ? PlayerLieutenantId : EntityId.Invalid;
            return Dispatch(new SplitArmyCommand(PlayerArmyId, new EntityId(1303), members, commander));
        }

        public CommandDispatchResult MergePlayerDetachment() => Dispatch(new MergeArmiesCommand(PlayerArmyId, new EntityId(1303)));

        public CommandDispatchResult AddSelectedToPlayerArmy(IReadOnlyList<EntityId> selected)
        {
            if (!Armies.TryGetState(PlayerArmyId, out _)) return Reject("Create the player army first.");
            EntityId[] members = (selected ?? Array.Empty<EntityId>()).Where(value =>
                Registry.TryGet(value, out PrototypeEntityRecord record) && record.FactionId == PlayerFactionId &&
                !Armies.TryGetArmyForUnit(value, out _)).Distinct().OrderBy(value => value).ToArray();
            if (members.Length == 0) return Reject("Select at least one unassigned player unit to join.");
            EntityId temporary = new EntityId(1310);
            CommandDispatchResult created = Dispatch(new CreateArmyCommand(temporary, PlayerFactionId, members));
            return created.WasHandled ? Dispatch(new MergeArmiesCommand(PlayerArmyId, temporary)) : created;
        }

        public CommandDispatchResult AssignPlayerLieutenantCommander() =>
            Dispatch(new AssignArmyCommanderCommand(PlayerArmyId, PlayerLieutenantId));

        public CommandDispatchResult StartPlayerSiege() => Dispatch(new PrototypeStartSiegeCommand());
        public CommandDispatchResult BreachGate() => Dispatch(new PrototypeBreachGateCommand());
        public CommandDispatchResult EnterFortress() => Dispatch(new PrototypeEnterFortressCommand());
        public CommandDispatchResult CaptureFortress() => Dispatch(new PrototypeCaptureFortressCommand());

        public bool TryGetSiegeStructure(EntityId structureId, out DefenseStructureSnapshot structure)
        {
            if (Sieges.TryGetState(PlayerSiegeId, out SiegeSnapshot siege))
            {
                foreach (DefenseStructureSnapshot value in siege.Structures)
                {
                    if (value.StructureId == structureId) { structure = value; return true; }
                }
            }
            structure = default;
            return false;
        }

        public void TriggerDefeat()
        {
            Scenario.SetFact("player.city.lost", 1d);
            Notify("Defeat condition triggered.");
        }

        public void Tick(double deltaSeconds)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PrototypeSystemComposition));
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            ElapsedSeconds += deltaSeconds;
            Economy.Tick(deltaSeconds);
            Buildings.Tick(deltaSeconds);
            Technologies.Tick(deltaSeconds);
            Recruitment.Tick(deltaSeconds);
            Navigation.Tick(deltaSeconds);
            Movement.Tick(deltaSeconds);
            SyncCombatPositions();
            Combat.Tick(deltaSeconds);
            CombatMovement.Tick();
            ProcessDeaths();
            Sieges.Tick(deltaSeconds);
            TickGateRepair(deltaSeconds);
            Scenario.Update(deltaSeconds);
            AI.Tick(deltaSeconds);
        }

        public bool TryGetPlayerEconomy(out EconomyAccountSnapshot snapshot) => Economy.TryGetState(PlayerCityId, out snapshot);

        public EntityId FindFirstEnemyTarget()
        {
            foreach (EntityId id in Registry.GetFactionEntities(EnemyFactionId))
                if (Combat.TryGetState(id, out CombatantSnapshot value) && value.IsAlive) return id;
            return EntityId.Invalid;
        }

        public EntityId FindPlayerSiegeUnit()
        {
            foreach (PrototypeEntityRecord record in Registry.Snapshot())
                if (record.FactionId == PlayerFactionId && HasTag(record.CombatProfile.Tags, "siege-unit")) return record.EntityId;
            return EntityId.Invalid;
        }

        public string GetObjectiveSummary()
        {
            if (!Scenario.TryGetSnapshot(out ScenarioSnapshot snapshot)) return "Objective unavailable";
            ObjectiveSnapshot objective = snapshot.Objectives.FirstOrDefault();
            return objective == null ? snapshot.Status.ToString() : $"{objective.Definition.DisplayName}: {objective.Status} ({objective.Value:0}/{objective.Definition.TargetValue:0})";
        }

        public string GetDebugSummary()
        {
            TryGetPlayerEconomy(out EconomyAccountSnapshot economy);
            string resources = economy.Resources == null ? "none" : string.Join(", ", economy.Resources.Select(value => $"{value.Key}={value.Value:0.0}"));
            string ai = AI.TryGetState(EnemyFactionId, out AiAgentSnapshot agent)
                ? $"{agent.Goal}/{agent.Layer}/{agent.Action}, Decisions={agent.DecisionCount}, Stalled={agent.StalledDecisionCount}"
                : "not registered";
            return $"Session={Scenario.Status}, Tick={ElapsedSeconds:0.0}s, Entities={Registry.Count}, Factions={Factions.FactionCount}\n" +
                   $"Resources=[{resources}], Population={economy.PopulationUsed:0}/{economy.PopulationCapacity:0}, Queues=B{Buildings.QueuedCount}/T{Technologies.QueuedCount}/R{Recruitment.QueuedCount}\n" +
                   $"Movement={Movement.GetDebugSummary()}, Combat={Combat.GetDebugSummary()}, Army={Armies.GetDebugSummary()}\n" +
                   $"Siege={Sieges.GetDebugSummary()}, NavRefresh={_siegeNavigation.RefreshCount}, AI={ai}\n" +
                   $"Objective={GetObjectiveSummary()}, Last={LastCommandSummary}";
        }

        public PrototypeSaveData CaptureState()
        {
            Economy.TryGetState(PlayerCityId, out EconomyAccountSnapshot playerEconomy);
            Economy.TryGetState(EnemyFortressId, out EconomyAccountSnapshot enemyEconomy);
            SeededRandomState random = RandomSource.CaptureState();
            AI.TryCaptureRuntimeState(EnemyFactionId, out AiRuntimeStateSnapshot aiRuntime);
            var entities = new List<PrototypeEntitySaveData>();
            foreach (PrototypeEntityRecord record in Registry.Snapshot())
            {
                Movement.TryGetState(record.EntityId, out MovementStateSnapshot movement);
                Combat.TryGetState(record.EntityId, out CombatantSnapshot combat);
                entities.Add(new PrototypeEntitySaveData
                {
                    entityId = record.EntityId.Value,
                    definitionId = record.DefinitionId,
                    factionId = record.FactionId.Value,
                    isHero = record.IsHero,
                    x = Round(movement.Position.X),
                    y = Round(movement.Position.Y),
                    z = Round(movement.Position.Z),
                    health = Round(combat.Health),
                    movementStatus = movement.Status.ToString(),
                    movementOrders = Movement.SnapshotOrders(record.EntityId).Select(value => new PrototypeMovementOrderSaveData
                    {
                        x = Round(value.Destination.X),
                        y = Round(value.Destination.Y),
                        z = Round(value.Destination.Z),
                        formationSlotIndex = value.FormationSlotIndex,
                    }).ToArray(),
                    combatTargetId = combat.TargetId.Value,
                    attackCooldownRemaining = Round(combat.AttackCooldownRemaining),
                    engagementMode = combat.EngagementMode.ToString(),
                    engagementTargetReason = combat.TargetReason.ToString(),
                    engagementOriginX = Round(combat.EngagementOrigin.X),
                    engagementOriginY = Round(combat.EngagementOrigin.Y),
                    engagementOriginZ = Round(combat.EngagementOrigin.Z),
                });
            }

            var armies = new List<PrototypeArmySaveData>();
            foreach (ArmySnapshot army in Armies.Snapshot())
                armies.Add(new PrototypeArmySaveData
                {
                    armyId = army.ArmyId.Value,
                    factionId = army.FactionId.Value,
                    commanderId = army.CommanderId.Value,
                    members = army.UnitIds.Select(value => value.Value).ToArray(),
                    formation = army.Formation.ToString(),
                    morale = Round(army.Morale),
                    supply = Round(army.Supply),
                    orderType = army.Order.Type.ToString(),
                    orderX = Round(army.Order.Destination.X),
                    orderY = Round(army.Order.Destination.Y),
                    orderZ = Round(army.Order.Destination.Z),
                    orderTargetId = army.Order.TargetId.Value,
                });

            PrototypeBuildingQueueSaveData[] buildingQueue = Buildings.SnapshotQueue().Select(value =>
                new PrototypeBuildingQueueSaveData
                {
                    settlementId = value.SettlementId.Value,
                    buildingId = value.BuildingId.Value,
                    remainingSeconds = Round(value.RemainingSeconds),
                }).ToArray();
            PrototypeTechnologyQueueSaveData[] technologyQueue = Technologies.SnapshotQueue().Select(value =>
                new PrototypeTechnologyQueueSaveData
                {
                    factionId = value.FactionId.Value,
                    technologyId = value.TechnologyId.Value,
                    remainingSeconds = Round(value.RemainingSeconds),
                }).ToArray();
            PrototypeRecruitmentQueueSaveData[] recruitmentQueue = Recruitment.SnapshotQueue().Select(value =>
                new PrototypeRecruitmentQueueSaveData
                {
                    settlementId = value.SettlementId.Value,
                    factionId = value.FactionId.Value,
                    unitId = value.UnitId.Value,
                    remainingSeconds = Round(value.RemainingSeconds),
                }).ToArray();

            PrototypeSiegeSaveData siege = new PrototypeSiegeSaveData();
            if (Sieges.TryGetState(PlayerSiegeId, out SiegeSnapshot siegeState))
            {
                siege.exists = true;
                siege.state = siegeState.State.ToString();
                siege.area = siegeState.CurrentArea.ToString();
                siege.gateHealth = TryGetSiegeStructure(FortressGateId, out DefenseStructureSnapshot gate)
                    ? Round(gate.Health) : 0d;
                siege.strongholdHealth = TryGetSiegeStructure(FortressStrongholdId, out DefenseStructureSnapshot stronghold)
                    ? Round(stronghold.Health) : 0d;
                siege.gateRepairRemainingSeconds = Round(_gateRepairRemainingSeconds);
            }

            return new PrototypeSaveData
            {
                contentPackId = ContentPackId,
                scenarioId = ScenarioId,
                elapsedSeconds = Round(ElapsedSeconds),
                nextEntityId = _nextEntityId,
                randomSeed = random.Seed,
                randomDrawCount = random.DrawCount,
                randomState = random.InternalState,
                playerMaterial = Round(GetResource(playerEconomy, "resource.material")),
                playerSupply = Round(GetResource(playerEconomy, "resource.supply")),
                enemyMaterial = Round(GetResource(enemyEconomy, "resource.material")),
                enemySupply = Round(GetResource(enemyEconomy, "resource.supply")),
                playerPopulationUsed = Round(playerEconomy.PopulationUsed),
                playerPopulationCapacity = Round(playerEconomy.PopulationCapacity),
                enemyPopulationUsed = Round(enemyEconomy.PopulationUsed),
                enemyPopulationCapacity = Round(enemyEconomy.PopulationCapacity),
                economyBuildingBuilt = Buildings.IsBuilt(PlayerCityId, new DefinitionId("building.economy")),
                recruitmentBuildingBuilt = Buildings.IsBuilt(PlayerCityId, new DefinitionId("building.recruitment")),
                enemyEconomyBuildingBuilt = Buildings.IsBuilt(EnemyFortressId, new DefinitionId("building.economy")),
                enemyRecruitmentBuildingBuilt = Buildings.IsBuilt(EnemyFortressId, new DefinitionId("building.recruitment")),
                siegeTechnologyResearched = Technologies.IsResearched(PlayerFactionId, new DefinitionId("technology.siege")),
                fortressCaptured = Scenario.GetFact("fortress.captured") >= 1d,
                playerCityLost = Scenario.GetFact("player.city.lost") >= 1d,
                aiDeployed = _aiDeployed,
                aiCounterattackIssued = _aiCounterattackIssued,
                aiDecisionRemaining = Round(aiRuntime.DecisionRemaining),
                aiDecisionCount = aiRuntime.DecisionCount,
                aiStalledDecisionCount = aiRuntime.StalledDecisionCount,
                aiGoal = aiRuntime.Goal.ToString(),
                aiLayer = aiRuntime.Layer.ToString(),
                aiAction = aiRuntime.Action.ToString(),
                aiLastError = aiRuntime.LastError,
                entities = entities.ToArray(),
                armies = armies.ToArray(),
                buildingQueue = buildingQueue,
                technologyQueue = technologyQueue,
                recruitmentQueue = recruitmentQueue,
                siege = siege,
            };
        }

        public void RestoreState(PrototypeSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!string.Equals(data.contentPackId, ContentPackId, StringComparison.Ordinal) ||
                !string.Equals(data.scenarioId, ScenarioId, StringComparison.Ordinal))
                throw new InvalidOperationException("Save content or scenario is incompatible.");

            RestoreProgression(data);
            var savedIds = new HashSet<EntityId>(data.entities.Select(value => new EntityId(value.entityId)));
            foreach (PrototypeEntityRecord existing in Registry.Snapshot())
                if (!savedIds.Contains(existing.EntityId)) RemoveEntity(existing.EntityId, false);

            foreach (PrototypeEntitySaveData saved in data.entities.OrderBy(value => value.entityId))
            {
                EntityId id = new EntityId(saved.entityId);
                WorldPoint position = new WorldPoint(saved.x, saved.y, saved.z);
                if (!Registry.TryGet(id, out _))
                {
                    EntityId factionId = new EntityId(saved.factionId);
                    if (saved.isHero) SpawnHero(id, Find(_pack.Heroes, saved.definitionId), factionId, position);
                    else SpawnDefinition(id, Find(_pack.Units, saved.definitionId), factionId, position);
                }
                Navigation.SetPosition(id, position);
                Combat.UpdatePosition(id, position);
                RestoreHealth(id, saved.health);
            }
            Movement.Tick(0d);

            foreach (PrototypeArmySaveData army in data.armies.OrderBy(value => value.armyId))
            {
                EntityId armyId = new EntityId(army.armyId);
                if (!Armies.TryGetState(armyId, out _))
                {
                    EntityId[] members = army.members.Select(value => new EntityId(value)).Where(value => Registry.TryGet(value, out _)).ToArray();
                    if (members.Length > 0)
                        Armies.Execute(new CreateArmyCommand(armyId, new EntityId(army.factionId), members,
                            new EntityId(army.commanderId), ParseEnum(army.formation, AegisRTS.Gameplay.Formation.FormationType.Box)));
                }
                else
                {
                    RestoreArmyMembership(armyId, army);
                }
                if (Armies.TryGetState(armyId, out _))
                {
                    var order = new ArmyOrder(ParseEnum(army.orderType, ArmyOrderType.Idle),
                        new WorldPoint(army.orderX, army.orderY, army.orderZ), new EntityId(army.orderTargetId),
                        ParseEnum(army.formation, AegisRTS.Gameplay.Formation.FormationType.Box));
                    Armies.RestoreRuntimeState(armyId, army.morale, army.supply, order);
                }
            }

            if (data.siege != null && data.siege.exists) RestoreSiege(data.siege);
            SetResourceBalance(PlayerCityId, "resource.material", data.playerMaterial);
            SetResourceBalance(PlayerCityId, "resource.supply", data.playerSupply);
            SetResourceBalance(EnemyFortressId, "resource.material", data.enemyMaterial);
            SetResourceBalance(EnemyFortressId, "resource.supply", data.enemySupply);
            Economy.TryGetState(PlayerCityId, out EconomyAccountSnapshot restoredPlayerEconomy);
            Economy.AddPopulationCapacity(PlayerCityId,
                Math.Max(0d, data.playerPopulationCapacity - restoredPlayerEconomy.PopulationCapacity));
            Economy.TryGetState(PlayerCityId, out restoredPlayerEconomy);
            Economy.TryReservePopulation(PlayerCityId, Math.Max(0d, data.playerPopulationUsed - restoredPlayerEconomy.PopulationUsed));
            Economy.TryGetState(EnemyFortressId, out EconomyAccountSnapshot restoredEnemyEconomy);
            Economy.AddPopulationCapacity(EnemyFortressId,
                Math.Max(0d, data.enemyPopulationCapacity - restoredEnemyEconomy.PopulationCapacity));
            Economy.TryGetState(EnemyFortressId, out restoredEnemyEconomy);
            Economy.TryReservePopulation(EnemyFortressId, Math.Max(0d, data.enemyPopulationUsed - restoredEnemyEconomy.PopulationUsed));
            foreach (PrototypeBuildingQueueSaveData job in data.buildingQueue)
                Buildings.RestoreQueuedJob(new EntityId(job.settlementId), new DefinitionId(job.buildingId), job.remainingSeconds);
            foreach (PrototypeTechnologyQueueSaveData job in data.technologyQueue)
                Technologies.RestoreQueuedJob(new EntityId(job.factionId), new DefinitionId(job.technologyId), job.remainingSeconds);
            foreach (PrototypeRecruitmentQueueSaveData job in data.recruitmentQueue)
                Recruitment.RestoreQueuedJob(new EntityId(job.settlementId), new EntityId(job.factionId),
                    new DefinitionId(job.unitId), job.remainingSeconds);
            foreach (PrototypeEntitySaveData saved in data.entities.OrderBy(value => value.entityId))
            {
                EntityId id = new EntityId(saved.entityId);
                var orders = (saved.movementOrders ?? Array.Empty<PrototypeMovementOrderSaveData>()).Select(value =>
                    new MovementOrderSnapshot(new WorldPoint(value.x, value.y, value.z), value.formationSlotIndex)).ToArray();
                Movement.RestoreOrders(id, orders, ParseEnum(saved.movementStatus, MovementStatus.Idle));
                Combat.RestoreRuntimeState(
                    id,
                    new EntityId(saved.combatTargetId),
                    saved.attackCooldownRemaining,
                    ParseEnum(saved.engagementMode, UnitEngagementMode.Normal),
                    string.IsNullOrWhiteSpace(saved.engagementMode)
                        ? new WorldPoint(saved.x, saved.y, saved.z)
                        : new WorldPoint(saved.engagementOriginX, saved.engagementOriginY, saved.engagementOriginZ),
                    ParseEnum(saved.engagementTargetReason, EngagementTargetReason.ManualOrder));
            }
            if (data.fortressCaptured) Scenario.SetFact("fortress.captured", 1d);
            if (data.playerCityLost) Scenario.SetFact("player.city.lost", 1d);
            _nextEntityId = Math.Max(data.nextEntityId, data.entities.Length == 0 ? 2000UL : data.entities.Max(value => value.entityId) + 1UL);
            RandomSource = SeededRandom.Restore(new SeededRandomState(data.randomSeed, data.randomDrawCount, data.randomState));
            _aiDeployed = data.aiDeployed;
            _aiCounterattackIssued = data.aiCounterattackIssued;
            _gateRepairRemainingSeconds = data.siege == null ? 0d : Math.Max(0d, data.siege.gateRepairRemainingSeconds);
            AI.RestoreRuntimeState(EnemyFactionId, new AiRuntimeStateSnapshot(data.aiDecisionRemaining,
                data.aiDecisionCount, data.aiStalledDecisionCount,
                ParseEnum(data.aiGoal, AiStrategicGoal.Economy), ParseEnum(data.aiLayer, AiDecisionLayer.Strategic),
                ParseEnum(data.aiAction, AiActionType.Wait), data.aiLastError));
            ElapsedSeconds = data.elapsedSeconds;
            LastCommandSummary = "Save restored.";
            Notify(LastCommandSummary);
        }

        public AiWorldSnapshot Observe(EntityId factionId)
        {
            EntityId accountId = factionId == EnemyFactionId ? EnemyFortressId : PlayerCityId;
            Economy.TryGetState(accountId, out EconomyAccountSnapshot economy);
            double stockpile = economy.Resources == null ? 0d : economy.Resources.Sum(value => value.Value);
            double income = economy.Production == null ? 0d : economy.Production.Sum(value => value.Value);
            int unitCount = Registry.GetFactionEntities(factionId).Count;
            bool armyReady = factionId == EnemyFactionId
                ? Armies.TryGetState(EnemyArmyId, out _)
                : Armies.TryGetState(PlayerArmyId, out _);
            bool targetCaptured = factionId == EnemyFactionId
                ? Scenario.GetFact("player.city.lost") >= 1d
                : Scenario.GetFact("fortress.captured") >= 1d;
            bool economyReady = _pack.Rules != null && _pack.Rules.StrongholdRecruitmentEnabled ||
                                Buildings.IsBuilt(accountId, new DefinitionId("building.economy")) &&
                                Buildings.IsBuilt(accountId, new DefinitionId("building.recruitment"));
            bool recruitmentQueued = Recruitment.SnapshotQueue().Any(value => value.FactionId == factionId);
            return new AiWorldSnapshot(factionId, stockpile, income, unitCount, armyReady ? 1 : 0, 1, 1,
                unitCount * 25d, Registry.GetFactionEntities(OpponentOf(factionId)).Count * 20d,
                factionId == EnemyFactionId ? PlayerCityId : EnemyFortressId,
                new[] { new EntityId(1503), new EntityId(1502), new EntityId(1501) },
                economyReady, recruitmentQueued, armyReady, _aiDeployed, _aiCounterattackIssued, false, false, targetCaptured);
        }

        public AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world)
        {
            if (factionId != EnemyFactionId) return AiActionResult.Rejected("Only the opponent faction is AI controlled.");
            if (action == AiActionType.DevelopEconomy)
            {
                DefinitionId building = new DefinitionId("building.economy");
                if (Buildings.IsBuilt(EnemyFortressId, building)) return AiActionResult.Waiting();
                if (Buildings.SnapshotQueue().Any(value => value.SettlementId == EnemyFortressId)) return AiActionResult.Waiting();
                CommandDispatchResult result = Dispatch(new ConstructBuildingCommand(EnemyFortressId, EnemyFactionId, building), "AI");
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }

            if (action == AiActionType.Recruit)
            {
                CommandDispatchResult result = Dispatch(new RecruitUnitCommand(EnemyFortressId, EnemyFactionId,
                    new DefinitionId("unit.infantry")), "AI");
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }

            if (action == AiActionType.MoveToTarget)
            {
                if (ElapsedSeconds < EnemyAttackGraceSeconds) return AiActionResult.Progress();
                CommandDispatchResult result = Dispatch(new MoveArmyCommand(EnemyArmyId, new WorldPoint(-5d, 0d, 0d)), "AI");
                _aiDeployed = result.WasHandled;
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }

            if (action == AiActionType.StartSiege || action == AiActionType.Breach || action == AiActionType.ProtectSiege)
            {
                if (ElapsedSeconds < EnemyAttackGraceSeconds) return AiActionResult.Progress();
                EntityId target = FindAliveTarget(PlayerFactionId);
                if (!target.IsValid) return AiActionResult.Waiting();
                CommandDispatchResult result = Dispatch(new AttackArmyCommand(EnemyArmyId, target), "AI");
                if (result.WasHandled) _aiCounterattackIssued = true;
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }

            if (action == AiActionType.Recover || action == AiActionType.Wait || action == AiActionType.HoldPosition)
                return AiActionResult.Waiting();
            return AiActionResult.Waiting();
        }

        public void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId)
        {
            UnitDefinition definition = Find(_pack.Units, unitId);
            SeededRandomState randomBefore = RandomSource.CaptureState();
            EntityId spawnedId = new EntityId(_nextEntityId);
            bool spawned = false;
            try
            {
                double lateralOffset = (RandomSource.NextDouble() - 0.5d) * 1.5d;
                WorldPoint spawn = factionId == PlayerFactionId
                    ? new WorldPoint(-12d + Registry.GetFactionEntities(factionId).Count, 0d, -2d + lateralOffset)
                    : new WorldPoint(12d - Registry.GetFactionEntities(factionId).Count, 0d, 3d + lateralOffset);
                SpawnDefinition(spawnedId, definition, factionId, spawn);
                spawned = true;
                if (factionId == EnemyFactionId && Armies.TryGetState(EnemyArmyId, out _))
                {
                    EntityId reinforcementArmy = new EntityId(1311);
                    CommandDispatchResult created = Dispatch(new CreateArmyCommand(reinforcementArmy, EnemyFactionId,
                        new[] { spawnedId }), "AI");
                    if (!created.WasHandled || !Dispatch(new MergeArmiesCommand(EnemyArmyId, reinforcementArmy), "AI").WasHandled)
                        throw new InvalidOperationException("AI reinforcement could not join its army through shared commands.");
                }
                _nextEntityId++;
            }
            catch
            {
                if (spawned) RemoveEntity(spawnedId, false);
                RandomSource = SeededRandom.Restore(randomBefore);
                throw;
            }
            Notify($"Recruit complete: {definition.DisplayName}.");
        }

        public void BuildingCompleted(EntityId settlementId, DefinitionId buildingId)
        {
            Settlements.AddBuilding(settlementId, buildingId.Value);
            Notify($"Building complete: {buildingId}.");
        }

        public void TechnologyCompleted(EntityId factionId, DefinitionId technologyId)
        {
            Factions.UnlockTechnology(factionId, technologyId.Value);
            Notify($"Research complete: {technologyId}.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            for (int index = _registrations.Count - 1; index >= 0; index--) _registrations[index].Dispose();
            _registrations.Clear();
            Commands.Clear();
            Events.Clear();
        }

        private void ConfigureWorld()
        {
            Factions.Register(PlayerFactionId, new FactionProfile("faction.player"));
            Factions.Register(EnemyFactionId, new FactionProfile("faction.enemy", "ai.prototype"));
            Factions.SetDiplomacy(PlayerFactionId, EnemyFactionId, DiplomacyStatus.War);
            Territories.RegisterNode(new EntityId(1501), new TerritoryNodeProfile("territory.player-city", 100d, PlayerCityId), PlayerFactionId);
            Territories.RegisterNode(new EntityId(1502), new TerritoryNodeProfile("territory.village", 50d, VillageId), PlayerFactionId);
            Territories.RegisterNode(new EntityId(1503), new TerritoryNodeProfile("territory.enemy-fortress", 120d, EnemyFortressId), EnemyFactionId);
            Territories.Connect(new EntityId(1501), new EntityId(1502));
            Territories.Connect(new EntityId(1502), new EntityId(1503));
            Settlements.Register(PlayerCityId, SettlementProfile.FromDefinition(Find(_pack.Settlements, "settlement.player-city")), PlayerFactionId);
            Settlements.Register(VillageId, SettlementProfile.FromDefinition(Find(_pack.Settlements, "settlement.village")), PlayerFactionId);
            Settlements.Register(EnemyFortressId, SettlementProfile.FromDefinition(Find(_pack.Settlements, "settlement.enemy-fortress")), EnemyFactionId);

            var startingResources = new[]
            {
                new ResourceCost(new DefinitionId("resource.material"), 200d),
                new ResourceCost(new DefinitionId("resource.supply"), 120d),
            };
            Economy.RegisterAccount(PlayerCityId, startingResources, populationCapacity: 12d);
            Economy.RegisterAccount(EnemyFortressId, startingResources, populationCapacity: 12d);

            SpawnHero(PlayerHeroId, Find(_pack.Heroes, "hero.commander"), PlayerFactionId, new WorldPoint(-11d, 0d, 0d));
            SpawnHero(PlayerLieutenantId, Find(_pack.Heroes, "hero.lieutenant"), PlayerFactionId, new WorldPoint(-10d, 0d, 2d));
            SpawnDefinition(new EntityId(_nextEntityId++), Find(_pack.Units, "unit.infantry"), PlayerFactionId, new WorldPoint(-9d, 0d, -1d));
            SpawnDefinition(new EntityId(_nextEntityId++), Find(_pack.Units, "unit.archer"), PlayerFactionId, new WorldPoint(-9d, 0d, 1d));
            SpawnHero(EnemyHeroId, Find(_pack.Heroes, "hero.opponent"), EnemyFactionId, new WorldPoint(11d, 0d, 3d));
            SpawnDefinition(new EntityId(_nextEntityId++), Find(_pack.Units, "unit.infantry"), EnemyFactionId, new WorldPoint(9d, 0d, 2d));
            SpawnDefinition(new EntityId(_nextEntityId++), Find(_pack.Units, "unit.archer"), EnemyFactionId, new WorldPoint(9d, 0d, 4d));
            SpawnDefinition(new EntityId(_nextEntityId++), Find(_pack.Units, "unit.cavalry"), EnemyFactionId, new WorldPoint(10d, 0d, 5d));
            ArmyCommandResult enemyArmy = Armies.Execute(new CreateArmyCommand(
                EnemyArmyId, EnemyFactionId, Registry.GetFactionEntities(EnemyFactionId), EnemyHeroId));
            if (!enemyArmy.Succeeded) throw new InvalidOperationException(enemyArmy.Error);

            Scenario.Start(_scenarioDefinition);
            AI.Register(EnemyFactionId, AiProfile.FromDefinition(Find(_pack.AiProfiles, "ai.prototype")), this, this);
        }

        private void RegisterCommands()
        {
            _registrations.Add(Commands.RegisterHandler<MoveUnitsCommand>(command =>
            {
                MovementCommandResult result = Movement.IssueMove(command);
                if (result.WasAccepted && !command.Queue) Combat.NotifyMoveOrder(command.ActorIds, command.Destination);
                LastCommandSummary = $"Move accepted {result.AcceptedActorCount}/{command.ActorIds.Count}.";
            }));
            _registrations.Add(Commands.RegisterHandler<StopUnitsCommand>(command =>
            {
                Movement.IssueStop(command);
                Combat.NotifyHoldOrder(command.ActorIds);
                LastCommandSummary = $"Stopped {command.ActorIds.Count} unit(s).";
            }));
            _registrations.Add(Commands.RegisterHandler<HoldUnitsCommand>(command =>
            {
                Movement.IssueHold(command);
                Combat.NotifyHoldOrder(command.ActorIds);
                LastCommandSummary = $"Holding {command.ActorIds.Count} unit(s).";
            }));
            _registrations.Add(Commands.RegisterValidator<SetUnitEngagementModeCommand>(command =>
                command.ActorIds.Any(id => Combat.TryGetState(id, out CombatantSnapshot actor) && actor.IsAlive)
                    ? CommandValidationResult.Valid()
                    : CommandValidationResult.Invalid("No living combat unit can change engagement mode.")));
            _registrations.Add(Commands.RegisterHandler<SetUnitEngagementModeCommand>(command =>
            {
                int count = Combat.SetEngagementMode(command);
                LastCommandSummary = $"Engagement mode {command.Mode} applied to {count}/{command.ActorIds.Count} unit(s).";
                Notify(LastCommandSummary);
            }));
            _registrations.Add(Commands.RegisterValidator<AttackTargetCommand>(command =>
                Combat.TryGetState(command.TargetId, out CombatantSnapshot target) && target.IsAlive
                    ? CommandValidationResult.Valid()
                    : CommandValidationResult.Invalid("Attack target is unavailable.")));
            _registrations.Add(Commands.RegisterHandler<AttackTargetCommand>(command =>
            {
                int count = IssueAttackAndApproach(command.ActorIds, command.TargetId, command.Queue);
                LastCommandSummary = $"Attack accepted {count}/{command.ActorIds.Count} against {command.TargetId}.";
            }));
            _registrations.Add(Commands.RegisterHandler<FollowTargetCommand>(command =>
            {
                if (Movement.TryGetState(command.TargetId, out MovementStateSnapshot target))
                    Movement.IssueMove(new MoveUnitsCommand(command.ActorIds, target.Position, command.Queue));
                LastCommandSummary = $"Follow {command.TargetId}.";
            }));
            _registrations.Add(Commands.RegisterHandler<InteractTargetCommand>(command =>
                LastCommandSummary = $"Interact {command.TargetId}."));

            RegisterValidatedCommand<ConstructBuildingCommand>(command => Buildings.Validate(command).Succeeded,
                command => Buildings.Validate(command).Error, command => Buildings.Request(command), "Construction queued");
            RegisterValidatedCommand<ResearchTechnologyCommand>(command => Technologies.Validate(command).Succeeded,
                command => Technologies.Validate(command).Error, command => Technologies.Request(command), "Research queued");
            RegisterValidatedCommand<RecruitUnitCommand>(command => Recruitment.Validate(command).Succeeded,
                command => Recruitment.Validate(command).Error, command => Recruitment.Request(command), "Recruitment queued");

            _registrations.Add(Commands.RegisterValidator<CreateArmyCommand>(command =>
            {
                ArmyCommandResult result = Armies.Validate(command);
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<CreateArmyCommand>(command =>
            {
                ArmyCommandResult result = Armies.Execute(command);
                LastCommandSummary = result.Succeeded ? $"Army {command.ArmyId} created with {result.AffectedUnitCount} members." : result.Error;
            }));
            RegisterArmyMutation<MergeArmiesCommand>(command => Armies.Validate(command), command => Armies.Execute(command), "Armies merged");
            RegisterArmyMutation<SplitArmyCommand>(command => Armies.Validate(command), command => Armies.Execute(command), "Army split");
            RegisterArmyMutation<AssignArmyCommanderCommand>(command => Armies.Validate(command), command => Armies.Execute(command), "Commander assigned");
            RegisterArmyOrder<MoveArmyCommand>(command => Armies.Validate(command), command => Armies.Execute(command));
            RegisterArmyOrder<AttackArmyCommand>(command => Armies.Validate(command), command => Armies.Execute(command));
            RegisterArmyOrder<DefendArmyCommand>(command => Armies.Validate(command), command => Armies.Execute(command));
            RegisterArmyOrder<RetreatArmyCommand>(command => Armies.Validate(command), command => Armies.Execute(command));

            _registrations.Add(Commands.RegisterValidator<PrototypeStartSiegeCommand>(command =>
                FindPlayerSiegeUnit().IsValid && Armies.TryGetState(PlayerArmyId, out ArmySnapshot army)
                    ? CommandValidationResult.Valid()
                    : CommandValidationResult.Invalid("A player army containing a siege unit is required.")));
            _registrations.Add(Commands.RegisterHandler<PrototypeStartSiegeCommand>(_ => HandleStartSiege()));
            _registrations.Add(Commands.RegisterValidator<PrototypeBreachGateCommand>(_ =>
            {
                if (!_playerSiegeRegistered) return CommandValidationResult.Invalid("Start the siege first.");
                SiegeActionResult result = Sieges.Validate(new AttackDefenseStructureCommand(
                    PlayerSiegeId, FindPlayerSiegeUnit(), FortressGateId));
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<PrototypeBreachGateCommand>(_ => HandleBreachGate()));
            _registrations.Add(Commands.RegisterValidator<PrototypeEnterFortressCommand>(_ =>
            {
                if (!_playerSiegeRegistered) return CommandValidationResult.Invalid("Start the siege first.");
                SiegeActionResult result = Sieges.Validate(new EnterSiegeAreaCommand(PlayerSiegeId, SiegeArea.InnerArea));
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<PrototypeEnterFortressCommand>(_ => HandleEnterFortress()));
            _registrations.Add(Commands.RegisterValidator<PrototypeCaptureFortressCommand>(_ =>
            {
                if (!_playerSiegeRegistered) return CommandValidationResult.Invalid("Start the siege first.");
                SiegeActionResult result = Sieges.Validate(new AttackDefenseStructureCommand(
                    PlayerSiegeId, FindPlayerSiegeUnit(), FortressStrongholdId));
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<PrototypeCaptureFortressCommand>(_ => HandleCaptureFortress()));
        }

        private void RegisterEvents()
        {
            _registrations.Add(Events.Subscribe<UnitDiedEvent>(value =>
            {
                if (!_pendingDeaths.Contains(value.EntityId)) _pendingDeaths.Add(value.EntityId);
                Sieges.NotifyUnitDied(value.EntityId);
                Notify($"Unit {value.EntityId} was defeated.");
            }));
            _registrations.Add(Events.Subscribe<DamageAppliedEvent>(value =>
                Notify($"Damage {value.Amount:0} to {value.TargetId}; HP {value.RemainingHealth:0}.")));
            _registrations.Add(Events.Subscribe<ProjectileLaunchedEvent>(value =>
                Notify($"Projectile {value.SourceId} → {value.TargetId} at {value.Speed:0.0}/s.")));
            _registrations.Add(Events.Subscribe<BreachCreatedEvent>(_ =>
            {
                Notify("Fortress gate breached; navigation refreshed.");
                FortressGateBreached?.Invoke();
            }));
            _registrations.Add(Events.Subscribe<DefenseStructureDamagedEvent>(value =>
            {
                if (value.SiegeId == PlayerSiegeId && value.StructureId == FortressGateId &&
                    _pack.Rules != null && _pack.Rules.GateRepairEnabled)
                    _gateRepairRemainingSeconds = GateRepairDelaySeconds;
            }));
            _registrations.Add(Events.Subscribe<DefenseStructureRepairedEvent>(value =>
            {
                if (value.SiegeId != PlayerSiegeId || value.StructureId != FortressGateId) return;
                Notify($"Defenders repaired the fortress gate to {value.Health:0} HP.");
                FortressGateRepaired?.Invoke();
            }));
            _registrations.Add(Events.Subscribe<BreachSealedEvent>(_ => Notify("Fortress gate breach was sealed.")));
            _registrations.Add(Events.Subscribe<SettlementOwnerChangedEvent>(value =>
            {
                if (value.SettlementId == EnemyFortressId && value.NewOwnerId == PlayerFactionId)
                {
                    Scenario.SetFact("fortress.captured", 1d);
                    FortressCaptured?.Invoke();
                }
                if (value.SettlementId == PlayerCityId && value.NewOwnerId == EnemyFactionId)
                    Scenario.SetFact("player.city.lost", 1d);
                Notify($"Settlement {value.SettlementId} owner changed to {value.NewOwnerId}.");
            }));
            _registrations.Add(Events.Subscribe<ScenarioCompletedEvent>(value => Notify($"Scenario completed: {value.Status}.")));
        }

        private void RegisterValidatedCommand<TCommand>(Func<TCommand, bool> succeeds, Func<TCommand, string> error,
            Action<TCommand> execute, string accepted) where TCommand : ICommand
        {
            _registrations.Add(Commands.RegisterValidator<TCommand>(command => succeeds(command)
                ? CommandValidationResult.Valid()
                : CommandValidationResult.Invalid(error(command))));
            _registrations.Add(Commands.RegisterHandler<TCommand>(command =>
            {
                execute(command);
                LastCommandSummary = accepted + ".";
                Notify(LastCommandSummary);
            }));
        }

        private void RegisterArmyOrder<TCommand>(Func<TCommand, ArmyCommandResult> validate,
            Func<TCommand, ArmyCommandResult> execute) where TCommand : ICommand
        {
            _registrations.Add(Commands.RegisterValidator<TCommand>(command =>
            {
                ArmyCommandResult result = validate(command);
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<TCommand>(command =>
            {
                ArmyCommandResult result = execute(command);
                LastCommandSummary = result.Succeeded ? $"Army order accepted for {result.AffectedUnitCount} member(s)." : result.Error;
            }));
        }

        private void RegisterArmyMutation<TCommand>(Func<TCommand, ArmyCommandResult> validate,
            Func<TCommand, ArmyCommandResult> execute, string accepted) where TCommand : ICommand
        {
            _registrations.Add(Commands.RegisterValidator<TCommand>(command =>
            {
                ArmyCommandResult result = validate(command);
                return result.Succeeded ? CommandValidationResult.Valid() : CommandValidationResult.Invalid(result.Error);
            }));
            _registrations.Add(Commands.RegisterHandler<TCommand>(command =>
            {
                ArmyCommandResult result = execute(command);
                LastCommandSummary = result.Succeeded ? $"{accepted}: {result.AffectedUnitCount} member(s)." : result.Error;
                Notify(LastCommandSummary);
            }));
        }

        private CommandDispatchResult Dispatch<TCommand>(TCommand command, string source = "Player") where TCommand : ICommand
        {
            CommandDispatchResult result = Commands.Dispatch(command);
            if (!result.WasHandled)
            {
                LastCommandSummary = $"{source} command rejected: {result.Error}";
                Notify(LastCommandSummary);
            }
            return result;
        }

        private CommandDispatchResult Reject(string error)
        {
            LastCommandSummary = $"Player command rejected: {error}";
            Notify(LastCommandSummary);
            return CommandDispatchResult.Rejected(error);
        }

        private int IssueAttackAndApproach(IReadOnlyList<EntityId> actors, EntityId targetId, bool queue = false)
        {
            int accepted = Combat.IssueAttack(new AttackTargetCommand(actors, targetId));
            if (accepted > 0) CombatMovement.Tick();
            return accepted;
        }

        private void HandleStartSiege()
        {
            if (!_playerSiegeRegistered)
            {
                Sieges.Register(PlayerSiegeId, new SiegeProfile(EnemyFortressId, PlayerFactionId, EnemyFactionId,
                    SiegeMode.Assault, PlayerArmyId));
                Sieges.RegisterStructure(PlayerSiegeId, FortressGateId,
                    DefenseStructureProfile.FromDefinition(Find(_pack.DefenseStructures, "structure.gate"), EnemyFactionId));
                Sieges.RegisterStructure(PlayerSiegeId, FortressStrongholdId,
                    DefenseStructureProfile.FromDefinition(Find(_pack.DefenseStructures, "structure.stronghold-core"), EnemyFactionId));
                _playerSiegeRegistered = true;
            }
            SiegeActionResult result = Sieges.Execute(new StartSiegeCommand(PlayerSiegeId));
            LastCommandSummary = result.Succeeded ? "Siege started." : result.Error;
            Notify(LastCommandSummary);
        }

        private void HandleBreachGate()
        {
            SiegeActionResult result = Sieges.Execute(new AttackDefenseStructureCommand(
                PlayerSiegeId, FindPlayerSiegeUnit(), FortressGateId));
            LastCommandSummary = result.Succeeded ? "Siege unit attacked the gate." : result.Error;
            Notify(LastCommandSummary);
        }

        private void HandleEnterFortress()
        {
            SiegeActionResult inner = Sieges.Execute(new EnterSiegeAreaCommand(PlayerSiegeId, SiegeArea.InnerArea));
            if (!inner.Succeeded)
            {
                LastCommandSummary = inner.Error;
                Notify(inner.Error);
                return;
            }
            SiegeActionResult objective = Sieges.Execute(new EnterSiegeAreaCommand(PlayerSiegeId, SiegeArea.CaptureObjective));
            if (objective.Succeeded) _gateRepairRemainingSeconds = 0d;
            LastCommandSummary = objective.Succeeded ? "Army controls the capture objective." : objective.Error;
            Notify(LastCommandSummary);
        }

        private void HandleCaptureFortress()
        {
            SiegeActionResult result = Sieges.Execute(new AttackDefenseStructureCommand(
                PlayerSiegeId, FindPlayerSiegeUnit(), FortressStrongholdId));
            if (result.Succeeded && TryGetSiegeStructure(FortressStrongholdId, out DefenseStructureSnapshot core) && core.IsDestroyed)
                result = Sieges.Execute(new CaptureSiegeCommand(PlayerSiegeId));
            LastCommandSummary = result.Succeeded ? "Stronghold subdued; enemy fortress captured intact." : result.Error;
            Notify(LastCommandSummary);
        }

        private void TickGateRepair(double deltaSeconds)
        {
            if (_gateRepairRemainingSeconds <= 0d || !_playerSiegeRegistered) return;
            if (!Sieges.TryGetState(PlayerSiegeId, out SiegeSnapshot siege) ||
                siege.CurrentArea == SiegeArea.InnerArea || siege.CurrentArea == SiegeArea.CaptureObjective ||
                siege.State == SiegeState.Completed || siege.State == SiegeState.Failed)
            {
                _gateRepairRemainingSeconds = 0d;
                return;
            }
            _gateRepairRemainingSeconds = Math.Max(0d, _gateRepairRemainingSeconds - deltaSeconds);
            if (_gateRepairRemainingSeconds > 0d) return;
            CommandDispatchResult result = Dispatch(new RepairDefenseStructureCommand(
                PlayerSiegeId, EnemyHeroId, FortressGateId, GateRepairAmount), "AI defender");
            if (result.WasHandled && TryGetSiegeStructure(FortressGateId, out DefenseStructureSnapshot gate) &&
                gate.Health < gate.Profile.MaxHealth)
                _gateRepairRemainingSeconds = GateRepairDelaySeconds;
        }

        private void SpawnHero(EntityId id, HeroDefinition definition, EntityId factionId, WorldPoint position)
        {
            Heroes.Register(id, HeroProfile.FromDefinition(definition, factionId));
            try
            {
                AttackProfile attack = new AttackProfile(factionId == PlayerFactionId ? 32d : 25d,
                    DamageType.Physical, 2.3d, 0.8d, 0.15d);
                var profile = new CombatantProfile(definition.Id.Value, factionId, definition.MaxHealth, attack,
                    new DefenseProfile(3d), tags: new[] { "unit", "hero" },
                    abilityIds: definition.AbilityIds.Select(value => value.Value));
                RegisterEntity(new PrototypeEntityRecord(id, definition.Id.Value, definition.PrefabId, factionId, position,
                    definition.MovementSpeed, profile, true));
            }
            catch
            {
                Heroes.Unregister(id);
                throw;
            }
        }

        private void SpawnDefinition(EntityId id, UnitDefinition definition, EntityId factionId, WorldPoint position)
        {
            bool ranged = HasTag(definition.Tags, "archer");
            bool cavalry = HasTag(definition.Tags, "cavalry");
            bool siege = HasTag(definition.Tags, "siege-unit");
            AttackProfile attack = new AttackProfile(siege ? 50d : cavalry ? 24d : ranged ? 14d : 18d,
                DamageType.Physical, siege ? 6d : ranged ? 9d : 1.8d,
                siege ? 1.6d : ranged ? 1.2d : 0.9d, 0.15d,
                ranged ? 15d : 0d, targetTags: siege ? new[] { "structure", "unit" } : null);
            var tags = definition.Tags.Select(value => value.Value).ToList();
            if (!tags.Contains("unit")) tags.Add("unit");
            var profile = new CombatantProfile(definition.Id.Value, factionId, definition.MaxHealth,
                attack, new DefenseProfile(cavalry ? 4d : 1d), tags: tags);
            RegisterEntity(new PrototypeEntityRecord(id, definition.Id.Value, definition.PrefabId, factionId, position,
                definition.MovementSpeed, profile, false));
        }

        private void RegisterEntity(PrototypeEntityRecord record)
        {
            bool registryRegistered = false;
            bool navigationRegistered = false;
            bool movementRegistered = false;
            bool combatRegistered = false;
            bool armyRegistered = false;
            try
            {
                Registry.Register(record); registryRegistered = true;
                Navigation.Register(record.EntityId, record.SpawnPosition, record.MovementSpeed); navigationRegistered = true;
                Movement.Register(record.EntityId, record.SpawnPosition); movementRegistered = true;
                Combat.Register(record.EntityId, record.CombatProfile, record.SpawnPosition); combatRegistered = true;
                Combat.SetEngagementMode(new SetUnitEngagementModeCommand(
                    new[] { record.EntityId }, UnitEngagementMode.Normal));
                Armies.RegisterMember(record.EntityId, record.FactionId); armyRegistered = true;
                EntitySpawned?.Invoke(record);
            }
            catch
            {
                if (armyRegistered) Armies.UnregisterMember(record.EntityId);
                if (combatRegistered) Combat.Unregister(record.EntityId);
                if (movementRegistered) Movement.Unregister(record.EntityId);
                if (navigationRegistered) Navigation.Unregister(record.EntityId);
                if (registryRegistered) Registry.Remove(record.EntityId);
                EntityRemoved?.Invoke(record.EntityId);
                throw;
            }
        }

        private void SyncCombatPositions()
        {
            foreach (MovementStateSnapshot state in Movement.Snapshot()) Combat.UpdatePosition(state.EntityId, state.Position);
        }

        private void ProcessDeaths()
        {
            if (_pendingDeaths.Count == 0) return;
            EntityId[] deaths = _pendingDeaths.ToArray();
            _pendingDeaths.Clear();
            foreach (EntityId id in deaths)
            {
                RemoveEntity(id, true);
            }
        }

        private void RemoveEntity(EntityId id, bool applyDefeat)
        {
            if (!Registry.TryGet(id, out PrototypeEntityRecord record)) return;
            Movement.Unregister(id);
            Navigation.Unregister(id);
            Combat.Unregister(id);
            Armies.UnregisterMember(id);
            if (record.IsHero) Heroes.Unregister(id);
            Registry.Remove(id);
            EntityRemoved?.Invoke(id);
            if (applyDefeat && id == PlayerHeroId) Scenario.SetFact("player.city.lost", 1d);
        }

        private void RestoreProgression(PrototypeSaveData data)
        {
            if (data.economyBuildingBuilt) RestoreBuiltBuilding(PlayerCityId, PlayerFactionId, "building.economy");
            if (data.recruitmentBuildingBuilt) RestoreBuiltBuilding(PlayerCityId, PlayerFactionId, "building.recruitment");
            if (data.enemyEconomyBuildingBuilt) RestoreBuiltBuilding(EnemyFortressId, EnemyFactionId, "building.economy");
            if (data.enemyRecruitmentBuildingBuilt) RestoreBuiltBuilding(EnemyFortressId, EnemyFactionId, "building.recruitment");
            if (data.siegeTechnologyResearched && !Technologies.IsResearched(PlayerFactionId, new DefinitionId("technology.siege")))
            {
                EnsureResources(PlayerCityId);
                Technologies.Request(new ResearchTechnologyCommand(PlayerCityId, PlayerFactionId, new DefinitionId("technology.siege")));
                Technologies.Tick(10d);
            }
        }

        private void RestoreArmyMembership(EntityId armyId, PrototypeArmySaveData saved)
        {
            if (!Armies.TryGetState(armyId, out ArmySnapshot current)) return;
            var desired = new HashSet<EntityId>((saved.members ?? Array.Empty<ulong>())
                .Select(value => new EntityId(value)).Where(value => Registry.TryGet(value, out _)));
            foreach (EntityId member in current.UnitIds.Where(value => !desired.Contains(value)).ToArray())
                Armies.UnregisterMember(member);

            EntityId[] missing = desired.Where(value => !Armies.TryGetArmyForUnit(value, out _)).OrderBy(value => value).ToArray();
            if (missing.Length > 0)
            {
                EntityId temporaryArmyId = new EntityId(910000UL + armyId.Value);
                ArmyCommandResult created = Armies.Execute(new CreateArmyCommand(
                    temporaryArmyId, new EntityId(saved.factionId), missing));
                if (!created.Succeeded) throw new InvalidOperationException($"Could not restore army members: {created.Error}");
                ArmyCommandResult merged = Armies.Execute(new MergeArmiesCommand(armyId, temporaryArmyId));
                if (!merged.Succeeded) throw new InvalidOperationException($"Could not merge restored army members: {merged.Error}");
            }

            EntityId commanderId = new EntityId(saved.commanderId);
            if (commanderId.IsValid && Armies.TryGetState(armyId, out ArmySnapshot restored) &&
                restored.CommanderId != commanderId)
            {
                ArmyCommandResult assigned = Armies.Execute(new AssignArmyCommanderCommand(armyId, commanderId));
                if (!assigned.Succeeded) throw new InvalidOperationException($"Could not restore army commander: {assigned.Error}");
            }
        }

        private void RestoreBuiltBuilding(EntityId settlementId, EntityId factionId, string buildingId)
        {
            var definitionId = new DefinitionId(buildingId);
            if (Buildings.IsBuilt(settlementId, definitionId)) return;
            EnsureResources(settlementId);
            BuildingRequestResult result = Buildings.Request(new ConstructBuildingCommand(settlementId, factionId, definitionId));
            if (!result.Succeeded) throw new InvalidOperationException($"Could not restore completed building '{buildingId}': {result.Error}");
            Buildings.Tick(10d);
        }

        private void RestoreSiege(PrototypeSiegeSaveData saved)
        {
            if (!_playerSiegeRegistered) HandleStartSiege();
            if (saved.gateHealth <= 45d && TryGetSiegeStructure(FortressGateId, out DefenseStructureSnapshot first) && first.Health > saved.gateHealth)
                HandleBreachGate();
            if (saved.gateHealth <= 0d && TryGetSiegeStructure(FortressGateId, out DefenseStructureSnapshot second) && second.Health > 0d)
                HandleBreachGate();
            if (string.Equals(saved.area, SiegeArea.InnerArea.ToString(), StringComparison.Ordinal) ||
                string.Equals(saved.area, SiegeArea.CaptureObjective.ToString(), StringComparison.Ordinal) ||
                string.Equals(saved.state, SiegeState.Completed.ToString(), StringComparison.Ordinal))
                HandleEnterFortress();
            if (string.Equals(saved.state, SiegeState.Completed.ToString(), StringComparison.Ordinal)) HandleCaptureFortress();
            _gateRepairRemainingSeconds = Math.Max(0d, saved.gateRepairRemainingSeconds);
        }

        private void RestoreHealth(EntityId targetId, double desiredHealth)
        {
            if (!Combat.TryGetState(targetId, out CombatantSnapshot target) || desiredHealth >= target.MaxHealth) return;
            double damage = target.MaxHealth - Math.Max(0.001d, desiredHealth);
            EntityId sourceId = new EntityId(800000UL + targetId.Value);
            EntityId sourceFaction = target.FactionId == PlayerFactionId ? EnemyFactionId : PlayerFactionId;
            Combat.Register(sourceId, new CombatantProfile("restore.damage", sourceFaction, 1d,
                new AttackProfile(damage, DamageType.True, 1d, 0d, 0d), tags: new[] { "restore" }), target.Position);
            Combat.IssueAttack(new AttackTargetCommand(new[] { sourceId }, targetId));
            Combat.Tick(0.001d);
            Combat.Unregister(sourceId);
        }

        private void EnsureResources(EntityId accountId)
        {
            Economy.AddResource(accountId, new DefinitionId("resource.material"), 1000d);
            Economy.AddResource(accountId, new DefinitionId("resource.supply"), 1000d);
        }

        private void SetResourceBalance(EntityId accountId, string resourceId, double desired)
        {
            Economy.TryGetState(accountId, out EconomyAccountSnapshot state);
            var id = new DefinitionId(resourceId);
            double current = state.Resources.TryGetValue(id, out double value) ? value : 0d;
            if (desired > current) Economy.AddResource(accountId, id, desired - current);
            else if (current > desired) Economy.TrySpend(accountId, new[] { new ResourceCost(id, current - desired) });
        }

        private static double GetResource(EconomyAccountSnapshot state, string resourceId)
        {
            return state.Resources != null && state.Resources.TryGetValue(new DefinitionId(resourceId), out double value) ? value : 0d;
        }

        private static double Round(double value) => Math.Round(value, 6, MidpointRounding.AwayFromZero);

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, out T parsed) && Enum.IsDefined(typeof(T), parsed) ? parsed : fallback;
        }

        private EntityId FindAliveTarget(EntityId factionId)
        {
            foreach (EntityId id in Registry.GetFactionEntities(factionId))
                if (Combat.TryGetState(id, out CombatantSnapshot state) && state.IsAlive) return id;
            return EntityId.Invalid;
        }

        private void Notify(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _notifications.Add(message.Trim());
            while (_notifications.Count > 12) _notifications.RemoveAt(0);
        }

        private static EntityId OpponentOf(EntityId factionId) => factionId == PlayerFactionId ? EnemyFactionId : PlayerFactionId;

        private static bool HasTag(IReadOnlyList<string> tags, string value)
        {
            foreach (string tag in tags) if (string.Equals(tag, value, StringComparison.Ordinal)) return true;
            return false;
        }

        private static bool HasTag(IReadOnlyList<ContentTag> tags, string value)
        {
            foreach (ContentTag tag in tags) if (string.Equals(tag.Value, value, StringComparison.Ordinal)) return true;
            return false;
        }

        private static T Find<T>(IReadOnlyList<T> values, string id) where T : IDefinition => Find(values, new DefinitionId(id));

        private static T Find<T>(IReadOnlyList<T> values, DefinitionId id) where T : IDefinition
        {
            foreach (T value in values) if (value.Id == id) return value;
            throw new InvalidOperationException($"Definition '{id}' was not found.");
        }

        private sealed class PrototypeSiegeNavigationSink : ISiegeNavigationSink
        {
            public int RefreshCount { get; private set; }
            public void RefreshAfterBreach(EntityId siegeId, EntityId structureId, SiegeArea openedArea) => RefreshCount++;
        }

        private sealed class FortifiedCitySiegeRule : ISiegeRule
        {
            public SiegeActionResult CanEnter(SiegeSnapshot siege, SiegeArea targetArea)
            {
                bool passage = siege.Structures.Any(value => value.Profile.Kind == DefenseStructureKind.Gate &&
                    (value.IsDestroyed || value.GateState == GateState.Open));
                if ((targetArea == SiegeArea.InnerArea || targetArea == SiegeArea.Breach) && !passage)
                    return SiegeActionResult.Failure("The fortress gate must be open or destroyed.");
                if (targetArea == SiegeArea.CaptureObjective && siege.CurrentArea != SiegeArea.InnerArea)
                    return SiegeActionResult.Failure("Attackers must enter the inner area first.");
                return SiegeActionResult.Success();
            }

            public SiegeActionResult CanCapture(SiegeSnapshot siege)
            {
                if (siege.CurrentArea != SiegeArea.CaptureObjective)
                    return SiegeActionResult.Failure("Attackers must control the stronghold courtyard.");
                return siege.CompletedConditions.HasFlag(CaptureCondition.CoreDestroyed)
                    ? SiegeActionResult.Success()
                    : SiegeActionResult.Failure("The stronghold must be subdued before ownership transfers.");
            }
        }

        private sealed class PrototypeArmyOrderExecutor : IArmyOrderExecutor
        {
            private readonly MovementSystem _movement;
            private readonly CombatSystem _combat;
            private readonly CombatMovementCoordinator _coordinator;

            public PrototypeArmyOrderExecutor(
                MovementSystem movement,
                CombatSystem combat,
                CombatMovementCoordinator coordinator)
            {
                _movement = movement;
                _combat = combat;
                _coordinator = coordinator;
            }

            public ArmyOrderExecutionResult Move(IReadOnlyList<EntityId> units, WorldPoint destination, AegisRTS.Gameplay.Formation.FormationType formation) =>
                MoveAndAnchor(units, destination, formation);

            public ArmyOrderExecutionResult Attack(IReadOnlyList<EntityId> units, EntityId targetId)
            {
                int accepted = _combat.IssueAttack(new AttackTargetCommand(units, targetId));
                if (accepted > 0) _coordinator.Tick();
                return accepted > 0 ? ArmyOrderExecutionResult.Success(accepted) : ArmyOrderExecutionResult.Failure("No army member could attack the target.");
            }

            public ArmyOrderExecutionResult AttackSettlement(IReadOnlyList<EntityId> units, EntityId settlementId) =>
                ArmyOrderExecutionResult.Failure("Settlement attacks must use the siege command flow.");

            public ArmyOrderExecutionResult Defend(IReadOnlyList<EntityId> units, WorldPoint position, AegisRTS.Gameplay.Formation.FormationType formation) =>
                MoveAndAnchor(units, position, formation);

            public ArmyOrderExecutionResult Retreat(IReadOnlyList<EntityId> units, WorldPoint destination, AegisRTS.Gameplay.Formation.FormationType formation) =>
                MoveAndAnchor(units, destination, formation);

            private ArmyOrderExecutionResult MoveAndAnchor(
                IReadOnlyList<EntityId> units,
                WorldPoint destination,
                AegisRTS.Gameplay.Formation.FormationType formation)
            {
                MovementCommandResult result = _movement.IssueMove(new MoveUnitsCommand(units, destination, formation: formation));
                if (result.WasAccepted) _combat.NotifyMoveOrder(units, destination);
                return FromMovement(result);
            }

            private static ArmyOrderExecutionResult FromMovement(MovementCommandResult result) => result.WasAccepted
                ? ArmyOrderExecutionResult.Success(result.AcceptedActorCount)
                : ArmyOrderExecutionResult.Failure("No army member accepted the movement order.");
        }
    }
}
