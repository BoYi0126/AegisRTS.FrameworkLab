# Framework API 目標介面

目標使用體驗：CreateFaction、CreateSettlement、SpawnUnit、CreateArmy、IssueCommand、Recruit、Build、Research、StartSiege、CaptureSettlement、AddResource、StartScenario、Save、Load。

核心 API 必須同時可由 Player、AI、Scenario、Test 使用，不能只靠 Inspector 操作。

## Phase 01 Core API

`AegisRTS.Core` 是 Pure C# assembly，`noEngineReferences` 啟用且不引用 Gameplay 或 Presentation。所有時間步進與訊息派送均由呼叫端明確驅動，不依賴 MonoBehaviour lifecycle。

### Entity

```csharp
var ids = new EntityIdGenerator();
EntityId entityId = ids.Next();
```

- `EntityId`：可比較、可序列化為 `ulong` 的 runtime entity identity；`0` 為 `Invalid`。
- `EntityIdGenerator`：產生非零、單調遞增且可重設的 deterministic ID sequence。

### Time / Random

```csharp
var clock = new GameClock();
clock.SetSpeed(2d);
clock.Advance(unscaledDeltaSeconds);

IRandomSource random = new SeededRandom(seed);
int index = random.NextInt(maxExclusive);
```

- `GameClock`：追蹤 scaled／unscaled time，支援 `Pause`、`Resume`、正數 `Speed` 與 `Reset`。
- `SeededRandom`：固定 PCG 演算法；相同 seed 與呼叫順序必須得到相同結果，不依賴 `System.Random` runtime implementation。

### Command

```csharp
using var validator = commandBus.RegisterValidator<MyCommand>(Validate);
using var handler = commandBus.RegisterHandler<MyCommand>(Handle);
CommandDispatchResult result = commandBus.Dispatch(command);
```

- Command 實作 `ICommand`。
- 每個 command type 最多一個 handler，可有多個依註冊順序執行的 validator。
- 任一 validator 拒絕時不執行 handler，原因由 `CommandDispatchResult.Error` 回傳。
- Player、AI、Scenario 與 Test 應呼叫同一個 `Dispatch` flow。
- 註冊回傳 `IDisposable`，dispose 後取消該 handler 或 validator。

### Event

```csharp
using var subscription = eventBus.Subscribe<MyEvent>(OnEvent);
eventBus.Publish(eventData);
```

- Event 實作 `IEvent`，代表已經發生的事實。
- `EventBus` 依註冊順序同步發布給 exact event type 的 subscriber。
- 發布使用 subscriber snapshot，因此 callback 內 unsubscribe 不會破壞當次迭代。

### State Machine

```csharp
var machine = new StateMachine<MyContext>(context);
machine.Start(initialState);
machine.Tick(deltaSeconds);
machine.TransitionTo(nextState);
machine.Stop();
```

- State 實作 `IState<TContext>` 的 `Enter`、`Tick`、`Exit` lifecycle。
- Transition 保證 previous `Exit` 在 next `Enter` 之前發生。

### Diagnostics / Debug

- `IDiagnosticSink`：Core diagnostics adapter boundary。
- `DiagnosticBuffer`：thread-safe bounded history，滿載時移除最舊紀錄。
- `NullDiagnosticSink`：未配置 diagnostics 時的 no-op implementation。
- `GameClock`、`SeededRandom`、`CommandBus`、`EventBus`、`StateMachine` 提供 `GetDebugSummary()`；Bus 與 State Machine 可將生命週期事件送至 `IDiagnosticSink`。

Command Bus、Event Bus、State Machine 與 Entity ID Generator 預設由單一 simulation thread 擁有；`DiagnosticBuffer` 可安全接受多執行緒寫入。

## Phase 02 Data-Driven / Content Pack API

Gameplay definition 與 Content Pack API 同樣是 Pure C#，不持有 `GameObject` 或其他 Unity runtime object。

### Deserialize / Validate / Activate

```csharp
ContentPack pack = jsonLoader.Load(json);
ContentPackLoadResult result = contentPackService.Load(pack, assetCatalog);

if (result.Succeeded)
{
    UnitDefinition unit = result.Catalog.GetRequired<UnitDefinition>(unitId);
}
```

