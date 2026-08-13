# Infantry Phase 03.5 Revision 01 Review

Status: `READY FOR PHASE03_5 REVISION REVIEW`

本資料包提交 `CHR_Infantry_A_v004_P035R1` 的 focused Arm／Hand／Head 比例修訂。來源是 immutable `CHR_Infantry_A_v004_P035`（SHA-256 `234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`）；原 P035 未被覆寫。

## 快速入口

- `00_Revision_Report.md`：範圍、source、修正與結果。
- `01_Arm_Proportion_Report.md`：L1、Before、After 比例與 posed landmark 表。
- `02_Before_After_Correction_Table.md`：修改／保留項目與數值。
- `03_Unity_Review_Status.md`：隔離 Unity A-Pose／L1Pose／RTS Normal 結果。
- `04_Open_Issues.md`：仍須人工判斷與 deferred 工作。
- `05_Review_Checklist.md`：19 項 gate，全部保留待 Reviewer 勾選。
- `06_Visual_Evidence_Index.md`：DCC／Overlay／Comparison／Unity evidence 索引。

正式 source 仍是 `POSE_SOURCE_A`；`REVIEW_ONLY_POSE_L1_COMPARE` 只供量測與審核，不是 bind pose、Idle 或 Animation Polish。本 Revision 沒有進 Phase 04、Final UV、Final Texture、Final Skinning、Animation Polish、正式 LOD 或 Runtime Prefab replacement。

