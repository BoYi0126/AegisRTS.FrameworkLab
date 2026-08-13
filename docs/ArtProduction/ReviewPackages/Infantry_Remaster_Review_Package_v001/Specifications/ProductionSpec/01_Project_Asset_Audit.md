# 01 — Project Asset Audit

- Specification Version：1.0
- Audit Date：2026-08-13
- Scope：Repository Root（排除 `.git/`、`Library/`、`Temp/`、`obj/`、`Logs/`、`TestResults/`）

## 掃描基準

- `VERIFIED` Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`；開始掃描時 working tree clean。
- `VERIFIED` Unity：`6000.5.7f1`；URP `17.5.0`；glTFast `6.19.0`；AI Navigation `2.0.14`。
- `VERIFIED` 相關副檔名基準：98 `.md`、26 `.fbx`、2 `.blend`、26 `.png`、2 `.controller`、6 `.mat`、7 `.prefab`、0 `.anim`；此數量包含 repository 內非角色用途檔案。
- `VERIFIED` `ArtSource/`：73 files／15,787,828 bytes；2 Blend、13 FBX、6 GLB、21 PNG。
- `VERIFIED` Unity Shared Art：86 files／9,886,843 bytes；13 FBX、2 GLB、4 PNG、5 Materials、2 Controllers、3 Prefabs（另含 `.meta` 與 4 個舊 GLB 衍生 Mesh `.asset`）。
- `VERIFIED` 可視為 Production Asset 的 source/runtime 檔案基準：71 files／24,898,052 bytes；combined manifest SHA-256 `A00DB4E5A733D90B021CCCE3F2CBBB660798EDB70F4B6D2FE4215DFBD5461C05`。

## 現有角色／兵種清單

新 L2 定義為 Production Character Sheet，不把舊版「可匯入模型」誤算為 L2。

| Asset | Type | L1 | L2 | L3 | L4 | Rig | Animation | Texture | Team Color | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Infantry／`unit.infantry` | Standard Unit | `VERIFIED` 概念圖 | `NOT FOUND`（舊 L2 是 Game Model） | `VERIFIED` Prototype | `VERIFIED` | Humanoid Valid | 5 clips | 1K placeholder set | Material-slot MPB | `GOLDEN_SAMPLE_CANDIDATE` |
| Archer／`unit.archer` | Standard Unit | `NOT FOUND` | `NOT FOUND` | `VERIFIED` Prototype | `VERIFIED` | Humanoid Valid | 5 clips | `NOT FOUND`（runtime constant colors） | Material-slot MPB | `GOLDEN_SAMPLE_CANDIDATE` |
| Cavalry／`unit.cavalry` | Standard Unit | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder tint | `BLOCKOUT` |
| Siege Unit／`unit.siege` | Special／Vehicle | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder tint | `BLOCKOUT` |
| Commander／`hero.commander` | Hero | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Hero Placeholder | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder tint | `BLOCKOUT` |
| Lieutenant／`hero.lieutenant` | Hero | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Hero Placeholder | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder tint | `BLOCKOUT` |
| Opponent Commander／`hero.opponent` | Hero | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Hero Placeholder | `NOT FOUND` | `NOT FOUND` | `NOT FOUND` | Placeholder tint | `BLOCKOUT` |

## Infantry 全部相關檔案

`.meta` 與來源重複副本未逐列，實體檔案如下。

| 路徑／群組 | 類型與用途 | Production Ready | Recheck |
| --- | --- | --- | --- |
| `ArtSource/Units/Infantry/CHR_Infantry_A/v001/Concepts/Unit_03_Infantry_L1_Concept_{Final,Alternate}.png` | L1；1254×1254，含正／側／背／3/4、藍紅、黑剪影 | No | 是；低於新 2048 建議且不是 neutral A-Pose sheet |
| `.../v001/Models/CHR_Infantry_A_v001_LOD{0,1}_{Blue,Red}.glb` | 舊 L2 model；紅藍重複幾何只供追溯 | No | 是；禁止回流成兩套 runtime mesh |
| `.../v001/Textures/T_Infantry_A_{BaseColor,Normal,ORM,TeamColor_Blue,TeamColor_Red}_1K.png` | 舊 L2 1K placeholder maps | No | 是；Normal／ORM 為近均值 placeholder |
| `.../v001/Previews/Camera/*.png`、`Previews/Dimensions/*.png`、`UV/*.png` | 數學相機預覽、尺寸與 UV 證據 | No | 是；明載不是 Unity screenshot |
| `.../v001/Documentation/*`、`ASSET_MANIFEST.md`、`Tools/UnityEditor/InfantryL2Validator.cs` | 舊交付、metrics、validator（未啟用） | N/A | 是；Provenance 不完整 |
| `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/CHR_Infantry_A_v002.blend` | 可編輯 Rig／Mesh source | No | 是；只讀稽核發現檔內 0 Actions |
| `.../v002/Source/build_unit03_l3_blender.py`、`BUILD_WINDOWS.bat`、`BUILD_RESULT.json` | 可重建 Blender／FBX／clips；4376／1512／542 tris | N/A | 是；build script 才是 animation source of truth |
| `.../v002/Models/SK_Infantry_A_v002.fbx` | Humanoid master、3 LOD | Prototype only | 是 |
| `.../v002/Animations/AN_Infantry_{Idle,Move,Attack_A,Hit,Death}.fbx` | 5 個 In Place clips | Prototype only | 是；professional polish 未驗 |
| `.../v002/Textures/T_Infantry_A_{BaseColor,Normal,ORM,TeamColorMask}_1K.png` | v002 texture set | No | 是；ORM／Mask 現行 shader 未取樣 |
| `.../v002/Documentation/*`、`Input_v001/*`、`Reference/*`、`README.md`、`ASSET_MANIFEST.md` | provenance、events、import、manifest、inputs | N/A | 是；原 v001 權利仍 gated |
| `Assets/AegisRTS/Content/Shared/Art/Units/Infantry/Models/{SK_Infantry_A_v002.fbx,CHR_Infantry_A_v001_LOD0.glb,CHR_Infantry_A_v001_LOD1.glb}` | Unity runtime／legacy import models | Prototype | 是；舊 GLB 衍生資產待 migration review |
| `.../Infantry/Animations/AN_Infantry_{Idle,Move,Attack_A,Hit,Death}.fbx`、`AC_Infantry.controller` | Humanoid clips／Animator | Prototype | 是 |
| `.../Infantry/Textures/*.png`、`Materials/MAT_Infantry_{Base,TeamColor}.mat` | URP Lit materials；BaseColor／Normal connected | Prototype | 是；ORM／Mask unused |
| `.../Infantry/Meshes/SM_Infantry_A_LOD{0,1}_{Base,Team}.asset` | 舊 GLB 合併 mesh 衍生物 | Legacy runtime | 是；確認是否仍被引用後再另案清理 |
| `.../Infantry/Resources/AegisRTS/Units/Infantry/PF_Unit_Infantry.prefab` | L4 Prefab、Humanoid、LOD、anchors、盾劍 | Prototype | 是；保留 stable ID |

## Archer 全部相關檔案

| 路徑／群組 | 類型與用途 | Production Ready | Recheck |
| --- | --- | --- | --- |
| `ArtSource/Units/Archer/CHR_Archer_A/v001/Source/CHR_Archer_A_v001.blend` | 可編輯 derivative Rig／Mesh source | No | 是；檔內 0 Actions |
| `.../Source/build_unit04_archer_blender.py`、`BUILD_RESULT.json` | 可重建 source；3344／1280／542 tris | N/A | 是 |
| `.../Models/SK_Archer_A_v001.fbx`、`PRJ_Arrow_Basic_v001.fbx` | Humanoid master與 0.82 m 箭矢 | Prototype | 是 |
| `.../Animations/AN_Archer_{Idle,Move,Attack_Ranged,Hit,Death}.fbx` | 5 個 In Place clips；release frame 22 | Prototype | 是；弓弦無 deform |
| `.../Documentation/{ANIMATION_EVENTS.json,GENERATION_AND_LICENSE_RECORD.md,L3_DELIVERY_REPORT.md,MANIFEST.json,PROMPT.txt,STATUS.md,UNITY_IMPORT_SETTINGS.md}`、`README.md` | 交付與來源記錄 | N/A | 是；權利沿用 Infantry v001 gate |
| `Assets/AegisRTS/Content/Shared/Art/Units/Archer/Models/{SK_Archer_A_v001,PRJ_Arrow_Basic_v001}.fbx` | Unity model與 projectile | Prototype | 是 |
| `.../Archer/Animations/AN_Archer_{Idle,Move,Attack_Ranged,Hit,Death}.fbx`、`AC_Archer.controller` | Humanoid clips／Animator | Prototype | 是 |
| `.../Archer/Materials/MAT_{Archer_Base,Archer_TeamColor,Arrow_Base}.mat` | URP Lit constant-color materials，無 texture references | No | 是；正式 texture set `NOT FOUND` |
| `.../Archer/Resources/AegisRTS/Units/Archer/PF_Unit_Archer.prefab` | L4 Prefab、LOD、anchors、bow、quiver、socket | Prototype | 是 |
| `.../Archer/Resources/AegisRTS/Projectiles/PRJ_Arrow_Basic_v001.prefab` | presentation-only pooled projectile | Prototype | 是 |

## DCC／Unity 技術驗證

- `VERIFIED` 兩個 `.blend` 可由 Blender 5.2 只讀開啟；Infantry 23 objects／12 meshes／1 armature／23 bones，Archer 36 objects／25 meshes／1 armature／23 bones。
- `VERIFIED` 兩者 body meshes 所有 vertices 都有權重，但最大 influences 為 1；這是 rigid-piece prototype skinning，不是 production smooth deformation。
- `VERIFIED` 兩個 `.blend` 重新開啟後皆為 0 Actions。Builder 在建立 actions 後取消 active action並儲存，未建立 durable user，因此文件「actions stored in file」與實檔不一致；獨立 animation FBX 與 script 仍存在。
- `VERIFIED` Unity master FBX `animationType: 3`／`avatarSetup: 1`；clips `avatarSetup: 2`，Prefabs `ApplyRootMotion: 0`。
- `VERIFIED` 兩個 Prefab LOD thresholds 同為 `0.04 / 0.012 / 0.003`，且都有 LOD0／1／2、Selection／Health／Ground anchors。
- `VERIFIED` Infantry 與 Archer controllers 都有 `Speed`、`MoveRate`、`AttackRate`、`Attack`、`Hit`、`Die`、`IsDead`。

## Shader／Material／Team Color Audit

- `CURRENT / VERIFIED` 所有五個 Shared Art materials 使用 URP Lit shader GUID `933532a4fcc9baf4fa0491de14d08ed7`；repository 內無自訂 `.shader`／`.shadergraph`。
- Infantry Base material連接 BaseColor 與 Normal；ORM 與 TeamColorMask 只存在／匯入，沒有連到 material。
- Archer Base、TeamColor、Arrow materials 都沒有 texture references，使用 constant `_BaseColor`。
- Team Color 不是 mask shader：`PrototypeUnitArtView` 只對 material name 包含 `TeamColor` 的 slot 寫 `_BaseColor`／`_Color`。此作法可用，但不可宣稱已實作單一材質的 Team Color Mask pipeline。

## 現有相關規格文件

| File | Purpose／Current Relevance | Conflict／Missing | Recommendation |
| --- | --- | --- | --- |
| `docs/50_AI_Art_Pipeline.md` | source/runtime 分流總則 | Provenance 欄位不足 | 保留；由本包 `14` 擴充 |
| `docs/51_Art_Bible_Template.md` | Art Bible 骨架 | 尚無正式 faction Art Bible | G01/G02 後填寫，不冒充 CURRENT |
| `docs/ArtSpecs/00`～`08` | Prototype 技術、視覺、camera、AI delivery | L2 定義、低模 budget、1K texture 與新 production standard 衝突 | 保留作 Legacy／Prototype；依 Migration 分流 |
| `docs/ArtSpecs/Unit_01_指揮官.md` | Commander 個別規格 | 無資產、正式世界觀 TBD | 只做 backlog／concept |
| `docs/ArtSpecs/Unit_02_副官武將.md` | Lieutenant 個別規格 | 無資產 | 只做 backlog／concept |
| `docs/ArtSpecs/Unit_03_步兵.md` | Infantry 摘要 | 把 Prototype checkbox 與 production 混用 | 以 `08`／`09` 作新 gate |
| `docs/ArtSpecs/Unit_03_步兵_L3骨架動畫交付規格.md` | v002 技術 contract | 2.5–6K budget、`.blend` actions 宣告與實檔衝突 | 保留 stable runtime contract；美術品質升級依 Migration |
| `docs/ArtSpecs/Unit_04_弓兵.md` | Archer 摘要 | 無 L1/L2，且 final texture 不存在 | 先補 L1/L2，再 remaster L3 |
| `docs/ArtSpecs/Unit_04_弓兵_L3實作交付與驗收.md` | Archer prototype evidence | 明確不是 production-ready | 保留為 baseline evidence |
| `docs/ArtSpecs/Unit_05_騎兵.md` | Cavalry backlog spec | Content 有 ID但資產缺失 | Golden Sample lock 後才進 L3 |
| `docs/ArtSpecs/Unit_06_攻城兵器.md` | Light ram backlog spec | 目前只有 placeholder | 視為 Tier S／Vehicle backlog |
| `docs/ArtSpecs/Building_01`～`07` | 七種 building／settlement 個別規格 | 全部缺 production assets；footprint 部分仍待 gameplay grid | 使用本包 `12` gate；不先量產 |
| `DevelopmentProgress.md` | 唯一實際歷史與測試證據 | 不是資產規格 | 所有狀態引用必須以其為 evidence |

## Audit 結論

Infantry／Archer 已證明 Source→FBX→Humanoid→Animator→Prefab→Gameplay 的技術路徑，但無法證明 Production Art 品質。兩者應 Preserve stable IDs、sockets、event／Animator contracts、source/runtime 分流與 reusable builders；Remaster 應優先改善 L2 production reference、primary silhouette、surface、smooth deformation、animation與 provenance，而不是重寫 gameplay 或無條件砍掉全部技術成果。

