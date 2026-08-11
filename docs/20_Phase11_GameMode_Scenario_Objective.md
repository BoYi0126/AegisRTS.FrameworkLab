# Phase 11 — GameMode / Scenario / Objective

GameMode：rules、start setup、objectives、victory/defeat、allowed systems。

預設：Conquest、Siege、Defense、Survival、Wave、Escort、Territory Control、Hero Scenario。

Objective：Capture/Hold/Destroy/Protect/Reach/Survive/Gather/Recruit/Defeat/Escort。

Trigger + Action 資料驅動。

Acceptance：不改 C# 即可用資料做至少 4 種不同關卡。

## 完成狀態（2026-08-11）

- Status：Completed。
- 新增 Pure C# `ScenarioSystem`，管理單一 active scenario、facts、elapsed time、objectives、triggers、actions 與 Victory／Defeat。
- 新增 `GameModeDefinition`：包含 rules、allowed systems、勝敗 policy；完整宣告 Conquest、Siege、Defense、Survival、Wave、Escort、Territory Control、Hero Scenario。
- 新增 `ObjectiveDefinition`：完整宣告 Capture、Hold、Destroy、Protect、Reach、Survive、Gather、Recruit、Defeat、Escort。
- Objective 支援 active／locked／completed／failed、optional、target value、continuous hold duration 與 failure fact／threshold。
- Trigger 支援 OnStart、Elapsed、FactAtLeast／FactAtMost、ObjectiveCompleted／ObjectiveFailed。
- Action 支援 Activate／Complete／Fail Objective、Add／Set Fact、EmitSignal、Victory、Defeat；`EmitSignal` 讓 composition layer 以既有 Gameplay commands 執行 spawn 或劇本 setup。
- 新增 `ScenarioJsonLoader`、`ScenarioCommandRouter`、immutable snapshots、debug summary 與 lifecycle events。
- 新增四份不含關卡專屬 C# 的 JSON：Conquest、Siege、Defense、Survival。
- `Sandbox_AI` 加入 `ScenarioSandboxBootstrap`，同一 generic driver 從四個 `TextAsset` 載入並完成四種模式。

## 驗收

- Conquest：Capture settlement fact 後 Victory，PASS。
- Siege：Destroy gate → Trigger → Activate capture objective → Capture → Victory，PASS。
- Defense：持續 Hold 五秒成功；中途失去控制會重置；command post destroyed 會 Defeat，PASS。
- Survival：elapsed time 與五秒 reinforcement trigger，PASS。
- Unity EditMode：117/117 passed、0 failed；Phase 11 新增 11 cases。
- Unity PlayMode：14/14 passed、0 failed；Phase 11 新增 1 case。
- Gameplay assembly 維持 Pure C# 與 `noEngineReferences=true`；四份 JSON 語法檢查通過。

## 架構邊界與限制

- Scenario facts 是跨系統投影邊界；Combat、Economy、Siege、Territory 等 authoritative systems 不被 Objective runtime 取代。
- Allowed systems 由 composition root 用來決定註冊哪些 systems／routers；Scenario core 只提供 gate query，不直接停用外部服務。
- 核心一次管理一個 active scenario；campaign graph、多個平行 scenario instance 與 checkpoint 留給 game production／save phase。
- 目前以 JSON／TextAsset authoring；尚未提供 Unity custom editor、graph view 或 schema autocomplete。
