# 99 — Open Issues and Missing Information

> Version: 1.0  
> Snapshot: 2026-08-13  
> Rule: `CANNOT VERIFY` is unresolved evidence, not approval

## 1. Priority definitions

- **P0 — production stop:** blocks Golden Sample lock or lawful distribution.
- **P1 — acceptance stop:** blocks the affected asset from Production Ready.
- **P2 — planning/performance stop:** must be resolved before measured scale production or the named family.
- **P3 — improvement:** valuable but does not block the current documentation package.

## 2. P0 — Golden Sample / rights / direction

| ID | Issue and repository evidence | Impact | Required owner / resolution |
|---|---|---|---|
| OI-P0-01 | Infantry v1 concept/source lineage and all Infantry/Archer source/runtime files lack a complete provenance/license registry suitable for commercial/distributed production. Repository authorship alone does not prove upstream rights. | Neither candidate can be lawfully declared a reusable production master. | Producer/legal + asset author: provide origin, creators/tools/models, prompts/inputs, licenses and edit trail; complete `14_AI_Asset_Provenance_and_License_Standard.md`. |
| OI-P0-02 | `mission/Infantry_Phase01_Production_L2_Remaster_Target.md` 的asset-specific方向已由使用者批准，但broader approved Art Bible仍不存在。 | 此資產方向不自動定義其他faction／unit family；大量生產仍可能硬編碼單一世界觀。 | Design／art director：保持runtime IDs world-neutral，另行批准broader faction extension rules與Art Bible。 |
| OI-P0-03 | P035 arm-length change request已由P035R1實作：UpperArm／Forearm為0.176H／0.165H，posed wrist／hand-end已收斂至L1差0.72%H／0.71%H，arm-focus與隔離Unity evidence存在，但19項Revision Gate仍全數待Reviewer。 | 比例候選尚未human sign-off；`READY FOR PHASE03_5 REVISION REVIEW`不得解讀為PRE-UV GEOMETRY LOCK、Phase 04 authorization或Production Ready。 | User + art + technical art：審查`Infantry_Phase03_5_Revision01_Review/05_Review_Checklist.md`，特別判斷arm drop、hand／head scale、grip、helmet clearance與Unity Close／RTS Normal。 |
| OI-P0-04 | Archer has no in-repository formal L1 concept sheet or L2 production character sheet; it was produced as an Infantry-derived playable prototype. | Archer identity and shared-versus-unique construction cannot be approved. | Art + design + technical art: author and approve Archer L1/L2 before remaster. |
| OI-P0-05 | Reopened `Infantry_L3_Source.blend` and `Archer_L3_Source.blend` each report zero saved Actions, although build scripts create Actions and legacy docs say they are stored. Independent animation FBXs exist and scripts can regenerate them. | Editable animation source is not proven durable; docs/source disagree. | Technical artist: reproduce with the approved Blender version, retain Actions with fake users/NLA or agreed library layout, reopen and validate; update legacy reports. |
| OI-P0-06 | No production team-color shader/channel contract is implemented. Current team color uses separate material slots selected by material name and a `MaterialPropertyBlock` setting `_BaseColor`/`_Color`. Infantry ORM and TeamColorMask are imported but not connected; Archer runtime materials have no texture references. | The proposed packed mask and scalable batching path are unverified; visual results cannot be production-approved. | Rendering/technical art: implement a scoped URP solution, validate blue/red masks/mips/batching on both Golden Samples, then freeze the contract. |

## 3. P1 — Asset acceptance blockers

| ID | Issue and repository evidence | Impact | Required resolution |
|---|---|---|---|
| OI-P1-01 | Current Infantry/Archer meshes are prototype budgets (Infantry 4376/1512/542 tris; Archer 3344/1280/542 tris), not the new production LOD0–LOD3/impostor hierarchy. | They do not demonstrate production fidelity or final distance transitions. | Create approved production revisions after L2; measure each LOD and preserve current prototypes as baselines. |
| OI-P1-02 | Both character bodies are fully weighted but use at most one influence per vertex in the audited `.blend` files. | Deformation quality at shoulders, elbows, hips and knees is below the proposed production target and visually unverified. | Author production topology/weights and pass extreme-pose review; document intentional rigid regions. |
| OI-P1-03 | Infantry textures are 1K prototype maps; the runtime material references BaseColor and Normal only. Archer Base/Team/Arrow materials contain constant colors and no texture references. | Neither candidate meets production texture/material requirements. | Author and connect approved production textures/materials after L2; validate importer color space and packed channels. |
| OI-P1-04 | Repository contains no current Unity acceptance screenshots for Infantry/Archer. Infantry has a mathematical camera preview, not a captured Unity render; progress notes mention external sibling captures that are not stored here. | Close/medium/normal/far, blue/red, LOD and animation readability cannot be independently reviewed from this repository. | Capture and commit an evidence set from the final candidate and referenced Unity revision. |
| OI-P1-05 | Required neutral-light and game-like-light DCC turntable renders for both candidates are not present. | Normals, tangents, material breakup, back/side construction and deformation cannot be approved visually. | Generate standardized DCC renders from the approved source revision. |
| OI-P1-06 | Normal/tangent correctness and UV/mip behavior were not visually inspected under a production material. | Static metrics cannot prove shading quality. | Run technical-art material/render review and store evidence. |
| OI-P1-07 | Current source/runtime exports supply five action FBXs per unit but final production animation polish, compression, loop/contact review and event alignment have not been approved. | Movement and combat readability remain prototype quality. | Review every required clip at normal distance; record event/contact timings and compression evidence. |
| OI-P1-08 | Infantry v1 has no editable `.blend` found. Infantry v2 and Archer have editable `.blend` files, but their derivation/provenance and source-action durability remain open. | Original concept-to-model reconstruction and ownership trail are incomplete. | Locate/register original source or explicitly record it as unavailable; preserve v2 sources and perform controlled remaster. |
| OI-P1-09 | No current Unity clean-reimport, EditMode/PlayMode, standalone build or soak test was run for this documentation-only task. Historical prototype tests cannot approve a future remaster. | The package defines acceptance but does not certify runtime state. | Run the full checklist against each final candidate and record exact current results. |
| OI-P1-10 | No Golden Sample sign-off authority, owner list or immutable lock revision is recorded. | Even complete evidence cannot transition to mass production consistently. | Producer assigns design/art/technical-art approvers and records lock decision/revision. |

