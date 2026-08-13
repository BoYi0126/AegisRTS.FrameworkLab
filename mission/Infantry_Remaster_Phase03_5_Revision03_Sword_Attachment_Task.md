# Infantry Remaster — Phase 03.5 Revision 03  
# Sword / RightHand Attachment & Weapon Socket Validation Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Stage:** Phase 03.5 — Revision 03  
> **Current Candidate:** `CHR_Infantry_A_v004_P035R2.blend`  
> **Revision Output:** `CHR_Infantry_A_v004_P035R3.blend`  
> **Decision:** `GEOMETRY PASS / WEAPON ATTACHMENT FIX REQUIRED`  
> **Primary Goal:** 修正 Sword 與 `RightHand` 的父子 / Socket / Local Transform 關係，使武器在 Blender、FBX、Unity 與 Review Pose 中始終正確跟隨右手。  
> **Next Stage if PASS:** Phase 04 — UV / Texture / Material / Team Color  
>
> **核心要求：**
>
> 本 Revision 只處理：
>
> 1. Sword Root hierarchy
> 2. RightHand / WeaponSocket_R attachment
> 3. Sword pivot / local transform
> 4. Grip contact
> 5. Blender pose validation
> 6. FBX export validation
> 7. Unity Humanoid / runtime review validation
>
> 禁止重新修改：
>
> - 人體比例
> - Arm length
> - Head size
> - Shield
> - Chest / Shoulder / Waist
> - Sword overall length
> - Sword blade geometry
> - Character height
> - Final UV / Texture
>
> 本 Revision 通過後，才可正式宣告：
>
> `PRE-UV GEOMETRY + ATTACHMENT LOCK`

---

# 0. Reviewer 發現的問題

目前 P035R2 的 Sword 在部分 Review Pose / Unity Capture 中沒有握在右手上，而是浮在角色右側。

現有 Manifest 顯示 Sword 相關物件主要 Parent 為：

```text
Armature
```

而不是：

```text
RightHand
```

因此當：

```text
RightUpperArm
RightLowerArm
RightHand
```

在 L1 Compare Pose 中被旋轉後：

```text
RightHand moves
Sword stays near Armature/world relative transform
```

造成：

```text
Hand        Sword
  \          |
   \         |
   gap
```

這不是 Animation Polish 問題。

這是：

```text
WEAPON ATTACHMENT CONTRACT BUG
```

---

# 1. 本 Revision 的正式目標

建立一個明確且可維護的：

```text
RightHand
└─ WeaponSocket_R
   └─ SwordRoot
      ├─ Sword
      ├─ Sword_Grip
      ├─ Sword_Guard
      ├─ Sword_Pommel
      └─ Sword secondary parts
```

或專案既有等價結構。

核心是：

> 所有 Sword visual parts 必須作為一個 weapon unit 跟隨 `RightHand`。

---

