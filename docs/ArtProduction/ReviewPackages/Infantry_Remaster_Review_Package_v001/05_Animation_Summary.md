# 05 — Animation Summary

> Source of current frame ranges/loops/events：copied Unity animation `.fbx.meta`, deterministic build script and `ANIMATION_EVENTS.json`. Unity runtime was not opened in this collection task.

## 1. Current clips

| Clip | Source export／Unity import | Frame span @30 FPS | Nominal time to last frame | Loop | In Place | Event | Status |
|---|---|---:|---:|---:|---:|---|---|
| `AN_Infantry_Idle` | FBX／FBX + meta | 0–89 | 2.9667 s | Yes | Yes | None | CURRENT; source Action absent |
| `AN_Infantry_Move` | FBX／FBX + meta | 0–24 | 0.8000 s | Yes | Yes | `Footstep_L` 1/30 s; `Footstep_R` 13/30 s | CURRENT |
| `AN_Infantry_Attack_A` | FBX／FBX + meta | 0–26 | 0.8667 s | No | Yes | `AttackImpact` 13/30 s | CURRENT |
| `AN_Infantry_Hit` | FBX／FBX + meta | 0–9 | 0.3000 s | No | Yes | None | CURRENT |
| `AN_Infantry_Death` | FBX／FBX + meta | 0–38 | 1.2667 s | No | Yes | `DeathSettled` 35/30 s | CURRENT |

“Nominal time” is frame/30 from serialized import data. Exact current `AnimationClip.length` is `CANNOT VERIFY` without a Unity query, which was out of scope.

All animation imports serialize:

- Humanoid (`animationType: 3`)
- Copy From Other Avatar (`avatarSetup: 2`)
- source Avatar GUID `15fa862142be95a44b46245b976ab639`
- global scale 1
- keyframe-reduction compression
- Read/Write off
- root orientation/Y/XZ based on original values and documented as in place

## 2. AttackImpact

```text
AttackImpact Source: Unity ModelImporter clip event, authored by InfantryL3PrefabBuilder
Source Record: Documentation/ANIMATION_EVENTS.json and build script
Clip: AN_Infantry_Attack_A
Frame: 13 @ 30 FPS
Absolute Time: 0.43333334 s
Normalized Time: approximately 0.500 over the serialized 0–26 frame span
Receiver: PrototypeUnitAnimatorView.AttackImpact()
Verified: YES by copied .fbx.meta + builder source; Unity runtime invocation NOT RERUN
```

`PrototypeUnitAnimatorView.AttackImpact()` increments a presentation diagnostic counter only. It does not apply damage. `CombatSystem` remains gameplay-authoritative; current AttackRate/cadence adaptation may time-scale presentation to the combat windup.

## 3. Other events

| Event | Clip | Frame／time | Purpose |
|---|---|---:|---|
| `Footstep_L` | Move | 1／0.0333333 s | audio/VFX presentation hook |
| `Footstep_R` | Move | 13／0.4333333 s | audio/VFX presentation hook |
| `DeathSettled` | Death | 35／1.1666667 s | death presentation completion |

No `.anim` assets exist; clips are embedded in separate FBXs and referenced by `AC_Infantry.controller`.

## 4. Animator flow

- States：Idle、Move、Attack、Hit、Death。
- Speed and `MoveRate` drive locomotion; `AttackRate` scales attack presentation.
- Triggers：Attack、Hit、Die；bool：IsDead。
- Apply Root Motion：off in Prefab。
- Presentation adapter has historical prototype-specific Idle leg correction and attack-move cancel blending; included runtime scripts document the exact behavior.

## 5. Documentation conflicts

Actual current `.meta` and source build script agree on:

```text
Idle 0–89
Move 0–24
Attack_A 0–26
Hit 0–9
Death 0–38
```

`Specifications/SourceRecords/v002/Documentation/UNITY_IMPORT_SETTINGS.md` instead lists Idle 0–60、Attack 0–30、Hit 0–18. Treat those three rows as stale documentation until an owner verifies intent and corrects the source report. Event times for Move、AttackImpact and DeathSettled agree.

## 6. Review gaps

- Saved `.blend` Actions：NOT FOUND。
- Neutral-light animated deformation captures：NOT FOUND。
- Foot sliding and contact review at final gameplay speed：historical movement PNGs exist, but no current synchronized clip/video measurement。
- Professional animation polish review：NOT FOUND。
- Compression-error and retarget-jitter comparison：NOT RUN。
- Hand/sword/shield clipping through the entire Attack/Hit/Death range：CANNOT VERIFY from static captures。

Recommended remaster review：preserve runtime parameter/event names unless an explicit migration is approved; rebuild/polish animation source only after durable Actions and production skinning are established.
