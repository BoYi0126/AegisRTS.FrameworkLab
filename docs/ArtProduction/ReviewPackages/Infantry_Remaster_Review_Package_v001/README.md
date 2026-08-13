# Infantry Remaster Review Package v001

此資料包用於 **Infantry Visual Remaster Review**，不代表已修改、改善或核准任何 Production Asset。資料來自 2026-08-13 的 workspace 唯讀掃描、原檔複製、Unity YAML／`.meta` 靜態解析，以及只對本資料包內 `.blend` 副本進行的 Blender 5.2 背景分析與 review render。

## Review Priority

1. L1 vs L3
2. Silhouette
3. Primary Forms
4. Armor
5. Texture
6. Skinning
7. Animation
8. Unity RTS Readability

## 核心判斷

- Current gameplay asset 是 `unit.infantry` → `PF_Unit_Infantry` → `SK_Infantry_A_v002.fbx`，不是 v001 GLB。
- Current source 是 `CHR_Infantry_A_v002.blend`；Blender 5.2 唯讀分析確認 23 objects、12 meshes、1 armature、23 bones、LOD0 4,376 triangles。
- L1 final／alternate 存在；**正式 L2 Production Character Sheet／turnaround NOT FOUND**。v001 的 `L2` 是 playable model delivery，不等於 production construction sheet。
- Current `.blend` 重開後有 0 Actions；五段動畫存在為 source/runtime FBX，並可由 build script 重建。這是 review blocker，不可把「有 FBX」誤寫成「editable Actions 已保存」。
- Current Unity team color 是獨立 `TeamColor` material slots 加 `MaterialPropertyBlock`，不是 mask-driven shader；TeamColorMask 與 ORM 存在但未被 current material 引用。
- Current shader 是 Unity URP Lit；repository 沒有 Infantry custom `.shader`／`.shadergraph` 可收集。
- 既有 Unity images 是歷史 validation output，缺少完整 close／medium／RTS normal／far、128／64／32 px 與成對 blue/red capture metadata。詳見 `02_Missing_Data.md`。

## 信任標籤

- `CURRENT`：由 ContentPack、Prefab、GUID／YAML 或 current source/runtime path 直接確認。
- `CURRENT SOURCE EXPORT`：由 v002 source package 產生，與 Unity runtime binary hash 相符。
- `LEGACY`：仍存在但不被 current Prefab／Content binding 使用。
- `HISTORICAL UNITY CAPTURE`：曾由 Unity validation 產生，但不是本次重新執行的 capture。
- `GENERATED REVIEW EVIDENCE`：本次只從 package `.blend` 副本產生。
- `CANNOT VERIFY`／`NOT FOUND`：沒有足夠證據；不是 pass。

## Package 導覽

| 路徑 | 內容 |
|---|---|
| `00_Collection_Report.md` | 執行範圍、Found、結論與驗證摘要 |
| `01_Asset_Inventory.md` | Current／Legacy 資產、原始與複製位置、解析狀態 |
| `02_Missing_Data.md` | Critical／Important／Optional 缺口與人工資料需求 |
| `03_Unity_Technical_Summary.md` | Content binding、Prefab、importer、LOD、team color、shader |
| `04_Blender_Model_Summary.md` | DCC objects、meshes、triangles、weights、bones、equipment |
| `05_Animation_Summary.md` | FBX clips、frame ranges、loops、events、AttackImpact |
| `06_Material_Texture_Summary.md` | Material references、texture channels／resolution／import |
| `07_Visual_Evidence_Index.md` | L1、legacy previews、historical Unity、new DCC renders 與缺圖 |
| `08_Source_Spec_Index.md` | 來源文件、規格新舊關係與衝突 |
| `Manifests/` | copy map、file manifest、Blender object/bone data、SHA-256 |

所有任務要求的核心分類資料夾均保留，即使 `Materials/Shaders/` 沒有可複製的 custom shader。

## 建議 Reviewer 流程

1. 先看 `L1_Concept/v001/Unit_03_Infantry_L1_Concept_Final.png`。
2. 對照 `Screenshots/Blender/` 的 actual-material、Clay 與 `Screenshots/Wireframe/`。
3. 查看 `Screenshots/Existing/Unity/Detail/` 與 `Movement/`，但注意其 historical 標籤。
4. 閱讀 `04_Blender_Model_Summary.md`、`06_Material_Texture_Summary.md`、`05_Animation_Summary.md`。
5. 依 `Specifications/ProductionSpec/09_Existing_Infantry_Archer_Remaster_Audit.md` 判斷 Preserve／Modify／Partial Rebuild／Rebuild。
6. 未補齊 `02_Missing_Data.md` 的 Critical items 前，不標為 Golden Sample 或 Production Ready。

## 完整性驗證

`Manifests/SHA256SUMS.txt` 包含 review 所需 binary／image／Unity asset 副本的 SHA-256。`Manifests/File_Manifest.csv` 是完整 package file index；`Source_Copy_Map.csv` 只列從原始位置複製的檔案。ZIP 驗證結果記錄於 `00_Collection_Report.md`。
