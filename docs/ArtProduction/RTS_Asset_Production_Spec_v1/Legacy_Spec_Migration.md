# Legacy Specification Migration

> Version: 1.0  
> Migration type: additive documentation migration  
> Current task mutation boundary: no existing Unity, Blender, FBX, texture, material, animation or prefab production asset is moved, overwritten or deleted

## 1. Purpose

The repository already contains valuable prototype specs and reports under `docs/ArtSpecs/`. This package does not erase that history. It separates historical implementation evidence from the new production-quality contract and defines a controlled path for future adoption.

Precedence for new production work:

1. repository architecture, naming, API and Definition contracts;
2. approved, versioned gameplay/design decisions;
3. this production specification package;
4. asset-specific approved L1/L2 and exceptions;
5. legacy ArtSpecs as historical baseline and prototype evidence.

When rules conflict, record the conflict and obtain the appropriate owner decision. Do not silently rewrite an existing asset to satisfy a paper standard.

## 2. Legacy artifacts retained

Retain all current ArtSpecs, including:

- Infantry L1 concept, L2 game-model specification, L3 production specification and reports;
- Archer L3 production specification/report;
- Cavalry, siege, hero and building L1 specifications;
- prototype presentation/camera/readability documents; and
- current ArtSource and Shared/Art README/report files.

They remain evidence of what was built, tested and intended at the prototype milestone. Add a legacy banner only in a separately authorized migration; do not alter their historical claims retroactively.

## 3. Terminology mapping

| Legacy term | New interpretation | Migration rule |
|---|---|---|
| L1 concept | L1 Concept and Readability Lock | Re-evaluate against the full new L1 checklist; existing evidence may satisfy individual items only. |
| L2 game model | Prototype playable model, not new L2 | Relabel only in future documentation metadata; the new L2 is a production construction sheet created before production modelling. |
| L3 production model | Existing DCC/runtime prototype baseline | Audit against production L3 requirements; retain reports and metrics, but do not infer production approval. |
| Unity presentation / L4-like integration | Prototype L4 baseline | Re-run current acceptance checklist for any production candidate. |
| Final / PASS in a legacy phase | Passed that historical phase and scope | Does not automatically mean Golden Sample or Production Ready. |

## 4. Known rule conflicts and resolutions

| Topic | Legacy/current repository | New production target | Resolution |
|---|---|---|---|
| Visual direction | World-neutral, East-Asian-ancient-inspired, stylized low-poly prototype | Stylized Fantasy RTS production quality requested | **OPEN:** approve an Art Bible. Keep runtime IDs/code world-neutral; allow visual family/faction data outside domain logic. |
| L2 meaning | Infantry L2 is a playable game-model spec | L2 is a production character/building sheet before modelling | Preserve legacy filename; create a new unambiguous L2 production sheet and link the legacy document as prototype reference. |
| Character polygon budget | Infantry/Archer low-thousands prototype triangles | Production LOD0 planning range around 20k–35k for standard characters, with tier-based exceptions | Treat both as different quality tiers. Validate the new range through measured Golden Samples before freezing it. |
| LOD count | Current characters use LOD0/1/2 | New target uses LOD0/1/2/3 or impostor | Add production LODs in versioned remaster sources; do not replace current prefab assets until acceptance. |
| Texture resolution | Current Infantry 1K; Archer constant-color materials | Tiered production textures and packed maps | Resolve target/platform budgets, author new maps, and preserve legacy textures as prototype inputs. |
| Packed channels | Infantry has separate ORM and TeamColorMask files; runtime material does not use them | `R=Metallic, G=AO, B=Roughness, A=TeamColorMask` proposed | Implement/test a shared URP shader first; migrate both Golden Samples together. The mapping remains `PROPOSED` until verified. |
| Team color | Separate TeamColor material slots; runtime property block sets `_BaseColor`/`_Color` by material name | Mask-driven shared material path | Preserve the current path as fallback until the new path passes blue/red, mip and batching tests. |
| Rig weights | 23-bone Humanoid, body vertices weighted, max one influence | Production deformation with bounded multi-influence weights where needed | Retain skeleton baseline; remaster topology/weights only after L2 and extreme-pose approval. |
| Animation source | Builder scripts export five action FBXs; reopened `.blend` contains zero Actions | Durable editable source actions plus reproducible exports | Fix action retention or approve an equivalent source library; do not claim stored Actions before reopen verification. |
| Naming | Current assets use established repository names such as `PF_Unit_Infantry` and `CHR_Infantry_*` | New names follow `13_Asset_Naming_and_Folder_Standard.md` | Preserve stable runtime names/GUIDs; apply conventions to new versioned sources and document aliases rather than mass-renaming. |
| Evidence | Historical reports and some external capture references | Repository-owned acceptance captures and records | Re-capture final candidates in DCC/Unity and store a complete evidence set. |

