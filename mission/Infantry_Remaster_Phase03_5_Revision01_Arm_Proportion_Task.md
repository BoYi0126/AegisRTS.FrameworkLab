# Infantry Remaster — Phase 03.5 Revision 01  
# Arm / Head / Hand Proportion Correction Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Stage:** Phase 03.5 — Revision 01  
> **Current Candidate:** `CHR_Infantry_A_v004_P035.blend`  
> **Revision Output:** `CHR_Infantry_A_v004_P035R1.blend`  
> **Decision:** `CHANGE REQUESTED`  
> **Primary Goal:** 修正已被量測與 Unity Review 證實仍存在的手臂、手掌與頭部比例差異。  
> **Next Stage if PASS:** Phase 04 — UV / Texture / Material / Team Color  
>
> **重要：**
>
> 本次不是重新做 Phase 03.5。
>
> Hip、Knee、Torso、Chest、Boot Width、Shield、Sword 等已經通過或已被修正到合理範圍的項目，原則上必須保留。
>
> 本 Revision 只處理：
>
> 1. UpperArm length
> 2. Forearm length
> 3. Elbow / Wrist presentation landmark
> 4. Hand size
> 5. Head mesh size
> 6. Arm-related armor / cloth / weapon refit
> 7. Final L1 Compare Pose + Unity validation

---

# 0. Reviewer 判定

目前 Phase 03.5 不直接 PASS。

原因：

目前報告已量測：

```text
UpperArm
L1 estimate: 0.1861 H
3D current: 0.1549 H
Relative gap: -16.8%

Forearm
L1 estimate: 0.1688 H
3D current: 0.1549 H
Relative gap: -8.2%
```

Agent 先前因 L1 joint 有部分遮擋而保留骨長。

但 Reviewer 重新檢查：

```text
L1 Compare Pose
+
Final Overlay
+
Unity L1Pose Close
```

後確認：

> 手臂差異不能再完全分類成 POSE_ONLY。

即使已使用接近 L1 的自然站姿：

- 手肘位置仍偏高。
- 手腕位置仍偏高。
- Sword hand 結束位置明顯高於 L1。
- 上臂與前臂總長度偏短。
- Hand 視覺體積仍過大。

因此：

```text
ARM LENGTH = REAL PROPORTION ISSUE
```

需要修正。

---

# 1. 已通過項目 — 原則上禁止再改

以下 P035 結果保留：

```text
Overall height ≈ 1.824 m
Hip Y
Knee Y
Torso ratio
UpperLeg ratio
LowerLeg ratio
Chest width
Anatomical shoulder width
Armored shoulder width
Helmet width
Helmet top
Boot width
Shield size
Sword size
Chest secondary forms
Shoulder armor design
Scarf design
Waist cloth design
Shield front/back construction
Material ID plan
23-bone hierarchy
```

除非 Arm revision 導致局部 fitting 必須調整。

---

# 2. 不再調 Hip / Knee

Current：

```text
Hip Y / H = 0.3564
L1 estimate = 0.3634

Knee Y / H = 0.2495
L1 estimate = 0.2578
```

這兩者已在 Gate 內。

本 Revision：

```text
PRESERVE
```

禁止再用低信心 Hip Landmark 進一步壓縮腿部。

---

# 3. 不再調 Torso Length

Current：

```text
Torso / H = 0.4011
L1 estimate = 0.4161
```

Gap 已縮小到合理程度。

本 Revision：

```text
PRESERVE
```

不要再延長 torso。

---

# 4. Arm Revision 必須先做「Posed Joint Measurement」

上一輪主要比較：

```text
bone segment length
```

本次還需要直接量：

```text
L1 Compare Pose 中
Shoulder
Elbow
Wrist
Hand End
```

的 world position。

建立：

```text
Measurements/3D_L1Pose_Arm_Landmarks_Before.json
```

以及修正後：

```text
Measurements/3D_L1Pose_Arm_Landmarks_After.json
```

