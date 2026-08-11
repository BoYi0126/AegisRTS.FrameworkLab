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
