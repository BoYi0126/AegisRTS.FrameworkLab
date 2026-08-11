# Phase 04 — Movement / Navigation / Formation

Move Command → Movement System → Navigation Adapter → View。

使用 NavMesh/AI Navigation：destination validation、unreachable、repath、stuck detection、local avoidance。

Formation：Line、Box；Group move 不能把所有單位送往同一座標。

Debug：path、destination、velocity、formation slot、stuck。

Acceptance：50 Unit 穿越障礙並形成隊形，不大量永久卡死。

## 實作基線

- `MoveUnitsCommand` 帶有 `FormationType`；Player、AI、Scenario 與 Test 仍走同一 `CommandBus`。
- `MovementSystem` 管理 per-entity order、queue、arrival、unreachable、repath、stuck 與 hold state，不持有 `GameObject` 或 `NavMeshAgent`。
- `INavigationAdapter` 是 Gameplay 對 navigation backend 的唯一 boundary；`NavMeshMovementAdapter` 負責 Unity destination sampling、完整路徑驗證與 Agent 操作。
- `FormationPlanner` 支援 `Line` 與 `Box`，依 group centroid→destination heading 旋轉 slots，且每個 actor 取得不同座標。
- `NavMeshAgent` 提供 local avoidance；priority 由穩定 `EntityId` 派生，避免所有 Agent 使用相同優先權。
- Sandbox 使用 `NavMeshSurface.BuildNavMesh()` 建立 runtime 測試場；production scene 應依規模選擇預先 bake 或受控 async update。

## Sandbox 操作與 Debug

- `RMB`：移動到 formation slots；`Shift + RMB`：queue。
- `Tab`：切換 `Box`／`Line`；`X`：Stop；`H`：Hold。
- Scene gizmos：綠色 path、黃色 destination／formation slot、青色 velocity。
- HUD：顯示 selected count、formation、Movement summary、Navigation summary 與最後 command。

## 驗收狀態

- Unity EditMode：44/44 passed，其中 Phase 04 新增 7 cases。
- Unity PlayMode：2/2 passed；包含 50 Agent runtime NavMesh composition 與跨障礙 movement acceptance。
- 50 units 全部取得 distinct Box slots；15 秒後至少 40 units 跨越中央障礙，永久 `Stuck`／`Unreachable` 不超過 5。
- Unity 6000.5.7f1 compatibility compile：0 warning、0 error。
