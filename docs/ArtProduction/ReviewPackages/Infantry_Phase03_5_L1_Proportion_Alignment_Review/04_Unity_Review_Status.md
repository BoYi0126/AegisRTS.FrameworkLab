# Unity Review Status

Status: `READY FOR PHASE03_5 REVIEW`

- Unity：6000.5.7f1
- A-Pose Prefab：`Assets/AegisRTS/Review/InfantryPhase035/PF_Unit_Infantry_P035_Review.prefab`
- L1 Pose Prefab：`Assets/AegisRTS/Review/InfantryPhase035/PF_Unit_Infantry_P035_L1Pose_Review.prefab`
- Scene：`Assets/AegisRTS/Review/InfantryPhase035/SCN_Infantry_P035_Review.unity`
- A-Pose：1.824011 m、98 renderers、boot ground Y 0。
- L1 Compare Pose：1.841581 m overall posed bounds、98 renderers、boot ground Y 0。
- Runtime Prefab replaced：false。

成果圖：`Unity_Apose_Close.png`、`Unity_L1Pose_Close.png`、`Unity_L1Pose_RTS_Normal.png`。三張均以 graphics-enabled hidden batch run產生並通過非空白目視 QA；RTS Normal 使用35° FOV、約7.5 m。

L1 Pose overall min Y為−0.017570 m，來源是 review-only 向下斜劍尖；靴底仍為0。這不代表角色離地，也不改Sword geometry。正式 Runtime Prefab、Animator、gameplay或正式Idle均未修改。

