# 16 — Master Production Checklist

> Version: 1.0  
> Purpose: a single reusable gate sheet from intake through Production Ready  
> Rule: unchecked means incomplete; `CANNOT VERIFY` never counts as checked

## A. Intake and ownership

- [ ] Stable Asset ID and display label assigned.
- [ ] Gameplay role, size tier and unit/building family identified without embedding lore in runtime code.
- [ ] Definition ID confirmed, or explicitly marked `TBD — no runtime registration`.
- [ ] Owner, art reviewer and technical-art reviewer assigned.
- [ ] Source origin, creator/tool/model, prompt/input, edit history and license evidence registered.
- [ ] Intended target platform, battle count, camera envelope and quality tier approved.
- [ ] Source/runtime folder and naming plan approved.
- [ ] Backlog entry exists with dependencies and next gate.

## B. L1 — Concept and readability lock

- [ ] Neutral front/back/side/front-three-quarter/rear-three-quarter character or building views.
- [ ] Size chart against approved reference units/grid.
- [ ] Black silhouette sheet at normal RTS presentation sizes.
- [ ] Weapon/tool/roofline/entrance readability shown.
- [ ] Blue/red team-color placement shown.
- [ ] Material and value-group callouts shown.
- [ ] Modular boundaries, sockets and non-swappable silhouette parts identified.
- [ ] L1 owner and art-direction approval recorded.

## C. L2 — Production character/building sheet

- [ ] Production orthographic sheet, not a game-ready mesh.
- [ ] Construction breakdown and thickness/depth callouts.
- [ ] Rig/deformation zones or modular construction rules documented.
- [ ] UV/material region and packed-channel intent documented.
- [ ] LOD silhouette hierarchy and removable details documented.
- [ ] Ambiguous geometry resolved before modelling.
- [ ] L2 technical-art approval recorded.

## D. L3 — Production source asset

- [ ] Editable DCC source opens in the approved tool/version.
- [ ] Geometry, topology, scale, pivots, normals and UVs pass validation.
- [ ] LOD0–LOD3/impostor are authored and budgeted.
- [ ] Rig/skeleton family, skinning and sockets pass validation where applicable.
- [ ] Required source actions/clips are durable and reproducible.
- [ ] Texture sources and final maps conform to the approved channel contract.
- [ ] Neutral-light and game-like-light DCC review renders are stored.
- [ ] Export script/profile and deterministic rebuild instructions are stored.
- [ ] L3 technical-art approval recorded.

## E. L4 — Unity-ready integration

- [ ] Runtime exports are derived from the approved source revision.
- [ ] Unity import settings, scale, rig/avatar, clips and compression are correct.
- [ ] URP materials and team-color path are connected.
- [ ] Prefab hierarchy, anchors/sockets and LODGroup conform to the contract.
- [ ] Animator/controller/adapters use established APIs and do not own gameplay truth.
- [ ] Content registration and stable IDs resolve.
- [ ] Close/medium/normal/far and 128/64/32 px captures are stored.
- [ ] Blue/red, every LOD and representative animation captures are stored.
- [ ] L4 integration approval recorded.

## F. QA and release

- [ ] `15_Unity_RTS_Asset_Acceptance_Checklist.md` completed.
- [ ] Automated tests, build, smoke test and normal-distance playtest pass.
- [ ] Representative-count performance profile is within approved target.
- [ ] No unresolved P0/P1 asset-blocking issue remains.
- [ ] Provenance/license review allows the intended distribution.
- [ ] Documentation, changelog and `DevelopmentProgress.md` are updated.
- [ ] Golden Sample lock, if applicable, is signed by design, art and technical art.
- [ ] Final status is explicitly changed to `Production Ready` by an authorized reviewer.

## G. Golden Sample checkpoint — current repository

The checked boxes below mean only that repository evidence exists. They do not imply production approval.

### Infantry (`unit.infantry`)

- [x] Legacy L1 concept sheet and size/silhouette presentation exist.
- [x] Asset-specific Phase 01 L2 construction target, conformance audit and approval checklist exist.
- [ ] New production-direction L1 is approved.
- [x] Phase 01 target is user-approved and a versioned Phase 02 exact-geometry Primary Forms／Clay review candidate exists.
- [x] Initial v003 change request has a preserved, versioned P02R1 source and required comparison evidence.
- [ ] Phase 02 Primary Forms is approved by human art／technical-art review and completes the L2 production reference.
- [x] Editable v2 `.blend`, deterministic rebuild script and runtime FBX exports exist.
- [x] Prototype LOD0/1/2 geometry, Humanoid avatar, 23-bone rig and five runtime animation FBXs exist.
- [ ] Production LOD0–LOD3/impostor meet the new quality budget.
- [ ] Durable source actions are verified in the reopened `.blend`.
- [ ] Production topology, four-influence skinning and deformation pass.
- [ ] Production texture/packed-map/team-mask material path is complete.
- [x] Unity prefab/controller/anchors and current prototype team-color slots exist.
- [ ] In-repository Unity acceptance captures and current build evidence exist.
- [ ] Provenance and commercial/distribution rights are complete.
- [ ] Golden Sample is locked.
- [ ] Production Ready.

Current outcome: **prototype baseline preserved; partial remaster candidate; not Production Ready**.

### Archer (`unit.archer`)

- [ ] Formal L1 concept and silhouette sheet exist.
- [ ] L2 production character sheet exists and is approved.
- [x] Editable `.blend`, deterministic rebuild script and runtime FBX exports exist.
- [x] Prototype LOD0/1/2 geometry, Humanoid avatar, 23-bone rig and five runtime animation FBXs exist.
- [x] Arrow source/runtime FBX, Projectile socket and current prefab/controller exist.
- [ ] Production LOD0–LOD3/impostor meet the new quality budget.
- [ ] Durable source actions are verified in the reopened `.blend`.
- [ ] Production topology, four-influence skinning and deformation pass.
- [ ] Production texture/packed-map/team-mask material path is complete.
- [ ] In-repository Unity acceptance captures and current build evidence exist.
- [ ] Provenance and commercial/distribution rights are complete.
- [ ] Golden Sample is locked.
- [ ] Production Ready.

Current outcome: **prototype baseline preserved; requires L1/L2 before remaster approval; not Production Ready**.

### Golden Sample lock

- [ ] Infantry and Archer meet all mandatory acceptance gates.
- [ ] Shared shader/team-color contract is verified.
- [ ] Shared skeleton/animation policy is verified.
- [ ] Measured normal-distance readability and performance targets are approved.
- [ ] Design, art and technical-art owners sign the lock record.
- [ ] Backlog is released for family-scale production.

**DO NOT MASS PRODUCE** until all Golden Sample lock boxes are checked.

## H. Building candidate mini-checklist

- [ ] Gameplay footprint, entrance, facing, selection bounds and construction stages are approved.
- [ ] L1/L2 roofline, function cue, damage readability and team-color placement are approved.
- [ ] Modular kit does not create ambiguous silhouettes or hidden navigation assumptions.
- [ ] LODs, colliders, shadow policy and material atlasing pass measured scene tests.
- [ ] Prefab stays a view/integration object; placement and gameplay remain runtime data.
- [ ] Production Ready is explicitly approved.

Current repository buildings are specification/placeholder baselines only; none satisfies this checklist.