---

# 5. L1 Front Arm Landmark

沿用既有 L1 Landmarks：

Sword Side：

```text
Shoulder
Elbow
Wrist
Palm
Hand End
```

Shield Side：

同樣。

如果 Shield 遮擋：

Sword-side arm 作主要比例依據。

Shield-side 作 secondary validation。

---

# 6. UpperArm Correction

Current：

```text
0.1549 H
```

L1 estimated：

```text
0.1861 H
```

因 Concept Joint 為估測，不直接硬拉到 0.1861。

Revision Target：

```text
0.172–0.180 H
```

Preferred first target：

```text
≈ 0.176 H
```

相對目前約：

```text
+11% ～ +16%
```

---

# 7. UpperArm 調整原則

修改：

```text
LeftUpperArm
RightUpperArm
```

bone length / elbow position。

保持：

- shoulder joint origin
- shoulder width
- chest width
- Humanoid hierarchy

不要：

```text
把 shoulder joint 向外移來假裝手臂變長
```

---

# 8. Forearm Correction

Current：

```text
0.1549 H
```

L1 estimated：

```text
0.1688 H
```

Revision Target：

```text
0.162–0.168 H
```

Preferred：

```text
≈ 0.165 H
```

相對目前約：

```text
+5% ～ +8%
```

---

# 9. Combined Arm Target

Shoulder → Wrist：

目前約：

```text
0.3099 H
```

L1 estimate 約：

```text
0.3549 H
```

Revision 建議：

```text
0.334–0.346 H
```

不要一次完全追到 Concept 極值。

原因：

- L1 joint 有 armor occlusion。
- RTS stylization 允許小幅差異。
- 需要保留 weapon handling。

---

# 10. Elbow Landmark Gate

L1 Compare Pose 修正後：

Sword-side elbow 的 normalized vertical landmark 應：

> 比目前更接近 L1。

如果仍相差：

```text
> 3% H
```

需在報告中說明。

---

# 11. Wrist Landmark Gate

Sword-side wrist 是本 Revision 最高優先。

修正後：

```text
Wrist vertical position
```

應明顯接近 L1。

不得再出現：

> L1 手腕已到大腿旁，但 3D 手腕仍停在腰部附近。

---

# 12. Hand End Gate

自然 L1 Compare Pose：

Sword hand 下緣應落在：

> 接近 L1 sword-hand 的相對高度。

不要求 pixel-perfect。

但視覺上不能再是：

```text
3D hand noticeably too high
```

---

# 13. Hand Width

Current：

```text
0.0694 H
```

L1 estimate：

```text
0.0497 H
```

Gap：

```text
+39.6%
```

目前仍過大。

但不能完全縮到 L1，避免破壞：

- Sword grip
- RTS readability

Revision Target：

```text
0.057–0.062 H
```

Preferred：

```text
≈ 0.060 H
```

相對目前大約：

```text
-10% ～ -15%
```

---

# 14. Hand Length

不只縮 width。

也檢查：

```text
Hand length / H
```

如果手掌整體仍顯得像大型拳套：

可以同步小幅縮短。

最大建議單次修正：

```text
≤ 10%
```

---

# 15. Thumb / Grip

Hand shrink 後：

必須重新 fit：

```text
Thumb
Sword grip contact
Shield hand grip
```

不能：

- floating hand
- grip through palm
- thumb detached

---

# 16. Head Width

Current：

```text
0.1267 H
```

L1 estimate：

```text
0.1118 H
```

Gap：

```text
+13.3%
```

Helmet width 已：

```text
PASS
```

因此：

> 不縮 Helmet。

只縮 Head mesh。

---

# 17. Head Target

Revision Target：

```text
0.118–0.122 H
```

Preferred：

```text
≈ 0.120 H
```

也就是保留一點 RTS stylization。

不要完全縮到 0.1118。

---

# 18. Helmet Clearance

Head 縮小後：

檢查：

