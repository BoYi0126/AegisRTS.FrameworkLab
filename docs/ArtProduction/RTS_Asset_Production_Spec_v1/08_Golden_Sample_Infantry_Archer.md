# 08 — Golden Sample: Infantry and Archer

- Specification Version：1.0
- Current Status：`GOLDEN_SAMPLE_CANDIDATE / NOT PRODUCTION_READY`

2026-08-13 Phase 03 update：使用者明確要求以 `CHR_Infantry_A_v003_P02R1` 直接執行Secondary Forms，故該immutable revision獲downstream authorization；既有P02R1 reviewer record未回寫。已建立 `CHR_Infantry_A_v004`（1.824 m／33,898 tris／106 meshes）與 `Infantry_Phase03_SecondaryForms_Review_v001/`。狀態僅為 `READY FOR PHASE03 REVIEW`；Unity preview、final topology／UV／texture／skin／animation／LOD、provenance、Golden Sample lock與Production Ready均未通過。

2026-08-13 Phase 03 Revision 01 update：依指定 change request 從 immutable `v004` 建立 `CHR_Infantry_A_v004_P03R1`（1.824 m／33,248 tris／98 meshes），修訂 waist cloth、scarf、upper-arm cloth、shield back 與 boots，並完成隔離 Unity 6000.5.7f1 Close／Normal／Far capture。狀態僅為 `READY FOR PHASE03 REVISION REVIEW`；18 項 gate 全數保留給 human reviewer，未授權 Phase 04 或 Golden Sample lock。

2026-08-13 Phase 03.5 update：任務明確指定P03R1為approved Secondary Forms baseline，已建立`CHR_Infantry_A_v004_P035`（1.824 m／33,248 tris／98 meshes／23 bones），完成L1 pixel landmarks、3D bone／mesh measurements、A-Pose與review-only L1 Compare Pose、controlled proportion correction及隔離Unity evidence。狀態僅為`READY FOR PHASE03_5 REVIEW`；19項gate全數待human review，未建立PRE-UV GEOMETRY LOCK或啟動Phase 04。

2026-08-13 Phase 03.5 Revision 01 update：Reviewer對P035提出Arm／Hand／Head focused change request；已建立`CHR_Infantry_A_v004_P035R1`（1.824 m／33,248 tris／98 meshes／23 bones），UpperArm／Forearm修至0.176H／0.165H、Hand width至0.06065H、Head width至0.120H，並重做posed landmarks、arm-focus overlays與隔離Unity A-Pose／L1Pose／RTS Normal evidence。狀態僅為`READY FOR PHASE03_5 REVISION REVIEW`；19項revision gate待human review，未建立PRE-UV GEOMETRY LOCK或啟動Phase 04。

## 目的

Infantry與Archer共同鎖定Production Pipeline。Infantry驗證Heavy Armor、Sword、Shield、Melee Animation、Heavy Silhouette、Hard Surface Armor、Team Color、Skinning；Archer驗證Light Armor、Bow、Arrow、Quiver、Projectile Spawn、Ranged Animation、Thin Silhouette、Arm Deformation。

## Current Evidence

| Gate | Infantry | Archer |
| --- | --- | --- |
| Source／Rebuild | Blender 5.2 `.blend`＋script＋FBX | Blender 5.2 `.blend`＋script＋FBX |
| Geometry | 4376／1512／542 tris | 3344／1280／542 tris |
| Rig | Valid Humanoid | Valid Humanoid |
| Animation | Idle／Move／Attack_A／Hit／Death | Idle／Move／Attack_Ranged／Hit／Death |
| L4 | `PF_Unit_Infantry` | `PF_Unit_Archer`＋Arrow Prefab |
| Team Color | slot-based MPB | slot-based MPB |
| Runtime | 可移動／近戰／Attack-Move | 可移動／射擊／pooled arrow／impact |
| Release | Blocked | Blocked |

## Golden Sample Required Gates

### Design／L1／L2

- [ ] Infantry L1更新或確認符合production visual target。
- [x] Infantry Phase 01 asset-specific L2 construction target與review checklist已存在。
- [x] Infantry Phase 01 target已由使用者批准，並建立versioned v003 Primary Forms candidate。
- [x] Infantry v003 initial change request已形成versioned P02R1 source與review evidence；reviewer decision仍pending。
- [x] 使用者以直接執行Phase 03的指示授權P02R1作為Secondary Forms baseline；`CHR_Infantry_A_v004`與完整DCC review evidence存在。
- [x] Phase 03 Revision 01 的versioned source、指定部位comparison、Unity RTS preview與verified review package存在；human revision gate仍pending。
- [x] Phase 03.5 versioned source、landmark diagnosis、A-Pose／L1Pose overlays、Unity evidence與verified package存在；human proportion gate仍pending。
- [x] Phase 03.5 Revision 01 versioned source、實際arm rest-length correction、posed landmark before／after、arm-focus／Unity evidence與verified package存在；human revision gate仍pending。
- [ ] Archer建立獨立L1；不能只是Infantry換弓。
- [ ] 兩者都有approved L2 production sheets、orthographic views、equipment breakdown與material callouts。
- [ ] 黑剪影在128／64／32 px blind test通過。

### L3 Visual

- [ ] LOD0符合對應Production quality；偏離20–35K有review證據。
- [ ] Primary／Secondary Forms完成agent-side候選；Phase 03 human review仍pending。
- [ ] 2K production texture與材質分離通過。
- [ ] Infantry shield／armor／sword重量感；Archer bow／quiver／arm draw path清楚。
- [ ] Skinning extreme poses與animation polish人工通過。

### Technical／L4

- [x] Stable Prefab IDs、Humanoid、Root Motion Off、LODGroup、anchors。
- [x] Gameplay／View authority分離。
- [x] Infantry `AttackImpact`、Archer `ProjectileRelease`與arrow Z+ contract。
- [ ] LOD3／pixel-calibrated switches、production material shader、target hardware profile。
- [ ] DCC neutral/game-like五視圖與Unity Close／Medium／Normal／Far captures存入repository source record。

### Provenance／Release

- [ ] 原始Infantry v001來源與commercial rights確認。
- [ ] Archer derivative rights跟隨來源並有明確commercial decision。
- [ ] Tool、Version、Model、Prompt、Negative Prompt、Seed、Job ID、Date、Human Modification、Inputs、Third-party、License、Commercial Use全部有值或`UNKNOWN`。

## Mass Production Gate

> **DO NOT MASS PRODUCE**

在上列兩個Golden Sample都達到`PRODUCTION_READY`前，Spearman、Heavy Infantry、Cavalry、Mage、Elite、Special、Hero與Building只可做Concept、Backlog、Design或單一技術spike；禁止批次製作Production L3。任何例外需記錄目的、不可重用風險與owner approval。

## Lock 後可重用項

- Skeleton Families與retarget tests。
- Material／Team Color shader與channel packing。
- Prefab hierarchy、Animator parameters、socket naming。
- LOD／pixel acceptance、DCC render templates。
- Provenance template、manifest、checklist與Unity builder validation。
