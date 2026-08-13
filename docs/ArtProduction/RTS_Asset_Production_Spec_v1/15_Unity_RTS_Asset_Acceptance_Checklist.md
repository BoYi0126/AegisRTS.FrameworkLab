# 15 — Unity RTS Asset Acceptance Checklist

> Version: 1.0  
> Status: Approved specification; no asset is approved by this document alone  
> Applies to: every character, creature, vehicle, projectile, prop and building submitted as an RTS production asset

## 1. Acceptance rule

An asset is **Production Ready** only when every applicable mandatory item below is checked, evidence is stored in the repository, and the owner/reviewer/date are recorded. A successful import is necessary but not sufficient. A prototype may remain playable while failing production-art acceptance.

Allowed outcomes:

- `PASS`: requirement is demonstrated by reproducible evidence.
- `CONDITIONAL PASS`: usable only for the named milestone and has a dated remediation item.
- `FAIL`: blocks Production Ready.
- `N/A`: reviewer records why the requirement does not apply.
- `CANNOT VERIFY`: evidence is absent; this is not a pass.

## 2. Required review views

Capture the following from the final submitted revision. Do not substitute a concept image for a model render.

- [ ] DCC neutral-light front, back, left/right side, front three-quarter and rear three-quarter renders.
- [ ] DCC game-like-light front three-quarter and rear three-quarter renders.
- [ ] Unity close view at 2.5–4 m or equivalent screen height.
- [ ] Unity medium view around 8 m.
- [ ] Unity normal RTS gameplay view at the project baseline of 31 m.
- [ ] Unity far view at 40 m or the final camera maximum, whichever is greater.
- [ ] Screen-height samples near 128 px, 64 px and 32 px.
- [ ] Blue and red team variants under the same camera, lighting and background.
- [ ] LOD0, LOD1, LOD2 and LOD3/impostor transition captures.
- [ ] Representative Idle, Walk/Move, Attack, Hit and Death frames or clips.

Store captures under the asset source review folder described in `13_Asset_Naming_and_Folder_Standard.md`; name them with asset ID, revision, view, distance/LOD and date.

## 3. Visual readability

- [ ] Silhouette identifies the gameplay class at normal RTS distance without relying on UI.
- [ ] Primary weapon/tool is visible and has a class-specific direction and rhythm.
- [ ] Head, torso, hands and feet do not collapse into one unreadable mass.
- [ ] Front/back and left/right readings are distinct enough for motion direction.
- [ ] Team color remains readable at 64 px and does not cover identity-defining materials.
- [ ] Value grouping survives neutral, bright and shadowed lighting.
- [ ] Hero/special-unit emphasis follows `10_Hero_and_Special_Unit_Standard.md` and does not imply different gameplay rules.
- [ ] No critical identification depends on sub-pixel ornament, text or a tiny emblem.
- [ ] Death pose stays inside the intended selection/collision footprint unless explicitly approved.

## 4. Geometry, scale and pivots

- [ ] Scale is 1 Unity unit = 1 metre and matches the approved size chart.
- [ ] Character origin is on the ground plane between the feet; forward axis is documented and import rotation is clean.
- [ ] Building origin and footprint alignment conform to `12_Building_Production_Standard.md`.
- [ ] Projectile forward direction, pivot and length are documented and tested.
- [ ] No non-manifold, duplicate or zero-area geometry affects shading, skinning or collider generation.
- [ ] Hidden/interior faces are removed where safe; silhouette-critical faces are retained.
- [ ] Normals/tangents are intentional and no visible hard-edge, UV seam or mirrored-normal artifact remains.
- [ ] Triangle counts meet the approved tier and every variance has a measured justification.
- [ ] LOD silhouette degradation is monotonic; no lower LOD is more complex than its predecessor.
- [ ] LOD switching has no visible scale, origin, bind-pose or material-slot jump.

## 5. UV, textures, materials and team color

- [ ] UV0 has no unintended overlap; mirrored/stacked areas are explicitly documented.
- [ ] Texel density is consistent with the asset tier and camera importance.
- [ ] Padding and mip-safe margins are validated at the lowest intended mip.
- [ ] BaseColor contains no baked directional lighting that conflicts with scene lighting.
- [ ] Normal maps import as Normal Map and have correct handedness.
- [ ] Packed maps use the approved channel contract; importer settings preserve linear data.
- [ ] Alpha is not reused ambiguously between opacity and team mask.
- [ ] Material count and draw-call cost meet `05_LOD_and_Performance_Standard.md`.
- [ ] All shaders are URP-compatible and build included; no pink/error material appears.
- [ ] Team-color mask is visually inspected for bleed, missing regions and over-coverage.
- [ ] The blue/red result is produced by the shared runtime path, not by manual scene material swapping.
- [ ] Texture source, generated maps and final runtime files have recorded provenance and licenses.

