# Infantry Remaster — Phase 03.5 Revision 02  
# Shield Placement / Grip Alignment Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Stage:** Phase 03.5 — Revision 02  
> **Current Candidate:** `CHR_Infantry_A_v004_P035R1.blend`  
> **Revision Output:** `CHR_Infantry_A_v004_P035R2.blend`  
> **Decision:** `CONDITIONAL PASS → SHIELD ALIGNMENT REQUIRED`  
> **Primary Goal:** 修正盾牌在自然站姿 / L1 Compare Pose / Unity RTS View 中的持握高度、局部位置、旋轉與左手握持關係。  
> **Next Stage if PASS:** Phase 04 — UV / Texture / Material / Team Color  
>
> **Important**
>
> 本 Revision 不重做盾牌尺寸，不重做盾牌模型，不重做人體比例。
>
> 目前 Shield Geometry 尺寸約：
>
> ```text
> Width  ≈ 0.600 m
> Height ≈ 0.862 m
> ```
>
> 已位於 L1 規格：
>
> ```text
> Width  0.55–0.65 m
> Height 0.75–0.95 m
> ```
>
> 因此本次真正要修的是：
>
> ```text
> Shield placement
> Grip alignment
> Forearm strap
> Shield local offset
> Shield rotation
> Left arm presentation
> Pivot / socket only if necessary
> ```
>
> 禁止用縮放盾牌來掩蓋持握位置錯誤。

---

# 0. Reviewer 判定

目前 Phase 03.5 Revision 01 已通過以下項目：

```text
Overall character proportion
UpperArm length
Forearm length
Elbow landmark
Wrist landmark
Hand size
Head size
Hip / Knee
Torso
Chest
Shoulder width
Sword size
Shield size
Secondary Forms
```

但 Reviewer 重新比對：

```text
Unit_03_Infantry_L1_Concept_Final.png
vs
Unity_L1Pose_Close.png
```

確認：

> **盾牌本體尺寸合理，但自然持盾位置明顯偏低。**

目前 Unity L1 Compare Pose 中：

```text
Shield top
≈ lower torso / waist region

Shield center
≈ thigh region

Shield bottom
≈ lower leg region
```

而 L1 Reference 的視覺關係較接近：

```text
Shield top
≈ chest / upper torso

Shield center
≈ abdomen / waist

Shield bottom
≈ knee region
```

因此本次只修：

> **Shield Alignment**

---

# 1. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v004_P035.blend
CHR_Infantry_A_v004_P035R1.blend
```

建立：

```text
CHR_Infantry_A_v004_P035R2.blend
```

原 P035R1 保留為：

```text
APPROVED BODY PROPORTION BASELINE
```

---

# 2. 禁止重新修改人體比例

本 Revision 禁止修改：

```text
Head scale
Shoulder width
UpperArm length
Forearm length
Hand scale
Torso length
Hip position
Knee position
Leg length
Boot size
Character height
```

如果 Shield Alignment 無法在既有人體比例下完成：

在：

```text
04_Open_Issues.md
```

回報。

不要自行再次改人體比例。

---

# 3. Shield Geometry Size — LOCK

目前：

```text
Width  ≈ 0.600 m
Height ≈ 0.862 m
```

L1 Target：

```text
Width  0.55–0.65 m
Height 0.75–0.95 m
```

因此：

```text
Shield Scale = LOCKED
```

禁止：

- Scale X
- Scale Y
- Scale Z
- Remodel overall outline
- Change overall height
- Change overall width

Minor fitting adjustment to back grip / strap：

允許。

---

# 4. Shield Front Geometry — LOCK

目前已通過：

```text
Wood body
Metal rim
Boss
Front reinforcement
Outline
Curvature
```

禁止重新設計。

---

# 5. Shield Back Geometry — PRESERVE

目前 Shield Back 已建立：

```text
Forearm support / strap
Hand grip
Structural brace
```

本 Revision：

允許：

```text
reposition
rotate
minor length adjustment
minor strap spacing adjustment
```

禁止：

```text
complete redesign
```

---

# 6. Primary Visual Target

L1 Front Reference 中盾牌應形成：

```text
       Head
        O

      Chest
   ┌────────┐
   │ Shield │
   │        │
   │        │
   └────────┘
      Knee
