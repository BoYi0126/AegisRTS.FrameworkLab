# Phase 07 — Faction / Settlement / Territory

## 目標

- Faction：resources、settlements、armies、technology、diplomacy、AI profile。
- Settlement：owner、population、garrison、resources、buildings、recruitment、defense、capture。
- Territory：node、connection、owner、visibility、value。
- Capture rule：clear defenders、capture zone、destroy core、kill commander、mixed。
- Acceptance：3 settlements 可改 owner，Faction territory 自動更新。

## 完成內容

- `FactionSystem` 管理 runtime resources、settlement／territory／army indices、technology、對稱 diplomacy 與 AI profile reference。
- `FactionArmyEventBridge` 監聽 Army create／split／merge events，自動維護 Faction army index。
- `TerritorySystem` 管理 bidirectional node graph、settlement mapping、owner、per-faction visibility 與 strategic value。
- `SettlementSystem` 管理 owner、population、garrison、resources、buildings、recruitment queue、defense 與 capture transaction。
- `CaptureRule` 使用 flags 組合 clear defenders／capture zone／destroy core／kill commander；Mixed 可要求多個條件。
- `CaptureSettlementCommand` 與 `SettlementCommandRouter` 讓 Player／AI／Scenario／Test 使用相同 validation／handler flow。
- Capture 成功後同步 Settlement owner、Faction settlement index、Territory owner 與 Faction territory index，並發布 immutable events。
- `SettlementArmyTargetValidator` 補強 Phase 06 `AttackSettlement`：目標必須存在、不可為己方，且 diplomacy 必須是 Hostile／War。
- `SettlementDefinition` 與三個 Content Packs 新增 initial population、max defense、capture rule／conditions；loader 與 validator 同步更新。
- `SettlementProfile.FromDefinition` 將 immutable authoring definition 轉成 runtime settlement profile。
- `Sandbox_Siege` 新增三座 settlement 與三個 territory nodes，自動使用三種 capture rule 轉移 owner，並顯示 graph／ownership HUD。

## 驗收結果

- Unity EditMode：72/72 passed；Phase 07 新增 12 cases。
- Unity PlayMode：8/8 passed；Phase 07 新增 2 cases。
- 三座 settlement 全部由 Faction A 轉移至 Faction B；兩個 Faction snapshot 的 settlement／territory indices 自動更新，PASS。
- Standard／Mixed capture rules、incomplete capture rejection、territory graph、visibility、diplomacy、Faction army bridge 與 AttackSettlement validation，PASS。
- 三個 Content Pack 的 settlement population／defense／capture authoring data 載入與 validation，PASS。

## 後續擴充界線

- Settlement resources、buildings 與 recruitment queue 已具備 runtime state；成本與完成時間由 Phase 08 Economy systems 實作。
- Visibility 目前為明確設定的 read model；Fog of War 探索／視野傳播會在 AI／UI phase 接入。
- Capture command 接受上游系統提供的 completed conditions；條件如何由 Combat／Siege objective 產生留給 Phase 09。
