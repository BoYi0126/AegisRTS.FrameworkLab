# Unity Review Status

Status: `READY FOR PHASE03_5 REVISION REVIEW`

- Unity：`6000.5.7f1`
- A-Pose Prefab：`Assets/AegisRTS/Review/InfantryPhase035Revision01/PF_Unit_Infantry_P035R1_Review.prefab`
- L1 Pose Prefab：`Assets/AegisRTS/Review/InfantryPhase035Revision01/PF_Unit_Infantry_P035R1_L1Pose_Review.prefab`
- Scene：`Assets/AegisRTS/Review/InfantryPhase035Revision01/SCN_Infantry_P035R1_Review.unity`
- A-Pose：`1.824011 m`、98 renderers、boot ground Y `0`。
- L1 Compare Pose：overall bounds `1.824011 m`、98 renderers、boot ground Y `0`、overall min Y `0`。
- Runtime Prefab replaced：`false`；P035 baseline review assets仍存在。

Graphics-enabled batch run輸出 `Unity_Apose_Close.png`、`Unity_L1Pose_Close.png`、`Unity_L1Pose_RTS_Normal.png`；RTS Normal使用Perspective、35° FOV、約7.5 m。另以既有P035 Close建立`Unity_P035_vs_P035R1_Close.png`。

初次P035R1 review pose因arm drop加長而讓preserved sword tip低於ground；該中間輸出未作final evidence。最終只調整review-only sword presentation angle，Unity final min Y已回到0，Sword geometry／1.061 m length完全不變。三張final captures與side-by-side均已目視檢查非空白、grounding、arm drop、head、grip與weapon relationship。