## 6. Rig, skinning and animation

- [ ] Skeleton family and version are recorded; bone names comply with the approved convention.
- [ ] Bind pose, avatar mapping and import rig type are correct.
- [ ] Skin has no unweighted vertices, invalid weights or unexpected deform-bone influences.
- [ ] Maximum influences and weight pruning meet `07_Rig_Skinning_Animation_Standard.md`.
- [ ] Shoulder, elbow, wrist, hip, knee, ankle, neck and extreme attack poses pass deformation review.
- [ ] Rigid equipment does not rubber-stretch; attachment sockets retain stable transforms.
- [ ] Root-motion policy is explicit and matches the shared command/movement architecture.
- [ ] Idle and Move loop without discontinuity; Move speed/rate is tunable.
- [ ] Attack event timing aligns with visible contact/release and uses the established runtime event path.
- [ ] Hit remains readable without displacing gameplay truth.
- [ ] Death is deterministic, non-looping and compatible with pooling/despawn.
- [ ] Animation clips have deliberate compression and no observable foot/weapon jitter at normal distance.
- [ ] Source actions or an equivalent reproducible animation source are present and openable.

## 7. Unity integration

- [ ] Asset imports without new Console errors or warnings requiring suppression.
- [ ] Prefab contains presentation components only; gameplay truth remains in Definition/Runtime systems.
- [ ] Player and AI continue to use the same command path; the asset adds no alternate gameplay execution.
- [ ] Existing Definition IDs and stable APIs are preserved unless a separate approved migration says otherwise.
- [ ] Animator parameters and state names match the documented adapter contract.
- [ ] Anchors/sockets exist at expected paths and are verified in play mode.
- [ ] Renderer bounds contain every animation pose without excessive padding.
- [ ] Shadow casting/receiving is appropriate per LOD and does not cause avoidable overdraw.
- [ ] Collision/selection/navigation components use approved runtime data rather than inferred mesh size.
- [ ] Address/content-pack registration, prefab references and GUIDs resolve after a clean reimport.
- [ ] Standalone/player build loads the asset and its shader variants.

## 8. Performance and stability evidence

- [ ] Per-LOD triangles, vertices, skinned renderers, materials and bones are recorded.
- [ ] Representative unit/building counts are profiled on the named target hardware and quality level.
- [ ] CPU animation/skinning, GPU frame time, batches, SetPass calls, memory and texture residency are recorded.
- [ ] No per-frame material instantiation, mesh allocation or unbounded property-block growth occurs.
- [ ] LOD cross-fade/transition policy is measured; overdraw is acceptable.
- [ ] Pooling/spawn/despawn test produces no missing references or retained unintended objects.
- [ ] Runtime soak test has no new unhandled exceptions.

Performance thresholds are gates only after target hardware and representative battle counts are approved. Until then, record measurements as `PROPOSED` or `CANNOT VERIFY`; do not invent a pass threshold.

## 9. Required automated and manual validation record

For each candidate, record exact command/menu, date, environment, result and evidence path:

- [ ] DCC scene-open and validation script.
- [ ] Unity asset import/reimport.
- [ ] EditMode tests relevant to import adapters and domain invariants.
- [ ] PlayMode tests relevant to presentation integration.
- [ ] Standalone build and smoke test.
- [ ] Normal-distance owner playtest.
- [ ] Art-direction review.
- [ ] Technical-art review.
- [ ] License/provenance review.

## 10. Acceptance record template

```text
Asset ID / display label:
Revision / source commit:
Candidate tier:
Reviewer(s) and role:
Review date:
Target Unity / renderer / platform:
DCC and version:
Evidence root:
Automated test results:
Manual view results:
Performance measurements:
License/provenance result:
Outcome: PASS | CONDITIONAL PASS | FAIL | CANNOT VERIFY
Exceptions and expiry:
Open issue links:
Approved production status by:
```

## 11. Current-repository application

This specification task did **not** open or save Unity production assets, run a new Unity import, or perform a new player build. Existing Infantry/Archer runtime evidence remains historical baseline only. Their present acceptance result is `CANNOT VERIFY / NOT PRODUCTION READY`; detailed reasons are in `08_Golden_Sample_Infantry_Archer.md`, `09_Existing_Infantry_Archer_Remaster_Audit.md` and `99_Open_Issues_and_Missing_Information.md`.
