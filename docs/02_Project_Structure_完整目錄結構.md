# 完整目錄結構

```text
C:\projects\Unity\AegisRTS.FrameworkLab
│
├─ Assets
│  └─ AegisRTS
│     ├─ Framework
│     │  ├─ Core
│     │  │  ├─ Commands
│     │  │  ├─ Events
│     │  │  ├─ Entities
│     │  │  ├─ Time
│     │  │  ├─ Random
│     │  │  ├─ StateMachine
│     │  │  └─ Diagnostics
│     │  ├─ Gameplay
│     │  │  ├─ Content
│     │  │  │  ├─ Definitions
│     │  │  │  ├─ Serialization
│     │  │  │  └─ Validation
│     │  │  ├─ Units
│     │  │  ├─ Movement
│     │  │  ├─ Formation
│     │  │  ├─ Combat
│     │  │  ├─ Abilities
│     │  │  ├─ Heroes
│     │  │  ├─ Armies
│     │  │  ├─ Factions
│     │  │  ├─ Settlements
│     │  │  ├─ Territory
│     │  │  ├─ Economy
│     │  │  ├─ Recruitment
│     │  │  ├─ Buildings
│     │  │  ├─ Technology
│     │  │  ├─ Siege
│     │  │  ├─ Objectives
│     │  │  └─ AI
│     │  ├─ Presentation
│     │  │  ├─ Camera
│     │  │  ├─ Input
│     │  │  ├─ Selection
│     │  │  ├─ UI
│     │  │  ├─ WorldUI
│     │  │  ├─ Audio
│     │  │  └─ VFX
│     │  ├─ Persistence
│     │  │  ├─ Save
│     │  │  └─ Replay
│     │  └─ Tools
│     │     ├─ Debug
│     │     ├─ Validation
│     │     └─ Editor
│     ├─ Content
│     │  ├─ Shared
│     │  ├─ DemoNeutral
│     │  ├─ DemoThreeKingdoms
│     │  └─ DemoFantasy
│     ├─ Demo
│     │  ├─ Scenes
│     │  ├─ Prefabs
│     │  ├─ Materials
│     │  ├─ Models
│     │  ├─ Sprites
│     │  ├─ Audio
│     │  └─ Config
│     └─ Tests
│        ├─ EditMode
│        └─ PlayMode
├─ Packages
├─ ProjectSettings
├─ docs
├─ .gitignore
└─ README.md
```

避免把所有東西扁平放在 `Assets/Scripts`、`Assets/Prefabs`、`Assets/Scenes`。

Phase 16 後 Framework 目標：

```text
Packages/com.boyi.aegis-rts/
├─ Runtime
├─ Editor
├─ Tests
├─ Samples~
├─ Documentation~
└─ package.json
```

Phase 16 已完成此結構；`Packages/com.boyi.aegis-rts/Runtime` 是 Framework 唯一原始碼位置，`Assets/AegisRTS` 只保留背景內容、Demo composition 與專案驗收測試，避免同一專案編譯兩份 assembly。
