# 00 — Collection Report

```text
Project: AegisRTS.FrameworkLab
Asset: unit.infantry / PF_Unit_Infantry / CHR_Infantry_A_v002
Collection Version: v001
Collection Date: 2026-08-13 (Asia/Taipei)
Workspace: C:\projects\Unity\AegisRTS.FrameworkLab
Branch / HEAD: main / ec0560192863a763d6beb3be6b9c0c642b1d4137
Unity Baseline: 6000.5.7f1 / URP 17.5.0
Blender Inspection: 5.2.0 LTS, package copy only, no .blend save
```

## Scope

本次只執行 `SCAN → READ → ANALYZE METADATA → COPY → DOCUMENT → PACKAGE`。沒有修改或重新匯入任何 Blender、FBX、GLB、Texture、Material、Prefab、Animation、Shader、Scene、C# 或 `.meta` production asset；沒有執行 Unity Editor、Unity tests、build、git commit 或 git push。

開始時工作樹已有上一項 production-spec 文件與使用者移入 `mission/` 的任務檔；本次保留其狀態，沒有覆寫、清理或重設。

## Found

| 類別 | 結果 | Current 判定 |
|---|---|---|
| L1 | 1 final、1 alternate，另有一份 v002-linked final physical copy | FOUND；1254×1254 RGB |
| L2 | front dimension preview、UV layouts、legacy L2 delivery docs | **Production Character Sheet NOT FOUND** |
| BLEND | `CHR_Infantry_A_v002.blend` | CURRENT；231,897 bytes；Blender 5.2 可開啟 |
| FBX | 1 current master source export、1 Unity master、5 source clip FBX、5 Unity clip FBX | CURRENT；source/runtime binaries 可比對 |
| GLB | 4 v001 blue/red models、2 v002 input copies、2 Unity legacy GLB | LEGACY／INPUT；current Prefab 不引用 |
| Prefab | `PF_Unit_Infantry.prefab` + `.meta` | CURRENT；ContentPack 直接綁定 |
| Materials | Base、TeamColor 及各自 `.meta` | CURRENT；URP Lit |
| Textures | BaseColor、Normal、ORM、TeamColorMask 及 `.meta` | CURRENT files；只有 BaseColor／Normal 被 current material 引用 |
| Rig | Armature、23 bones；Unity master importer Humanoid/Create From This Model | FOUND；歷史 builder 報告 valid Human，這次未重跑 Unity |
| Animations | Idle、Move、Attack_A、Hit、Death；source/runtime FBX + import metadata | FOUND；current `.blend` 0 saved Actions |
| Screenshots | 6 legacy mathematical previews、16 historical Unity captures、12 newly rendered DCC views | FOUND，但 standardized current Unity set incomplete |
| Scripts | deterministic Blender build、Unity L3 builder、legacy builder、view adapters、validators/tests | FOUND，僅複製 |
| Specifications | source delivery records、legacy ArtSpecs、production spec、runtime contracts | FOUND |

## Collection totals before package-authored reports/manifests

- Copied from source/external evidence: 148 files.
- Generated review evidence: 12 Blender PNGs, 1 Blender technical JSON, 2 Blender CSV manifests, 1 review script.
- Source-copy map: 148 rows.
- No whole Unity project、`Library/`、`Temp/`、`Logs/`、`obj/`、`.git/` 或 package cache was copied.

Final file/byte counts and ZIP SHA-256 are added by final validation below and reflected in `Manifests/File_Manifest.csv`.

## Current chain of evidence

```text
ContentPack.json
  unit.infantry.prefabId = PF_Unit_Infantry
        ↓
PrototypeUnitArtCatalog.InfantryResourcePath
  AegisRTS/Units/Infantry/PF_Unit_Infantry
        ↓
PF_Unit_Infantry.prefab
  Avatar + AC_Infantry + LODGroup + current master GUID
        ↓
SK_Infantry_A_v002.fbx
        ↑
CHR_Infantry_A_v002.blend + build_unit03_l3_blender.py
```

The source-export master FBX and Unity runtime master FBX have identical SHA-256; all four v002 source/runtime textures also match byte-for-byte.

## Review-ready evidence produced

- Neutral/material views：front、left、right、back、front 3/4、rear 3/4。
- Clay views：front、side、back、3/4。
- Wireframe views：front、3/4。
- Object manifest：23 objects with type、parent、bone、material、triangles、UV、modifier。
- Bone manifest：23 bones with parent and deform flag。
- Static Unity summary：Prefab components、renderers、anchors、controller、importers、events、materials and team color path。

## Validation result

At final packaging, validation must prove:

- required 9 top-level Markdown reports exist and are non-empty;
- all 13 required core category folders exist;
- current `.blend`, current runtime FBX, current Prefab/meta, Unity summary and Specifications exist;
- each `Source_Copy_Map.csv` row has an existing copied file with identical SHA-256 to its original at collection time;
- ZIP entries exactly match the source folder files and hashes;
- protected production-asset baseline hash is unchanged; and
- Git shows no production-asset path changes, commit or push.

## Final folder validation

```text
Package files: 180
Markdown files: 50
Required top-level reports missing: 0
Required category folders missing: 0
Empty files: 0
Markdown files with unmatched code fences: 0
Copied source rows: 148
Original/copy SHA-256 mismatches: 0
Visual evidence images indexed: 40
Important binary/Unity/image SHA-256 entries: 102
Specifications/source records collected: 45 files
Current BLEND / runtime FBX / Prefab: present / present / present
```

Protected source/runtime/code baseline remained 123 files／15,899,635 bytes with combined manifest SHA-256 `E1FE28875FBC49CFA206F5230BD903025970FE3A963D3E9603D29488104A4A17` before and after collection; relevant production paths have 0 Git changes.

ZIP was subsequently opened, every entry streamed and compared to the folder by relative path and SHA-256; entry count, required-file result and mismatch count are recorded in root `DevelopmentProgress.md` and the final Agent report. The ZIP cannot contain its own checksum without a circular value, so its SHA-256 is reported externally.

Missing visual/art decisions remain in `02_Missing_Data.md` rather than being auto-repaired.
