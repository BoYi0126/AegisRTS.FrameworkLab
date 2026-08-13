# Unit_03 Infantry L3 corrected delivery status

## Status

`Integrated / technically validated for the playable prototype`

`Commercial release gate: original v001 provenance and commercial-use rights must remain documented.`

## Completed

- Blender 5.2.0 LTS generated an editable `.blend` file and real FBX files; no file was produced by renaming GLB extensions.
- The original `CHR_Infantry_A_v001` LOD0/LOD1 geometry remains the source.
- A-Pose Humanoid-style hierarchy includes Root, Hips, spine, head, arms, hands, legs, and feet.
- Shield and sword remain separate rigid objects attached to `LeftHand` and `RightHand`.
- Idle, Move, Attack_A, Hit, and Death clips are 30 FPS and In Place; Root is not keyed.
- LOD0/LOD1/LOD2 are 4376/1512/542 triangles.
- One grayscale Team Color Mask replaces duplicate blue/red runtime geometry.
- Unity 6000.5.7f1 imports the master Avatar as Humanoid, Human, and Valid.
- Separate animation FBXs copy the Avatar from the master model.
- Events are imported for footsteps, AttackImpact, and DeathSettled.
- The L3 prefab, Animator Controller, materials, LODGroup, anchors, collider, and animation view bridge are integrated.
- Batch Game View smoke validation and the targeted L3 PlayMode test pass.
- Move was rebuilt after an actual-scene capture exposed a stiff low-amplitude loop that appeared to slide. The corrected 0–24 clip uses alternating heel-contact and passing poses with knees, feet, hip/chest counter-twist, and controlled shield/sword motion.
- Runtime animation rate now follows authoritative movement speed, and 2.5–4 m close zoom supports visual model inspection without enabling camera rotation.
- A close-view review exposed that the first Unity prefab reset the FBX basis and displayed the Z-up character along the ground. The prefab now performs a deterministic basis conversion, snaps renderer bounds to Y=0, and rejects non-upright builds.
- Team tint is material-slot scoped, preserving the L2 brown shield and Base atlas instead of recoloring the full shield renderer. Friendly/enemy colors now use the specified #4AA3D8/#D94A45 values.
- A directional key light and 38° close-inspection pitch make helmet, face, lamellar chest, scarf, shield, sword, hands, and legs readable without increasing triangle count.

## Remaining gates

- The original v001 source rights/provenance are a project-owner release responsibility.
- The generated animations are deterministic prototype animations. The visible sliding defect is corrected, but professional hand-authored animation polish is still recommended before a final art-quality release.
