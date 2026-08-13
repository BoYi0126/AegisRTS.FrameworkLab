# Infantry Phase 03.5 L1 Proportion Alignment Review

Status: `READY FOR PHASE03_5 REVIEW`

本資料包提交 `CHR_Infantry_A_v004_P035` 的比例與姿勢 Gate。工作依序完成未修改 P03R1 幾何的 A-Pose／L1 Compare Pose 診斷、L1 pixel landmarks、Blender bone／mesh landmarks、只針對量測差異的 controlled correction，以及隔離 Unity review。

## 快速入口

- `00_Phase03_5_Report.md`：範圍、診斷與結果。
- `01_L1_vs_3D_Landmark_Report.md`：normalized landmark／width／segment 比較。
- `02_Proportion_Correction_Table.md`：Before／After 與具體 correction。
- `03_Pose_Difference_Report.md`：POSE／PROPORTION／ARMOR 分類。
- `04_Unity_Review_Status.md`：A-Pose、L1 Pose、RTS Normal 驗證。
- `05_Open_Issues.md`：估測不確定性與 Reviewer 決策。
- `06_Review_Checklist.md`：19 項 Gate，全部保留待 Reviewer 勾選。

`POSE_SOURCE_A` 仍是正式 neutral source pose；`REVIEW_ONLY_POSE_L1_COMPARE` 只供比較，不是 bind pose、Idle 或 Animation Polish。沒有進入 Phase 04、Final UV、Final Texture、Final Skinning、Animation Polish 或正式 LOD。

