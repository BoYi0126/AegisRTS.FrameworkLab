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