## 4. P2 — Product, performance and family decisions

| ID | Missing decision/information | Impact | Required resolution |
|---|---|---|---|
| OI-P2-01 | Target hardware, graphics quality, resolution, representative battle counts and frame/memory budgets are unspecified. | Triangle/texture limits remain planning budgets; performance cannot receive a measured pass. | Product/engineering establish a benchmark matrix and profile both Golden Samples. |
| OI-P2-02 | Final gameplay camera envelope, FOV/orthographic policy and approved LOD pixel thresholds are not locked beyond the current 31 m prototype baseline. | LOD switching and silhouette targets may be miscalibrated. | Design/graphics approve camera test matrix and derive LOD thresholds from screen size. |
| OI-P2-03 | Building grid cell size, footprints, entrances, navigation blockers, construction/damage states and destruction ownership are not fully specified for production art. | Building pivots, modular pieces and animations could encode incorrect gameplay assumptions. | Design/engineering publish a versioned data/API contract before building L2/L3. |
| OI-P2-04 | Spearman, Heavy Infantry and Mage are requested backlog labels but have no approved Definitions/roles in current content. | Creating assets or IDs now would invent gameplay/content. | Design approve roster, roles and IDs before L1 work. |
| OI-P2-05 | Hero visual identities, faction/world motifs and uniqueness limits are unapproved. | Hero work risks hard-coding lore and disrupting unit readability hierarchy. | Design/art approve world-neutral identity briefs and tier comparisons. |
| OI-P2-06 | Shared skeleton family/version, modular socket schema and retargeting policy are not frozen. | Later units may fragment into incompatible rigs or constrain silhouette. | Validate proposed contracts on both Golden Samples and one variance case before freeze. |
| OI-P2-07 | Final production texture resolution per tier, atlas strategy and streaming/residency budgets lack target-platform measurements. | Texture budgets are provisional. | Profile representative scenes, then publish approved per-tier tables. |

## 5. P3 — Repository/process improvements

| ID | Improvement | Suggested action |
|---|---|---|
| OI-P3-01 | Acceptance evidence currently has no machine-readable manifest. | Add a small versioned manifest containing asset ID, source revision, capture settings, hashes and reviewers after the Golden Sample workflow is proven. |
| OI-P3-02 | Legacy reports use phase labels that conflict with the new pipeline semantics. | Apply `Legacy_Spec_Migration.md`; retain history and add explicit legacy-status banners. |
| OI-P3-03 | Project/framework license file is `UNLICENSED`, and production-asset distribution terms are not separately enumerated. | Producer/legal should distinguish framework code license, third-party dependencies and individual asset rights. |

## 6. Facts explicitly not found

The repository scan did not find:

- an approved production Art Bible resolving the visual-direction conflict;
- a fully approved Infantry production reference（v004_P035R1與Unity evidence存在，但Phase 03.5 Revision 01 human review／PRE-UV geometry lock仍pending）;
- an Archer L1 or L2 production sheet;
- a production custom shader or Shader Graph for packed team color;
- saved `.anim` assets for the character actions;
- durable Actions in either reopened L3 `.blend`;
- production LOD3/impostor character assets;
- in-repository current Unity acceptance captures;
- target-hardware profiling evidence for production-quality units;
- formal Golden Sample signatures; or
- approved Definition IDs for Spearman, Heavy Infantry or Mage.

Future contributors must search again before acting; this snapshot is not a permanent claim.

## 7. Human decision record template

```text
Open Issue ID:
Decision / evidence:
Owner and role:
Date:
Affected assets/contracts:
Approved revision or file:
Exceptions and expiry:
Backlog/checklist changes:
```

Closing an issue requires repository evidence and updates to the relevant audit, checklist, backlog and `DevelopmentProgress.md`.
