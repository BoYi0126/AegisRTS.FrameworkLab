# 17 — Asset Production Backlog

> Version: 1.0  
> Snapshot: 2026-08-13, repository HEAD `0422d6b98907095ffe8b47a7673a90aa72c8bea0`
> Status vocabulary: `BLOCKED`, `READY FOR GATE`, `IN PROGRESS`, `PROTOTYPE`, `PROPOSED`, `PRODUCTION READY`

## 1. Scheduling rules

1. Golden Sample Infantry and Archer are the only character-production priorities until the lock gate passes.
2. A backlog row may describe a future asset, but it must not create a runtime Definition ID before design approves one.
3. `PROTOTYPE` means playable evidence exists; it does not mean the art is reusable as a production master.
4. Assets with unknown rights, missing L1/L2, or unresolved shared shader/skeleton contracts remain blocked.
5. Buildings and hero identities stay world-neutral until design/art direction provides approved identity sheets.
6. Do not batch-generate, outsource or mass-remaster a family while the Golden Sample is unlocked.

## 2. P0 — Golden Sample

| Priority | Candidate | Current runtime ID | Current status | Blocking gates | Next authorized work |
|---|---|---|---|---|---|
| P0.1 | Infantry | `unit.infantry` | PHASE 03.5 R2 `READY FOR PHASE03_5 REVISION02 REVIEW` / v004_P035R2 pre-UV candidate | Human 18-item shield-alignment review and PRE-UV geometry-lock decision; provenance; production texture/team-mask shader; production skin/deformation and LOD | Review `docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision02_Shield_Alignment_Review/05_Review_Checklist.md`, especially shield top／center／bottom、grip／strap、clipping and Unity Close／RTS Normal; Phase 04 requires explicit approval |
| P0.2 | Archer | `unit.archer` | PROTOTYPE / partial-remaster candidate | Formal L1 and L2 absent; source actions; materials/textures; production LOD/deformation; Unity evidence | Author and approve L1/L2 from the preserved prototype role; then scope shared-body versus class-specific rebuild |
| P0.3 | Shared character foundation | N/A | PROPOSED | Skeleton family/version, modular socket contract, production URP/team-color shader and packing, target performance | Prototype and validate through Infantry/Archer only; freeze after dual-sample evidence |
| P0.4 | Golden Sample lock | N/A | BLOCKED | Both candidates and shared foundation must pass `16_Master_Production_Checklist.md` | Run design, art and technical-art sign-off; record lock revision and evidence root |

## 3. P1 — Next standard units (blocked by Golden Sample)

| Priority | Candidate | Runtime ID | Repository baseline | Status | First gate after unlock |
|---|---|---|---|---|---|
| P1.1 | Spearman | `TBD` | No current Definition, spec or production asset found | PROPOSED / BLOCKED | Design role + ID decision, then L1 silhouette study against Infantry |
| P1.2 | Heavy Infantry | `TBD` | No current Definition, spec or production asset found | PROPOSED / BLOCKED | Design role + ID decision, then L1 armour/value hierarchy |
| P1.3 | Cavalry | `unit.cavalry` | Content registration and placeholder prefab/spec exist; no production art | PROTOTYPE PLACEHOLDER / BLOCKED | Reconfirm scale/mount skeleton family, then L1 movement silhouette and size chart |
| P1.4 | Mage | `TBD` | No current Definition, spec or production asset found | PROPOSED / BLOCKED | Design role + ID decision, then L1 casting silhouette and effect ownership boundary |

No rows above authorize creation of Spearman, Heavy Infantry or Mage runtime content. Their names are production-planning labels requested by this specification task.

## 4. P2 — Special/vehicle units

| Priority | Candidate | Runtime ID | Repository baseline | Status | First gate |
|---|---|---|---|---|---|
| P2.1 | Light battering ram / siege baseline | `unit.siege` | Placeholder prefab and `Unit_06_Siege_L1_Spec.md`; no production model | PROTOTYPE PLACEHOLDER / BLOCKED | Confirm final gameplay identity and vehicle skeleton/wheel policy; approve L1/L2 |
| P2.2 | Additional special units | `TBD` | None approved | PROPOSED / BLOCKED | Design roster approval before concept generation |

Special assets must demonstrate exceptional silhouette without changing combat truth or adding prefab-local gameplay rules.