- brow clearance
- side clearance
- chin
- neck
- helmet rim

禁止出現：

```text
head floating inside oversized helmet
```

必要時：

只做極小 Helmet interior / vertical fitting。

Helmet outer silhouette 不改。

---

# 19. Neck

Head 縮小後檢查 Neck。

如果 Neck 看起來過粗：

可小幅調整：

```text
≤ 5%
```

但不要破壞 Scarf。

---

# 20. Shoulder Width

Current anatomical shoulder：

```text
+6.4% vs L1 estimate
```

在 Gate 內。

本 Revision：

```text
PRESERVE
```

不要縮 skeleton shoulder width。

手臂視覺過寬主要先由：

```text
arm length + pose + armor
```

處理。

---

# 21. Shoulder Armor

Secondary Form 已 PASS。

Arm bone 長度修改後：

只允許：

```text
clearance refit
```

不要：

- 重設計
- 加新層
- 變寬
- 變窄超過必要值

---

# 22. UpperArm Mesh Refit

Bone 變長後：

UpperArm cloth / sleeve 必須重新拉長。

保持 Phase 03 已通過：

- broad folds
- cloth planes
- non-cylinder form

不要拉伸成：

```text
long smooth tube
```

---

# 23. Elbow Transition

重新建立：

```text
UpperArm
↓
Elbow
↓
Forearm
```

節奏。

Elbow 不應因 bone stretch：

- collapsed
- too thin
- too round

---

# 24. Bracer Refit

Forearm 增長後：

Bracer 可以：

- reposition
- proportionally extend slightly

但不要讓 Bracer 變成整個 Forearm。

保留 L1 cloth / armor ratio。

---

# 25. Shield Arm

左側 Shield Arm 修改後：

重新 fit：

```text
Forearm strap
Hand grip
Shield position
```

Shield geometry / size：

```text
PRESERVE
```

---

# 26. Sword Arm

右側 Sword Arm 修改後：

重新 fit：

```text
Sword grip
Hand
Guard clearance
Sword angle
```

Sword geometry / length：

```text
PRESERVE
```

---

# 27. L1 Compare Pose 必須重建

舊：

```text
REVIEW_ONLY_POSE_L1_COMPARE
```

不可直接假設仍正確。

Arm bone 長度改後：

重新調整 rotation，使姿勢保持：

- relaxed shoulder
- arms close to body
- sword downward
- shield near torso

---

# 28. Source A-Pose

正式 Source：

仍保持：

```text
A-Pose
```

不要把 Compare Pose 當 bind pose。

---

# 29. A-Pose Arm Angle

Arm length 修改後：

A-Pose angle 保持 Production Neutral。

不要因為 arm 長了：

```text
把手臂張更開來躲 clipping
```

應該修裝甲 clearance。

---

# 30. Lower Body

本 Revision 禁止修改：

```text
Hip
Knee
Ankle
UpperLeg length
LowerLeg length
Boot width
```

除非出現由 Arm refit 完全無關的 critical bug。

---

# 31. Chest

Current chest width：

```text
+3.4% vs L1
```

PASS。

不再縮。

---

# 32. Shield

Current：

```text
0.600 × 0.862 m
```

L1 Spec：

```text
0.55–0.65 × 0.75–0.95 m
```

PASS。

不改尺寸。

---

# 33. Sword

Current：

```text
1.061 m
```

L1 Spec：

```text
0.90–1.10 m
```

PASS。

不改尺寸。

---

# 34. Geometry Budget

Current：

```text
33,248 tris
```

Revision Target：

```text
32K–35K
```

Arm refit 不應增加大量 geometry。

---

# 35. Bone Count

保持：

```text
23
```

禁止增加 Bone。

---

# 36. Bone Hierarchy

保留：

```text
names
hierarchy
Humanoid mapping
socket contract
```

只改必要 bone endpoint / rest length。

---

# 37. Mirror Symmetry

UpperArm / Forearm 長度必須：

