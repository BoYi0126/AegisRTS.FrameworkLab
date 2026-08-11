# Phase 14 — Performance

先 profiling 再最佳化。

Metrics：FPS、frame/simulation/AI/navigation ms、unit/projectile count、GC、memory。

不同 tick frequency；Projectile/VFX/float text pooling；spatial query 避免 N²；LOD/instancing/culling。

Stress：100、300、500、1000 exploratory units。

正式性能門檻依目標硬體 benchmark 決定。
