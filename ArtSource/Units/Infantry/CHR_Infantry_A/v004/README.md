# CHR_Infantry_A v004 — Phase 03 Secondary Forms

Status: `READY FOR PHASE03 REVIEW`

`Source/CHR_Infantry_A_v004.blend` 是由不可變的 `CHR_Infantry_A_v003_P02R1.blend` 建立之獨立候選。它只加入中尺度裝備 construction：lamellar 分段、肩甲連接、頭盔／披巾、腰帶／腰甲、護腕／手部、腿綁帶／靴、盾牌正反面與短劍結構。

本版本使用六個 `MATID_*` preview materials，但沒有 Final Texture、Final UV、Final Skinning、Animation Polish 或正式 LOD，也沒有匯入或替換 `PF_Unit_Infantry`。

## Rebuild

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background `
  '..\v003\Source\CHR_Infantry_A_v003_P02R1.blend' `
  --python '.\Source\build_infantry_v004_secondary_forms.py' -- `
  --output-root '.\ArtSource\Units\Infantry\CHR_Infantry_A\v004' `
  --expected-input-sha256 '6569A14825B53393B72306F79BF8B29F9DEA5C0FFAB8E30686E901D61964220F'
```

建置結果在 `Documentation/P03_BUILD_RESULT.json`；正式審核資料在 `docs/ArtProduction/ReviewPackages/Infantry_Phase03_SecondaryForms_Review_v001/`。

## Phase 03 Revision 01

`Source/CHR_Infantry_A_v004_P03R1.blend` 是由上述不可變 `v004` 建立的新候選，狀態為 `READY FOR PHASE03 REVISION REVIEW`。本次只修訂 Front Waist Cloth、Scarf、Upper Arm Cloth、Shield Back 與 Boots integration，並提供隔離 Unity RTS preview；沒有進入 Phase 04，也沒有 Final Texture、Final UV 或 Animation Polish。

原 `v004` SHA-256 為 `E1B6B3DFF07258184484186E5AEF8A380457A04E7899107D77908ECABCAA0046`，P03R1 輸出 SHA-256 為 `C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31`。建置稽核在 `Documentation/P03R1_BUILD_RESULT.json`；複審資料在 `docs/ArtProduction/ReviewPackages/Infantry_Phase03_Revision01_Review/`。

## Phase 03.5 L1 Proportion Alignment

`Source/CHR_Infantry_A_v004_P035.blend` 從immutable P03R1建立，狀態為`READY FOR PHASE03_5 REVIEW`。本階段先用未修改幾何的A-Pose／L1 Compare Pose與pixel／bone landmarks診斷，再只修正hip／knee／torso／leg ratios及受量測支持的head、chest、hand、boot fit；Shield、Sword、Helmet與arm lengths保留。

P035 SHA-256為`234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`。正式neutral pose仍是`POSE_SOURCE_A`；`REVIEW_ONLY_POSE_L1_COMPARE`不可匯出成gameplay Idle。資料包位於`docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_L1_Proportion_Alignment_Review/`，未進Phase 04或開始Final UV／Texture。

## Phase 03.5 Revision 01 — Arm / Head / Hand

`Source/CHR_Infantry_A_v004_P035R1.blend` 從上述exact-hash P035建立，狀態為`READY FOR PHASE03_5 REVISION REVIEW`。本次依Reviewer change request實際延長UpperArm至`0.176H`、Forearm至`0.165H`，使Shoulder→Wrist為`0.341H`；Hand width修至`0.06065H`、Head width修至`0.120H`，並重做arm cloth／bracer／grip attachment與review-only L1 Compare Pose。Hip、Knee、Torso、Chest、Shoulder width、Helmet、Boot、Shield與Sword尺寸保持。

P035R1 SHA-256為`A0CCC9771CD7A62D966891784745F138A9DCBF1230DF5E71A4F5D60900A84D0A`。Source A-Pose與23-bone hierarchy保留，33,248 tris不變；review package位於`docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision01_Review/`。沒有進Phase 04或替換正式Runtime Prefab。