## 5. P3 — Heroes and named visual tiers

| Priority | Candidate | Runtime ID | Repository baseline | Status | First gate |
|---|---|---|---|---|---|
| P3.1 | Commander visual tier | `hero.commander` | Hero placeholder registration/spec | PROTOTYPE PLACEHOLDER / BLOCKED | Identity brief, scale/ornament limits and L1 comparison with standard units |
| P3.2 | Lieutenant visual tier | `hero.lieutenant` | Hero placeholder registration/spec | PROTOTYPE PLACEHOLDER / BLOCKED | Identity brief and L1 role hierarchy |
| P3.3 | Opponent hero visual tier | `hero.opponent` | Hero placeholder registration/spec | PROTOTYPE PLACEHOLDER / BLOCKED | Identity brief without hard-coded faction/world assumptions |

Hero names, faces, culture, gender presentation and faction motifs are `TBD`; current IDs are gameplay/content labels, not an approved art bible.

## 6. P4 — Buildings and settlements

| Priority | Candidate family | Current IDs / specs | Status | Blocking gates | Next gate |
|---|---|---|---|---|---|
| P4.1 | Defense gate | `defense.gate`, `Building_04_DefenseGate_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Footprint, gate states, navigation ownership, L1/L2 | Cross-discipline footprint/state contract |
| P4.2 | Stronghold core | `defense.stronghold-core`, `Building_03_StrongholdCore_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Gameplay scale, roofline, damage/construction states | L1 size/readability lock |
| P4.3 | Economy building | `building.economy`, `Building_05_Economy_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Final function/footprint and material kit | L1 function-cue comparison |
| P4.4 | Recruitment building | `building.recruitment`, `Building_06_Recruitment_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Final function/footprint and material kit | L1 entrance/yard readability |
| P4.5 | Player city | `settlement.player-city`, `Building_01_PlayerCity_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Settlement composition, footprint and modular rules | Composition L1 after building kit lock |
| P4.6 | Village | `settlement.village`, `Building_07_Village_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Settlement composition and prop density | Composition L1 after building kit lock |
| P4.7 | Enemy fortress | `settlement.enemy-fortress`, `Building_02_EnemyFortress_L1_Spec.md` | PROTOTYPE PLACEHOLDER | Identity direction and modular defense kit | Composition L1 without hard-coded lore |

Building production begins only after grid/footprint, navigation, damage/construction-state and team-color contracts are approved.

## 7. Cross-cutting production-enabler backlog

| Priority | Work item | Status | Acceptance |
|---|---|---|---|
| E0 | Resolve Infantry/Archer source provenance and distribution rights | BLOCKED — human/legal evidence needed | Complete registry rows and evidence files per `14_AI_Asset_Provenance_and_License_Standard.md` |
| E1 | Approve production visual direction versus current East-Asian-inspired low-poly prototype | BLOCKED — art/design decision | Versioned Art Bible decision with allowed variation and world-neutral runtime boundary |
| E2 | Implement and verify shared URP team-color shader + packed-map contract | PROPOSED | Golden Samples render blue/red correctly; no material instances; masks/mips verified |
| E3 | Freeze character skeleton family, socket schema and animation delivery policy | PROPOSED | Both Golden Samples pass retarget, deformation and source-action reproducibility |
| E4 | Establish target hardware, quality level and representative battle counts | BLOCKED — product/performance decision | Named profile targets and recorded Unity Profiler baseline |
| E5 | Add repository-owned acceptance capture convention and review manifest | READY FOR GATE | Both samples have complete evidence sets referenced by acceptance records |
| E6 | Validate production LOD0–LOD3/impostor policy | PROPOSED | Measured visual transitions and cost on both samples |
| E7 | Building grid/footprint/navigation interface | BLOCKED — design/engineering decision | Versioned API/data contract before building production |

## 8. Exit from backlog hold

Mass production may begin only when:

- Golden Sample lock is signed and points to immutable revisions/evidence;
- all P0 blockers are closed or explicitly accepted with expiry;
- shared material, rig, LOD, naming and provenance contracts are versioned;
- target performance measurements are approved; and
- the first unlocked P1 asset has an approved Definition/role and L1 brief.

Until then: **DO NOT MASS PRODUCE**.
