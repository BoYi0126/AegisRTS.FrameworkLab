# Phase 01 Evidence Index

## Primary Visual Sources

| Evidence | Repository path | Use | Limitation |
|---|---|---|---|
| L1 Final | `ArtSource/Units/Infantry/CHR_Infantry_A/v001/Concepts/Unit_03_Infantry_L1_Concept_Final.png` | master identity、multi-view、team color、equipment | beauty/concept reference；不是exact geometry |
| L1 Alternate | `ArtSource/Units/Infantry/CHR_Infantry_A/v001/Concepts/Unit_03_Infantry_L1_Concept_Alternate.png` | rejected/alternate comparison | 不可混合成新design |
| v002-linked L1 | `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Reference/Unit_03_Infantry_L1_Concept_Final.png` | 證明v002輸入reference | 與v001 final重複，不是新批准 |
| Legacy dimensions | `ArtSource/Units/Infantry/CHR_Infantry_A/v001/Previews/Dimensions/PREVIEW_Dimensions_Front.png` | baseline size comparison | 單一正面、非正式production orthographic set |
| Phase 01 target | `mission/Infantry_Phase01_Production_L2_Remaster_Target.md` | v003 visual／construction contract | approval pending |

## Current Baseline Sources

| Evidence | Repository path | Verified fact |
|---|---|---|
| Editable source | `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/CHR_Infantry_A_v002.blend` | Blender 5.2 source；不得覆寫 |
| Master FBX | `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Models/SK_Infantry_A_v002.fbx` | current export baseline |
| Unity Prefab | `Assets/AegisRTS/Content/Shared/Art/Units/Infantry/Resources/AegisRTS/Units/Infantry/PF_Unit_Infantry.prefab` | stable L4 contract |
| Review report | `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/00_Collection_Report.md` | source/runtime chain、collection scope、integrity |
| Blender summary | `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/04_Blender_Model_Summary.md` | 1.83 m、23 bones、4,376-tri LOD0、max influence 1 |
| Unity summary | `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/03_Unity_Technical_Summary.md` | Prefab、Animator、anchors、events、materials |
| Missing data | `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/02_Missing_Data.md` | evidence gaps that must remain visible |

## Review Images

Existing review copies are already centralized under:

```text
docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/
├─ L1_Concept/
├─ L2_Reference/
└─ Screenshots/
   ├─ Blender/       # six actual-material views and four Clay views
   ├─ Wireframe/     # front and 3/4
   └─ Existing/      # legacy camera previews and historical Unity captures
```

Phase 01沒有重複複製或重新生成影像。Review Package已提供逐檔manifest與checksum，可直接用於批准比較。

## Production Standards Applied

- `02_Asset_Pipeline_L1_L4.md`
- `03_Character_Production_Quality_Standard.md`
- `04_RTS_Silhouette_and_Readability_Standard.md`
- `05_LOD_and_Performance_Standard.md`
- `06_Texture_Material_TeamColor_Standard.md`
- `07_Rig_Skinning_Animation_Standard.md`
- `08_Golden_Sample_Infantry_Archer.md`
- `09_Existing_Infantry_Archer_Remaster_Audit.md`
- `13_Asset_Naming_and_Folder_Standard.md`
- `15_Unity_RTS_Asset_Acceptance_Checklist.md`
- `16_Master_Production_Checklist.md`
- `99_Open_Issues_and_Missing_Information.md`

Root：`docs/ArtProduction/RTS_Asset_Production_Spec_v1/`。

## Evidence Classification

- `CURRENT VERIFIED`：repository file／YAML／hash／read-only DCC inspection可證明。
- `HISTORICAL`：既有Unity capture或舊報告；不代表本次重跑。
- `TARGET`：Phase 01規格值；批准後才對v003生效。
- `NOT RUN`：新Unity import、test、build、Profiler、v003 render。
- `CANNOT VERIFY`：rights、approver、target hardware及其他欠缺的人工作業。