```text
Left = Right
```

人體 source 保持對稱。

L1 Compare Pose 可以左右姿勢不同。

---

# 38. Diagnostic Capture — Before

保留 P035：

```text
Before_L1Pose_Front
Before_Unity_L1Pose_Close
```

作 comparison。

---

# 39. Final Clay Captures

輸出：

```text
Final_Apose_Front.png
Final_Apose_3Q.png
Final_L1Pose_Front.png
Final_L1Pose_3Q.png
```

---

# 40. Arm Detail Captures

必須新增：

```text
Arm_SwordSide_Front.png
Arm_SwordSide_3Q.png
Arm_ShieldSide_Front.png
```

---

# 41. Skeleton Overlay

建立：

```text
Skeleton_Arm_Landmarks_Front.png
```

顯示：

- shoulder
- elbow
- wrist
- hand end

---

# 42. L1 Overlay

重新產：

```text
Final_Overlay_L1Pose_Front.png
```

這次需額外建立：

```text
Final_Overlay_L1Pose_Front_ArmFocus.png
```

裁切：

```text
shoulder → hand
```

讓 Reviewer 可以直接看手臂。

---

# 43. Comparison

建立：

```text
P035_vs_P035R1_L1Pose_Front.png
P035_vs_P035R1_L1Pose_3Q.png
```

以及：

```text
P035_vs_P035R1_ArmFocus.png
```

---

# 44. Posed Arm Measurement Table

建立：

```text
01_Arm_Proportion_Report.md
```

至少：

| Metric | L1 Estimate | Before | After | Gap After |
|---|---:|---:|---:|---:|
| UpperArm / H | | | | |
| Forearm / H | | | | |
| Shoulder→Wrist / H | | | | |
| Hand Width / H | | | | |
| Head Width / H | | | | |

---

# 45. Posed Landmark Table

另列：

```text
Sword-side Shoulder Y
Sword-side Elbow Y
Sword-side Wrist Y
Sword-side HandEnd Y
```

比較：

```text
L1
Before
After
```

---

# 46. Confidence

仍保留：

```text
L1 joint confidence
```

但不得再使用：

> confidence medium

作為完全不修 16.8% mismatch 的唯一理由。

本次應使用：

```text
measurement
+
overlay
+
Unity visual evidence
```

綜合判斷。

---

# 47. Unity Review — Required

建立隔離 Review Prefab。

禁止替換 Production Runtime Prefab。

---

# 48. Unity Required Captures

至少：

```text
Unity_L1Pose_Close.png
Unity_L1Pose_RTS_Normal.png
Unity_Apose_Close.png
```

---

# 49. Unity Side-by-side

建立：

```text
Unity_P035_vs_P035R1_Close.png
```

尤其看：

- arm drop
- hand position
- head proportion
- weapon relationship

---

# 50. Unity Acceptance — Arm

Close View：

Sword arm hand 應比 P035 明顯下降。

但：

- 不應長到膝蓋。
- 不應像猿臂。
- elbow 不應過低。

---

# 51. Unity Acceptance — Head

Head shrink 後：

- Helmet 仍合理。
- Face 不顯得過小。
- RTS 64px 時仍可讀。

---

# 52. 64px / 32px

重新輸出：

```text
64px
32px
```

確保 Hand / Head correction 不破壞 readability。

---

