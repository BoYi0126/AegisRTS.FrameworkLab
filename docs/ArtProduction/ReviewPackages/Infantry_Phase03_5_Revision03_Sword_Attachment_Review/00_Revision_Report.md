# 00 — Revision Report

## Baseline

- Task: `mission/Infantry_Remaster_Phase03_5_Revision03_Sword_Attachment_Task.md`, 20,693 bytes, SHA-256 `898E6762E786D8301F597009AD140AE88C1712ADAF8E8B033CA031EDF7E7BE6A`.
- Input: `CHR_Infantry_A_v004_P035R2.blend`, SHA-256 `D8DCD84D888204D65385A94CF15B0C07BEA227236D47EA5EC3D54992999E551D`.
- Output: `CHR_Infantry_A_v004_P035R3.blend`, 412,646 bytes, SHA-256 `03DED168297B95C88E0289C3A91A34B1EB6CB640509A1D39816B2D1C12A40D46`.
- Tools: Blender 5.2.0 LTS; Unity 6000.5.7f1.

## Scope

In scope: audit every sword visual, establish a real right-hand socket hierarchy, preserve world transforms, test right-hand follow, export/reimport FBX, validate Unity Humanoid and prefab hierarchy, and package review evidence.

Out of scope and unchanged: body/arm/head/shield/sword geometry, shield transform, weapon scale, UV, texture, final skinning, gameplay animations, runtime prefab replacement, equipment-system implementation, Phase 04.

## Change

P035R2 had seven sword meshes parented to the Armature as ordinary object children. `AttachmentBone=RightHand` was metadata only, so moving the right arm did not move the sword. P035R3 adds one non-deforming `Socket_R_Hand` helper bone under `RightHand`, adds exported transform `WPN_SwordRoot_R`, and parents all seven sword visuals beneath the root with preserved world transforms.

`Socket_R_Hand` is the existing documented project API; a synonymous `WeaponSocket_R` was deliberately not added.

## Quantitative lock

- Character height: 1.824011 m before/after.
- Geometry: 98 meshes, 16,858 vertices, 33,248 triangles before/after.
- Topology: 0 non-manifold, 0 boundary, 0 loose, 0 zero-area.
- Bones: 23 → 24; the added bone is `Socket_R_Hand`, `use_deform=false` in source.
- Sword bounds unchanged: 0.345361 × 0.105496 × 1.061217 m.
- All 98 mesh fingerprints and all world bounds matched before/after.
- Grip center to hand center: `(0.000062, -0.049994, -0.010220)` m.

## Validation result

- Blender source hierarchy: valid.
- RightHand ±15° follow: valid; SwordRoot translation deltas 0.013258 m up/down and orientation changes are recorded.
- Clean Blender FBX reimport: A-Pose and L1 both retain 98 meshes, 24 bones, socket/root/part hierarchy.
- Unity: A-Pose and L1 Humanoid avatars both `isValid && isHuman`; both prefabs retain the full hierarchy and seven children.
- Unity visual captures: A-Pose grip, L1 grip, RTS normal, and ±15° follow produced with GPU batch rendering.

## Acceptance

- [x] P035R2 preserved; P035R3 created.
- [x] All sword parts audited and grouped.
- [x] Real RightHand → socket → SwordRoot hierarchy exists.
- [x] Grip contacts right-hand primitive in A-Pose and L1 review pose.
- [x] Right-hand follow tested up/down.
- [x] Geometry and scale locks pass.
- [x] FBX hierarchy survives clean reimport.
- [x] Unity Humanoid and hierarchy pass.
- [x] Runtime contract audited; no infantry-only runtime special case added.
- [x] Review package and ZIP verification prepared.
- [ ] Reviewer approval / formal PASS; agent does not self-approve.

Status: `READY FOR PHASE03_5 REVISION03 REVIEW`.
