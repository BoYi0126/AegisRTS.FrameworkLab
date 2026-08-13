# Infantry Remaster — Phase 03.5 L1 Proportion & Pose Alignment Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Stage:** Phase 03.5 — L1 Proportion & Pose Alignment Gate  
> **Approved Secondary Forms Baseline:** `CHR_Infantry_A_v004_P03R1.blend`  
> **Output Candidate:** `CHR_Infantry_A_v004_P035.blend`  
> **Primary Visual Reference:** `Unit_03_Infantry_L1_Concept_Final.png`  
> **Status Goal:** `READY FOR PHASE03_5 REVIEW`  
> **Next Stage if PASS:** Phase 04 — UV / Texture / Material / Team Color  
>
> **核心原則：**
>
> 本階段不是再增加裝甲細節，而是要確認並修正：
>
> 1. L1 與 3D 的人體比例是否一致。
> 2. A-Pose 造成的視覺差異有多少。
> 3. 手臂、肩膀、頭、軀幹、腿的真實比例是否需要修正。
> 4. 建立一個「L1 Comparison Pose」來區分：
>    - Pose 差異
>    - Geometry / Proportion 差異
>
> **禁止在這一關通過前開始 Final UV 或 Final Texture。**

---

# 0. 為什麼需要 Phase 03.5

目前 Phase 03 Revision 01 已完成：

- Front Waist Cloth 修正
- Scarf 修正
- Upper Arm cloth form 修正
- Shield Back 修正
- Boots integration 修正
- Unity RTS Review

Secondary Forms 可接受。

但 Reviewer 仍觀察到：

```text
L1 Concept
vs
Current 3D
```

存在明顯的人體 / 姿態視覺差異。

尤其：

- 手臂張開角度差異巨大
- Arm silhouette 與 L1 差異明顯
- 肩部總寬度可能偏大
- Armor width 與 Body shoulder width 尚未拆開檢查
- 頭盔 / Head proportion 需要量測確認
- Torso / Leg proportion 需要量測確認
- Hands / Boots 的視覺尺寸需要量測確認
- A-Pose 可能放大了上述差異

因此不能直接判斷：

```text
Model proportion wrong
```

也不能直接假設：

```text
It's only A-Pose
```

必須量化。

---

# 1. Phase 03 Revision 01 狀態

以下視為通過，不需要再次大改：

```text
Secondary Chest Structure
Shoulder Armor Structure
Helmet Construction
Scarf Secondary Form
Front Waist Cloth Secondary Form
Shield Front
Shield Back Functional Hierarchy
Sword Structure
Boot Secondary Integration
Material ID Plan
Overall Heavy Infantry Identity
```

Phase 03.5 不應把本階段變成另一次 Secondary Forms 重建。

---