# 2. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v004_P035.blend
CHR_Infantry_A_v004_P035R1.blend
CHR_Infantry_A_v004_P035R2.blend
```

建立：

```text
CHR_Infantry_A_v004_P035R3.blend
```

P035R2 視為：

```text
APPROVED GEOMETRY + SHIELD ALIGNMENT BASELINE
```

---

# 3. 不得再修改 Body Geometry

本 Revision 禁止調整：

```text
Character Height
Head size
Shoulder width
UpperArm length
Forearm length
Hand size
Torso
Hip
Knee
Leg
Boot
```

若 Sword attachment 無法在現有比例下成立：

記錄在：

```text
04_Open_Issues.md
```

不要改人體比例。

---

# 4. Shield 完全鎖定

以下不動：

```text
Shield size
Shield position
Shield grip
Shield strap
Shield rotation
Shield geometry
Shield attachment
```

P035R2 的 Shield Alignment 已 PASS。

---

# 5. Sword Geometry 尺寸鎖定

目前 Sword overall length 已在 L1 規格內。

因此：

```text
Sword overall scale = LOCKED
```

禁止：

- Scale sword to reach hand
- Shorten blade
- Lengthen blade
- Move hand to reach sword
- Change character proportion to reach sword

只修 attachment transform。

---

# 6. Sword Visual Parts Audit

開始前列出全部 Sword 相關物件。

至少搜尋：

```text
Sword
Blade
BladeSpine
Guard
Grip
GripWrap
Pommel
Weapon
```

建立：

```text
01_Sword_Object_Hierarchy_Before.md
```

內容至少包含：

| Object | Parent Before | Parent Type | Local Position | Local Rotation | Local Scale |
|---|---|---|---|---|---|

---

# 7. 不允許遺漏 Sword Secondary Parts

所有屬於 Sword 的：

```text
Blade
Spine
Guard
Grip
Wrap
Pommel
Decoration
```

都必須被納入同一 Sword Root hierarchy。

禁止：

```text
Blade follows hand
but GripWrap stays at Armature
```

---

# 8. 建議建立 SwordRoot

若目前沒有單一武器 Root：

建立：

```text
SwordRoot
```

命名依專案規範可使用：

```text
WPN_SwordRoot_R
```

或：

```text
SOCKET_CONTENT_Sword_R
```

但必須在 Report 說明實際命名。

---

# 9. SwordRoot 原點

SwordRoot 原點建議設在：

> Grip 的實際手掌握持中心附近。

不要設在：

- blade tip
- world origin
- shield
- arbitrary mesh origin

---

# 10. SwordRoot Orientation

建立一致 local axes。

建議：

```text
Local forward
= blade direction

Local up
= sword guard / grip orientation

Local right
= side axis
```

若專案有既有 Weapon Axis Convention：

以專案規格為準。

---

# 11. RightHand Bone

確認正式 Humanoid 手骨：

```text
RightHand
```

存在。

記錄：

```text
Bone name:
Parent:
Rest transform:
```

不得 rename。

---

# 12. WeaponSocket_R

優先使用：

```text
WeaponSocket_R
```

作為右手武器 socket。

如果目前專案已存在等價 socket：

使用既有名稱。

不要重複建立第二個功能相同的 socket。

---

# 13. 若 WeaponSocket_R 不存在

可以建立：

```text
WeaponSocket_R
```

但必須判斷它應該是：

### Option A — Bone

或

### Option B — Empty / Transform child of RightHand

依專案 Runtime / Unity pipeline。

---

# 14. Socket 最重要原則

Socket 必須：

```text
follow RightHand
```

不可以只 parent 到：

```text
Armature
```

---

# 15. Unity Humanoid 注意事項

如果 WeaponSocket_R 為額外 Bone：

必須確認：

- 不破壞 Humanoid mapping
- 不需要映射到 Avatar humanoid bone
- Export FBX 包含 socket
- Unity 能找到該 Transform

---

# 16. Bone Count

目前角色：

```text
23 bones
```

如果使用 Empty / Transform Socket：

Bone Count 保持 23。

如果新增 Socket Bone：

可以變成 24，但必須在 Report 解釋。

優先：

> 不改 Humanoid required bone hierarchy。

---

# 17. 建議策略

若專案目前沒有 weapon socket standard：

優先：

```text
RightHand
└─ WeaponSocket_R (non-deforming helper bone or exported transform)
   └─ SwordRoot