```

而不是：

```text
       Head
        O

      Chest

      Waist
   ┌────────┐
   │ Shield │
   │        │
   └────────┘
      Calf
```

---

# 7. Shield Vertical Placement Target

以人物總高 `H` 做 normalize。

請先量測目前 L1 Compare Pose：

```text
ShieldTopY / H
ShieldCenterY / H
ShieldBottomY / H
```

同時從 L1 Reference 估測：

```text
L1_ShieldTopY / H
L1_ShieldCenterY / H
L1_ShieldBottomY / H
```

建立：

```text
01_Shield_Vertical_Alignment_Report.md
```

---

# 8. 不要只靠手動目測

本 Revision 必須量：

```text
Shield top
Shield center
Shield bottom

Left shoulder
Left elbow
Left wrist
Shield grip center
Forearm strap center
```

避免：

> 只是把盾牌往上拖一點。

---

# 9. Recommended Target Relationship

不要求 pixel-perfect。

但自然持盾姿勢下：

## Shield Top

應大致位於：

```text
upper abdomen
to
mid chest
```

偏防禦姿勢時可更高。

---

# 10. Shield Center

應大致對應：

```text
abdomen / waist
```

---

# 11. Shield Bottom

應大致落在：

```text
upper knee
to
slightly below knee
```

不要長期垂到小腿中下段。

---

# 12. Shield Grip Position

目前若 Grip 在 Shield 背面太靠上 / 太靠下：

可以重新定位。

目標：

> 左手握住後，盾牌中心不應自然掉到大腿中段。

---

# 13. Grip Functional Rule

Hand Grip 必須：

- 可由手掌正常握持
- 不穿 palm
- 不穿 shield body
- 不造成 wrist extreme rotation
- 不逼迫 elbow 進入怪異角度

---

# 14. Forearm Strap Position

Forearm Strap 必須與：

```text
Left forearm
```

自然對齊。

允許：

```text
move
rotate
slightly resize
```

但保持其已通過的功能結構。

---

# 15. Shield Hold Model

優先嘗試：

```text
Hand Grip
+
Forearm Strap
```

共同決定 Shield。

不要只把 Shield 固定在 Wrist 下方。

---

# 16. Shield Local Offset

本 Revision 核心調整之一。

需要重新檢查：

```text
Shield local position relative to left hand / shield socket
```

尤其：

```text
local Y
local Z
```

或專案實際軸向。

---

# 17. Shield Rotation

目前位置低也可能部分是 Rotation 導致。

允許調整：

```text
pitch
yaw
roll
```

但目標不是讓盾牌完全正對 Camera。

應保持：

> 自然持盾角度。

---

# 18. Front View Rotation

L1 Compare Pose 正面：

盾牌應該有：

```text
slight inward rotation
```

但仍能看清主要正面。

不要：

```text
perfect billboard
```

---

# 19. Side View Rotation

側面：

Shield 不應：

- 貼進 torso
- 完全離身體過遠
- 穿過 thigh
- 形成極端 wrist bend

---

# 20. Shield Distance From Torso

自然站姿：

Shield front plane 與 torso 應具有合理間距。

目標：

```text
close enough to defend
+
enough arm clearance
```

不要：

```text
shield hanging far outside body
```

---

# 21. Left Shoulder

人體比例已 LOCK。

不要為了提高 Shield：

```text
raise left shoulder bone unnaturally
```

盾牌要靠：

- elbow flex
- forearm orientation
- grip position
- shield local offset

修正。

---

# 22. Left Elbow

可以在 L1 Compare Pose 中：

```text
slightly flex
slightly raise
```

以支撐盾牌。

但 Source A-Pose：

不改 bind proportion。

---

# 23. Left Wrist

Wrist 不可：

```text
extreme bend
```

為了把盾牌抬高。

如果 Wrist 必須大幅扭曲：

表示 Grip / Strap / Offset 仍不合理。

---

# 24. L1 Compare Pose

本 Revision 主要針對：

```text
REVIEW_ONLY_POSE_L1_COMPARE
```

重新調整左側持盾姿勢。

右側 Sword arm：

```text
PRESERVE
```

除非只是為了全身 presentation 做極小 rotation。

---

# 25. Source A-Pose

Source A-Pose：

```text
PRESERVE
```

盾牌在 A-Pose 的 review placement 只需：

- 不穿模
- attachment 正確
- pivot / socket 正常

正式 gameplay position 仍由 animation 控制。

---

# 26. Shield Socket / Pivot

如果目前 Shield Root / Pivot 導致調整非常困難：

允許修：

```text
Shield pivot
Shield attachment transform
Shield socket offset
```

但：

不得改既有 naming contract。

---

# 27. Naming Contract

必須保留專案既有：

```text
Shield object name
Shield socket name
Left hand bone
Attachment naming
```

不得因 Revision 改 API / runtime contract。

---

# 28. Weapon Separation

Shield 仍為獨立物件。

禁止合併到：

```text
Body Mesh
```

---

# 29. Shield Back Grip vs Animation

本次是：

```text
Neutral / Review holding setup
```

Phase 06 仍會進一步做：

```text
Idle shield pose
Combat idle
Move
Attack
Hit
Death
```

但 Phase 06 不應再負責：

> 修正一個基礎持盾 attachment 就是錯的問題。

所以現在先鎖正確。

---

# 30. Diagnostic Before Capture

保留 P035R1：

```text
Before_Unity_L1Pose_Close.png
Before_L1Pose_Front.png
Before_L1Pose_3Q.png
```

---

# 31. Shield Alignment Measurements

建立：

```text
Measurements/Shield_Alignment_Before.json
Measurements/Shield_Alignment_After.json
```

至少：

```json
{
  "character_height_m": 1.824,
  "shield_height_m": 0.862,
  "shield_width_m": 0.600,
  "shield_top_y_normalized": 0.0,
  "shield_center_y_normalized": 0.0,
  "shield_bottom_y_normalized": 0.0,
  "left_shoulder_y_normalized": 0.0,
  "left_elbow_y_normalized": 0.0,
  "left_wrist_y_normalized": 0.0
}
```

`0.0` 為格式示例。

必須實際量測。

---

# 32. L1 Shield Estimate

另外建立：

```text
Measurements/L1_Shield_Alignment_Estimate.json
```

記：

```text
ShieldTop
ShieldCenter
ShieldBottom
```

以及：

```text
confidence
```

---

# 33. Comparison Report

建立：

```text
01_Shield_Vertical_Alignment_Report.md
```

表格：

| Metric | L1 Estimate | Before | After | Difference After |
|---|---:|---:|---:|---:|
| Shield Top / H | | | | |
| Shield Center / H | | | | |
| Shield Bottom / H | | | | |
| Grip Center / H | | | | |
| Wrist / H | | | | |

---

# 34. Shield Size Report

明確寫：

```text
Shield Size Before:
Shield Size After:
```

Expected：

```text
same
```

如果尺寸變了：

必須回報理由。

---

# 35. Blender Captures

輸出：

```text
01_L1Pose_Front.png
02_L1Pose_Left.png
03_L1Pose_Back.png
04_L1Pose_3Q_Front.png
05_L1Pose_3Q_Back.png
```

---

# 36. Shield Focus Captures

必須：

```text
Shield_Front_Focus.png
Shield_3Q_Focus.png
Shield_LeftSide_Focus.png
Shield_Back_WithArm.png
```

---

# 37. Grip Close-Up

必須：

```text
Shield_Grip_Close.png
```

Reviewer 要看：

- palm
- grip
- strap
- forearm

---

# 38. Before / After Comparison

建立：

```text
P035R1_vs_P035R2_Front.png
P035R1_vs_P035R2_3Q.png
P035R1_vs_P035R2_ShieldFocus.png
```

---

# 39. L1 Overlay

建立：

```text
Final_Overlay_L1_vs_P035R2_Front.png
```

另外：

```text
Final_Overlay_L1_vs_P035R2_ShieldFocus.png
```

主要比較盾牌：

- top
- center
- bottom

---

# 40. Overlay Normalization

L1 與 3D：

先對齊：

```text
ground
character height
```

不可 stretch。

---

# 41. Unity Review — Required

本 Revision 必須再跑 Unity。

---

# 42. Unity Review Prefab

建立：

```text
PF_Unit_Infantry_P035R2_Review
```

或等價 temporary review prefab。

禁止替換：

```text
PF_Unit_Infantry
```

---

# 43. Required Unity Captures

至少：

```text
Unity_L1Pose_Close.png
Unity_L1Pose_RTS_Normal.png
Unity_L1Pose_Far.png
```

---

# 44. Unity Before / After

必須建立：

```text
Unity_P035R1_vs_P035R2_Close.png
Unity_P035R1_vs_P035R2_RTS_Normal.png
```

---

# 45. Unity Close Gate

Close View：

盾牌應：

- 明顯比 P035R1 上移
- 保持合理握持
- 不遮住整個臉
- 不穿 torso
- 不掉到小腿

---

# 46. Unity RTS Normal Gate

正常遊玩距離：

盾牌應讀成：

> 前側防禦裝備

而不是：

> 垂掛在腿邊的大板子

---

# 47. Shield Top RTS Rule

在 Unity RTS Normal：

盾牌頂部應視覺上至少進入：

```text
lower chest / torso
```

區域。

如果仍只到：

```text
waist
```

Fail。

---

# 48. Shield Bottom RTS Rule

盾牌底部：

可以接近：

```text
knee
```

或略低。

但不要：

```text
mid / lower calf
```

---

# 49. Shield Center RTS Rule

盾牌 boss / 中心：

應大致與：

```text
abdomen / belt / upper hip
```

形成合理關係。

---

# 50. Silhouette Check

Shield 上移後：

重新確認：

- Head visible
- Sword readable
- Shield readable
- Legs still readable
- silhouette not overly merged

---

# 51. 64 px / 32 px

輸出：

```text
64px
32px
```

使用 L1 Compare Pose / gameplay presentation pose。

---

# 52. Shield at 64 px

64 px：

Shield 必須仍然：

```text
clear left-side defensive mass
```

---

# 53. Shield at 32 px

32 px：

不需要看到細節。

但：

```text
shield-bearing infantry
```

身份必須保留。

---

# 54. Collision / Clipping Check

至少檢查：

```text
Shield vs torso
Shield vs thigh
Shield vs knee
Shield vs shoulder armor
Shield grip vs hand
Shield strap vs forearm
```

---

# 55. Animation Clearance Preview

可以做簡單姿勢測試：

```text
neutral
slight combat raise
slight relaxed lower
```

但不是正式 animation。

用途：

確定新的 Grip / Strap 不只在一個 Pose 可用。

---

# 56. Shield Raise Range

簡單測試：

```text
+ small upward combat rotation
```

確定：

- 不撞 helmet
- 不撞 shoulder
- wrist 不爆

---

# 57. Shield Lower Range

簡單測試：

```text
relaxed lower position
```

確定：

- 不撞 knee
- 不穿 thigh

---

# 58. Geometry Budget

本 Revision 不應顯著改面數。

Current 約：

```text
33K tris
```

Target：

```text
same ± small grip / strap changes
```

---

# 59. No UV Work

仍禁止：

```text
Final UV
Final Texture
Final Bake
```

因為還沒正式 PRE-UV Lock。

---

# 60. Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_5_Revision02_Shield_Alignment_Review/
```

