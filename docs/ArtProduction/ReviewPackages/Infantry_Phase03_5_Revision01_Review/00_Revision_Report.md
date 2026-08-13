# Phase 03.5 Revision 01 Report

Status: `READY FOR PHASE03_5 REVISION REVIEW`

## Source

- Input：`CHR_Infantry_A_v004_P035.blend`
- Input SHA-256：`234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`
- Output：`CHR_Infantry_A_v004_P035R1.blend`
- Output SHA-256：`A0CCC9771CD7A62D966891784745F138A9DCBF1230DF5E71A4F5D60900A84D0A`
- DCC：Blender `5.2.0 LTS`

## Scope

只修改 UpperArm／Forearm rest length與Elbow／Wrist landmark、Hand／Thumb比例、Head mesh、≤5% Neck refit、arm cloth／bracer／grip／weapon attachment，以及重建 review-only L1 Compare Pose。Hip、Knee、Torso、Chest、Shoulder origins／width、Helmet outer silhouette、Boot、Shield geometry／size、Sword geometry／length與Secondary Forms均保留。

## Corrections

- UpperArm：`0.154943H → 0.176000H`，肩點不動，左右完全對稱。
- Forearm：`0.154943H → 0.165000H`，Elbow／Wrist沿原rest axis延伸，左右完全對稱。
- Combined Shoulder→Wrist：`0.309886H → 0.341000H`，位於任務 `0.334–0.346H` 建議範圍。
- Hand width：`0.069423H → 0.060652H`；Hand length：`0.062856H → 0.057828H`，單次縮短8%。
- Head width：`0.126651H → 0.120000H`；Helmet width／outer silhouette不變；Neck只做4% width refit。
- UpperArm cloth與Forearm／Bracer沿對應bone axis延長；Elbow piece reposition；Shield與Sword只隨新palm landmark平移，Sword在review pose內重調角度以保留ground clearance。

## Result

最終 source 為 `1.824011 m`、98 meshes、16,858 vertices、33,248 triangles、23 bones、6 Material IDs。Topology為non-manifold 0、boundary 0、loose 0、zero-area 0。P035、P03R1與更早來源均未覆寫；正式Runtime Prefab未連接或替換。

