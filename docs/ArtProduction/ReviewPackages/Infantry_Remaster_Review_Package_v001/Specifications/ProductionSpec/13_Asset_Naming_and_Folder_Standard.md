# 13 — Asset Naming and Folder Standard

- Specification Version：1.0
- Rule：優先沿用repository現有命名，避免另建`ANM/TEX/PFB`平行系統

## Current Prefixes

| Prefix | 用途 | Current example |
| --- | --- | --- |
| `CHR_` | Character source／master | `CHR_Infantry_A_v002` |
| `SK_` | Skinned master／LOD mesh | `SK_Archer_A_v001.fbx` |
| `SM_` | Static／separate equipment mesh | `SM_Infantry_Shield_LOD0` |
| `VEH_` | Vehicle／siege／mount assembly | `VEH_SiegeRam_Light_A_v001`（spec） |
| `BLD_` | Building | `BLD_FortressGate_A_v001`（spec） |
| `AN_` | Animation FBX／clip | `AN_Archer_Attack_Ranged.fbx` |
| `AC_` | Animator Controller | `AC_Infantry.controller` |
| `MAT_` | Material | `MAT_Archer_TeamColor.mat` |
| `T_` | Legacy/current texture | `T_Infantry_A_Normal_1K.png` |
| `PF_` | Runtime Prefab／stable prefabId | `PF_Unit_Archer` |
| `PRJ_` | Projectile | `PRJ_Arrow_Basic_v001` |

`PROPOSED` 新資產仍沿用上述prefix。若未來改用`TEX_`或`PFB_`，需一次性migration與reference update，不可混用。

## Naming Pattern

```text
CHR_[FactionOrSpecies]_[Unit]_[Variant]_v###
BLD_[FactionOrTheme]_[Building]_[Variant]_v###
VEH_[FactionOrTheme]_[Type]_[Variant]_v###
SK_[Asset]_[Variant]_v###
AN_[Unit]_[Action]_[Variant]
MAT_[Asset]_[Purpose]
T_[Asset]_[MapType]_[Resolution]
PF_[Category]_[Asset]
```

世界觀未定時使用穩定role token（如`Neutral`／`HUM`）而非自行發明faction名稱。檔名只用ASCII字母、數字、底線；版本三位數。

## Source／Export／Runtime Separation

```text
ArtSource/<Category>/<Role>/<Asset>/v###/
├─ Concepts/             # L1
├─ CharacterSheet/       # L2 production reference
├─ Source/               # .blend/.ma/.psd and rebuild scripts
├─ Models/               # reviewed exports
├─ Animations/
├─ Textures/
├─ Renders/              # DCC neutral/game-like
├─ UnityCaptures/
├─ Documentation/        # provenance, manifest, QA
└─ README.md

Assets/AegisRTS/Content/Shared/Art/<Category>/<Asset>/
├─ Models/
├─ Animations/
├─ Textures/
├─ Materials/
└─ Resources/AegisRTS/.../PF_*.prefab
```

- `ArtSource`不由Unity import，保存全部source與provenance。
- Runtime只複製遊戲需要且通過integration的export；Concept、UV、review render、Prompt不進`Assets`。
- Source version與Runtime Prefab ID分離：remaster可從v002升v003，但`PF_Unit_Infantry`保持stable。
- 不移動現有檔案作為本版規格的一部分；任何legacy cleanup另立reference-safe task。

## Socket／Anchor Naming

沿用`Socket_R_Hand`、`Socket_L_Hand`、`Socket_WeaponTip`、`Socket_Projectile`、`FX_Hit_Center`、`FX_Foot_L/R`、`SelectionAnchor`、`HealthBarAnchor`、`GroundContact`。新增名稱先搜尋現有同義契約。

