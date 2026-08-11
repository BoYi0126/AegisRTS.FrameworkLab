# Naming / Namespace / asmdef

## Root Namespace

```csharp
AegisRTS
```

## Namespace

```text
AegisRTS.Core.*
AegisRTS.Gameplay.*
AegisRTS.Presentation.*
AegisRTS.Persistence.*
AegisRTS.Tools.*
AegisRTS.Demo.*
AegisRTS.Tests.*
```

## 類型命名

- `UnitDefinition`：靜態設定
- `UnitState`：runtime state
- `UnitView`：Unity 表現
- `CombatSystem`：系統
- `MoveUnitsCommand`：命令
- `UnitKilledEvent`：事件
- `IUnitQuery`：查詢介面

## asmdef 第一版

```text
AegisRTS.Core
AegisRTS.Gameplay
AegisRTS.Presentation
AegisRTS.Persistence
AegisRTS.Tools
AegisRTS.Demo
AegisRTS.Tests.EditMode
AegisRTS.Tests.PlayMode
```

依賴方向：

```text
Core
↑
Gameplay
↑       ↑
Presentation  Persistence
↑       ↑
Demo / Tools
```

禁止：

```text
Core → Gameplay
Core → Presentation
Gameplay → Demo
Gameplay → 特定世界觀 Content code
```

## Scene

```text
Bootstrap
Sandbox_RTS
Sandbox_Combat
Sandbox_Siege
Sandbox_AI
VerticalSlice_01
```

## Prefab

```text
PF_Unit_DebugInfantry
PF_Structure_DebugGate
PF_UI_SelectionPanel
```

## Definition Asset

```text
DEF_Unit_DebugInfantry
DEF_Hero_DebugCommander
DEF_Ability_Rally
```