```

而不是：

```text
RightHand
└─ all 8 sword mesh objects separately
```

---

# 18. SwordRoot Parenting

所有 Sword Mesh：

```text
Parent = SwordRoot
```

SwordRoot：

```text
Parent = WeaponSocket_R
```

WeaponSocket_R：

```text
Parent = RightHand
```

---

# 19. Preserve World Transform

重新 Parenting 時：

必須使用：

```text
Keep Transform
```

或等價流程。

先確保 Sword 不會瞬間跳到錯誤位置。

---

# 20. 再修 Local Transform

Parent 完成後：

以 Local Transform 調整：

```text
position
rotation
```

讓 Grip 對準 RightHand。

禁止用 World Position workaround。

---

# 21. Grip Center

建立或量測：

```text
SwordGripCenter
```

可使用：

- Grip mesh center
- designed grip pivot
- helper empty

---

# 22. Palm Contact

A-Pose 中：

```text
Sword Grip
```

應落入：

```text
Right palm
```

合理範圍。

不要求 finger wrap。

---

# 23. Grip Position Gate

至少確認：

- Grip axis 穿過 palm center 附近
- Guard 不切入 wrist
- Pommel 不穿 forearm
- Blade 不穿 body

---

# 24. Hand Primitive 限制

目前 Hand 尚未 final skin / fingers。

所以 PASS 標準是：

```text
credible palm contact
```

不是：

```text
fully wrapped fingers
```

手指細節留後續 Rig / Skinning / Animation。

---

# 25. Sword Angle — A-Pose

A-Pose 中 Sword 不必呈現 Gameplay Idle。

但必須：

- 跟手
- 不穿腿
- 不穿 torso
- 不碰 ground
- Review 時可見

---

# 26. Sword Angle — L1 Compare Pose

L1 Compare Pose 中：

右手劍應接近 L1：

```text
hand naturally down
+
sword angled downward
```

但：

> 不要求 pixel-perfect。

---

# 27. 不能用 Pose 假裝 Attachment 正確

測試必須包含：

```text
RightHand rotation changes
```

然後觀察 Sword 是否跟著。

如果 Sword 只是剛好擺在手旁：

Fail。

---

# 28. Required Pose Test A — Source A-Pose

```text
POSE_SOURCE_A
```

確認：

```text
Sword follows RightHand
```

---

# 29. Required Pose Test B — L1 Compare Pose

```text
REVIEW_ONLY_POSE_L1_COMPARE
```

確認 Sword：

```text
still in hand
```

---

# 30. Required Pose Test C — Hand Rotation Test

建立 temporary test：

```text
RightLowerArm rotate ±20°
RightHand rotate ±15°
```

至少三個姿勢：

```text
Neutral
Test_Up
Test_Down
```

Sword 必須跟著。

---

# 31. Required Pose Test D — Right Arm Raise

簡單把右手提高。

Sword 必須：

```text
follow full arm chain
```

不要留下原位。

---

# 32. Required Pose Test E — Right Arm Lower

簡單把右手降低。

Sword 仍保持 grip。

---

# 33. Blender Evidence

至少輸出：

```text
Apose_SwordGrip_Close.png
L1Pose_SwordGrip_Close.png
SwordFollow_TestUp.png
SwordFollow_TestDown.png
SwordFollow_3Q.png
```

---

# 34. Hierarchy Screenshot

必須提供：

```text
Blender_Hierarchy_RightHand_Sword.png
```

顯示：

```text
RightHand
→ WeaponSocket_R
→ SwordRoot
→ Sword parts
```

---

# 35. Transform Evidence

建立：

```text
02_Sword_Attachment_Transform_Report.md
```

記錄：

```text
RightHand:
WeaponSocket_R:
SwordRoot:
```

的：

```text
Local Position
Local Rotation
Local Scale
```

---

# 36. Scale Rule

所有 attachment transforms 優先：

```text
Local Scale = 1,1,1
```

避免使用 non-uniform scaling。

---

# 37. Sword Mesh Scale

Sword mesh world size維持。

Parenting 後不得出現：

```text
0.01
100
```

等奇怪 compensating scale。

若因 Blender/Unity unit conversion存在：

需說明。

---

# 38. FBX Export

輸出 Review FBX：

```text
SK_Infantry_A_v004_P035R3_Apose_Review.fbx
SK_Infantry_A_v004_P035R3_L1Pose_Review.fbx
```

---

# 39. FBX 必須包含

- Armature
- RightHand
- WeaponSocket_R（若使用）
- SwordRoot / sword hierarchy
- Character meshes
- Shield
- Sword

---

# 40. FBX Validation

重新 Import / inspect。

必須確認：

```text
Sword attachment survives export
```

不能 Blender 正確、FBX 斷掉。

---

# 41. Reimport Validation

如果可以：

將 Export FBX 重新 import 到乾淨 Blender temp scene。

確認：

```text
RightHand → Sword
```

仍成立。

---

# 42. Unity Review — Required

本 Revision 必須跑 Unity。

不能只用 Blender PASS。

---

# 43. Unity Review Folder

建立隔離 Review：

```text
Assets/AegisRTS/Review/InfantryPhase035Revision03/
```

不要污染正式 Runtime。

---

# 44. Unity Review Prefab

建立：

```text
PF_Unit_Infantry_P035R3_Review
```

以及必要 Compare Pose Review。

禁止替換：

```text
PF_Unit_Infantry
```

---

# 45. Unity Hierarchy Validation

Unity Inspector / hierarchy 中：

必須能找到：

```text
RightHand
WeaponSocket_R
SwordRoot
```

或實際等價名稱。

---

# 46. Unity Sword Parenting

Unity 中 SwordRoot 最終 Parent：

應為：

```text
WeaponSocket_R
```

或：

```text
RightHand
```

不可為：

```text
Model Root
Armature Root
Scene Root
```

---

# 47. Unity A-Pose Capture

至少：

```text
Unity_Apose_Close.png
```

Sword 必須接觸 RightHand。

---

# 48. Unity L1 Pose Capture

至少：

```text
Unity_L1Pose_Close.png
Unity_L1Pose_RTS_Normal.png
```

Sword 必須仍在手上。

---

# 49. Unity Hand Follow Test

若可以用 Review Script：

對右手 / forearm 做兩個 temporary rotation test。

產：

```text
Unity_SwordFollow_TestUp.png
Unity_SwordFollow_TestDown.png
```

---

# 50. Runtime Attachment Validation

如果專案已有：

```text
WeaponSocket
Equipment system
AttachWeapon()
```

不要重寫一套新系統。

優先：

> 使用既有 runtime contract。

---

# 51. Runtime Audit

搜尋：

```text
WeaponSocket_R
RightHand
Sword
Attach
Equip
Weapon
```

確認目前角色 Runtime 如何掛裝備。

---

# 52. 不允許破壞 Generic Weapon Pipeline

這個 Infantry 未來只是第一個 Golden Sample。

因此 Sword attachment 不應做成：

```text
Infantry-only hard-coded special case
```

如果專案已有通用 system：

必須相容。

---

# 53. 建議通用契約

未來：

```text
Unit
├─ WeaponSocket_R
├─ WeaponSocket_L
└─ Equipment
```

例如：

```text
Infantry → Sword_R + Shield_L
Spearman → Spear_R
Archer → Bow_L / Arrow_R
Hero → HeroWeapon_R
```

---

# 54. 本 Revision 不要求實作完整 Equipment System

如果專案目前沒有：

不要因本 Revision 擴張成大型系統工程。

只需要：

- 正確 Socket
- 正確 Parent
- 可供未來系統掛載

---

# 55. Shield 不改

左手 Shield：

```text
P035R2 APPROVED
```

不要為了統一 hierarchy 重做 Shield。

除非只是 document existing pattern。

---

# 56. Sword Separation Requirement

Sword 必須保持：

```text
separate from character body
```

未來可以：

- unequip
- replace
- hide
- swap

---

# 57. Sword Mesh Grouping

可以：

### Option A

保留多個 Sword Mesh，統一 parent SwordRoot。

### Option B

合併為少數 Sword meshes。

本 Revision 不要求合併。

優先：

> 不破壞已通過 Geometry。

---

# 58. Geometry Budget

目前約：

```text
33,248 tris
```

本 Revision：

```text
same
```

除非新增 Socket helper 不影響 tris。

---

# 59. Character Height

保持：

```text
≈ 1.824 m
```

Sword attachment 不得改身高。

---

# 60. Sword Size

保持上一階段：

```text
≈ 1.061 m
```

或實際 current measurement。

---

# 61. Humanoid Contract

確認：

```text
Animator Rig Type = Humanoid
```

Review FBX 不應因 Socket 新增變成：

```text
Generic
```

---

# 62. Root Motion

本 Revision 不改：

```text
Root Motion
```

---

# 63. Animation Clips

本 Revision 不做：

```text
Idle
Move
Attack
Hit
Death
```

Animation Polish 留 Phase 06。

---

# 64. 但要做 Pose Follow Test

Pose test 只是：

```text
attachment validation
```

不是 Animation asset。

---

# 65. AttackImpact

本 Revision 不修改：

```text
AttackImpact
```

---

# 66. Team Color

不改。

---

# 67. UV / Texture

仍禁止：

```text
Final UV
Final Texture
Final Bake
```

---

# 68. Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_5_Revision03_Sword_Attachment_Review/
```

