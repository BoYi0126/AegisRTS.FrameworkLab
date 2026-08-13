# 03 — FBX Attachment Validation

Two review-only FBXs were exported with Blender 5.2.0 LTS:

- A-Pose: 784,156 bytes, SHA-256 `2D198FDEDAB7F722885B17F4DE332E004AA8A82F2E70A6751E1F4B8BF5C7020F`.
- L1 Pose: 789,660 bytes, SHA-256 `970A66F1ABA47E841CD445F24D88C3799128E18D282C81832C99B0AB799136B0`.

Each was imported into a clean Blender factory scene. Both returned:

- 1 armature, 98 meshes, 16,858 vertices, 33,248 triangles, 24 bones.
- `Socket_R_Hand` exists and its parent is `RightHand`.
- `WPN_SwordRoot_R` is bone-parented to `Socket_R_Hand`.
- All seven sword parts are direct children of `WPN_SwordRoot_R`.

FBX does not round-trip Blender's `use_deform` flag, so `Bone_Manifest.csv` and the source `.blend` are authoritative for the socket's non-deforming status. Unity Humanoid validity provides the downstream rig gate.

The L1 FBX is a review-only static comparison: its arm bones are baked as the FBX rest pose so rigid Phase 03.5 visual meshes and the socket agree after import. It is not a gameplay animation clip and does not replace the source A-Pose contract.
