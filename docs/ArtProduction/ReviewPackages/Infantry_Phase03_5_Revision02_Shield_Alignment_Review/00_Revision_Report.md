# Phase 03.5 Revision 02 Report

Status: `READY FOR PHASE03_5 REVISION02 REVIEW`

## Source

- Input：`CHR_Infantry_A_v004_P035R1.blend`
- Input SHA-256：`A0CCC9771CD7A62D966891784745F138A9DCBF1230DF5E71A4F5D60900A84D0A`
- Output：`CHR_Infantry_A_v004_P035R2.blend`
- Output SHA-256：`D8DCD84D888204D65385A94CF15B0C07BEA227236D47EA5EC3D54992999E551D`
- DCC：Blender `5.2.0 LTS`

## Change Declaration

| Item | Changed? | Detail |
|---|---|---|
| Shield geometry size | NO | `0.600000 × 0.862424 m` before and after |
| Shield front geometry | NO | wood body、rim、boss、reinforcement與outline均只作剛體 placement |
| Shield local offset | YES | base local `+0.080 m`；L1 review attachment另加 `+0.113 m` |
| Shield rotation | YES | review-only pitch `-3°`、inward `4°` |
| Grip | YES | 隨base offset後回調 `-0.040 m`，保持palm contact |
| Forearm strap | YES | 隨base offset後回調 `-0.060 m`，對齊左前臂 |
| Left arm pose | YES | review-only UpperArm `-10°`、LowerArm額外 `-20°` |
| Pivot / socket | NO | bone、object、attachment naming與hierarchy未更名或增刪 |
| Body proportion | NO | P035R1的head／arm／torso／leg比例與23-bone rest lengths保留 |
| Sword | NO | geometry、size與右側review presentation保留 |

## Result

最終 source 為 `1.824011 m`、98 meshes、16,858 vertices、33,248 triangles、23 bones。Topology audit為non-manifold 0、boundary 0、loose 0、zero-area 0。Source A-Pose的body／bone rest pose保留；盾牌base attachment上移，但正式 `PF_Unit_Infantry` 未替換。