# 53. Phase 03.5 Revision Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_5_Revision01_Review/
```

---

# 54. Package Structure

```text
Infantry_Phase03_5_Revision01_Review/
│
├─ README.md
├─ 00_Revision_Report.md
├─ 01_Arm_Proportion_Report.md
├─ 02_Before_After_Correction_Table.md
├─ 03_Unity_Review_Status.md
├─ 04_Open_Issues.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004_P035R1.blend
│
├─ Measurements/
│  ├─ 3D_L1Pose_Arm_Landmarks_Before.json
│  └─ 3D_L1Pose_Arm_Landmarks_After.json
│
├─ Screenshots/
│  ├─ Apose/
│  ├─ L1Pose/
│  ├─ ArmDetail/
│  ├─ Overlay/
│  ├─ Comparison/
│  ├─ ScreenSize/
│  └─ Unity/
│
└─ Manifests/
```

---

# 55. Phase 03.5 Revision PASS Gate

Reviewer 將確認：

- [ ] UpperArm 不再明顯短於 L1。
- [ ] Forearm 已接近 L1。
- [ ] L1 Compare Pose 中 Elbow 位置更接近 Concept。
- [ ] Wrist 位置更接近 Concept。
- [ ] Sword hand 不再停在過高位置。
- [ ] Hand width 不再明顯巨大。
- [ ] Weapon grip 仍合理。
- [ ] Head width 已縮到較合理範圍。
- [ ] Helmet outer silhouette 沒被破壞。
- [ ] Shoulder width 沒被不必要更改。
- [ ] Hip / Knee / Torso 沒再被改動。
- [ ] Shield 尺寸保持。
- [ ] Sword 尺寸保持。
- [ ] Secondary Forms 沒被破壞。
- [ ] A-Pose 保留。
- [ ] L1 Compare Pose 只作 Review。
- [ ] Unity Close 比 P035 更接近 L1。
- [ ] Unity RTS Normal readability 保持。
- [ ] 原 P035 未被覆寫。

---

# 56. FAIL 條件

若發生以下任一項：

不能進 Phase 04：

- UpperArm 仍約 0.155H 且未合理說明。
- Sword hand 仍明顯停得比 L1 高。
- 只是改 Pose 沒改真正 bone length。
- 為了拉長手臂把 Shoulder joint 外移。
- Hand 還是明顯 oversized glove。
- Head 還是明顯 oversized。
- Hip / Knee 被再次大改。
- Helmet / Shield / Sword 被不必要改尺寸。
- 開始 Final UV / Texture。

---

# 57. PASS 後正式 Geometry Lock

若 Reviewer PASS：

```text
CHR_Infantry_A_v004_P035R1
```

即成為：

```text
PRE-UV GEOMETRY LOCK
```

下一階段：

```text
Phase 04
UV / Texture / Material / Team Color
```

---

# 58. Agent 最終狀態

只能標：

```text
READY FOR PHASE03_5 REVISION REVIEW
```

不要自行宣告：

```text
PASS
```

---

# 59. ZIP

建立：

```text
Infantry_Phase03_5_Revision01_Review.zip
```

---

# 60. ZIP Verification

至少確認：

- v004_P035R1.blend
- Arm Proportion Report
- Before/After table
- Arm landmarks before/after
- Final L1Pose front
- Arm focus overlay
- P035 vs P035R1 comparison
- Unity L1Pose Close
- Unity RTS Normal
- README

---

# 61. Git Rule

禁止：

```text
git commit
git push
git reset --hard
```

---

# 62. 最終核心要求

這次不是：

> 把手臂弄得跟圖片完全一樣。

而是：

> **修正已被量測與實機視覺證實的 Arm / Hand / Head 比例 mismatch，同時保留已經正確的整體身高、下半身、胸甲、盾牌、短劍與 Secondary Forms。**

---

# 63. 立即執行

```text
Preserve P035
↓
Create P035R1
↓
Measure posed arm landmarks
↓
Increase UpperArm length controlled
↓
Increase Forearm length controlled
↓
Reduce Hand size controlled
↓
Reduce Head mesh size controlled
↓
Refit sleeve / bracer / grips
↓
Rebuild L1 Compare Pose
↓
Generate arm-focused overlays
↓
Unity Review
↓
Package ZIP
↓
Report READY FOR PHASE03_5 REVISION REVIEW
```

不要進 Phase 04。
不要 Final UV。
不要 Final Texture。
不要 Final Skinning。
不要 Animation Polish。
不要 git commit。
不要 git push。