# 2. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
CHR_Infantry_A_v003.blend
CHR_Infantry_A_v003_P02R1.blend
CHR_Infantry_A_v004.blend
CHR_Infantry_A_v004_P03R1.blend
```

建立：

```text
CHR_Infantry_A_v004_P035.blend
```

原 `P03R1` 必須完整保留。

---

# 3. 不進 Phase 04

本階段禁止：

- Final UV unwrap
- Final UV packing
- Final Texture
- BaseColor painting
- Normal bake
- AO bake
- ORM bake
- Final Team Color Mask
- Final Shader
- Final Skinning
- Animation Polish
- LOD finalization

比例未鎖定前不得開始這些工作。

---

# 4. 本階段分成兩個子階段

必須按順序執行：

```text
Phase 03.5-A
DIAGNOSE
↓
Overlay + Landmark + Measurement
↓
Phase 03.5-B
CORRECT
↓
Only correct measured mismatch
```

禁止跳過 Diagnostic 直接憑感覺縮放。

---

# 5. Phase 03.5-A — 建立兩種 Pose

必須明確區分：

## Pose A — Production Source A-Pose

這是正式 Source Neutral Pose。

用途：

- Rigging
- Skinning
- Humanoid
- Modeling

不得為了看起來像 L1 而破壞 A-Pose。

命名建議：

```text
POSE_SOURCE_A
```

---

# 6. Pose B — L1 Comparison Pose

建立一個：

```text
POSE_L1_COMPARE
```

只供 Review。

目的：

> 將角色暫時擺成接近 L1 正面 Concept 的自然持盾 / 持劍姿勢，用來排除 A-Pose 帶來的視覺誤差。

這不是 Final Idle Animation。

---

# 7. L1 Comparison Pose 原則

參考 L1 正面：

- Torso upright
- Head forward
- Shoulders relaxed
- Upper arms 靠近身體
- Elbows 輕微彎曲
- Left shield 靠近 torso
- Right sword arm 自然垂下
- Sword 斜向下
- Legs 約肩寬或略小
- Feet 穩定站立

---

# 8. 不要求 Pose 完全複製畫面

L1 Concept 是展示圖，不是 rig specification。

因此不要要求：

```text
pixel-perfect joint pose
```

只需要：

> 接近相同的 presentation stance，使人體比例可被公平比較。

---

# 9. L1 Compare Pose 不修改 Geometry

先只：

```text
Pose bones
```

不要先改 Mesh。

第一輪 Comparison 必須使用：

```text
unchanged P03R1 geometry
+
temporary L1 comparison pose
```

這是為了判斷：

> 差異到底有多少只是 Pose。

---

# 10. 建立 Source A-Pose Capture

輸出：

```text
Apose_Front.png
Apose_Left.png
Apose_Back.png
Apose_3Q.png
```

---

# 11. 建立 L1 Compare Pose Capture

輸出：

```text
L1Pose_Front.png
L1Pose_Left.png
L1Pose_Back.png
L1Pose_3Q.png
```

---

# 12. Camera 必須使用 Orthographic

比例分析禁止 Perspective distortion。

使用：

```text
Orthographic Front
Orthographic Left
Orthographic Back
```

3/4 可以使用：

```text
Orthographic 3/4
```

---

# 13. Feet / Height Alignment

所有 Overlay：

必須先對齊：

```text
Ground / foot sole
```

再將總人物高度 normalize。

不要：

- 任意 stretch X/Y
- 用非等比例縮放讓圖看起來接近

只允許：

```text
uniform scale
+
translation
```

用於 image comparison。

---

# 14. L1 Explicit Dimensions

L1 已明確提供：

## Character

```text
Height: 1.75–1.85 m
```

目前 Production Target：

```text
≈ 1.824 m
```

保持。

---

# 15. L1 Shield

```text
Height: 0.75–0.95 m
Width: 0.55–0.65 m
```

Current shield 必須重新量測並記錄。

如果在範圍：

> 不因視覺感覺任意縮放。

---

# 16. L1 Sword

```text
Overall length: 0.90–1.10 m
```

Current sword 必須重新量測。

如果在範圍：

> 原則上保留。

---

# 17. L1 Character Width

L1 正面標示：

```text
approximately 0.60–0.72 m
```

請先確認此標註在原設計中的意義。

優先視為：

```text
main character / armored body width
```

不包含：

- extended sword
- extended shield projection

如果文件 / image context 支持。

---

# 18. Body vs Armor Width 必須分開

非常重要。

Agent 必須量：

```text
Anatomical Shoulder Width
Armored Shoulder Width
```

不能只量最外層 Shoulder Armor。

因為：

```text
large shoulder armor
≠
wide human skeleton
```

---

# 19. 必須建立 Landmark System

建立以下垂直 Landmark：

```text
L00 Ground
L01 Ankle
L02 Knee
L03 Crotch / Hip Joint
L04 Belt / Waist
L05 Chest Center
L06 Shoulder Joint
L07 Chin
L08 Head Top
L09 Helmet Top
L10 Plume Top
```

---

# 20. Arm Landmarks

左右各量：

```text
Shoulder Joint
Elbow Joint
Wrist Joint
Palm Center
Hand End
```

---

# 21. Horizontal Measurements

至少：

```text
Head width
Helmet width
Neck width
Anatomical shoulder width
Armored shoulder width
Chest width
Waist width
Hip width
Upper arm max width
Forearm max width
Hand width
Thigh width
Calf width
Boot width
```

---

# 22. Segment Lengths

至少量：

```text
UpperArm length
Forearm length
Hand length
Torso length
UpperLeg length
LowerLeg length
Foot length
```

---

# 23. Normalized Ratios

所有人體量測除了 meter 值，也要輸出：

```text
Measurement / Character Height
```

例如：

```text
ShoulderWidth / Height
UpperArmLength / Height
ForearmLength / Height
LegLength / Height
```

目的：

避免因 image scale 差異造成誤判。

---

# 24. L1 Landmark Measurement

請從 L1 Front / Side 圖：

以 Pixel Coordinate 建立 landmark。

需要保存：

```text
L1_Landmarks_Front.json
L1_Landmarks_Side.json
```

格式例如：

```json
{
  "image_width": 1254,
  "image_height": 1254,
  "ground_y": 454,
  "head_top_y": 145,
  "shoulder_y": 210
}
```

以上數字只是格式示例。

**不得直接使用示例數字。**

必須實際量取。

---

# 25. 不確定 Landmark

L1 裝甲會遮住 Joint。

如果無法精準判斷：

標記：

```text
confidence: low
```

或：

```text
estimated: true
```

不要假裝精確。

---

# 26. 3D Landmark Measurement

從 Blender Armature / Mesh 直接讀取世界座標。

優先使用：

```text
Bone head/tail positions
```

對：

- shoulder
- elbow
- wrist
- hip
- knee
- ankle

比從 render 猜測更可靠。

---

# 27. Landmark Report

建立：

```text
01_L1_vs_3D_Landmark_Report.md
```

至少表格：

| Metric | L1 Normalized | 3D Normalized | Difference | Confidence | Action |
|---|---:|---:|---:|---|---|

---

# 28. Difference Threshold

使用以下初始 Gate：

## Critical Structural Landmark

例如：

- Shoulder Y
- Elbow Y
- Wrist Y
- Hip Y
- Knee Y

若 normalized difference：

```text
> 3% of total height
```

標：

```text
REVIEW
```

---

# 29. Segment Ratio Threshold

例如：

```text
UpperArm
Forearm
Torso
UpperLeg
LowerLeg
```

若與 L1 Estimate 差：

```text
> 8%
```

標：

```text
CORRECTION CANDIDATE
```

---

# 30. Width Threshold

例如：

```text
Head
Shoulder
Chest
Waist
Boot
```

若視覺 / normalized width 差：

```text
> 8%
```

標：

```text
CORRECTION CANDIDATE
```

但 Armor Width 必須依 L1 Explicit Dimension 優先。

---

# 31. 不要機械式套 Threshold

Threshold 是 Review 警示。

不是：

```text
7.9% = correct
8.1% = automatically wrong
```

最後仍需對照 L1 Visual Identity。

---

# 32. Overlay 必須有兩組

## Overlay A

```text
L1
vs
Current 3D A-Pose
```

目的：

顯示現在使用者看到的差距。

---

# 33. Overlay B

```text
L1
vs
Current 3D L1 Compare Pose
```

目的：

排除 Pose 差異。

這一張非常重要。

---

# 34. Overlay Alpha

建議：

```text
L1: 50%
3D: 50%
```

或使用：

```text
L1 outline
+
3D solid
```

必須容易辨識差異。

---

# 35. Front Overlay

至少輸出：

```text
Overlay_Apose_Front.png
Overlay_L1Pose_Front.png
```

---

# 36. Side Overlay

至少：

```text
Overlay_Apose_Side.png
Overlay_L1Pose_Side.png
```

---

# 37. Back Overlay

至少：

```text
Overlay_Apose_Back.png
Overlay_L1Pose_Back.png
```

---

# 38. 3/4 Comparison

3/4 不要求 Pixel Overlay。

可以：

```text
Side-by-side
```

用途：

- volume
- silhouette
- equipment relationship

---

# 39. Annotated Comparison

建立：

```text
Annotated_L1_vs_3D_Front.png
```

標示：

- head
- shoulder
- elbow
- wrist
- belt
- hip
- knee
- ankle

---

# 40. Diagnostic Result 分類

每一項只能標：

```text
POSE_ONLY
PROPORTION_MISMATCH
ARMOR_MISMATCH
WITHIN_TOLERANCE
UNCERTAIN
```

---

# 41. Phase 03.5-B — 修正順序

Diagnostic 完成後才開始修改。

修正優先順序：

```text
Skeleton / Body Landmarks
↓
Body Mesh
↓
Armor fit
↓
Cloth fit
↓
Weapon / Shield attachment
```

不要反過來。

---

# 42. Overall Height Lock

任何修正後：

```text
Character total body height ≈ 1.824 m
```

Target tolerance：

```text
± 0.01 m
```

Plume 可另計。

---

# 43. Head / Helmet

不要因現在看起來「頭很大」就直接縮。

先量：

```text
Head width / Height
Helmet width / Height
Helmet height / Height
```

如果 mismatch 才修改。

---

# 44. Shoulder Width

這是本 Phase 高優先。

必須分：

```text
Skeleton shoulder width
Body shoulder width
Armor shoulder width
```

如果：

```text
Skeleton/body too wide
```

修改骨架與人體。

如果只是：

```text
Armor too wide
```

只修改 armor。

不要一起縮。

---

# 45. Upper Arm Length

若 Diagnostic 顯示 UpperArm 過長：

同步修改：

```text
UpperArm bone
UpperArm mesh
Shoulder armor clearance
Elbow position
```

---

# 46. Forearm Length

若 Forearm 過長 / 過短：

同步：

```text
LowerArm bone
Forearm mesh
Bracer
Wrist
```

---

# 47. Arm Thickness

Arm thickness 不只比較 armor。

分開：

```text
Sleeve thickness
Anatomical arm volume
Bracer volume
```

目標是：

> L1 看起來是裝甲 / 布料帶來的厚重，而不是人體本身像巨型圓柱。

---

# 48. Hand Size

Current hands 需要量測。

如果相對 L1：

```text
too large
```

才縮。

不要因 RTS readability 把手縮得過小。

Recommended correction：

```text
small controlled adjustment
```

避免一次 > 10%。

---

# 49. Torso Length

必須確認：

```text
Shoulder → Hip
```

相對總高比例。

如果 current torso 過短：

不要只把腿縮短來補。

應調整：

- spine landmark
- waist position
- hips

保持總高。

---

# 50. Leg Length

確認：

```text
Hip → Ground
```

以及：

```text
UpperLeg
LowerLeg
```

分別比較。

---

# 51. Knee Height

Knee 是非常重要 Landmark。

Current knee position 若與 L1 normalized height 差異 > Gate：

必須修。

---

# 52. Boot Size

Boot 已完成 Secondary integration。

Phase 03.5 只檢查：

```text
Boot width
Boot length
Boot height
```

相對人物比例。

如果比例合理：

不改。

---

# 53. Shield Size

L1：

```text
0.75–0.95 m height
0.55–0.65 m width
```

Current 必須以 meter 實測。

若範圍內：

原則上：

```text
PRESERVE
```

不要為了 Overlay 亂縮。

---

# 54. Shield Position

Shield Size 與 Shield Pose 必須分開。

如果 Compare Pose 中看起來差異主要來自：

```text
shield rotation / arm pose
```

只修 Compare Pose。

不要修 geometry。

---

# 55. Sword Size

L1：

```text
0.90–1.10 m
```

若 Current 在範圍：

```text
PRESERVE
```

只調 Compare Pose angle。

---

# 56. Source A-Pose 必須保留

非常重要。

Phase 03.5 修完比例後：

仍要留下正式：

```text
SOURCE A-POSE
```

不要把 L1 的站姿存成 bind pose。

---

# 57. A-Pose 建議

Production A-Pose：

- Upper arm downward from horizontal
- Shoulder relaxed
- Elbow slightly bent or near neutral
- Symmetrical body base

依目前 Humanoid pipeline。

不要求看起來像 Idle。

---

# 58. L1 Compare Pose 是 Review Pose

可以存在：

```text
Pose Library
```

或：

```text
Review Action
```

但必須明確命名：

```text
REVIEW_ONLY
```

不要誤匯出成正式 Idle。

---

# 59. Idle Pose 留 Phase 06

真正：

```text
Idle
Combat Idle
Shield stance
Sword stance
```

在：

```text
Phase 06 Animation Polish
```

處理。

Phase 03.5 只做比例與 Review Pose。

---

# 60. Armor Refit

人體比例修改後：

需重新 fit：

- Chest armor
- Shoulder armor
- Bracer
- Waist armor
- Boots
- Scarf

但：

> 不重新設計 Secondary Forms。

只做：

```text
refit / reposition / controlled scale
```

---

# 61. Cloth Refit

Scarf / Waist Cloth 已通過 Phase 03 Revision。

如果 Body 改變：

只允許：

```text
fit adjustment
```

不得退回原本 tube / plate 形狀。

---

# 62. Shield Grip Refit

如果 Arm landmark 改：

Shield Back：

- grip
- forearm strap
- brace

需重新 fit 左手。

保持已通過功能邏輯。

---

# 63. Triangle Budget

目前：

```text
≈ 33,248 tris
```

Phase 03.5 不應大幅增加。

Target：

```text
31K–35K
```

合理。

比例修改不需要增加細節。

---

# 64. Bone Count

保持：

```text
23 bones
```

除非專案 Humanoid Contract 有必要。

禁止為了比例修正新增 bones。

---

# 65. Bone Names / Hierarchy

嚴格保留：

```text
Bone names
Hierarchy
Humanoid mapping contract
Weapon socket contract
Shield socket contract
```

---

# 66. Skeleton Edit

如果需要調 Joint：

可以修改：

```text
Edit Bone position
```

但不要：

- rename
- delete required bone
- break hierarchy

---

# 67. Skinning

Final Skinning 尚未進行。

本 Phase 只需確保：

```text
temporary binding follows revised skeleton
```

正式 Weight Polish：

```text
Phase 05
```

---

# 68. Diagnostic Before / After Table

建立：

```text
02_Proportion_Correction_Table.md
```

格式：

| Metric | L1 | Before | After | Correction |
|---|---:|---:|---:|---|

---

# 69. 必須寫明哪些沒有改

例如：

```text
Shield size: PRESERVED
Sword size: PRESERVED
Chest detail: PRESERVED
Helmet construction: PRESERVED
```

避免 Agent 每次都重做。

---

# 70. Before / After A-Pose

輸出：

```text
Before_Apose_Front.png
After_Apose_Front.png

