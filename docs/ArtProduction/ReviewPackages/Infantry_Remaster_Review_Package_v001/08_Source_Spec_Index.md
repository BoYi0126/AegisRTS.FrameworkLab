# 08 — Source Specification Index

## 1. Precedence for review

1. Current repository architecture／Definition／runtime contracts.
2. Approved asset-specific decisions and current serialized asset evidence.
3. `Specifications/ProductionSpec/` as proposed Production Ready gates.
4. Legacy ArtSpecs and source delivery reports as historical intent/evidence.

Legacy `PASS` means a historical prototype gate passed; it does not mean current Production Ready.

## 2. Legacy ArtSpecs

| Document | Original path | Purpose／applies to Infantry | Status | Conflict／review note |
|---|---|---|---|---|
| `00_美術製作總覽與AI任務索引.md` | `docs/ArtSpecs/00_美術製作總覽與AI任務索引.md` | legacy art task overview | LEGACY/CURRENT CONTEXT | phase labels predate production pipeline |
| `01_技術規格_比例座標與輸出.md` | `docs/ArtSpecs/01_技術規格_比例座標與輸出.md` | units/axes/export | LEGACY BASELINE | retain scale/axes where verified |
| `03_視覺規格_風格材質與陣營色.md` | `docs/ArtSpecs/03_視覺規格_風格材質與陣營色.md` | style/material/team colour | LEGACY | current slot-based implementation differs from proposed mask shader |
| `05_AI交付格式與驗收清單.md` | `docs/ArtSpecs/05_AI交付格式與驗收清單.md` | delivery checklist | LEGACY | superseded for Production Ready by new checklist/provenance standard |
| `06_尺寸總表.md` | `docs/ArtSpecs/06_尺寸總表.md` | character/building scale | CURRENT REFERENCE | verify against actual 1.8300 m source |
| `07_製作批次與目前整合缺口.md` | `docs/ArtSpecs/07_製作批次與目前整合缺口.md` | backlog/integration gaps | LEGACY SNAPSHOT | current GS hold/backlog supersedes scheduling |
| `Unit_03_步兵.md` | `docs/ArtSpecs/Unit_03_步兵.md` | Infantry L1/L2 prototype role | LEGACY ASSET SPEC | `L2` means game model, not new production sheet |
| `Unit_03_步兵_L3骨架動畫交付規格.md` | `docs/ArtSpecs/Unit_03_步兵_L3骨架動畫交付規格.md` | v002 rig/animation delivery | CURRENT PROTOTYPE CONTRACT | production deformation/source-action gates remain open |

## 3. Production specification

| Document | Original path | Purpose／applies to Infantry | Status | Conflict／review note |
|---|---|---|---|---|
| `README.md` | `docs/ArtProduction/RTS_Asset_Production_Spec_v1/README.md` | package terms and GS hold | CURRENT PROPOSED STANDARD | current Infantry is candidate only |
| `01_Project_Asset_Audit.md` | same root + filename | repository/Infantry actual audit | CURRENT AUDIT | snapshot; revalidate before future work |
| `02_Asset_Pipeline_L1_L4.md` | same root + filename | formal L1→L4 gates | CURRENT | redefines L2 as production sheet |
| `03_Character_Production_Quality_Standard.md` | same root + filename | form/topology quality | PROPOSED | higher quality than current prototype |
| `04_RTS_Silhouette_and_Readability_Standard.md` | same root + filename | gameplay-distance readability | PROPOSED | needs standardized Unity captures |
| `05_LOD_and_Performance_Standard.md` | same root + filename | LOD0–3/impostor and measurement | PROPOSED | current only LOD0–2 |
| `06_Texture_Material_TeamColor_Standard.md` | same root + filename | texture/packed/team mask | PROPOSED | current ORM/mask unconnected |
| `07_Rig_Skinning_Animation_Standard.md` | same root + filename | skeleton/weights/actions/events | PROPOSED | current max influence 1 and 0 saved Actions |
| `08_Golden_Sample_Infantry_Archer.md` | same root + filename | Golden Sample lock | CURRENT GATE | `DO NOT MASS PRODUCE` |
| `09_Existing_Infantry_Archer_Remaster_Audit.md` | same root + filename | preserve/modify/rebuild audit | CURRENT AUDIT | recommends scoped partial remaster, not automatic rebuild |
| `13_Asset_Naming_and_Folder_Standard.md` | same root + filename | source/runtime/evidence paths | CURRENT | preserve stable runtime IDs/GUIDs during migration |
| `14_AI_Asset_Provenance_and_License_Standard.md` | same root + filename | rights/provenance registry | CURRENT | current source rights incomplete |
| `15_Unity_RTS_Asset_Acceptance_Checklist.md` | same root + filename | Unity/art/performance acceptance | CURRENT GATE | this collection does not certify it |
| `16_Master_Production_Checklist.md` | same root + filename | L1–L4 master checklist | CURRENT GATE | Infantry boxes remain incomplete |
| `17_Asset_Production_Backlog.md` | same root + filename | GS-first scheduling | CURRENT | remaster blocked by rights/L1/L2/shared foundation |
| `99_Open_Issues_and_Missing_Information.md` | same root + filename | P0–P3 blockers | CURRENT | aligns with this package missing-data list |
| `Legacy_Spec_Migration.md` | same root + filename | terminology/conflict migration | CURRENT | do not rename/delete current assets in-place |

