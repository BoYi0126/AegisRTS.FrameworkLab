# 01 — Asset Inventory

> Exact per-file origin、copy path、size、modified time、status and notes：`Manifests/File_Manifest.csv`。  
> This document highlights review-significant assets and distinguishes `CURRENT` from `LEGACY`.

## 1. Current identity and binding

| Item | Original path | Copied path | Status | Evidence |
|---|---|---|---|---|
| Unit Definition | `Assets/AegisRTS/Content/PrototypeNeutral/ContentPack.json` | `Unity/ContentBinding/ContentPack.json` | CURRENT | `unit.infantry` binds `PF_Unit_Infantry` |
| Runtime Prefab | `Assets/AegisRTS/Content/Shared/Art/Units/Infantry/Resources/AegisRTS/Units/Infantry/PF_Unit_Infantry.prefab` | `Unity/Prefabs/PF_Unit_Infantry.prefab` | CURRENT | Resource path and Prefab YAML |
| Editable DCC | `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/CHR_Infantry_A_v002.blend` | `L3_Source/CHR_Infantry_A_v002.blend` | CURRENT | Corresponds to v002 master; opened in Blender 5.2 |
| Master source export | `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Models/SK_Infantry_A_v002.fbx` | `Runtime_Models/Current_SourceExport/SK_Infantry_A_v002.fbx` | CURRENT SOURCE EXPORT | Hash equals Unity copy |
| Unity master/import | `Assets/.../Infantry/Models/SK_Infantry_A_v002.fbx[.meta]` | `Unity/ModelImports/Current/` | CURRENT | Prefab mesh/avatar GUID `15fa862...` |
| Animator | `Assets/.../Infantry/Animations/AC_Infantry.controller[.meta]` | `Unity/Animator/` | CURRENT | Prefab controller GUID `013fcea...` |

## 2. L1 Concept inventory

| File | Original path | Copied path | Resolution／format | Likely role | Verification |
|---|---|---|---|---|---|
| `Unit_03_Infantry_L1_Concept_Final.png` | `ArtSource/.../v001/Concepts/` | `L1_Concept/v001/` | 1254×1254, 8-bit RGB PNG | Legacy final concept／silhouette／team variants | VERIFIED by PNG header and source docs |
| `Unit_03_Infantry_L1_Concept_Alternate.png` | `ArtSource/.../v001/Concepts/` | `L1_Concept/v001/` | 1254×1254, 8-bit RGB PNG | Alternate concept | VERIFIED |
| `Unit_03_Infantry_L1_Concept_Final.png` | `ArtSource/.../v002/Reference/` | `L1_Concept/v002_LinkedReference/` | 1254×1254, 8-bit RGB PNG | Physical concept reference used by v002 source package | VERIFIED; duplicate role, retained for lineage |

The L1 sheet contains multiple presentation views and blue/red treatments, but it is not an orthographic neutral A-pose production construction sheet.

## 3. L2 Reference inventory

| File | Original path | Copied path | Role | Status |
|---|---|---|---|---|
| `PREVIEW_Dimensions_Front.png` | `ArtSource/.../v001/Previews/Dimensions/` | `L2_Reference/v001/` | Front dimension/supporting preview, 1400×1000 | LEGACY SUPPORTING |
| `UV_Infantry_A_Base_LOD0.png` | `ArtSource/.../v001/UV/` | `L2_Reference/v001/` | Base UV layout, 1024×1024 | LEGACY SUPPORTING |
| `UV_Infantry_A_TeamColor_LOD0.png` | `ArtSource/.../v001/UV/` | `L2_Reference/v001/` | Team region UV layout, 1024×1024 | LEGACY SUPPORTING |

`L2 Production Character Sheet: NOT FOUND.` Legacy filenames/reports calling v001 a `L2 game model` are retained under `Specifications/SourceRecords/v001/` but not relabeled as a production sheet.

## 4. L3 source and models

