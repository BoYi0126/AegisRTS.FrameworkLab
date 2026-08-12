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
├─ ArtSource
│  ├─ Units
│  ├─ Buildings
│  ├─ UI
│  └─ VFX
├─ docs
├─ .gitignore
└─ README.md
```

`ArtSource/` 保存 AI／外包的概念圖、來源模型、預覽、UV、報告與授權紀錄，不由 Unity 自動匯入。只有通過格式、授權、尺寸與 Unity 驗收的 Runtime 檔案，才複製到 `Assets/AegisRTS/Content/Shared/Art/`。來源交付與正式遊戲資產不可混為同一狀態。

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
