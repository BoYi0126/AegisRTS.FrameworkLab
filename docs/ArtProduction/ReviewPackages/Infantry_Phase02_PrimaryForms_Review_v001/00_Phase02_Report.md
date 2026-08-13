# Phase 02 Primary Forms Report

## Status

`READY FOR REVIEW`

Phase 01 target was treated as user-approved when the user explicitly instructed execution of this Phase 02 task. Phase 02 itself is not self-approved.

## Baseline

- Repository branch／HEAD at start: `main` / `ec0560192863a763d6beb3be6b9c0c642b1d4137`.
- Preserved source: `CHR_Infantry_A_v002.blend`, SHA-256 `5D9D93F9559D2A1608FB4B57A7BC0AC284C4F3ED99BA826F0A5E98E1D5F51632`.
- v002 remains the Prototype baseline. No file beneath its source/export folders was overwritten.
- Stable runtime identity remains `unit.infantry` / `PF_Unit_Infantry`; no Runtime Prefab, material, texture, shader, scene, animation or C# asset was modified.

## Scope In

- A new versioned source: `CHR_Infantry_A_v003`.
- Primary Forms, silhouette and major geometry only.
- Heavy-infantry read: curved helmet, layered shoulders, lamellar-inspired chest mass, tapered waist, stable lower body, left shield and right short sword.
- Review-only ID materials, static A-pose binding and evidence generation.
- Clay, silhouette, wireframe, 128／64／32 px and v002 comparison evidence.

## Scope Out

Final UV、Final Texture、production Team Color mask、Final Skinning、Animation Polish、正式 LOD、FBX export、Unity import、Runtime Prefab replacement、shader work、gameplay/API changes均未執行。

## Build Record

1. Initial construction correctly stopped before save when inherited bone-parent transforms produced a 2.179808 m outlier.
2. Static review parenting removed the bone-space scale issue; first valid source measured 30,946 triangles.
3. Shoulder and wrap density was reduced to keep the candidate inside the task's temporary 30K ceiling.
4. Visual QA found square boot forms and an unreliable appended-scene comparison layout; both were rejected as evidence.
5. Boots were rebuilt with rounded sole／instep／toe profiles. A Blender operator-context issue on the sword pommel was eliminated with deterministic custom geometry.
6. Final rebuild produced 28,138 triangles at 1.830 m. Comparison sheets were composed from immutable v002 review images and final v003 captures.

## Result

- New source SHA-256: `5F7C799C3B57A64B6E289A38FF2AA6E5509247C547FD4B2DE0EC47FF63AA4DBE`.
- 72 mesh objects, 14,211 vertices, 28,138 triangles.
- One preserved armature with 23 bones; no Actions were added or polished.
- Automated mesh audit reports zero non-manifold, boundary, loose edges and zero-area faces for this generated Primary Forms candidate.
- Six review-only materials; no image textures or final UV deliverable.

## Review Questions

- Does the broad shield-side mass remain readable without hiding too much body identity?
- Are helmet／shoulders／chest／waist／boots sufficiently distinct at 128／64／32 px?
- Is the shoulder layering appropriate before deformation clearance work?
- Does the v003 change go far enough beyond the v002 blockout while staying within the approved direction?

No PASS decision is recorded here. Reviewer disposition is pending.
