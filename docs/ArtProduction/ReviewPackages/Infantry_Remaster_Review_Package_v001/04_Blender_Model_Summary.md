# 04 — Blender Model Summary

```text
Primary Blender: CHR_Infantry_A_v002.blend
Original Path: ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/CHR_Infantry_A_v002.blend
Copied Path: L3_Source/CHR_Infantry_A_v002.blend
File Size: 231,897 bytes
Modified Date: 2026-08-13 11:51:11 local
Opened With: Blender 5.2.0 LTS
Saved By Review Script: NO
```

## 1. Scene totals

| Metric | Result |
|---|---:|
| Objects | 23 |
| Meshes | 12 |
| Armatures | 1 (`Armature`) |
| Materials | 3 (`MAT_Infantry_Base`, `MAT_Infantry_TeamColor`, `Material`) |
| Actions | **0** |
| Bones | 23 |
| Total vertices, all LOD/equipment objects | 3,635 |
| Total triangles, all LOD/equipment objects | 6,430 |
| LOD0 vertices／triangles | 2,400／4,376 |
| UV sets | 1 per mesh |
| Shape keys | 0 |
| Character height | 1.8300 m in Blender Z-up bounds |
| LOD0 A-pose/equipment bounds | 2.4016 × 0.5000 × 1.8300 m (X/Y/Z) |

The 285 globally “unweighted” vertices belong to rigid sword/shield objects parented directly to hand bones. Every Base/Team skinned-body vertex across all LODs is weighted; maximum body influence count is 1.

## 2. Per-LOD geometry

| LOD | Body Base | Body Team | Shield | Sword | Total triangles | Vertices |
|---|---:|---:|---:|---:|---:|---:|
| LOD0 | 4,044 | 112 | 104 | 116 | 4,376 | 2,400 |
| LOD1 | 1,228 | 88 | 80 | 116 | 1,512 | 860 |
| LOD2 | 442 | 30 | 28 | 42 | 542 | 375 |

Each LOD uses four mesh objects, two unique materials and five material slots across those objects. There is no LOD3 or impostor in the source.

## 3. Object roles

```text
Main Character Mesh:
  SK_Infantry_A_LOD0_Base
  SK_Infantry_A_LOD0_Team

Helmet: part of Base body mesh; not a separate object
Shoulder Armor: part of Base/Team body meshes
Chest Armor: part of Base body mesh
Waist Armor: part of Base/Team body meshes
Bracer: part of Base body mesh
Leg Armor: part of Base body mesh
Boots: part of Base body mesh
Shield: SM_Infantry_Shield_LOD0/1/2, separate rigid objects, LeftHand parent bone
Sword: SM_Infantry_Sword_LOD0/1/2, separate rigid objects, RightHand parent bone
```

The armour is visually modular in form but mostly fused into the body meshes. A remaster cannot assume helmet/shoulders/chest/waist are swappable modules without rebuilding boundaries.

## 4. Rig／skeleton

```text
Armature Name: Armature
Root: Root
Pelvis: Hips
Spine: Spine → Chest → UpperChest
Neck/Head: Neck → Head
Arms: Left/Right Shoulder → UpperArm → LowerArm → Hand
Legs: Left/Right UpperLeg → LowerLeg → Foot → Toes
```

Bone count is 23. The hierarchy is listed in `Manifests/Blender_Bone_Manifest.csv`. Unity importer is Humanoid/Create From This Model; historical reports say valid, but this task did not query Unity.

### Skinning

- Body Base／Team：100% weighted, maximum 1 influence per vertex.
- Sword／shield：0 vertex groups by design; rigid objects use bone parenting.
- Shape keys：none.
- Review concern：single-influence construction limits natural shoulder、elbow、hip and knee deformation. No extreme-pose review was generated because saved Actions are absent.

## 5. Modifiers and source images

- Skinned body meshes carry Armature modifiers; rigid equipment is bone-parented.
- Six packed images are stored in the `.blend` plus two named external image references.
- Named external references are `T_Infantry_A_BaseColor_1K.png` and `T_Infantry_A_TeamColorMask_1K.png`; the relative `../Textures/` path resolves inside this review package.
- Blender source does not prove that Unity's Normal/ORM files are connected in the DCC material graph.

## 6. Equipment and sockets

| Item | Parent | Independence | Review note |
|---|---|---|---|
| Sword meshes | `Armature`, parent type `BONE`, bone `RightHand` | Separate object per LOD | replaceable only with an approved pivot/socket contract; no separate Prefab |
| Shield meshes | `Armature`, bone `LeftHand` | Separate object per LOD | shield uses two materials to isolate team panel |
| `Socket_R_Hand` | RightHand | Empty | current attachment anchor |
| `Socket_L_Hand` | LeftHand | Empty | current attachment anchor |
| `Socket_WeaponTip` | RightHand | Empty | weapon-tip timing/effect anchor |
| `FX_Hit_Center` | Chest | Empty | hit presentation anchor |
| `FX_Foot_L/R` | feet | Empty | footstep presentation anchors |

## 7. Actions conflict

The source build script creates five Actions, assigns each for export, then clears `arm.animation_data.action`. On reopening the saved `.blend`, `bpy.data.actions` is empty. The current animation FBXs are therefore usable runtime exports, but the `.blend` alone does not retain editable actions.

Status：`REBUILD SCRIPT PRESENT / SAVED ACTIONS NOT FOUND / NEEDS TECHNICAL-ART DECISION`。

## 8. Generated visual evidence

The review script opened **the package copy**, hid LOD1/2, rendered LOD0 with neutral lights, then generated Clay and wireframe views in memory. It did not call any Blender save operation.

- Actual-material views：`Screenshots/Blender/01_Front.png` through `06_ThreeQuarter_Back.png`。
- Clay：`Clay_Front`, `Clay_Side`, `Clay_Back`, `Clay_3Q`。
- Wireframe：`Screenshots/Wireframe/Wireframe_Front.png`, `Wireframe_3Q.png`。

Raw machine data：`Manifests/Blender_Technical_Summary.json`, `Blender_Object_Manifest.csv`, `Blender_Bone_Manifest.csv`。

## 9. Preliminary remaster implications—not an approval

- Preserve：overall 1.83 m scale, sword/shield class cue, simple readable value blocks, stable skeleton/socket names unless L2 proves a need to change.
- Modify／partial rebuild candidate：primary/secondary form hierarchy, armour thickness, shoulder/hip deformation, equipment pivots, fused modular boundaries, production UV/material treatment.
- Rebuild trigger：approved L2 silhouette materially diverges, provenance blocks reuse, or production deformation cannot be achieved without replacing body topology.
- Do not choose whole-model rebuild solely because current triangle count is low; make the decision after formal L2 and normal-distance Unity evidence.
