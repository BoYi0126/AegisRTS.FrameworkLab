# Phase 03 Open Issues

Status: `READY FOR PHASE03 REVIEW`

| ID | Priority | Issue / risk | Required next action |
|---|---|---|---|
| P03-001 | P0 | Human Phase 03 decision尚未記錄。 | Reviewer填寫`06_Review_Checklist.md`。 |
| P03-002 | P0 | Unity RTS Normal capture未執行。 | 依`04_Unity_Review_Status.md`在隔離 review path補拍。 |
| P03-003 | P1 | v004仍是static A-Pose object parenting，未驗證肩、肘、髖、膝 deformation。 | Final Skinning phase做pose／weight／clipping tests。 |
| P03-004 | P1 | Shield grip／forearm strap為visual attachment logic，不是已驗證手臂 binding。 | Skinning與animation階段驗證握持與穿插。 |
| P03-005 | P1 | Chest／shoulder modular source pieces尚未做final deformation retopo。 | Form批准後才合併／retopologize。 |
| P03-006 | P1 | Final UV、texture maps、Team Color mask與production shader不存在。 | Phase 04建立並單獨驗收。 |
| P03-007 | P2 | L1 back view的局部 attachment資訊有限；rear scarf／shield brace為合理化推導。 | Art reviewer確認或標示變更。 |
| P03-008 | P2 | 33,898 tris位於建議範圍上緣，尚無 battle-count profiler evidence。 | 正式 LOD／integration階段量測。 |
| P03-009 | P0 | Upstream provenance／commercial distribution evidence仍未完整。 | Production Ready前由資產擁有者補齊。 |

`PENDING`、`MANUAL REQUIRED`與`NOT RUN`均不等於 PASS。