- `ContentPackJsonLoader.Load`：將 JSON authoring data 轉成 immutable definitions；格式錯誤拋出 `ContentPackFormatException`。
- `ContentPackValidator.Validate`：一次回傳所有 duplicate ID、missing reference、invalid stat／cost、technology cycle、missing prefab／tag 問題。
- `IContentAssetCatalog`：由 Unity 或 Test adapter 提供 prefab ID existence check，Gameplay 不直接依賴 AssetDatabase 或 GameObject。
- `ContentPackService.Load`：驗證成功才 atomically 切換 `ActiveCatalog`；失敗時保留前一個 catalog。
- `ContentCatalog.TryGet<TDefinition>`／`GetRequired<TDefinition>`：以 `DefinitionId` 進行 exact typed query。

### Definition 與規則

- `DefinitionId`：trim、lowercase 並限制穩定字元；reference 不使用 display name。
- `ContentTag`：content-neutral classification；使用前必須由 pack 宣告。
- `ResourceDefinition`、`UnitDefinition`、`HeroDefinition`、`AbilityDefinition`、`BuildingDefinition`、`TechnologyDefinition`、`SettlementDefinition`：載入後為 read-only。
- `ResourceCost`：以 Resource Definition ID 表達成本。
- `GameRuleSet`：提供 Morale、Supply、Hero Capture／Death、Population、Fog of War、Destructible Walls switches。

`ContentPackService`、`ContentCatalog` 與 `ContentValidationResult` 提供 `GetDebugSummary()` 或可直接取得 validation issues，供 Debug／Validation tools 顯示。

## Phase 03 RTS Input / Selection / Camera API

Phase 03 把 command intent 留在 Gameplay，把可測試的 selection／camera state 留在 Presentation model，再由 Unity adapter 處理 Input System、Physics raycast、screen projection 與 GameObject visual。

### Shared unit commands

```csharp
ICommand command = new MoveUnitsCommand(
    selectedIds,
    new WorldPoint(x, y, z),
    queue: shiftPressed);

commandBus.Dispatch((MoveUnitsCommand)command);
```

- `WorldPoint`：不依賴 `UnityEngine.Vector3` 的 immutable world position。
- `UnitCommand.ActorIds`：建構時複製、去重並驗證非空的 actor IDs；外部修改原集合不影響 command。
- `MoveUnitsCommand`、`AttackTargetCommand`、`FollowTargetCommand`、`InteractTargetCommand`、`StopUnitsCommand`、`HoldUnitsCommand`：Player／AI／Scenario／Test 共用的 intent types。
- `Queue` 只表達是否排入既有 command queue；實際移動、尋路與 formation execution 由後續 Phase 實作。

### Selection

```csharp
selection.Register(descriptor);
selection.SelectMany(idsInsideDragBox, SelectionModifier.Replace);
selection.AssignControlGroup(1);
selection.RecallControlGroup(1);
```

- `SelectableDescriptor`：只含 `EntityId`、definition ID、`SelectableKind` 與 `SelectionAffiliation`，不持有 GameObject。
- `SelectionService`：提供 register／unregister、single／multi selection、replace／add／toggle／remove、same-definition selection 與 `0–9` control groups。
- `ISelectionQuery`：讓 camera、UI 與其他 read-side adapter 查詢 selection，不暴露修改權限。
- `SelectionChangedEvent`：selection 實際變更時才透過可選的 `EventBus` 發布 immutable snapshot。
- `ContextCommandResolver`：Ground→Move、Enemy→Attack、Friendly→Follow、Settlement→Interact；neutral non-settlement 不產生未定義命令。

### Camera / Unity adapters

- `RtsCameraRigModel`：提供 `Pan`、`Focus`、`ZoomBy`，所有 pivot／zoom 都限制在建構時設定的 bounds。
- `RtsCameraController`：套用 WASD、edge pan、middle drag、wheel zoom 與 focus-selected 到 Unity Camera transform。
- `UnitySelectableView`：Entity descriptor 與 scene renderer 的 bridge；selection highlight 使用 `MaterialPropertyBlock`，不複製 shared material。
- `UnityRtsInputAdapter`：將 Input System actions、drag rectangle 與 raycast 轉成 Selection API 或共用 Gameplay commands。
- `RtsSandboxBootstrap`：僅作 composition root 與 debug acceptance visualization，不是全域 God Manager。

## Phase 04 Movement / Navigation / Formation API

Phase 04 維持 `Move Command → MovementSystem → INavigationAdapter → Unity View`。Gameplay 決定 order、formation 與狀態轉移；Unity adapter 只回答路徑與驅動 View。

### Formation

```csharp
IReadOnlyList<FormationSlot> slots = FormationPlanner.Plan(
    destination,
    actorCount,
    FormationType.Box,
    spacing: 1.8,
    forwardX,
    forwardZ);
```

