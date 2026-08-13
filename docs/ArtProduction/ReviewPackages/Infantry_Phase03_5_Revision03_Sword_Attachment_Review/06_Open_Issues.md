# 06 — Open Issues

## Not completed / deferred

- Reviewer approval and formal `PRE-UV GEOMETRY + ATTACHMENT LOCK` are pending; this package is review-ready, not self-approved PASS.
- Final skinning and gameplay animation validation remain Phase 06/08 work. P035R3 is still a Phase 03.5 static-form source.
- The L1 FBX is a review-only baked rest-pose comparison, not a shippable animation clip.
- Generic melee equipment APIs, runtime swapping, weapon-tip VFX, LOD, UV, texture, material/team-color production work, and runtime prefab replacement remain deferred.
- The L1 comparison places the blade close to the right thigh as inherited from the established review pose. Grip/socket attachment is correct; animation collision polish is deferred.

## Known tooling note

Unity `-nographics` produced blank capture frames on this machine because the active renderer did not support required constant buffers. Final evidence was regenerated in GPU batch mode and visually checked. This does not affect FBX, Avatar, hierarchy, or prefab validation.

No unresolved attachment, FBX hierarchy, Humanoid, scale, geometry-lock, or ZIP-integrity issue is known at handoff.
