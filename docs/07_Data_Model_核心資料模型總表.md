# 核心資料模型

主要 Entity：

```text
Faction
Settlement
Territory
Unit
Hero
Army
Building
Resource
Technology
Ability
DefenseStructure
Objective
Scenario
```

Definition：靜態資料。

Runtime State：遊戲進行中可變資料。

Save DTO：持久化資料。

禁止直接把 GameObject、Transform、NavMeshAgent、Animator、MonoBehaviour 當成 Save Model。

## Phase 02 Definition Model

所有靜態內容使用穩定 `DefinitionId`；ID 只允許小寫英數、`.`、`-`、`_`，不可使用 display name 當 reference。

```text
ContentPack
├─ DeclaredTags
├─ ResourceDefinition
├─ UnitDefinition ── ResourceCost / AbilityDefinition ID / Prefab ID
├─ HeroDefinition ── ResourceCost / AbilityDefinition ID / Prefab ID
├─ AbilityDefinition
├─ BuildingDefinition ── ResourceCost / Prefab ID
├─ TechnologyDefinition ── ResourceCost / Prerequisite Technology ID
├─ SettlementDefinition ── ResourceDefinition ID / Prefab ID
└─ GameRuleSet
```

Definition 是載入後的 immutable authoring data；`ContentCatalog` 提供 typed query。Prefab 只以穩定 string asset ID 存入 Gameplay definition，由 Unity adapter 建立 `IContentAssetCatalog` 驗證，不讓 domain model 持有 `GameObject`。

## Phase 05 Combat Runtime Model

```text
CombatantProfile (immutable configuration)
├─ DefinitionId / FactionId / ArmyId
├─ MaxHealth / Tags / AbilityIds
├─ AttackProfile
│  └─ DamageType / Range / Cooldown / Windup / ProjectileSpeed / SplashRadius / TargetTags
└─ DefenseProfile
   └─ Armor / PhysicalResistance / MagicalResistance

CombatantSnapshot (read-only runtime state)
└─ HP / State / Target / Position / Cooldown / MovementMultiplier / StatusCount

AbilityProfile
└─ TargetType / ActivationType / Cooldown / Range / Radius / Damage / StatusEffect
```

- Runtime mutable state 僅由 `CombatSystem` 擁有，Presentation 透過 `ICombatQuery` 讀取 snapshot。
- `EntityId` 表示 runtime identity；definition ID 表示 content identity，兩者不可混用。
- `StatusEffectProfile` 是 immutable 規則；duration、DoT tick 與 shield remaining amount 是每個 combatant 的 runtime instance state。
- Faction 決定敵我傷害篩選，ArmyId 保留跨 Phase 的軍團歸屬；Combat 不直接依賴未完成的 Faction／Army service。

## Phase 06 Hero / Army Runtime Model

```text
Unit Entity
├─ Movement state (existing)
├─ Combat state (existing)
└─ Hero component (optional)
   └─ DefinitionId / FactionId / Leadership / AbilityIds / ArmyId

Army
├─ ArmyId / FactionId / CommanderId
├─ UnitIds / Formation
├─ Morale / Supply (optional rules)
└─ Order
   └─ Idle / Move / Attack / AttackSettlement / Defend / Retreat
```

- Hero 是既有 unit entity 的額外 component，不繼承或複製 Combat state。
- `HeroDefinition.Leadership` 是 world-neutral authoring stat；runtime 使用 `HeroProfile`。
- `ArmySystem` 是 unit-to-army membership 的 authoritative owner，並透過 `IArmyMembershipSink` 同步需要顯示 ArmyId 的其他 runtime read model。
- `ArmySnapshot.UnitIds` 為複製後的唯讀排序集合，外部無法修改軍團 composition。

## Phase 07 Faction / Settlement / Territory Runtime Model

```text
Faction
├─ Resources / Technologies / Diplomacy / AI Profile
└─ SettlementIds / TerritoryIds / ArmyIds

Settlement
├─ Owner / Population / Garrison / Defense
├─ Resources / Buildings / RecruitmentQueue
└─ CaptureRule
   └─ DefendersCleared / ZoneControlled / CoreDestroyed / CommanderKilled

TerritoryNode
├─ Owner / Value / SettlementId
├─ Bidirectional Connections
└─ Visibility by Faction
```

- Settlement 是 ownership change 的 transaction entry；成功 capture 同步更新 Faction settlement index 與 mapped Territory owner。
- TerritorySystem 更新 Faction territory index，因此 Faction snapshot 不需要掃描全部 territory 才能顯示領土。
- `SettlementDefinition` authoring data 包含 `initialPopulation`、`maxDefense`、`captureRule`、`captureConditions`；runtime mutable state 不回寫 definition。
- Resources、building、technology 與 AI profile 只保存 stable content ID，不依賴 display name 或世界觀類別。

## Phase 08 Economy / Production Runtime Model

```text
EconomyAccount
├─ ResourceWallet<DefinitionId, Balance>
├─ ResourceProduction<DefinitionId, AmountPerSecond>
└─ PopulationUsed / PopulationCapacity (optional rule)

Building Job ── SettlementId / Definition / RemainingSeconds
Technology Job ── FactionId / Definition / RemainingSeconds
Recruitment Job ── SettlementId / FactionId / Definition / RemainingSeconds
```

- `ResourceWallet` 以 stable Resource Definition ID 儲存餘額，跨資源成本先完整驗證再原子扣除。
- Definition 是成本、時間、前置條件、產出與 modifier 的 immutable authoring source；queue 與 timer 只存在 runtime system。
- `BuildingSystem` 與 `TechnologySystem` 分別提供已完成狀態 query，作為建築、科技與單位的資料驅動 unlock 條件。
- `TechnologyModifierRegistry` 依 Faction 與 world-neutral stat ID 聚合 additive／multiplicative modifier。
- `GameplayEconomyStateBridge` 將 authoritative economy 變動投影到既有 Faction／Settlement snapshot；Unity spawn 經 `IUnitSpawnSink` 隔離。