---

# 69. Package Structure

```text
Infantry_Phase03_5_Revision03_Sword_Attachment_Review/
│
├─ README.md
├─ 00_Revision_Report.md
├─ 01_Sword_Object_Hierarchy_Before.md
├─ 02_Sword_Attachment_Transform_Report.md
├─ 03_FBX_Attachment_Validation.md
├─ 04_Unity_Attachment_Validation.md
├─ 05_Runtime_Weapon_Contract_Audit.md
├─ 06_Open_Issues.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004_P035R3.blend
│
├─ RuntimeReview/
│  ├─ SK_Infantry_A_v004_P035R3_Apose_Review.fbx
│  └─ SK_Infantry_A_v004_P035R3_L1Pose_Review.fbx
│
├─ Screenshots/
│  ├─ Blender/
│  ├─ Grip/
│  ├─ FollowTests/
│  ├─ Hierarchy/
│  ├─ Comparison/
│  └─ Unity/
│
└─ Manifests/
```

---

# 70. Before / After Comparison

建立：

```text
P035R2_vs_P035R3_L1Pose_Close.png
```

必須清楚看到：

```text
Before = sword floating
After = sword in hand
```

---

# 71. Blender Grip Close-Up

建立：

```text
Blender_SwordGrip_Close.png
```

