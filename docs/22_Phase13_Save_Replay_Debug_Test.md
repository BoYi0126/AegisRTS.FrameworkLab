# Phase 13 — Save / Replay / Debug / Test

Save 純 GameState：faction、settlement、unit、hero、army、resource、building、tech、objective、clock、random state。

Metadata：SaveVersion、FrameworkVersion、ContentVersion、ScenarioId、Timestamp。

Replay：InitialState + Seed + Commands + Tick。

Debug Console：spawn、kill、damage、give_resource、capture、set_speed、toggle_ai、show_path、show_threat。

Tests：EditMode、PlayMode、AI simulation/soak。

Acceptance：戰鬥中 save→reload 後核心狀態一致。

## 完成狀態（2026-08-11）

- Status：Completed。
- 新增 typed `GameStateDocument`，涵蓋 faction、settlement、unit、hero、army、resource account、building、technology、objective、clock、random 與 extension sections。
- 新增 `SaveMetadata`：SaveVersion、FrameworkVersion、ContentVersion、ScenarioId、Timestamp。
- 新增 `GameStateSaveService`：JSON envelope、SHA-256 integrity、strict version compatibility 與 deterministic fingerprint。
- 新增 `IGameStateCaptureSource`／`IGameStateRestoreSink`／`GameStateCoordinator`，Persistence 不持有任何 Unity object 或具體 Gameplay manager。
- 新增 `MemorySaveStore`／`FileSaveStore`，file store 使用 temporary file 再 replace／move。
- `SeededRandom`、`GameClock` 新增 capture／restore state。
- 新增 Replay InitialState／Seed／Commands／Tick／Sequence、recorder、JSON serializer 與 forward-only player。
- 新增 Debug Console：spawn、kill、damage、give_resource、capture、set_speed、toggle_ai、show_path、show_threat；console 只解析並委派 `IDebugCommandExecutor`。
- Persistence assembly 設為 `noEngineReferences=true`。

## 驗收

- 戰鬥中 HP=45／resources／objective／clock／random save，故意 mutation 後 reload fingerprint 完全一致：PASS。
- checksum tamper 與 incompatible version rejection：PASS。
- Replay same-tick stable order、JSON roundtrip、forward-only playback：PASS。
- Debug Console 九種 commands、quoted arguments、disabled／invalid handling：PASS。
- Unity EditMode：136/136 passed、0 failed；Phase 13 新增 11 cases。
- Unity PlayMode：16/16 passed、0 failed；Phase 13 新增 1 case。

## 架構邊界與限制

- DTO 只保存 stable IDs、數值與 enum strings，不保存 GameObject、Transform、MonoBehaviour、NavMeshAgent 或 Animator。
- 版本 policy 目前 strict exact match；正式發行前需為舊版本加入 explicit migration chain。
- Replay payload 是 data JSON；實際 command reconstruction 由 game composition 的 `IReplayCommandSink` 負責。
- 尚未實作 async I/O、compression、cloud sync、incremental checkpoints 或 replay seek snapshots。
