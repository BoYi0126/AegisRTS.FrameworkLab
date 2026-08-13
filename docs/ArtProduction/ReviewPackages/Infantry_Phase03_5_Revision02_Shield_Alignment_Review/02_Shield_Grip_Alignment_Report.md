# Shield Grip Alignment Report

Status: `READY FOR PHASE03_5 REVISION02 REVIEW`

- Hand grip：保留`GEO_Infantry_Shield_HandGrip_P03R1`命名與結構；盾牌base上移0.080 m後，握把相對回調0.040 m。L1 Compare Pose中手掌與垂直握把相交，沒有以wrist極端折彎追位置。
- Forearm strap：保留`GEO_Infantry_Shield_ForearmStrap_P03R1`；相對base回調0.060 m。背面與close-up可見strap橫跨左前臂上段。
- Left arm：只重建`REVIEW_ONLY_POSE_L1_COMPARE`，UpperArm為`-10°`、LowerArm追加`-20°`；肩點、rest lengths、hand size與Source A-Pose body不變。
- Shield attachment：物件仍分離於Body Mesh；object／bone naming、23-bone hierarchy與runtime socket contract未改。沒有新增臨時God object或runtime dependency。
- Clipping：Blender front／left／back／3Q、ShieldFocus、Grip close-up與Unity Close中未見盾牌穿入torso、thigh、knee或shoulder armor；hand／strap與arm的接觸是預期hold contact。
- Phase 06：本包只有neutral A-Pose與review-only compare pose。Idle／Combat Idle／Move／Attack／Hit／Death仍需在正式skin／animation階段個別驗證raise／lower range；不可把本次object-pose proof當成final deformation proof。