Before_Apose_3Q.png
After_Apose_3Q.png
```

---

# 71. Before / After L1 Compare Pose

輸出：

```text
Before_L1Pose_Front.png
After_L1Pose_Front.png

Before_L1Pose_3Q.png
After_L1Pose_3Q.png
```

---

# 72. Final L1 Overlay

必須：

```text
Final_Overlay_L1Pose_Front.png
Final_Overlay_L1Pose_Side.png
Final_Overlay_L1Pose_Back.png
```

---

# 73. Final A-Pose Captures

輸出：

```text
Final_Apose_Front
Final_Apose_Left
Final_Apose_Back
Final_Apose_3Q
```

這是後續 Rig / UV Source Reference。

---

# 74. Final Comparison Pose Captures

輸出：

```text
Final_L1Pose_Front
Final_L1Pose_Left
Final_L1Pose_Back
Final_L1Pose_3Q
```

---

# 75. Unity Review

建立隔離 Review-only：

```text
PF_Unit_Infantry_P035_Review
```

不要替換正式 Runtime Prefab。

---

# 76. Unity 必須提供兩組 Review

## A-Pose

至少：

```text
Unity_Apose_Close.png
```

## L1 Compare Pose / Neutral Presentation Pose

至少：

```text
Unity_L1Pose_Close.png
Unity_L1Pose_RTS_Normal.png
```

---

# 77. Unity RTS Camera

使用與 Phase 03 Revision 相同或可比較的：

```text
Perspective
35° FOV
≈ 7.5 m normal distance
```

若專案實際 Camera 有更正式設定：

以正式設定為準。

---

# 78. Unity Normal RTS Gate

Review：

- body proportion
- head size
- shoulder width
- arms
- shield/body relationship
- leg length
- boots
- silhouette

---

# 79. 64 px Review

Final L1 Compare Pose：

輸出：

```text
64 px
32 px
```

確認比例修正沒有破壞 readability。

---

# 80. Phase 03.5 Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_5_L1_Proportion_Alignment_Review/
```

