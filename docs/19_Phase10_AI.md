# Phase 10 — AI

分 Strategic / Operational / Tactical / Unit。

Strategic 決 economy/expand/attack/defend/recover；Operational 組軍/增援/移動/攻城；Tactical 處理 target、position、protect siege、breach、retreat。

Utility AI 使用 score。

AI Profile：aggression、defense/economy bias、risk、siege preference。

Debug 必須顯示 goal、scores、target、strength、threat、route。

Acceptance：AI 自己經濟→招兵→組軍→攻城→佔領，長時間不 deadlock。

## 完成內容

- 新增 Pure C# `AiSystem`，以 per-agent decision interval 執行 deterministic Utility AI，不依賴 MonoBehaviour lifecycle。
- 新增 Strategic／Operational／Tactical／Unit 四層 action taxonomy；所有層共用 score list 與穩定 tie-break。
- Strategic goals 支援 Economy、Expand、Attack、Defend、Recover。
- Operational actions 支援 Recruit、AssembleArmy、Reinforce、MoveToTarget、StartSiege。
- Tactical actions 支援 SelectTarget、ProtectSiege、Breach、AdvanceToObjective、Retreat、Capture。
- Unit actions 支援 HoldPosition／Wait，作為局部行為與非阻塞等待狀態。
- 新增 `AiProfileDefinition` 與 runtime `AiProfile`，包含 aggression、defense bias、economy bias、risk tolerance、siege preference、decision interval、desired army size。
- 三個 Content Packs 各新增不同 AI profile；Three Kingdoms 與 Fantasy 只替換 personality data，不複製 AI code。
- 新增 `IAiWorldQuery`／`IAiActionExecutor` 邊界，AI 只讀黑板 snapshot 並透過 executor 使用既有 Commands。
- 新增 `AiStrategicMapAnalyzer`，依 territory value 選擇敵方 settlement，並以 deterministic BFS 產生 route。
- Debug snapshot 顯示 goal、layer、action、完整 scores、target、strength、threat、route、decision／stall counts 與 last error。
- 連續無進展達 profile threshold 時，Recover action 取得最高分；有進展後立即清除 stall count。
- `Sandbox_AI` 實際串接 Economy、Recruitment、Army、Combat、Siege、Faction、Settlement、Territory 與 CommandBus，自動完成經濟→招兵→組軍→移動→攻城→破口→佔領。

## 驗收結果

- Unity EditMode：106/106 passed；Phase 10 新增 11 cases。
- Unity PlayMode：13/13 passed；Phase 10 新增 2 Sandbox_AI cases。
- 1000 次 Pure C# decision loop 完成完整攻城循環，佔領後維持 HoldPosition，stall count 為 0。
- Sandbox_AI 3 秒內完成 economy→recruit→army→siege→capture；5 秒長跑 owner 不回退且無 deadlock。
- Profile validation、四層 scores、interval throttling、target／route、recovery、debug snapshot：PASS。

## 後續擴充界線

- Strength／threat 目前由 composition adapter 聚合；後續可加入 unit composition、terrain、morale、supply 與 technology modifier 權重。
- Utility scores 是可預測的基線公式；平衡工具、曲線資產與難度 scaling 留待後續 Game Production phase。
- Tactical micro 目前輸出 intent；cover、focus fire、formation flank 與局部 path cost 由 Movement／Combat adapter 實作。
- AI 不直接呼叫 Unity API；多人 authoritative server 可替換 world query／executor 而重用同一 planner。
