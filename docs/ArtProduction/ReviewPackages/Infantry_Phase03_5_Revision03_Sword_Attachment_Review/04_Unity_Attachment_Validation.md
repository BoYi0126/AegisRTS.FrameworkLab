# 04 — Unity Attachment Validation

Unity version: 6000.5.7f1.

Review assets are isolated under `Assets/AegisRTS/Review/InfantryPhase035Revision03/`:

- `PF_Unit_Infantry_P035R3_Review.prefab`
- `PF_Unit_Infantry_P035R3_L1Pose_Review.prefab`
- `SCN_Infantry_P035R3_Review.unity`

Validation results:

- A-Pose Avatar: valid Humanoid (`isValid=true`, `isHuman=true`).
- L1 Avatar: valid Humanoid (`isValid=true`, `isHuman=true`).
- A-Pose/L1 height: 1.824011 m / 1.824011 m.
- Renderers: 98 / 98.
- Hierarchy valid in both: `RightHand/Socket_R_Hand/WPN_SwordRoot_R/[7 parts]`.
- SwordRoot and all seven child scales: unit scale.
- GPU batch capture completed successfully for A-Pose, L1 close, L1 RTS normal, grip close-ups, and RightHand +15°/-15° follow tests.
- Formal `PF_Unit_Infantry` runtime prefab was not changed.

The first headless `-nographics` capture produced blank frames due the renderer lacking constant-buffer support; those frames were replaced by a successful GPU batch rerun and are not claimed as evidence. The final captures in this package were visually inspected.

Machine-readable result: `Manifests/Unity_Attachment_Result.json`.
