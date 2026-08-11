# Phase 14 — Performance

先 profiling 再最佳化。

Metrics：FPS、frame/simulation/AI/navigation ms、unit/projectile count、GC、memory。

不同 tick frequency；Projectile/VFX/float text pooling；spatial query 避免 N²；LOD/instancing/culling。

Stress：100、300、500、1000 exploratory units。

正式性能門檻依目標硬體 benchmark 決定。

## 完成狀態（2026-08-11）

- Status：Completed（Framework baseline／exploratory benchmark）。
- 新增 sliding `PerformanceMetricsCollector`：FPS、frame／simulation／AI／navigation ms、unit／projectile count、GC bytes、memory。
- 新增 data-supplied `PerformanceBudget`／evaluator；不在 Framework code 硬寫特定硬體門檻。
- 新增 `TickScheduler`：per-system frequency、deterministic order、maximum catch-up cap。
- 新增 bounded generic `ObjectPool<T>`；`UnityCombatDriver` projectile visuals 已從 Create／Destroy 改成 rent／return，VFX／floating text 可重用相同 API。
- 新增 generic 2D `SpatialHash<T>`：insert／update／remove／radius query 與 optional deterministic sort。
- 新增 `SimulationLodPolicy`：Full 30 Hz、Reduced 15 Hz、Coarse 5 Hz、Culled。
- 新增 `PerformanceStressHarness` 與 Sandbox 100／300／500／1000 exploratory unit reports。

## 驗收

- metrics window／P95／budget violations：PASS。
- simulation 30 Hz、AI 5 Hz、navigation 10 Hz；long-frame catch-up cap：PASS。
- pool reuse／retention cap／Unity projectile pool return：PASS。
- spatial query／update／remove 與 local-neighbor result growth：PASS。
- LOD／simulation culling tiers：PASS。
- 100／300／500／1000 exploratory scenarios：PASS；未將開發機 elapsed 值宣告成正式硬體 budget。
- Unity EditMode：146/146 passed、0 failed；Phase 14 新增 10 cases。
- Unity PlayMode：17/17 passed、0 failed；Phase 14 新增 1 case，既有 Combat pool assertions PASS。

## 架構邊界與限制

- Performance toolkit 位於 Pure C# Core；Unity Profiler／ProfilerRecorder 可在 adapter 層把實測 samples 寫入 collector。
- SpatialHash 是 broad-phase/local query baseline，不取代 NavMesh pathfinding 或 physics collision。
- LOD policy 輸出決策，正式 renderer 必須再接 GPU instancing、Animator LOD、occlusion 與 camera culling。
- 正式 thresholds 仍需指定 CPU／GPU／resolution／quality、build type、map、content pack 與 sample duration。