- `FormationType.Line`：單列並以 destination heading 的 right vector 展開。
- `FormationType.Box`：使用接近方形的 rows／columns；不把 group actors 送往同一點。
- slot index 與 actor assignment 依排序後的 `EntityId` 穩定產生。

### Movement system

```csharp
movement.Register(entityId, initialPosition);
MovementCommandResult result = movement.IssueMove(command);
movement.Tick(deltaSeconds);

if (movement.TryGetState(entityId, out MovementStateSnapshot state))
{
    // state.Status / Destination / Velocity / RepathCount / StuckSeconds
}
```

- `MovementSystem.IssueMove`：replace 或 queue order，為每個 actor 配置 formation slot，再交由 navigation adapter 驗證。
- `IssueStop`／`IssueHold`：清除 queue 並停止 navigation；下一個 Move 可解除 hold。
- `Tick`：同步 position／velocity／remaining distance，判斷 arrival、partial／invalid path、低速 stuck，最多自動 repath 3 次。
- `MovementStatus`：`Idle`、`Moving`、`Arrived`、`Unreachable`、`Stuck`、`Holding`。
- `Snapshot` 與 `GetDebugSummary()`：提供 deterministic read/debug view，不暴露可修改的 runtime record。

### Navigation adapter

- `INavigationAdapter.SetDestination`：回傳 accepted、resolved destination、path corner count 或 rejection reason。
- `INavigationAdapter.TryGetSnapshot`：回傳 position、velocity、remaining distance、path state 與是否位於 navigation surface。
- `NavMeshMovementAdapter`：使用 `NavMesh.SamplePosition`、`NavMesh.CalculatePath` 與 `NavMeshAgent.SetPath`；只接受完整 path。
- `UnityMovementDriver`：從 Unity frame loop 呼叫 `MovementSystem.Tick`，不加入 gameplay decision。

## Phase 05 Unit Combat / Ability API

```csharp
var events = new EventBus();
var combat = new CombatSystem(events);

combat.Register(entityId, combatantProfile, initialPosition);
combat.RegisterAbility(abilityProfile);
combat.IssueAttack(new AttackTargetCommand(actorIds, targetId));
combat.IssueAbility(new UseAbilityCommand(casterId, abilityId, targetId, targetPoint));
combat.Tick(deltaSeconds);

if (combat.TryGetState(entityId, out CombatantSnapshot state))
{
    // state.Health / State / TargetId / AttackCooldownRemaining / MovementSpeedMultiplier
}
```

### Combat simulation

- `CombatSystem`：Pure C# authoritative combat state；處理 attack、projectile、damage、status、ability cooldown 與 death。
- `ICombatQuery`：提供 `TryGetState`、sorted `Snapshot` 與 `GetDebugSummary`，供 UI／AI／tests 使用。
- `AttackProfile`：定義 damage type、range、cooldown、windup、projectile speed、splash radius 與 target tags。
- `DefenseProfile`：定義 armor 與 physical／magical resistance；True damage 不套用 defense／resistance。
- `CombatantProfile`：把 definition identity、faction、army、HP、attack、defense、tags、abilities 組成 runtime spawn configuration。

### Ability and status

- `UseAbilityCommand`：Unity-independent ability intent；可帶 caster、ability、unit target、point 與 direction。
- `AbilityProfile`：定義 Self／Unit／Point／Area／Direction／Settlement 與 Active／Passive／Aura／Triggered／Toggle 分類。
- `StatusEffectProfile`：支援 Buff／Debuff／Stun／Slow／Root／Shield／DamageOverTime。
- 手動 command 只接受 Active／Toggle；Passive／Aura／Triggered 的觸發策略由擁有該規則的後續系統呼叫 combat API。

### Events and Unity presentation

- `DamageAppliedEvent`、`ProjectileLaunchedEvent`、`StatusAppliedEvent`、`UnitDiedEvent`、`AbilityUsedEvent` 是 immutable simulation events。
- `UnityCombatDriver` 負責 frame tick、transform position bridge 與 event-driven projectile visual。
- `UnityCombatView` 只渲染 snapshot（血條、受傷顏色、死亡外觀），不持有 authoritative HP 或傷害規則。

## Phase 06 Hero / Army / Command API