---

# 61. Package Structure

```text
Infantry_Phase03_5_Revision02_Shield_Alignment_Review/
│
├─ README.md
├─ 00_Revision_Report.md
├─ 01_Shield_Vertical_Alignment_Report.md
├─ 02_Shield_Grip_Alignment_Report.md
├─ 03_Unity_Review_Status.md
├─ 04_Open_Issues.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004_P035R2.blend
│
├─ Measurements/
│  ├─ Shield_Alignment_Before.json
│  ├─ Shield_Alignment_After.json
│  └─ L1_Shield_Alignment_Estimate.json
│
├─ Screenshots/
│  ├─ L1Pose/
│  ├─ ShieldFocus/
│  ├─ Grip/
│  ├─ Overlay/
│  ├─ Comparison/
│  ├─ ScreenSize/
│  └─ Unity/
│
└─ Manifests/
```

---

# 62. Revision Report

至少寫：

```text
Shield geometry size changed?
YES / NO

Shield local offset changed?
YES / NO

Shield rotation changed?
YES / NO

Grip changed?
YES / NO

Forearm strap changed?
YES / NO

Left arm pose changed?
YES / NO

Pivot / socket changed?
YES / NO
```

---

# 63. Grip Alignment Report

建立：

```text
02_Shield_Grip_Alignment_Report.md
```

