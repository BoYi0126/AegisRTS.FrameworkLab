# 02 — Sword Attachment Transform Report

## Resulting chain

```text
RightHand (existing Humanoid bone)
└─ Socket_R_Hand (new non-deforming helper bone)
   └─ WPN_SwordRoot_R (Empty/exported transform)
      └─ seven existing sword meshes
```

The task calls this role `WeaponSocket_R`; repository standards in `07_Rig_Skinning_Animation_Standard.md`, `13_Asset_Naming_and_Folder_Standard.md`, and the existing Infantry L3 contract define the equivalent name as `Socket_R_Hand`. No parallel synonym was created.

## Key transforms

- RightHand head/tail: `(0.781617, 0, 1.079597)` / `(0.874216, 0, 1.029089)` m.
- Socket head/tail: `(0.827978, -0.049994, 1.044123)` / `(0.880652, -0.049994, 1.015392)` m.
- SwordRoot world origin: `(0.827978, -0.049995, 1.044123)` m, at the grip-center anchor.
- SwordRoot local scale: approximately `(1,1,1)`; Unity and all sword-child scales validate as unit scale.
- Seven sword meshes retained their exact world bounds and geometry fingerprints during reparenting.

## Follow test

Blender source audit rotates `RightHand` ±15°:

- Neutral SwordRoot: `(0.827978, -0.049995, 1.044123)` m.
- Up: `(0.821928, -0.045977, 1.033031)` m; delta 0.013258 m.
- Down: `(0.834320, -0.050605, 1.055750)` m; delta 0.013258 m.

The recorded matrices also change orientation, proving transform inheritance rather than a pose-specific visual offset. Full precision is in `Measurements/Sword_Attachment_After.json` and `Manifests/Geometry_Hierarchy_Follow_Summary.json`.
