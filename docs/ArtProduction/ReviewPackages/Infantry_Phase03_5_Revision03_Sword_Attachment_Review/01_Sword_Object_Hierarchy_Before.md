# 01 — Sword Object Hierarchy Before

P035R2 contained exactly seven sword visual meshes:

| Object | Actual parent | Parent type | Metadata only |
|---|---|---|---|
| `Sword` | `Armature` | Object | `AttachmentBone=RightHand` |
| `Sword_Grip` | `Armature` | Object | `AttachmentBone=RightHand` |
| `Sword_Guard` | `Armature` | Object | `AttachmentBone=RightHand` |
| `Sword_Pommel` | `Armature` | Object | `AttachmentBone=RightHand` |
| `GEO_Infantry_Sword_GripContact` | `Armature` | Object | `AttachmentBone=RightHand` |
| `GEO_Infantry_Sword_BladeSpine` | `Armature` | Object | `AttachmentBone=RightHand` |
| `GEO_Infantry_Sword_GripWraps` | `Armature` | Object | `AttachmentBone=RightHand` |

There was no `Socket_R_Hand`, `WeaponSocket_R`, or SwordRoot node in P035R2. All objects had unit local scale and identity object transforms because mesh vertices were authored in armature/world coordinates. Consequently, the `AttachmentBone` custom property did not establish transform inheritance.

The exact pre-change records and world matrices are in `Measurements/Sword_Hierarchy_Before.json`.