說明：

- 手怎麼握
- Forearm 怎麼進 Strap
- Shield attachment hierarchy
- 是否有 clipping
- 是否需要 Phase 06 animation-specific adjustment

---

# 64. Open Issues

如果：

- L1 shield pose ambiguity
- Unity socket limitation
- rig constraint
- prefab import issue

記錄。

不要偷偷忽略。

---

# 65. PASS Gate

Reviewer 將確認：

- [ ] Shield 尺寸仍約 0.600 × 0.862 m。
- [ ] Shield Geometry 沒被不必要重做。
- [ ] Shield Top 明顯比 P035R1 高。
- [ ] Shield Top 進入胸腹防禦區域。
- [ ] Shield Bottom 不再落到小腿中下段。
- [ ] Shield Center 與 torso / abdomen 關係合理。
- [ ] 左手 Grip 可理解。
- [ ] Forearm Strap 可理解。
- [ ] Wrist 不需要極端扭轉。
- [ ] Elbow posture 自然。
- [ ] Shield 與 torso / thigh 無重大穿模。
- [ ] L1 Compare Pose 更接近 Concept。
- [ ] Unity Close 更自然。
- [ ] Unity RTS Normal 看起來像持盾防禦，而不是垂盾。
- [ ] 64 px Readability 保持。
- [ ] 人體比例沒有被再次修改。
- [ ] Sword 沒被不必要修改。
- [ ] 原 P035R1 未被覆寫。