---

# 81. Package Structure

```text
Infantry_Phase03_5_L1_Proportion_Alignment_Review/
│
├─ README.md
├─ 00_Phase03_5_Report.md
├─ 01_L1_vs_3D_Landmark_Report.md
├─ 02_Proportion_Correction_Table.md
├─ 03_Pose_Difference_Report.md
├─ 04_Unity_Review_Status.md
├─ 05_Open_Issues.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004_P035.blend
│
├─ Measurements/
│  ├─ L1_Landmarks_Front.json
│  ├─ L1_Landmarks_Side.json
│  └─ 3D_Landmarks.json
│
├─ Screenshots/
│  ├─ Diagnostic/
│  ├─ Overlay/
│  ├─ Annotated/
│  ├─ Apose/
│  ├─ L1Pose/
│  ├─ Comparison/
│  ├─ ScreenSize/
│  └─ Unity/
│
└─ Manifests/
```

---

# 82. Pose Difference Report

建立：

```text
03_Pose_Difference_Report.md
```

至少回答：

1. 使用者目前看到的差距中，有多少主要來自 A-Pose？
2. 哪些差距在 L1 Compare Pose 下仍然存在？
3. 哪些是真正 Body Proportion mismatch？
4. 哪些只是 Armor silhouette 差異？
5. 哪些已在 tolerance 內，不應再改？

