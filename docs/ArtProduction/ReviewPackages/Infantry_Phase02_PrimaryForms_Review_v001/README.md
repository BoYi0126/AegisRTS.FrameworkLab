# Infantry Phase 02 Primary Forms Review v001

This package contains the Phase 02 Primary Forms candidate for CHR_Infantry_A_v003.

This is not a final Production Ready character.
Final UV, textures, skinning, animation polish and LOD are intentionally deferred.

Status: `READY FOR REVIEW`

本包供 Art、Design 與 Technical Art 審查 Primary Forms、silhouette、major geometry、RTS 距離辨識度與 v002→v003 改造方向。它不表示 Phase 02 已通過，也不授權替換正式 Runtime Prefab。

## Review Order

1. `00_Phase02_Report.md`
2. `02_Changes_From_v002.md`
3. `Screenshots/Comparison/`
4. `Screenshots/Clay/` 與 `Screenshots/Silhouette/`
5. `Screenshots/ScreenSize/` 與 `Screenshots/Wireframe/`
6. `01_Geometry_Stats.md`、`03_Open_Issues.md`、`05_Review_Checklist.md`
7. `Manifests/`

## Package Map

- `Blender/`：v003 source copy、build/render/comparison scripts。
- `Screenshots/Clay/`：六方向 Clay evidence。
- `Screenshots/Silhouette/`：四方向黑白輪廓。
- `Screenshots/ScreenSize/`：128／64／32 px 目標高度預覽。
- `Screenshots/Wireframe/`：三方向 topology density overview。
- `Screenshots/Comparison/`：immutable v002 baseline 對 v003 Primary Forms。
- `Screenshots/Unity/`：本階段未修改 Runtime Prefab；保留 manual capture 說明。
- `Specifications/`：本階段任務與已批准的 Phase 01 target snapshot。
- `Manifests/`：geometry、objects、bones、screenshots、files與SHA-256。

## Candidate Summary

| Field | Value |
|---|---:|
| Source | `CHR_Infantry_A_v003.blend` |
| Source status | `WIP_MODEL` |
| Height | 1.830 m |
| Mesh objects | 72 |
| Vertices | 14,211 |
| Triangles | 28,138 |
| Review materials | 6 |
| Armature / bones | 1 / 23 |
| Saved Actions | 0 |

Reviewers must record `APPROVE` or `CHANGE REQUESTED` in `05_Review_Checklist.md`. Until that happens, the only package status is `READY FOR REVIEW`.