```csharp
var heroes = new HeroSystem();
heroes.Register(heroUnitId, HeroProfile.FromDefinition(heroDefinition, factionId));

var armies = new ArmySystem(
    heroes,
    ArmyRuleOptions.From(gameRuleSet),
    new GameplayArmyOrderExecutor(movement, combat),
    new CombatArmyMembershipSink(combat),
    events);

armies.RegisterMember(heroUnitId, factionId);
armies.RegisterMember(infantryId, factionId);

using var router = new ArmyCommandRouter(commandBus, armies);
commandBus.Dispatch(new CreateArmyCommand(armyId, factionId, memberIds, heroUnitId));
commandBus.Dispatch(new SplitArmyCommand(armyId, newArmyId, splitMemberIds));
commandBus.Dispatch(new MergeArmiesCommand(armyId, newArmyId));
commandBus.Dispatch(new AssignArmyCommanderCommand(armyId, replacementHeroId));
```

### Hero component

- `HeroSystem`／`IHeroQuery`：管理 unit entity 上的 Hero／Leadership／Ability component 與 ArmyId，不建立 HeroCombatSystem。
- `HeroProfile.FromDefinition`：從 `HeroDefinition` 建立 runtime component；Faction 是 runtime ownership，不寫入 content definition。
- Commander 必須是 army member、已註冊 hero，且與 army 同 faction。

### Army composition and rules

- `ArmySystem`／`IArmyQuery`：建立、拆分、合併 army，查詢 immutable snapshot 與 unit membership。
- `ArmyRuleOptions.From(GameRuleSet)`：啟用或停用 Morale／Supply；disabled rule 不接受數值調整。
- `AdjustMorale`／`AdjustSupply`：只在對應 rule enabled 時更新，結果限制於 0–100。
- `IArmyMembershipSink`：Army composition 更新的 adapter boundary；`CombatArmyMembershipSink` 讓 Combat snapshot 反映最新 ArmyId。

### Commands and orders

- Commands：`CreateArmyCommand`、`MergeArmiesCommand`、`SplitArmyCommand`、`AssignArmyCommanderCommand`、`MoveArmyCommand`、`AttackArmyCommand`、`AttackSettlementArmyCommand`、`DefendArmyCommand`、`RetreatArmyCommand`。
- `ArmyCommandRouter`：為每種 command 同時註冊 validator 與 handler，Player／AI／Scenario／Test 共用相同 flow。
- `IArmyOrderExecutor`：隔離 Army order state 與執行 backend；`GameplayArmyOrderExecutor` 接到既有 Movement／Combat。
- Events：`ArmyCreatedEvent`、`ArmiesMergedEvent`、`ArmySplitEvent`、`ArmyCommanderAssignedEvent`、`ArmyOrderIssuedEvent`。

## Phase 07 Faction / Settlement / Territory API

```csharp
var factions = new FactionSystem(events);
factions.Register(factionA, new FactionProfile("faction.a", "ai.defend"));
factions.Register(factionB, new FactionProfile("faction.b", "ai.attack"));
factions.SetDiplomacy(factionA, factionB, DiplomacyStatus.War);

var territories = new TerritorySystem(factions, events);
territories.RegisterNode(territoryId,
    new TerritoryNodeProfile("territory.border", value: 25, settlementId), factionA);

var settlements = new SettlementSystem(factions, territories, events);
settlements.Register(settlementId, SettlementProfile.FromDefinition(definition), factionA);

using var router = new SettlementCommandRouter(commandBus, settlements);
commandBus.Dispatch(new CaptureSettlementCommand(
    settlementId,
    factionB,
    CaptureCondition.DefendersCleared,
    capturingArmyId));
```

### Faction

- `FactionSystem`／`IFactionQuery`：resources、technology、settlement／territory／army ownership indices、diplomacy 與 AI profile。
- `SetDiplomacy` 對兩個 factions 對稱更新並發布 `DiplomacyChangedEvent`。
- `FactionArmyEventBridge`：將 Army lifecycle events 投影到 Faction army index。

### Settlement and capture

- `SettlementSystem`／`ISettlementQuery`：owner、population、garrison、resources、buildings、recruitment、defense 與 capture state。
- `CaptureRule`：支援 `ClearDefenders`、`CaptureZone`、`DestroyCore`、`KillCommander`、`Mixed`。
- `CaptureSettlementCommand`：提供 completed condition flags 與 optional capturing army；validator 確認 rule、owner、faction 與 army ownership。
- `SettlementCommandRouter`：在 mutation 前由 CommandBus validator 拒絕 incomplete capture。
- Events：`SettlementOwnerChangedEvent`、`TerritoryOwnerChangedEvent`。

### Territory and army validation

- `TerritorySystem`／`ITerritoryQuery`：node／connection graph、owner、settlement mapping、visibility 與 value。
- `Connect` 建立雙向 connection；snapshot connection IDs 依 EntityId 排序。
- `SettlementArmyTargetValidator`：注入 `ArmySystem` 後，AttackSettlement 只接受存在、非己方、外交關係為 Hostile／War 的目標。