---

# 72. Unity Grip Close-Up

建立：

```text
Unity_SwordGrip_Close.png
```

---

# 73. Follow Test Evidence

至少：

```text
SwordFollow_Neutral.png
SwordFollow_TestUp.png
SwordFollow_TestDown.png
```

---

# 74. Hierarchy Evidence

至少：

```text
Hierarchy_RightHand_WeaponSocket_SwordRoot.png
```

---

# 75. Runtime Contract Report

建立：

```text
05_Runtime_Weapon_Contract_Audit.md
```

回答：

1. 目前正式 Unit weapon attachment 方式是什麼？
2. Infantry Sword 是否遵守既有方式？
3. 是否新增 WeaponSocket_R？
4. Socket 是否可供未來 Unit reuse？
5. 是否存在 hard-coded Infantry-only dependency？
6. Phase 06 / Phase 08 還需要驗證什麼？

---

# 76. PASS Gate — Blender

- [ ] Sword 跟隨 RightHand。
- [ ] L1 Compare Pose Sword 仍在手上。
- [ ] A-Pose Sword 仍在手上。
- [ ] RightHand rotation test Sword 跟隨。
- [ ] Sword Grip 與 palm 合理接觸。
- [ ] Guard 不切 wrist。
- [ ] Sword 不穿 torso / leg。
- [ ] Sword scale 未改。

---

# 77. PASS Gate — Hierarchy

- [ ] Sword 有 single logical root。
- [ ] Sword parts 都在 SwordRoot 下。
- [ ] SwordRoot Parent 為 WeaponSocket_R / RightHand。
- [ ] 不再直接掛 Model Root / Armature Root。
- [ ] Naming 清楚。
- [ ] Local Scale 無異常。

---

# 78. PASS Gate — FBX

- [ ] Export FBX 保留 attachment。
- [ ] Reimport 後 Sword 跟手。
- [ ] Humanoid mapping 未破壞。
- [ ] Socket transform 存在。
- [ ] Sword parts 沒散掉。

---

# 79. PASS Gate — Unity

- [ ] Unity A-Pose Sword 在右手。
- [ ] Unity L1 Pose Sword 在右手。
- [ ] RTS Normal Sword 可辨識。
- [ ] RightHand follow test 正常。
- [ ] Hierarchy 可找到 RightHand → Socket → Sword。
- [ ] Runtime Prefab 沒被正式替換。
- [ ] Shield 沒被改壞。