---

# 83. 不能只給結論

例如不能只寫：

```text
Arms look better.
```

需要：

```text
UpperArm ratio before:
L1 estimated:
After:
Action:
```

---

# 84. Phase 03.5 PASS Gate

Reviewer 會確認：

- [ ] 已建立公平的 L1 Comparison Pose。
- [ ] Source A-Pose 仍保留。
- [ ] 已建立 Front / Side / Back Overlay。
- [ ] 已量測 L1 landmarks。
- [ ] 已量測 3D bone landmarks。
- [ ] Body vs Armor shoulder width 已分開。
- [ ] UpperArm / Forearm ratio 已檢查。
- [ ] Elbow / Wrist landmark 已檢查。
- [ ] Torso / Hip / Knee / Ankle 已檢查。
- [ ] Head / Helmet width 已檢查。
- [ ] Boot size 已檢查。
- [ ] Shield / Sword 實際尺寸已驗證。
- [ ] 只修正實際 mismatch。
- [ ] Overall height 保持約 1.824 m。
- [ ] Secondary Forms 沒被破壞。
- [ ] L1 identity 更接近。
- [ ] Unity L1Pose RTS Normal 已提供。
- [ ] 64 px readability 保持。
- [ ] 原 P03R1 沒被覆寫。

---

# 85. FAIL 條件

