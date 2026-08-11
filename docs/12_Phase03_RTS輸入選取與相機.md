# Phase 03 — RTS Input / Selection / Camera

Camera：WASD、edge pan、middle drag、zoom、bounds、focus selected。

Input Actions：Point、Select、AddSelection、Command、CameraMove、CameraZoom、ControlGroup、QueueCommand、Stop、Hold。

Selection：click、drag box、shift add/remove、double click same type、control groups。

Context：Ground→Move、Enemy→Attack、Friendly→Follow、Settlement→Interact。

Acceptance：20 debug units 可框選、編隊、下指令。

## 實作基線

- Gameplay command：`MoveUnitsCommand`、`AttackTargetCommand`、`FollowTargetCommand`、`InteractTargetCommand`、`StopUnitsCommand`、`HoldUnitsCommand`。Player、AI、Scenario 與 Test 必須走相同 `CommandBus`。
- Selection model：`SelectionService` 管理註冊、click／box、add／toggle／remove、同 Definition 雙擊選取，以及 `0–9` control-group snapshot／recall；Unity 物件查詢與畫框由 adapter 處理。
- Camera model：`RtsCameraRigModel` 管理 pivot、zoom 與 bounds；`RtsCameraController` 只轉換 Unity input／transform。
- Input contract：`Demo/Config/AegisRTS_RTS.inputactions` 宣告所有必要 actions；runtime adapter 建立同名 action map，讓 Sandbox 不依賴 Inspector wiring 也可驗收。
- Scene composition：`Sandbox_RTS` 的 `RtsSandboxBootstrap` 只負責組合服務、20 個友軍 debug units、友軍／敵軍／聚落目標與 ground，不承擔 simulation domain logic。

## Sandbox 操作

- `LMB`：click／drag box；`Shift + LMB`：add/remove；double click：選取同 Definition、同 affiliation。
- `Ctrl + 0–9`：建立 control group；`0–9`：recall；`Shift + 0–9`：加入目前 selection。
- `RMB`：Ground→Move、Enemy→Attack、Friendly→Follow、Settlement→Interact；`Shift + RMB`：queue。
- `X`：Stop；`H`：Hold。
- `WASD`／螢幕邊緣／middle drag：pan；mouse wheel：zoom；`F`：focus selected。

## 驗收狀態

- Pure C# selection、context command、camera 與 shared command path：自動測試通過。
- Unity 6.3 runtime adapters、Demo 與 EditMode／PlayMode test source：相容性編譯通過，0 warning、0 error。
- Unity EditMode Test Runner：37/37 passed；PlayMode `Sandbox_RTS` composition test：1/1 passed。
- `Sandbox_RTS` 已在 PlayMode 載入並完成 20 debug units、selection/input/camera composition；實際操作手感仍可在互動式 Editor 做 exploratory tuning。
