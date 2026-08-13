# Phase 02 Entry Gate — Primary Forms

- Current State：`SATISFIED — PHASE 02 EXECUTED / REVIEW PENDING`
- Planned Task：`Infantry_Remaster_Phase02_PrimaryForms_Task.md`
- Planned Source：`CHR_Infantry_A_v003.blend`
- Protected Baseline：`CHR_Infantry_A_v002.blend`、`SK_Infantry_A_v002.fbx`、current Unity runtime assets

## Preconditions

- [x] `02_Approval_Checklist.md` 15項全部由使用者的Phase 02直接執行指示批准。
- [x] Phase 01 Approval Record填入reviewer／date／target revision。
- [x] v003工作資料夾與命名符合`13_Asset_Naming_and_Folder_Standard.md`。
- [x] v002 protected baseline checksum在工作開始前記錄。
- [x] Phase 02 task明確排除final texture、UV polish、animation polish、final LOD、shader rewrite、VFX及runtime replacement。

## Required Phase 02 Work

1. 由v002與批准的L1/L2 reference建立新versioned v003 source；不可Save As覆蓋v002。
2. 重建Primary Forms：body/head、helmet、shoulders、chest、waist、boots、shield、sword。
3. 保留1.83 m、root/pivot、Humanoid hierarchy、sockets與左右手equipment role。
4. 以Clay/Solid Shading先通過形體，不以texture掩飾。
5. 產生Front、Side、Back、3/4 Clay、black silhouette、wireframe evidence。
6. 同時產生128／64／32 px silhouette tests；至少檢查helmet/plume、shield、sword、shoulder mass、heavy torso與team-color預留區。
7. 報告actual dimensions、triangle count、object separation、geometry QA及所有偏離。

## Phase 02 Must Not Do

- 不覆寫、刪除或移動v001／v002 source/runtime。
- 不更新current `PF_Unit_Infantry`或Content binding。
- 不完成或宣稱final texture／shader／animation／LOD chain。
- 不以primitive blockout、beauty render或AI圖片冒充approved 3D Primary Forms。
- 不改Asset ID、Animator parameter、`AttackImpact`、socket、anchor或gameplay authority。

## Required Review Evidence

```text
Front
Side
Back
3/4 Front
3/4 Back (recommended)
Clay
Black Silhouette
Wireframe
128 px / 64 px / 32 px
Dimension and triangle report
Object separation manifest
Deviation / open-issue log
```

## Exit

Phase 02只在Primary Forms、silhouette、dimensions、construction clearance與evidence全部通過人工Art／Technical Art review後結束。Unity temporary preview可以加入，但不能取代DCC形體gate，也不能在本階段把candidate綁到production Prefab。