## 5. Migration phases

### M0 — Specification baseline (completed by this documentation task)

- Inventory current source/runtime assets and contracts.
- Establish new pipeline, standards, checklists, backlog and issue registry.
- Preserve every production asset byte-for-byte.
- Mark Infantry/Archer as Golden Sample candidates, not Production Ready.

### M1 — Direction, rights and L1/L2 lock

- Resolve OI-P0-01 through OI-P0-04.
- Approve visual direction and provenance.
- Approve Infantry and Archer L1/L2 independently.
- Record shared-versus-unique construction decisions.

Exit: both candidates may enter scoped production remaster.

### M2 — Shared technical foundation

- Prototype/freeze shader and team-color packing.
- Validate skeleton family, sockets, source-action retention and export reproducibility.
- Establish target hardware, camera and performance measurements.

Exit: shared contracts are versioned and verified on both candidates.

### M3 — Versioned Golden Sample remaster

- Create new source revisions beside preserved legacy assets.
- Perform only the rebuilds approved in `09_Existing_Infantry_Archer_Remaster_Audit.md`.
- Author production topology, weights, materials, textures and LOD0–LOD3/impostor.
- Generate full DCC evidence.

Exit: L3 candidates pass technical-art review.

### M4 — Unity acceptance

- Stage new runtime exports without destroying legacy fallback.
- Integrate through existing view/content contracts.
- Complete Unity captures, tests, build, playtest and performance profile.
- Resolve every applicable P0/P1 issue.

Exit: each candidate receives PASS or remains blocked with evidence.

### M5 — Golden Sample lock and controlled rollout

- Record design/art/technical-art sign-off and immutable revisions.
- Update checklist/backlog/progress records.
- Unlock exactly one next standard-unit L1 at a time until the workflow is proven.

Exit: family-scale production can be scheduled. Before this point: **DO NOT MASS PRODUCE**.

## 6. Stable contracts to preserve during migration

- existing Definition IDs such as `unit.infantry` and `unit.archer`;
- Definition/Runtime/View separation;
- Player/AI shared command path;
- established presentation adapter and animation-event architecture;
- content-pack registration mechanism;
- prefab/asset GUIDs until an explicit migration validates every reference;
- current prototype assets as rollback and comparison baselines; and
- repository package versions unless a separately approved engineering task changes them.

## 7. Per-asset migration record template

```text
Asset ID and legacy revision:
New candidate revision:
Legacy documents retained:
Applicable new standards:
Rule conflicts and approved decisions:
Preserved parts:
Modified parts:
Rebuilt parts:
Runtime/API/GUID migration:
Evidence and tests:
Fallback path:
Approval and date:
Open issues:
```

## 8. Rollback and archive policy

- Never overwrite the only editable DCC source.
- Keep old runtime assets referenced until the new candidate passes clean import, tests, build and visual acceptance.
- Do not delete legacy specs after migration; append links to successor evidence.
- If a candidate fails, revert references through an explicit, reviewed change while retaining the failed source/evidence for diagnosis.
- Hash source/runtime packages at handoff so reviewers can identify exactly what was approved.

This document authorizes no asset mutation by itself.