---

# 80. FAIL 條件

以下任何一項 Fail：

不能進 Phase 04：

- Sword 仍浮在右側。
- Sword 只在 A-Pose 剛好對齊，但手一動就分離。
- Blade 跟手但 Grip / Guard 留在原位。
- 透過改 Arm length 讓手碰到 Sword。
- 透過改 Sword scale 讓它碰到手。
- Sword Parent 仍只有 Armature root。
- Unity import 後 hierarchy 丟失。
- WeaponSocket 新增後破壞 Humanoid。
- 為了本 Bug 重做完整 Equipment System。
- 開始 Final UV / Texture。

---

# 81. PASS 後正式 Lock

若 Reviewer 通過：

```text
CHR_Infantry_A_v004_P035R3
```

正式標記：

# PRE-UV GEOMETRY + ATTACHMENT LOCK

接下來：

```text
Phase 04
UV
Texture
Material
Team Color
```

---

# 82. Phase 06 還會處理什麼

Phase 06：

```text
Idle grip
Combat idle grip
Attack swing
Move weapon behavior
Hit weapon behavior
Death weapon behavior
```

但：

> Sword follow RightHand 的基礎 contract 必須在本 Revision 完成。

---

# 83. Phase 08 還會再驗證

Golden Sample：

```text
Runtime Prefab
Animator
Equipment
Socket
LOD
Material
Team Color
```

都會再驗證。

但本 Revision 必須先把最基本 attachment 修正。

---

# 84. Agent 不得自行宣告 PASS

最終只能：

```text
READY FOR PHASE03_5 REVISION03 REVIEW
```

---

# 85. ZIP

建立：

```text
Infantry_Phase03_5_Revision03_Sword_Attachment_Review.zip
```

---

# 86. ZIP Verification

至少包含：

```text
CHR_Infantry_A_v004_P035R3.blend
Revision Report
Hierarchy Before
Attachment Transform Report
FBX Validation
Unity Validation
Runtime Contract Audit
Blender Sword Grip
Unity Sword Grip
Follow Test Up
Follow Test Down
Hierarchy Screenshot
Before / After Comparison
```

---

# 87. Git Rule

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

# 88. Agent 最終回覆格式

## Source

```text
Input:
Output:
```

## Sword Hierarchy

```text
RightHand:
WeaponSocket_R:
SwordRoot:
Sword Parts:
```

## Blender

```text
A-Pose:
L1 Compare Pose:
Follow Test Up:
Follow Test Down:
```

## FBX

```text
Attachment preserved:
Humanoid preserved:
```

## Unity

```text
A-Pose:
L1 Compare Pose:
RTS Normal:
Hierarchy:
```

## Runtime Contract

```text
Existing system:
Socket:
Reusable:
```

## Package

```text
Folder:
ZIP:
```

## Status

```text
READY FOR PHASE03_5 REVISION03 REVIEW
```

---

# 89. 最終核心原則

這次不是：

> 把劍移到手旁邊看起來像握住。

而是：

> **建立真正的 RightHand → WeaponSocket_R → SwordRoot attachment hierarchy，使右手不論怎麼 Pose、FBX Export、Unity Import，Sword 都會保持在正確 Grip 位置。**

---

# 90. 立即執行

```text
Preserve P035R2
↓
Create P035R3
↓
Audit Sword Objects
↓
Audit Existing Runtime Weapon Contract
↓
Create / Reuse WeaponSocket_R
↓
Create / Reuse SwordRoot
↓
Parent All Sword Parts
↓
Align Grip to RightHand
↓
Validate A-Pose
↓
Validate L1 Compare Pose
↓
Run Hand Follow Tests
↓
Export FBX
↓
Reimport / Validate
↓
Unity Review
↓
Capture Hierarchy / Grip / Follow Evidence
↓
Package ZIP
↓
Report READY FOR PHASE03_5 REVISION03 REVIEW
```

不要進 Phase 04。
不要 Final UV。
不要 Final Texture。
不要改人體比例。
不要改 Sword 尺寸。
不要改 Shield。
不要 Git Commit。
不要 Git Push。
