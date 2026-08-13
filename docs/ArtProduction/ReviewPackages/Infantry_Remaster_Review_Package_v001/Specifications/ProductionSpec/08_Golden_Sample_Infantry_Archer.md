# 08 — Golden Sample: Infantry and Archer

- Specification Version：1.0
- Current Status：`GOLDEN_SAMPLE_CANDIDATE / NOT PRODUCTION_READY`

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
- [ ] Archer建立獨立L1；不能只是Infantry換弓。
- [ ] 兩者都有approved L2 production sheets、orthographic views、equipment breakdown與material callouts。
- [ ] 黑剪影在128／64／32 px blind test通過。

### L3 Visual

- [ ] LOD0符合對應Production quality；偏離20–35K有review證據。
- [ ] Primary／Secondary Forms完整；無primitive／blockout外觀。
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