| Asset | Package location | Size | Status／role |
|---|---|---:|---|
| `CHR_Infantry_A_v002.blend` | `L3_Source/` | 231,897 bytes | CURRENT editable source; 0 saved Actions on reopen |
| `BUILD_RESULT.json` | `L3_Source/` | see manifest | CURRENT build metrics |
| `SK_Infantry_A_v002.fbx` | `Runtime_Models/Current_SourceExport/` | 1,282,204 bytes | CURRENT master source export |
| v001 blue/red LOD0/1 GLBs (4) | `Runtime_Models/Legacy_v001/` | see manifest | LEGACY comparison models |
| v001 blue LOD0/1 GLBs (2) | `Runtime_Models/v002_Input_v001/` | see manifest | Inputs used by v002 build |
| v001 Unity GLB + metas (2×2) | `Unity/ModelImports/Legacy/` | see manifest | LEGACY runtime imports, not current Prefab reference |

## 5. Unity runtime inventory

| Category | Files | Current use |
|---|---|---|
| Prefab | `PF_Unit_Infantry.prefab` + meta | Current resource prefab |
| Master model | `SK_Infantry_A_v002.fbx` + meta | Current Humanoid avatar and all mesh references |
| Animator | `AC_Infantry.controller` + meta | Current state machine |
| Materials | `MAT_Infantry_Base.mat`, `MAT_Infantry_TeamColor.mat` + metas | Current, two unique materials |
| Textures | BaseColor、Normal、ORM、TeamColorMask + metas | Files current; material-use status differs |
| Animation | 5 source FBX + 5 Unity FBX/metas | Current separate Humanoid clips |
| Legacy meshes | 4 `.asset` + metas | Legacy v001 static extracted meshes; retained for comparison only |

## 6. Rig, equipment and anchors

| Item | Blender object／bone | Unity status | Review status |
|---|---|---|---|
| Main body | `SK_Infantry_A_LOD{0,1,2}_Base/Team` | Six SkinnedMeshRenderers | CURRENT |
| Sword | `SM_Infantry_Sword_LOD{0,1,2}` parented to `RightHand` | Three MeshRenderers／MeshFilters | Independent rigid objects per LOD; no separate weapon Prefab |
| Shield | `SM_Infantry_Shield_LOD{0,1,2}` parented to `LeftHand` | Three MeshRenderers／MeshFilters | Independent rigid objects per LOD; Base + TeamColor slots; no separate shield Prefab |
| Socket | `Socket_R_Hand`, `Socket_L_Hand`, `Socket_WeaponTip`, `Socket_Head` | Present as transforms | CURRENT |
| FX anchors | `FX_Hit_Center`, `FX_Foot_L`, `FX_Foot_R` | Present | CURRENT |
| Gameplay anchors | `SelectionAnchor`, `HealthBarAnchor`, `GroundContact` | Present; health Y=2.1, selection Y=0.02 | CURRENT |

Sword／shield pivots are the imported object origins attached through their hand bones. Unity LOD0 local positions are approximately sword `(0.1874, 0.8602, -0.3436)` and shield `(-0.1874, 0.8602, -0.3436)` within the imported hierarchy. Suitability for a future interchangeable equipment system is not visually certified.

## 7. Scripts and specifications

- Source build：`Scripts/SourceBuild/build_unit03_l3_blender.py`, `BUILD_WINDOWS.bat`。
- Current Unity integration：`InfantryL3PrefabBuilder.cs`, `PrototypeUnitArtCatalog.cs`, `PrototypeUnitArtView.cs`, `PrototypeUnitAnimatorView.cs`, bootstrap, smoke validator and relevant PlayMode test file。
- Legacy builder/validator：`InfantryArtPrefabBuilder.cs`, `InfantryL2Validator.cs`。
- Specifications：40 copied Markdown documents before this package's authored reports; see `08_Source_Spec_Index.md` and `Specifications/`.

## 8. Excluded on purpose

- Entire Unity project、scenes、assemblies、packages and caches。
- Archer or other unit art except references inside shared standards/code files。
- Build executables／DLLs／logs。
- No copied custom shader because current material uses package-provided URP Lit and no Infantry custom shader was found。
