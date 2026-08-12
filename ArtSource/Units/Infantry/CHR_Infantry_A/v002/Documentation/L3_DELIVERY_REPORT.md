# Unit_03 Infantry corrected L3 delivery report

## Contract

- Source: existing `CHR_Infantry_A_v001` L2 geometry; no character redesign.
- Target: `CHR_Infantry_A_v002`.
- Editable source: Blender `.blend`.
- Runtime models: Unity Humanoid-compatible FBX.
- Rest pose: A-Pose.
- Weapons: shield and sword are separate rigid objects attached to left/right hands.
- Clips: Idle, Move, Attack_A, Hit, Death at 30 FPS.
- Motion: In Place; Root bone unkeyed; Animator Root Motion disabled.
- Team color: one grayscale mask and one geometry set.

## Physical outputs

| Output | Result |
|---|---|
| `Source/CHR_Infantry_A_v002.blend` | Generated with Blender 5.2.0 LTS |
| `Models/SK_Infantry_A_v002.fbx` | Generated and imported as Valid Humanoid Avatar |
| `Animations/AN_Infantry_Idle.fbx` | Generated; looping |
| `Animations/AN_Infantry_Move.fbx` | Generated; 0–24 looping grounded stride; Footstep_L/R at frames 1/13 |
| `Animations/AN_Infantry_Attack_A.fbx` | Generated; AttackImpact at frame 13 |
| `Animations/AN_Infantry_Hit.fbx` | Generated |
| `Animations/AN_Infantry_Death.fbx` | Generated; DeathSettled at frame 35 |
| `Textures/T_Infantry_A_TeamColorMask_1K.png` | Delivered and integrated |

## Geometry

| LOD | Triangles | Unity transition height |
|---|---:|---:|
| LOD0 | 4376 | 0.040 |
| LOD1 | 1512 | 0.012 |
| LOD2 | 542 | 0.003 |

The previous delivery only contained LOD0/LOD1. The corrected build derives LOD2 from LOD1 and keeps the result below the 600-triangle target.

## Unity integration

- Unity version: 6000.5.7f1.
- Master model Rig: Humanoid / Create From This Model.
- Avatar: `isHuman=True`, `isValid=True`.
- Separate clips: Humanoid / Copy From Other Avatar.
- Animator Controller: `AC_Infantry.controller`.
- Runtime prefab: `PF_Unit_Infantry.prefab`.
- Presentation bridge: `PrototypeUnitAnimatorView`; gameplay damage remains authoritative in `CombatSystem`.
- Animator `applyRootMotion=False`.
- Gameplay movement velocity drives the presentation-only `MoveRate`; the 4.5 m/s prototype speed uses a 1.8 playback rate so the stride does not visibly moonwalk against world translation.
- The FBX retains the Blender Z-up/-Y-forward source basis. The Unity prefab applies one deterministic -90° X visual-basis conversion, then offsets the imported renderer bounds so the feet are at gameplay Y=0. The prefab root remains at zero/identity/scale one.
- The builder rejects the asset unless combined Unity renderer bounds are upright on Y, at least 1.65 m tall, grounded, and within the 1.95 m vertical envelope; a green Humanoid Avatar alone is no longer accepted as orientation proof.
- Team Color is applied per material slot. The wood/metal Base slot on the shield remains unchanged while only its TeamColor panel receives #4AA3D8 or #D94A45.
- Runtime Base material uses the supplied palette atlas and correctly imported flat Normal reference; the prototype adds one low-cost Directional Light so L2 geometry facets and armor layers remain readable.
- The fixed RTS camera supports 2.5–40 m zoom. At 2.5–4 m it raises the focus pivot and lowers pitch to 38° for model inspection; yaw rotation remains locked.

## Validation evidence

- Blender build completed and recorded `BUILD_RESULT.json` with LOD counts and event timing.
- Unity editor builder validation: PASS.
- Playable-prototype Game View smoke validation: PASS; both team variants visible with anchors and LOD/team renderers.
- Targeted PlayMode test: 1 passed, 0 failed. It verifies the Avatar, Animator, LODGroup, Move footsteps, AttackImpact, DeathSettled, and unchanged gameplay root position.
- Actual-scene locomotion capture: the playable prototype issued its real move command, followed the moving infantry at 2.5 m zoom, and captured eight frames. The sequence verifies alternating contact/passing poses, forward-facing travel, visible displacement, and close-inspection framing.
- Detail regression capture: front, side, back, and four fixed Move phases verify upright Unity bounds, L2 base colors, isolated Team Color slots, key lighting, grounded poses, vertical envelope, and no VisualRoot planar drift.

## Acceptance status

`Technical L3 acceptance: Passed for prototype integration.`

`Final art/release acceptance: Deferred pending professional animation polish review and confirmation of the original v001 commercial-rights record.`