## Phase 08 Economy / Recruitment / Building / Technology API

```csharp
var economy = new EconomySystem(gameRules.PopulationEnabled, events, stateBridge);
economy.RegisterAccount(settlementId, startingResources, populationUsed, populationCapacity);

var technologies = new TechnologySystem(pack.Technologies, economy, modifiers, stateBridge, events);
var buildings = new BuildingSystem(pack.Buildings, economy, technologies, stateBridge, events);
var recruitment = new RecruitmentSystem(pack.Units, economy, buildings, technologies, spawnSink, events);

commandBus.Dispatch(new ConstructBuildingCommand(settlementId, factionId, buildingId));
commandBus.Dispatch(new ResearchTechnologyCommand(settlementId, factionId, technologyId));
commandBus.Dispatch(new RecruitUnitCommand(settlementId, factionId, unitId));

economy.Tick(deltaSeconds);
buildings.Tick(deltaSeconds);
technologies.Tick(deltaSeconds);
recruitment.Tick(deltaSeconds);
```

- `ResourceWallet`：以 `DefinitionId` 查詢、存入、檢查與原子扣除任意資源，不包含 Gold／Food 等固定欄位。
- `EconomySystem`：管理 economy accounts、resource production 與 optional population reservation，提供 immutable snapshot。
- `BuildingSystem`：驗證資源與 building／technology prerequisites，完成後套用 production／population effects。
- `TechnologySystem`：驗證 DAG prerequisites，完成後記錄 Faction research state 並套用 `TechnologyModifierRegistry`。
- `RecruitmentSystem`：依序執行 request→validate→cost／population reserve→queue→timer→`IUnitSpawnSink`。
- `BuildingCommandRouter`、`TechnologyCommandRouter`、`RecruitmentCommandRouter`：將上述三種 request 接到共用 CommandBus validation／handler flow。
- Events：`ResourceProducedEvent`、`BuildingCompletedEvent`、`TechnologyCompletedEvent`、`UnitRecruitedEvent`。
- `GameplayEconomyStateBridge`：把 resource delta、building completion、technology completion 同步到 Phase 07 read model。

## Phase 09 Siege / 城池攻防 API

```csharp
var sieges = new SiegeSystem(
    new CombatSiegeAttackerQuery(combat),
    navigationSink,
    new SettlementSiegeCaptureSink(settlements),
    customSiegeRule,
    events);

sieges.Register(siegeId, new SiegeProfile(
    settlementId, attackerFactionId, defenderFactionId, SiegeMode.Assault));
sieges.RegisterStructure(siegeId, gateId,
    DefenseStructureProfile.FromDefinition(gateDefinition, defenderFactionId));

using var router = new SiegeCommandRouter(commandBus, sieges);
commandBus.Dispatch(new StartSiegeCommand(siegeId));
commandBus.Dispatch(new AttackDefenseStructureCommand(siegeId, attackerUnitId, gateId));
commandBus.Dispatch(new EnterSiegeAreaCommand(siegeId, SiegeArea.InnerArea));
commandBus.Dispatch(new EnterSiegeAreaCommand(siegeId, SiegeArea.CaptureObjective));
commandBus.Dispatch(new CaptureSiegeCommand(siegeId));
```

- `SiegeSystem`／`ISiegeQuery`：管理 siege lifecycle、areas、structures、capture conditions、waves、timer 與 winner snapshot。
- `CombatSiegeAttackerQuery`：將一般 Unit 的 immutable `AttackProfile`／tags 接到攻城流程；不存在特殊 SiegeUnit runtime class。
- `ISiegeNavigationSink`：Gate 開啟或 Wall／Gate 摧毀時通知 navigation backend 刷新通道與新路徑。
- `ISiegeCaptureSink`／`SettlementSiegeCaptureSink`：將 capture objective 結果送入既有 Settlement owner transaction。
- `ISiegeRule`：可替換 area entry 與 capture eligibility，不讓世界觀或 scenario 規則硬寫在 system。
- Commands：Start、AttackDefenseStructure、SetGateState、EnterSiegeArea、ReportSiegeCondition、CompleteSiegeWave、CaptureSiege。
- Events：SiegeStarted、DefenseStructureDamaged／Destroyed、GateStateChanged、BreachCreated、SiegeAreaEntered、SiegeCompleted。
- `SiegeCombatEventBridge`：將 `UnitDiedEvent` 投影成 DefendersCleared／CommanderKilled capture conditions。
