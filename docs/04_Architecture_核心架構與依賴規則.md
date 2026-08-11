# 核心架構與依賴規則

## 資料流

```text
Player Input / AI
        ↓
     Command
        ↓
 Command Validator
        ↓
 Gameplay System
        ↓
 Runtime State
        ↓
      Event
        ↓
Presentation / UI / Audio / VFX
```

## 三層分離

```text
Definition → Runtime State → View
```

`UnitDefinition` 不存當前 HP；`UnitState` 不直接等於 GameObject；`UnitView` 不作為唯一遊戲真實狀態。

## Pure C# 優先

傷害、資源、科技、任務、Army composition、Save DTO 等可不依賴 MonoBehaviour 的邏輯，優先使用一般 C#。

## Unity Adapter

例如：

```text
NavMeshMovementAdapter
UnityInputAdapter
UnityAudioAdapter
UnityUnitView
```

## Command / Event / Query

- Command：玩家/AI 想做什麼。
- Event：事情已發生。
- Query：UI/外部模組讀取狀態。

## GameRuleSet

不同遊戲以規則切換：

```text
MoraleEnabled
SupplyEnabled
HeroCaptureEnabled
HeroPermanentDeath
PopulationEnabled
FogOfWarEnabled
DestructibleWalls
```

## Extension

```text
ICaptureRule
IDamageRule
IRecruitRule
ISiegeRule
IVictoryCondition
IAbilityEffect
IAIConsideration
```

## 禁止 God Manager

Bootstrap 可組裝服務，但不可讓單一 GameManager 同時負責 Combat、AI、Save、UI、Economy、Scene、Audio。
