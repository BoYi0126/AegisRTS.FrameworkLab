# CHR_Infantry_A Phase 01 Production L2 Review Record

- Asset：`unit.infantry` / `PF_Unit_Infantry` / `CHR_Infantry_A`
- Phase：01 — Production L2 / Visual & Construction Target
- Record Version：v001
- Source Specification：`mission/Infantry_Phase01_Production_L2_Remaster_Target.md`
- Source SHA-256：`DBD67A4021ED8FD56AD7F4F2B197BB430185B1817647445460753DCDB5316540`
- Baseline：`CHR_Infantry_A_v002`
- Next Candidate：`CHR_Infantry_A_v003`
- Status：`APPROVED — PHASE 02 AUTHORIZED 2026-08-13`

## Outcome

Phase 01 任務規格已與 repository 的 L1、v002 source/runtime、Unity contract、RTS production standard 及既有 review evidence 交叉稽核；15 個批准決策都有明確、可執行且未互相矛盾的目標值。

2026-08-13 使用者明確要求完整執行 `Infantry_Remaster_Phase02_PrimaryForms_Task.md`，因此視為接受本 checklist 並授權進入 Phase 02。Phase 01 方向已批准；這不等於 Phase 02 candidate 通過，也不授權修改 `PF_Unit_Infantry`。

## Authoritative Use

本資產的 v003 remaster 採以下優先序：

1. 已存在且必須保留的 runtime contract：Asset／Prefab ID、scale、root、Humanoid、socket、Animator parameter、`AttackImpact`、anchors、Root Motion Off。
2. `mission/Infantry_Phase01_Production_L2_Remaster_Target.md`：v003 的 asset-specific visual、construction、proportion、Primary Forms 與 Phase 02 scope。
3. `docs/ArtProduction/RTS_Asset_Production_Spec_v1/`：通用 production quality、L1→L4、silhouette、rig、material、LOD、provenance 與 acceptance gate。
4. `docs/ArtSpecs/Unit_03_步兵*.md`：不與上述新版目標衝突的 legacy/runtime 細節。

其中 v003 LOD0 採 asset-specific `20K–30K`、preferred `24K–27K`；舊 `2.5K–6K` 僅保留為 v002 Prototype／後續較低 LOD 參考，不能再當 v003 production LOD0 上限。

## Files

- `01_Conformance_Audit.md`：目標與 repository 規格的逐項一致性稽核。
- `02_Approval_Checklist.md`：15 個使用者批准項、目前狀態與批准紀錄欄位。
- `03_Evidence_Index.md`：L1、v002、DCC、Unity、technical evidence 的來源與限制。
- `04_Open_Issues.md`：Phase 02／Production Ready 仍須處理的問題。
- `05_Phase02_Entry_Gate.md`：批准後才可建立的 v003 Primary Forms 工作邊界。

## Phase Boundary

Phase 01 只鎖定方向與施工規格；不包含 final texture、UV polish、animation polish、final LOD chain、shader rewrite、VFX 或 runtime Prefab replacement。Phase 02 只允許 versioned v003 Primary Forms candidate 與 Clay／Silhouette／Wireframe evidence。

## Approval Record

Phase 01 已由本次直接執行 Phase 02 的使用者指示批准。Phase 02 輸出位於 `docs/ArtProduction/ReviewPackages/Infantry_Phase02_PrimaryForms_Review_v001/`，狀態僅為 `READY FOR REVIEW`。
