# Infantry Phase 03.5 Revision 03 — Sword Attachment Review

Status: `READY FOR PHASE03_5 REVISION03 REVIEW`

This package repairs the P035R2 sword attachment contract without changing body, arm, head, shield, or sword geometry. The repository-standard socket name `Socket_R_Hand` is used as the task-equivalent of `WeaponSocket_R`.

Validated hierarchy:

```text
RightHand
└─ Socket_R_Hand (24th bone, non-deforming in Blender source)
   └─ WPN_SwordRoot_R
      ├─ Sword
      ├─ Sword_Grip
      ├─ Sword_Guard
      ├─ Sword_Pommel
      ├─ GEO_Infantry_Sword_GripContact
      ├─ GEO_Infantry_Sword_BladeSpine
      └─ GEO_Infantry_Sword_GripWraps
```

Start with `00_Revision_Report.md`, then review the transform, FBX, Unity, and runtime-contract reports. Machine-readable evidence is under `Measurements/` and `Manifests/`.

No runtime prefab was replaced. No equipment system was introduced. Phase 04, final UV, final texture, final skinning, and animation polish were not started.
