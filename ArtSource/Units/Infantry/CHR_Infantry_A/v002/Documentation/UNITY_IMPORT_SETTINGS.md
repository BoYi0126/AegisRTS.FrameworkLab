# Unity 6 import and Humanoid validation

These settings are applied by `AegisRTS.Editor.InfantryL3PrefabBuilder.BuildAndValidate` in Unity 6000.5.7f1.

## Master model

File: `Models/SK_Infantry_A_v002.fbx`

- Scale Factor: `1`
- Rig / Animation Type: `Humanoid`
- Avatar Definition: `Create From This Model`
- Avatar result: Human and Valid
- Material import: disabled; project URP materials are assigned by the prefab builder
- Root: top transform; Hips: humanoid pelvis
- Blender source basis: Z-up / -Y-forward; meter units; no leaf bones.
- The imported FBX visual is converted inside `PF_Unit_Infantry.prefab` with local rotation `X=-90°`, then offset so the combined renderer `bounds.min.y` rests on the gameplay root ground plane. Do not reset the visual child to identity rotation.
- Unity validation requires the combined standing bounds to use Y as the longest body axis, be at least `1.65 m` tall, and remain approximately within ground Y=`-0.08…1.95 m` before the prefab is accepted.

## Separate animation FBXs

| Clip | Frames | Loop | Events |
|---|---:|---:|---|
| `AN_Infantry_Idle` | 0-60 | Yes | None |
| `AN_Infantry_Move` | 0-24 | Yes | Footstep_L frame 1; Footstep_R frame 13 |
| `AN_Infantry_Attack_A` | 0-30 | No | AttackImpact frame 13 |
| `AN_Infantry_Hit` | 0-18 | No | None |
| `AN_Infantry_Death` | 0-38 | No | DeathSettled frame 35 |

All clips use:

- Animation Type: `Humanoid`
- Avatar Definition: `Copy From Other Avatar`
- Source Avatar: the master `SK_Infantry_A_v002` Avatar
- Root Transform Rotation / Position Y / Position XZ: baked into pose and based on original values
- Animation Compression: keyframe reduction

All clips are authored at 30 FPS. The Blender build never keys `Root`, and the runtime Animator has `Apply Root Motion` disabled.

## Animation event contract

- `Footstep_L` and `Footstep_R` are presentation hooks for audio/VFX.
- `AttackImpact` occurs at frame 13 / `0.4333333333 s` and is a visual timing signal only.
- `DeathSettled` occurs at frame 35 / `1.1666666667 s`.
- Gameplay damage remains authoritative in `CombatSystem`; UI or animation events do not apply damage.

## LOD and prefab

- LOD0: 4376 triangles, transition height 0.040
- LOD1: 1512 triangles, transition height 0.012
- LOD2: 542 triangles, transition height 0.003
- Culled below the final LOD transition
- Runtime prefab: `PF_Unit_Infantry.prefab`
- Animator Controller: `AC_Infantry.controller`
- Animator Root Motion: Off

## Team color

Use one geometry set and `Textures/T_Infantry_A_TeamColorMask_1K.png`:

- white: team-color contribution enabled
- black: no team-color contribution

The runtime applies team colors with MaterialPropertyBlock only to material slots whose material name contains `TeamColor`. The shield Base slot keeps its wood/metal colors; selection highlighting follows the same slot rule. Do not reintroduce blue/red duplicate FBX meshes or apply a renderer-wide tint.

## Validation

The editor builder rejects missing/invalid Humanoid Avatars, missing clips, wrong LOD triangle counts, missing AttackImpact, invalid standing bounds, or an invalid prefab. The PlayMode L3 tests additionally run Move, Attack_A, and Death, check their events, verify that the gameplay root does not move, sample Move at four phases, and confirm that only TeamColor material slots receive the faction tint.
