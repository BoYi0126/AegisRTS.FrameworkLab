# Phase 12 — UI / UX

ResourceBar、SelectionPanel、CommandPanel、AbilityBar、ArmyPanel、SettlementPanel、Minimap、Notification、Objective、Pause。

UI 只 Query/ViewModel + Event + Command，不直接修改 gameplay state。

同 Layout 可換 ThreeKingdoms/Medieval/Fantasy Theme。

Acceptance：Theme 替換不修改 Gameplay。

## 完成狀態（2026-08-11）

- Status：Completed。
- 新增 `RtsHudViewModel`、`IHudQuery`、`IHudCommandSink` 與 immutable `HudSnapshot`，UI 只透過 Query／Event／Command boundary 工作。
- 新增固定十區 layout：ResourceBar、SelectionPanel、CommandPanel、AbilityBar、ArmyPanel、SettlementPanel、Minimap、Notification、Objective、Pause。
- 新增 event-driven notification queue、HUD invalidation、command delegation 與 pause command entry。
- 新增 `RtsHudPresenter` Unity adapter；同一 layout 使用資料 theme，不包含世界觀 Gameplay 分支。
- 新增 `HudThemeJsonLoader` 與 Neutral、Three Kingdoms、Fantasy 三份 JSON theme。
- `Sandbox_AI` 加入 `HudSandboxBootstrap`，執行三種 theme swap acceptance。

## 驗收

- 三份 Theme JSON 使用相同十區 layout signature：PASS。
- Theme 替換前後 gameplay revision 不變、command count 保持 0：PASS。
- ViewModel query cache／event invalidation、notification capacity／dismiss、command delegation：PASS。
- Unity EditMode：125/125 passed、0 failed；Phase 12 新增 8 cases。
- Unity PlayMode：15/15 passed、0 failed；Phase 12 新增 1 case。

## 架構邊界與限制

- `RtsHudPresenter` 是可替換 Unity adapter；authoritative state 始終位於 Gameplay systems。
- Theme 僅定義 color、scale、opacity；不保存 runtime resource、selection、objective 或 pause state。
- 目前 Minimap 是 query-driven panel baseline，尚未包含 render texture、fog overlay 或 click-to-world adapter。
- IMGUI renderer 是 FrameworkLab placeholder；正式產品可用同一 ViewModel 替換成 UI Toolkit／uGUI，而不修改 Gameplay。