---

# 66. FAIL 條件

不能進 Phase 04，如果：

- 用縮放 Shield 解決位置問題。
- Shield Top 還停在腰部。
- Shield Bottom 仍在小腿中下段。
- Wrist 嚴重扭曲。
- Grip 不在手掌。
- Strap 穿 Forearm。
- Shield 穿 torso。
- 為了 Shield 再次改 Arm length。
- 為了 Shield 改 Character Height。
- 開始 Final UV / Texture。

---

# 67. PASS 後正式 PRE-UV LOCK

如果 Reviewer PASS：

```text
CHR_Infantry_A_v004_P035R2
```

正式定義為：

# PRE-UV GEOMETRY LOCK

從 Phase 04 開始：

原則上不再修改：

```text
Character proportion
Head size
Arm length
Leg length
Shield size
Shield base attachment
Sword size
Major armor silhouette
```

---

# 68. Phase 06 的責任

Phase 06 仍會決定：

```text
Idle shield height
Combat idle shield height
Move shield behavior
Attack shield reaction
Hit
Death
```

但 Phase 06 不負責修：

```text
wrong socket / wrong grip / fundamentally low shield attachment
```

---

# 69. Agent 最終狀態

只能：

```text
READY FOR PHASE03_5 REVISION02 REVIEW
```

不要自行宣告：

```text
PASS
```

---

# 70. ZIP

建立：

```text
Infantry_Phase03_5_Revision02_Shield_Alignment_Review.zip
```

---

# 71. ZIP Verification

至少有：

```text
CHR_Infantry_A_v004_P035R2.blend
Revision Report
Shield Alignment Report
Grip Alignment Report
Before / After Measurements
L1 Overlay
Shield Focus
Grip Close-up
Unity Close
Unity RTS Normal
Unity Before / After
```

---

# 72. Git Rule

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

# 73. 最終核心要求

這次不是：

> 把盾牌做大或做小。

而是：

> **保持已驗證正確的 Shield 尺寸與 Geometry，修正其持握高度、Grip / Strap、Local Offset 與 Rotation，使它在 L1 Compare Pose 與實際 Unity RTS Camera 下真正讀成「架在身體前方的重裝步兵盾牌」。**

---

# 74. 立即執行

```text
Preserve P035R1
↓
Create P035R2
↓
Measure Current Shield Alignment
↓
Measure L1 Shield Alignment
↓
Adjust Grip / Strap
↓
Adjust Shield Local Offset
↓
Adjust Shield Rotation
↓
Adjust Left Arm Review Pose only as needed
↓
Check Wrist / Elbow
↓
Check Clipping
↓
Generate L1 Overlay
↓
Generate Unity Close / RTS Normal
↓
Generate Before / After
↓
Package ZIP
↓
Report READY FOR PHASE03_5 REVISION02 REVIEW
```

不要進 Phase 04。
不要 Final UV。
不要 Final Texture。
不要改 Shield 尺寸。
不要改人體比例。
不要 Git Commit。
不要 Git Push。