以下任何一項都不能進 Phase 04：

- 直接把 L1 pose 當作正式 bind pose。
- 沒量測就憑感覺縮手臂。
- 用 Pose 掩蓋真正 Arm length mismatch。
- Body shoulder 過寬但只縮 Shoulder Armor。
- Armor 過寬卻去縮 Skeleton。
- 改完總身高明顯漂移。
- L1 Overlay 仍有明顯 torso / arm / leg 比例差異卻未說明。
- 為了比例修改破壞已通過 Chest / Shield / Scarf 等 Secondary Forms。
- 開始 Final UV / Texture。

---

# 86. Agent 不得自行宣告 PASS

最終只可：

```text
READY FOR PHASE03_5 REVIEW
```

Phase 03.5 是否 PASS：

由 Reviewer / 使用者決定。

---

# 87. PASS 後 Mesh Freeze

如果 Reviewer 通過：

將此 Candidate 視為：

```text
PRE-UV GEOMETRY LOCK
```

從 Phase 04 開始：

原則上不再修改：

- overall proportion
- limb length
- major armor silhouette
- shield size
- weapon size

除非發現重大錯誤。

---

# 88. 為什麼這一步很重要

Phase 04 開始：

```text
UV
Texture
Bake
Material
```

都依賴 Mesh。

