# Phase 03.5 Report

Status: `READY FOR PHASE03_5 REVIEW`

## Source

- Input：`CHR_Infantry_A_v004_P03R1.blend`
- Input SHA-256：`C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31`
- Output：`CHR_Infantry_A_v004_P035.blend`
- Output SHA-256：`234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`

## Diagnostic

主要 Pose 差異是 source A-Pose 將雙臂與裝備拉到外側，放大手臂長度、肩寬與裝備距離的視覺感。未改幾何的 L1 Compare Pose 證明上臂／前臂長度本身不是首要問題。真正量測 mismatch 集中於 hip／knee 高度、upper-leg／lower-leg／torso ratio，以及 chest、head mesh、hand、boot 的局部寬度。

## Correction

以 piecewise vertical remap 保持地面、ankle、upper body、helmet 與 plume endpoints，將 hip 由 normalized 0.4414 調到 0.3564（L1 estimate 0.3634），knee 由 0.2859 調到 0.2495（L1 0.2578）。Chest／body X scale 0.94、head mesh uniform 0.93、hand uniform 0.93、boot X/Y 0.94、shoulder armor X 0.98。UpperArm／Forearm、Helmet construction、Shield、Sword 與總高保留。

## Result

1.824011 m、33,248 tris、98 meshes、23 bones、6 Material IDs。Topology audit 為 non-manifold 0、boundary 0、loose 0、zero-area 0。Source A-Pose 保留且 review action 未設為 active action；Secondary Forms 只做 refit／reposition，未重新設計。

