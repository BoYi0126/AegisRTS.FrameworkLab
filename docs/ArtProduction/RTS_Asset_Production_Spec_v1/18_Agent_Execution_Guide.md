# 18 — Agent Execution Guide

> Version: 1.0  
> Audience: future human/AI contributors executing one scoped asset-production task  
> Prime directive: preserve gameplay architecture and production sources; advance only through evidenced gates

## 1. Start-of-task mandatory read

Before changing the repository, read the repository `AGENTS.md`, `docs/00_README_開發總覽.md`, `docs/03_Naming_Namespace_Asmdef_命名規範.md`, `docs/04_Architecture_核心架構與依賴規則.md`, `docs/09_DevelopmentProgress_開發進度紀錄規範.md`, `docs/27_Definition_of_Done_總驗收.md`, `DevelopmentProgress.md`, the current phase documents, affected API documents, this package README, the asset backlog row and every linked asset spec.

Record branch, HEAD, working-tree state, Unity/package versions and the exact asset paths before editing. Treat pre-existing changes as user work.

## 2. Determine authority and scope

Write a short execution record containing:

- asset ID/label and current status;
- exact scope in and out;
- source and runtime files allowed to change;
- Definition/API/material/skeleton contracts affected;
- evidence and tests required;
- provenance/license state; and
- the next approval gate.

If the request does not authorize production-asset changes, perform a read-only audit. Never overwrite or delete `.blend`, FBX, texture, animation, material or prefab sources merely to make them match a new convention.

## 3. Resolve the backlog gate

1. Find the row in `17_Asset_Production_Backlog.md`.
2. Confirm that every predecessor gate is closed.
3. If the asset has no approved runtime Definition ID, do not invent one.
4. If Golden Sample is unlocked, do not start family-scale production.
5. If rights are unknown, quarantine output as review-only and do not ship or train/reuse it.

## 4. Execute L1 and L2 before modelling

For new or materially remastered assets:

1. Produce L1 concept/readability evidence per `02_Asset_Pipeline_L1_L4.md` and `04_RTS_Silhouette_and_Readability_Standard.md`.
2. Review at actual RTS presentation sizes, including blue/red variants.
3. Obtain explicit L1 owner/art approval.
4. Produce the L2 construction sheet, deformation/modular/UV/material/LOD callouts.
5. Obtain explicit L2 technical-art approval.

Do not treat an existing gameplay mesh, beauty render or prompt image as an approved L2 sheet.

## 5. Produce L3 reproducibly

1. Work in a new versioned source revision; preserve the prior source.
2. Use the approved DCC/tool version and record it.
3. Validate units, axes, scale, transforms, pivots, topology, normals, UVs, LODs and material slots.
4. For deforming assets, validate skeleton version, bind pose, weights, sockets and every required action.
5. Ensure source Actions/clips are durable after reopening the saved file. Rebuild scripts are supporting evidence, not a substitute for verifying saved output.
6. Produce neutral-light and game-like-light review renders.
7. Export deterministically to a staged runtime-output location; do not hand-edit derived FBX output.
8. Record hashes or source revision links between DCC source and runtime export.

When using generated or AI-assisted content, update the provenance registry before review and preserve prompts, source references, model/tool/version, edit trail and license evidence.

## 6. Integrate L4 without changing gameplay truth

1. Keep Definition, Runtime and View separated.
2. Preserve Player/AI shared command execution.
3. A prefab presents runtime state; it must not calculate combat, pathfinding, economy or ownership truth.
4. Reuse established view adapters, animator parameters, anchors/sockets and content registration.
5. Do not create a God Manager or a parallel material/animation/gameplay pipeline for one asset.
6. Do not change Unity packages unless separately authorized.
7. Never edit `Library/`, `Temp/` or `obj/`.

If a stable API or Definition ID must change, stop the art-only task and create an explicit migration plan with affected callers/tests.

## 7. Validate and collect evidence

Complete `15_Unity_RTS_Asset_Acceptance_Checklist.md` and a fresh copy of `16_Master_Production_Checklist.md` for the candidate.

Minimum evidence:

- DCC open/reopen validation and geometry/rig/texture metrics;
- Unity import and prefab-reference validation;
- close, medium, 31 m normal and far captures;
- 128/64/32 px, blue/red and every LOD view;
- representative animations and attachment/socket views;
- relevant EditMode and PlayMode tests;
- standalone build/smoke test;
- representative-count performance profile;
- normal-distance owner playtest;
- art, technical-art and provenance review.

Write `NOT RUN` or `CANNOT VERIFY` for missing evidence. Never convert historical results into a current pass.

## 8. Change-control procedure

- Prefer additive, versioned files and recoverable changes.
- Before bulk operations, enumerate exact resolved paths and verify they remain inside the intended asset folder.
- Compare production-asset file hashes before and after any documentation-only or automation task.
- Inspect `git diff --check`, `git status --short` and changed file types before handoff.
- Do not commit, push, publish or upload unless the request explicitly authorizes it.
- Never conceal generated output or unrelated working-tree changes.

## 9. Status and documentation update

Update the asset audit, acceptance record, checklist, backlog and open issues with actual results. Then update root `DevelopmentProgress.md` according to `docs/09_DevelopmentProgress_開發進度紀錄規範.md`.

The progress entry must include baseline; scope in/out; files/assets; before/after behavior; Architecture/API/Data impact; exact tests and counts; itemized acceptance; Completed; Not Completed/Deferred; Known Issues; Git state; and ordered Next steps. An incomplete gate remains `In Progress` or `Blocked`; it is not `Completed` because time or budget ended.

## 10. Final handoff template

```text
Changed:
Architecture:
Files/assets:
API/data:
Tests (exact commands, totals, pass/fail/not run):
Validation and evidence:
Acceptance checklist result:
Provenance/license result:
Known issues:
Git status / commit / push:
Next gate:
```

## 11. Stop conditions

Stop and request human direction when any of these would materially change the outcome:

- visual direction or gameplay identity is contradictory or unapproved;
- source rights cannot support the intended distribution;
- the required Definition/API contract is absent;
- a destructive asset migration is the only proposed path;
- target platform/performance requirements are required for approval but undefined; or
- approval authority for a Golden Sample exception is unknown.

The correct result in these cases is a precise Open Issue with evidence and owner, not an invented decision.