## Phase 09 Siege Runtime Model

```text
Siege
├─ SiegeProfile ── Settlement / Attacker / Defender / Mode / TimeLimit
├─ State / CurrentArea / CompletedCaptureConditions / Winner
├─ DefenderIds / CommanderId / CompletedWaves
└─ DefenseStructures
   └─ Kind / Area / Faction / HP / Armor / GateState / Tags
```

- Siege areas：OuterArea、Walls、Gates、Towers、Breach、InnerArea、CaptureObjective。
- Defense structure kind：Wall、Gate、Tower、Barricade、Trap、Core；未內建的 content type 以 Extension＋stable type ID 保存。
- `DefenseStructureDefinition` 是 immutable authoring data；structure HP、Gate state 與 destroyed state 僅存在 runtime record。
- Siege unit 不建立額外 entity type；既有 Unit 的 tags、Faction 與 `AttackProfile` 經 query adapter 提供攻城能力。
- Gate／Wall destruction 產生 breach；Core／defender／commander／objective 狀態轉成共用 `CaptureCondition` flags。
- Siege capture 不直接寫 owner，而是呼叫 Settlement capture transaction，維持 Faction／Territory indices 一致。

## Phase 10 AI Runtime Model

```text
AiAgent
├─ AiProfile ── aggression / defense / economy / risk / siege / cadence
├─ AiWorldSnapshot (blackboard)
│  └─ economy / units / armies / settlements / strength / threat / target / route / siege progress
├─ Utility Scores
│  └─ Action / StrategicGoal / DecisionLayer / Score
└─ Decision State
   └─ selected goal / layer / action / decision count / stalled count / last error
```

- Strategic／Operational／Tactical／Unit 是責任層級，不是互相持有的四套 AI manager。
- `AiWorldSnapshot` 是一次決策使用的 immutable 黑板；AI 不保存或修改 Unity object。
- `AiProfileDefinition` 是 Content Pack authoring data，runtime 轉成驗證後的 `AiProfile`。
- `AiActionResult.MadeProgress` 驅動 deadlock detection；超過 threshold 後 Recover 取得最高 utility。
- `AiStrategicMapAnalyzer` 從 Settlement／Territory query 選擇高價值敵方目標並輸出 deterministic route。

## Phase 11 GameMode / Scenario / Objective Runtime Model

```text
ScenarioDefinition
├─ GameModeDefinition
│  └─ Type / Rules / AllowedSystems / VictoryPolicy / DefeatPolicy
├─ StartSetup Actions
├─ ObjectiveDefinitions
│  └─ Type / Fact / Target / HoldDuration / FailureFact / Optional
└─ TriggerDefinitions
   └─ Condition → Actions

Scenario Runtime
├─ Status / ElapsedSeconds / Facts
├─ Objective Status / Value / HeldSeconds
└─ Fired Trigger IDs
```

- Definition 是 immutable JSON authoring data；facts、objective progress、trigger history 與勝敗只存在 runtime。
- `ScenarioSystem` 不擁有 Combat／Economy／Siege state；composition layer 將既有 events／queries 投影成 stable fact IDs。
- continuous Hold 在 fact 不再符合 target 時將 `HeldSeconds` 歸零；Protect 等目標可用 failure fact 轉成 Failed／Defeat。
- `EmitSignal` action 只發布具名 event，讓 composition adapter 執行 start setup 或劇本演出，不讓核心引用 Unity object。
- Snapshot 複製 objectives 與 facts 為唯讀集合，可供 Phase 12 UI 與 Phase 13 Save／Replay／Debug 使用。

## Phase 12 UI / UX Presentation Model

```text
Gameplay Queries → IHudQuery → HudSnapshot
                                 ├─ 10 HudPanelViewModels
Gameplay Events  → ViewModel     └─ HudEntries
UI Intent        → IHudCommandSink → shared Gameplay Command flow

HudThemeDefinition
└─ colors / scale / opacity (presentation only)
```

- `HudSnapshot`／`HudPanelViewModel`／`HudEntry` 是 immutable presentation data，不是第二份 Gameplay state。
- `RtsHudViewModel` 以 invalidation event 控制 query refresh，並擁有短生命週期 notification queue。
- `HudThemeDefinition` 只描述視覺 tokens；切換 theme 不重建或修改 Gameplay systems。
- 十個 `HudPanelId` 固定 layout responsibility；隱藏／內容差異由 query snapshot 決定。

## Phase 13 Save / Replay Model

```text
SaveEnvelope
├─ Metadata ── SaveVersion / FrameworkVersion / ContentVersion / ScenarioId / Timestamp
├─ GameStateDocument
│  ├─ Faction / Settlement / Unit / Hero / Army
│  ├─ ResourceAccount / Building / Technology / Objective
│  └─ Clock / Random / Extensions
└─ SHA-256 Checksum

ReplayDocument
├─ Initial SaveEnvelope
├─ Seed
└─ Commands ── Tick / Sequence / CommandId / PayloadJson
```

- Save DTO 只保存 persistent values 與 stable IDs；runtime service／Unity object 不可序列化。
- capture source 與 restore sink 由 composition root 實作，因此每個 authoritative system 仍擁有自己的 runtime state。
- Clock 保存 scaled／unscaled time、delta、tick、pause、speed；Random 保存 seed、draw count、internal PCG state。
- Checksum 同時覆蓋 metadata 與 state；任何內容 mutation 在 restore 前被拒絕。
- Replay 同 tick 使用 sequence 維持 command dispatch order，只允許向前推進。
