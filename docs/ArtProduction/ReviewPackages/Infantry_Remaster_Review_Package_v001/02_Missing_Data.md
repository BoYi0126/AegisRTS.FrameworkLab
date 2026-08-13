# 02 — Missing Data

Missing evidence does not block package creation and is not auto-generated as production art. `NOT FOUND` and `CANNOT VERIFY` do not mean pass.

## Critical

### L2 Production Character Sheet: NOT FOUND

No neutral orthographic A/T-pose front、side、back、3/4 turnaround with equipment construction、thickness、deformation、material and LOD callouts was found. Existing concept and v001 `L2 game model` reports are not substitutes.

Required next：Art／Technical Art author and approve formal L2 before remaster geometry decisions.

### Standardized current Unity capture set: MANUAL CAPTURE REQUIRED

Historical Unity validation PNGs exist and are included, but the repository/current package does not prove a single current-revision standardized set with camera metadata:

- Unity Close
- Unity Medium
- Unity RTS Normal
- Unity Far
- 128 px、64 px、32 px screen-height samples
- paired blue/red at identical camera/lighting
- explicit LOD0／LOD1／LOD2 transition captures

Required next：capture from the exact candidate commit/build and record camera distance/FOV、resolution、LOD、team and asset hash.

### Editable animation source Actions: NOT FOUND IN REOPENED BLEND

`CHR_Infantry_A_v002.blend` opens with `Actions: 0`. Five animation FBXs and deterministic build code exist, but saved editable Actions are not durable evidence. Legacy report text claiming Actions are stored conflicts with the reopened file.

Required next：on a new versioned source copy, retain Actions through fake users/NLA or an approved action library; reopen and validate. Do not alter current source during review collection.

### Source provenance／commercial rights: CANNOT VERIFY

Generation records state deterministic procedural rebuild from v001 input, but the original v001 commercial-rights chain is not complete. This blocks production-release reuse.

Required next：producer/legal records creator/tool/model、original input rights、prompts、third-party elements、human edits and allowed distribution.

## Important

### Production material/team-color implementation: NOT FOUND

- Current Base material references BaseColor and Normal only.
- Current TeamColor material has no texture.
- ORM and TeamColorMask files are imported but not referenced by the two current materials.
- Runtime team color is separate named material slots plus `MaterialPropertyBlock`, not a mask-driven shader.
- No Infantry custom `.shader`／`.shadergraph` was found.

Required next：Technical Art decides whether to preserve this slot path or implements a versioned shared URP mask shader on remaster candidates; verify mips and batching before lock.

### Production LOD3／Impostor: NOT FOUND

Current Prefab has LOD0／1／2 at 4,376／1,512／542 triangles and then culls. This is a playable prototype hierarchy, not the proposed production LOD0–LOD3/impostor contract.

### Production deformation review: CANNOT VERIFY

Body vertices are all weighted, but every body vertex has at most one influence. Rigid sword/shield vertices are intentionally bone-parented and unweighted. No extreme-pose deformation sheet or current neutral/game lighting skinning capture was found.

### Current runtime clip lengths via Unity API: NOT RUN

`.meta` proves imported frame ranges/events, but this docs-only task did not query `AnimationClip.length` or Avatar validity from Unity. Actual metadata frame ranges also conflict with one source import document:

- actual Idle 0–89 vs document 0–60;
- actual Attack 0–26 vs document 0–30;
- actual Hit 0–9 vs document 0–18.

Required next：fix documentation only after a current Unity/DCC owner verifies intended delivery.

### Original v001 editable DCC source: NOT FOUND

The only `.blend` is v002. v001 provides GLB, textures and reports; original modelling source is absent.

## Optional／valuable

- Current animation turntable/video for Idle、Move、Attack、Hit、Death under neutral review lighting.
- Close weapon-hand、shield-arm、shoulder、hip and knee deformation captures.
- UV distortion/checker render and mip-chain screenshots.
- Material channel diagnostic renders for BaseColor、Normal、ORM and TeamColor mask.
- Measured LOD switching and representative-unit performance profile on named target hardware.
- Historical v001 Unity capture tied to an immutable source revision.
- Exact per-object origin/pivot acceptance for modular equipment replacement.

## Evidence present but limited

| Evidence | What it proves | What it does not prove |
|---|---|---|
| L1 final／alternate | Intended broad identity and blue/red concept | Production construction, topology or final art direction |
| Historical Unity detail/movement PNGs | Asset was rendered in Unity validation and movement sequences existed | Exact current commit, standardized distances, complete visual acceptance |
| New package Blender renders | Current copied `.blend` LOD0 geometry/materials can render from six views | Unity lighting/import result, animation deformation, gameplay-camera readability |
| FBX `.meta` | Current importer values, clips and events serialized | Fresh reimport success or runtime clip API values |
| Builder/test code and historical reports | Intended validation contract and prior results | This task reran those Unity tests |

## Human decisions still required

1. Approve world-neutral Stylized Fantasy direction relative to current East-Asian-inspired low-poly prototype.
2. Choose Preserve／Modify／Partial Rebuild／Rebuild after L1/L2 and evidence review.
3. Approve shader/team-color and skeleton/modular policies.
4. Assign Golden Sample art、technical-art and design reviewers.
5. Define target hardware、battle counts and camera/LOD acceptance thresholds.