如果人體比例在 Phase 04 後才修改：

可能造成：

- UV 重做
- texture rebake
- normal rebake
- skinning 重做
- material validation 重跑

因此 Phase 03.5 是：

> **正式鎖 Mesh 之前最後的人體 / 比例 Gate。**

---

# 89. ZIP

建立：

```text
Infantry_Phase03_5_L1_Proportion_Alignment_Review.zip
```

---

# 90. ZIP 驗證

必須確認至少有：

```text
README
Phase03_5 Report
Landmark Report
Correction Table
Pose Difference Report
P035 Blender
L1 Landmarks
3D Landmarks
Front Overlay
Side Overlay
Back Overlay
A-Pose captures
L1Pose captures
Unity L1Pose RTS Normal
```

---

# 91. Git Rule

禁止：

```text
git commit
git push
git reset --hard
```

允許：

```text
git status
```

---

# 92. Agent 最終回覆格式

## Diagnostic

```text
Main Pose Difference:
Main True Proportion Differences:
```

## Corrections

```text
Head:
Shoulder:
UpperArm:
Forearm:
Torso:
Hip:
Leg:
Hand:
Boot:
Shield:
Sword:
```

## Output

```text
Blend:
Review Folder:
ZIP:
```

## Unity

```text
A-Pose:
L1 Compare Pose:
RTS Normal:
```

## Status

```text
READY FOR PHASE03_5 REVIEW
```

---

# 93. 核心判斷原則

永遠區分：

```text
POSE
≠
PROPORTION
≠
ARMOR SILHOUETTE
```

例如：

```text
Arm looks too long
```

可能是：

1. A-Pose 張開造成視覺錯覺。
2. UpperArm bone 真的太長。
3. Forearm 真的太長。
4. Shoulder armor 讓肩點看起來向外。
5. Hand / Bracer 太大。

必須先量測，才能知道是哪一項。

---

# 94. 立即執行

請直接：

```text
Preserve P03R1
↓
Create P035 Candidate
↓
Create Source A-Pose Review
↓
Create L1 Comparison Pose
↓
Measure L1 Landmarks
↓
Measure 3D Bone / Mesh Landmarks
↓
Create Overlay
↓
Classify Pose vs Proportion vs Armor Difference
↓
Correct only measured mismatch
↓
Refit Armor / Cloth
↓
Recreate A-Pose + L1Pose Evidence
↓
Unity Review
↓
Package
↓
Report READY FOR PHASE03_5 REVIEW
```

不要進 Phase 04。
不要 Final UV。
不要 Final Texture。
不要 Final Skinning。
不要 Animation Polish。
不要 Git Commit。
不要 Git Push。