All “same root” originals are fully preserved under `Specifications/ProductionSpec/`; the complete exact path remains in `Manifests/Source_Copy_Map.csv`.

## 4. Runtime contracts

| Document | Original path | Purpose | Status／conflict |
|---|---|---|---|
| `26_Framework_API_目標介面.md` | `docs/26_Framework_API_目標介面.md` | public runtime/presentation API context | CURRENT; art must not become gameplay truth |
| `37_PlayablePrototype_01_操作與驗收手冊.md` | `docs/37_PlayablePrototype_01_操作與驗收手冊.md` | operation and historical validation | CURRENT PROTOTYPE; captures need immutable evidence metadata |
| `38_PlayablePrototype_01_架構與維護.md` | `docs/38_PlayablePrototype_01_架構與維護.md` | presentation boundaries and maintenance | CURRENT; preserve Definition/Runtime/View separation |
| `45_Attack_Cadence_OrbWalking_攻速與取消後搖規範.md` | `docs/45_Attack_Cadence_OrbWalking_攻速與取消後搖規範.md` | attack timing/cancel semantics | CURRENT; animation timing may scale but cannot apply damage |

## 5. Source delivery records — v001

| Document | Original path | Purpose | Status／conflict |
|---|---|---|---|
| `ASSET_MANIFEST.md` | `ArtSource/.../v001/ASSET_MANIFEST.md` | v001 contents | LEGACY inventory |
| `DELIVERY_README.md` | `ArtSource/.../v001/Documentation/DELIVERY_README.md` | delivery overview | LEGACY |
| `L2_DELIVERY_REPORT.md` | `ArtSource/.../v001/Documentation/L2_DELIVERY_REPORT.md` | playable game-model delivery | LEGACY; not production L2 sheet |
| `L2_METRICS.json` | `ArtSource/.../v001/Documentation/L2_METRICS.json` | mesh/material metrics | LEGACY machine data |
| `Unity_Hierarchy.txt` | `ArtSource/.../v001/Documentation/Unity_Hierarchy.txt` | legacy hierarchy | LEGACY; current v002 Prefab supersedes |

## 6. Source delivery records — v002

| Document | Original path | Purpose | Status／conflict |
|---|---|---|---|
| `README.md` | `ArtSource/.../v002/README.md` | v002 source package overview | CURRENT PROTOTYPE |
| `ASSET_MANIFEST.md` | `ArtSource/.../v002/ASSET_MANIFEST.md` | source contents | CURRENT |
| `ANIMATION_EVENTS.json` | `ArtSource/.../v002/Documentation/ANIMATION_EVENTS.json` | event timing | CURRENT; agrees with importer events |
| `GENERATION_AND_LICENSE_RECORD.md` | `ArtSource/.../v002/Documentation/GENERATION_AND_LICENSE_RECORD.md` | generation/rights record | CURRENT BUT INCOMPLETE; v001 upstream rights unresolved |
| `L3_DELIVERY_REPORT.md` | `ArtSource/.../v002/Documentation/L3_DELIVERY_REPORT.md` | technical delivery and historical PASS | CURRENT PROTOTYPE; final art deferred; stored-Action claim conflicts with reopen |
| `MANIFEST.json` | `ArtSource/.../v002/Documentation/MANIFEST.json` | source hashes | CURRENT historical manifest |
| `PROMPT.txt` | `ArtSource/.../v002/Documentation/PROMPT.txt` | generation request | CURRENT provenance input |
| `STATUS.md` | `ArtSource/.../v002/Documentation/STATUS.md` | delivery status | CURRENT PROTOTYPE; not production approval |
| `UNITY_IMPORT_SETTINGS.md` | `ArtSource/.../v002/Documentation/UNITY_IMPORT_SETTINGS.md` | importer/clip guidance | PARTIALLY STALE; Idle/Attack/Hit ranges differ from actual meta/script |
| `Input_v001/L2_DELIVERY_REPORT.md` | `ArtSource/.../v002/Input_v001/L2_DELIVERY_REPORT.md` | copied v001 input report | LEGACY INPUT |

## 7. Collection task

`Specifications/Task/Infantry_Remaster_Review_Data_Collection_Task.md` is copied from `mission/Infantry_Remaster_Review_Data_Collection_Task.md`. It defines collection scope only and authorizes no production-asset repair.

## 8. Main conflicts Reviewer must resolve

1. Legacy `L2` semantics vs formal production L2 sheet.
2. Historical prototype triangle/texture targets vs proposed production quality tiers.
3. Current material-slot team color vs proposed packed-mask shader.
4. Documentation claiming stored Actions vs reopened `.blend` with zero Actions.
5. Actual Idle/Attack/Hit frame ranges vs stale import document rows.
6. Current East-Asian-inspired low-poly presentation vs unapproved broader Stylized Fantasy production direction.
7. Technical L3 prototype PASS vs unmet provenance, visual, deformation, screenshot and performance gates.
