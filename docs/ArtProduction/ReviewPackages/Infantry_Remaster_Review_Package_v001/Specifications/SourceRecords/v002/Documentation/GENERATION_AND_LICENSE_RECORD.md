# Generation and license record

## Asset

- Asset ID: `unit.infantry`
- Source asset: `CHR_Infantry_A_v001`
- Target asset: `CHR_Infantry_A_v002`
- Delivery stage: corrected L3 build and Unity prototype integration

## Corrected prompt

```text
請基於 `CHR_Infantry_A_v001` 補交 L3。
需要可編輯 `.blend` 原始檔及 Unity Humanoid 相容 FBX。角色必須為 A-Pose 或 T-Pose，盾牌與短劍分離並可掛到左右手骨骼。
動畫包含 Idle、Move、Attack_A、Hit、Death，全部 In Place、Root Motion 關閉。
Attack_A 需要 `AttackImpact` 事件時間。
另提供單一灰階 Team Color Mask，不要再輸出藍紅兩套重複網格。
請附生成工具、版本、完整 Prompt、Seed／Job ID、人工修改、第三方素材及商用授權紀錄。
```

## Tools and versions

- Original planning/build-script generation record: OpenAI ChatGPT, GPT-5.6 Sol.
- Corrective implementation and Unity integration: OpenAI Codex.
- DCC execution: Blender 5.2.0 LTS.
- Game-engine validation: Unity 6000.5.7f1.
- FBX export: Blender bundled FBX exporter.
- Seed / Job ID: `N/A - deterministic procedural rig/animation conversion; no generative 3D service was invoked for this L3 stage.`

## Input files

- `CHR_Infantry_A_v001_LOD0_Blue.glb`
- `CHR_Infantry_A_v001_LOD1_Blue.glb`
- `T_Infantry_A_BaseColor_1K.png`
- `T_Infantry_A_Normal_1K.png`
- `T_Infantry_A_ORM_1K.png`
- L1 concept reference and L2 delivery report

The red v001 GLB is retained as an audit input but is intentionally not used as an L3 runtime geometry source.

## Applied modifications

1. Imported the existing v001 LOD0 and LOD1 geometry without redesigning the character.
2. Raised the existing arm components into an A-Pose.
3. Built a Unity Humanoid-compatible hierarchy using standard Root/Hips/spine/limb naming.
4. Rigid-bound the existing low-poly component meshes to corresponding deformation bones.
5. Kept shield and sword as separate rigid objects, bone-parented to `LeftHand` and `RightHand`.
6. Derived LOD2 from LOD1 with deterministic decimation; final LOD counts are 4376/1512/542 triangles.
7. Added stable gameplay anchors and hand/head/foot/effect sockets.
8. Authored Idle, Move, Attack_A, Hit, and Death actions at 30 FPS.
9. Kept Root unkeyed for every action; Unity Animator Root Motion is disabled.
10. Added Footstep_L/R, AttackImpact, and DeathSettled event records.
11. Saved the editable Blender scene and exported one master plus five separate animation FBXs.
12. Replaced duplicated team variants with one grayscale Team Color Mask and runtime material tinting.
13. Configured a valid Unity Humanoid Avatar, shared animation Avatar, Animator Controller, LODGroup, prefab, and presentation-only animation bridge.
14. Rebuilt Move as a 0–24 grounded stride after actual gameplay capture exposed a stiff, low-amplitude loop; added contact/passing poses, foot and knee articulation, torso counter-twist, and restrained weapon-arm motion.
15. Synchronized Move playback rate to authoritative world velocity while keeping Root Motion disabled, then validated the result through the real playable-scene move command and close-zoom frame capture.
16. Preserved the original v001 LOD0/LOD1 triangle counts and palette UVs; no simplification or redesign was applied to the close-view LOD0.
17. Corrected the Unity visual basis from Blender Z-up/-Y-forward to Unity Y-up/Z-forward and aligned the combined renderer bounds to gameplay ground Y=0.
18. Scoped MaterialPropertyBlock team tint to TeamColor material slots so shield wood/metal Base colors are not overwritten.
19. Added upright bounds, vertical envelope, material isolation, front/side/back, and four-phase Move-pose validation.

## Corrections made to the received build source

- Adapted action handling for Blender 5, where the previous direct `Action.fcurves` assumption failed.
- Implemented the documented but previously missing LOD2 build.
- Preserved the shield team-color material slot during mesh joining.
- Added gameplay anchors/sockets and exported them with the master model.
- Exported each animation with its own action frame range.
- Added deterministic build metadata and package checksum generation.
- Corrected the mojibake prompt record to UTF-8.

## Third-party assets

No new third-party model, animation, texture, mocap clip, logo, or IP asset was added during this corrected L3 build.

The result is derived from project-supplied `CHR_Infantry_A_v001`. The project owner must retain the provenance and commercial-use rights record for that source. Blender and Unity are tools and are not embedded as redistributable game assets.

## Commercial license status

- New third-party assets introduced at L3: `None`.
- Original v001 source rights: `Owner verification required before commercial release`.
- Technical prototype use: `Integrated and validated`.
- Commercial release: `Gated by the v001 provenance/rights record and final art approval`.
