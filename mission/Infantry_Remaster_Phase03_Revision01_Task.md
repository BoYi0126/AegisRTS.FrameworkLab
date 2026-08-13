# Infantry Remaster — Phase 03 Revision 01 Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Stage:** Phase 03 — Revision 01  
> **Current Candidate:** `CHR_Infantry_A_v004.blend`  
> **Approved Primary Baseline:** `CHR_Infantry_A_v003_P02R1.blend`  
> **Revision Output:** `CHR_Infantry_A_v004_P03R1.blend`  
> **Decision:** `CHANGE REQUESTED`  
> **Purpose:** 修正 Phase 03 Secondary Forms Gate Review 中剩餘的局部結構問題，並補做 Unity RTS 實機 Preview。  
> **Important:** 本次是局部 Revision，不是重新設計角色。已通過的 Primary / Secondary Forms 禁止任意大改。

---

# 0. Reviewer 結論

目前 `CHR_Infantry_A_v004` 已經成功完成大部分 Secondary Forms。

以下已通過，不需要重做：

```text
Overall Primary Silhouette
Character Height
Head / Helmet Primary Form
Backward Plume Direction
Three-layer Shoulder Armor Concept
Chest Lamellar Main Structure
Chest Armor Mass
Sword Primary + Secondary Structure
Shield Front Primary Structure
Material ID Planning
64 px Readability
32 px Readability
L1 East-Asian Heavy Infantry Identity
```

因此：

> **不要回到 Phase 02。**
>
> **不要整隻角色重建。**

本 Revision 只修以下六項：

1. Front Waist Cloth
2. Scarf / Neck Cloth
3. Shield Back Construction
4. Upper Arm Cloth Form
5. Boot Integration
6. Unity RTS Preview

完成並通過後：

```text
Phase 03 = PASS
↓
Phase 04 = UV / Texture / Material / Team Color
```

---

# 1. 執行前必讀

開始前請完整閱讀：

```text
Infantry_Phase01_Production_L2_Remaster_Target.md
Infantry_Remaster_Phase02_PrimaryForms_Task.md
Infantry_Remaster_Phase02_Revision01_Task.md
Infantry_Remaster_Phase03_SecondaryForms_Task.md
```

以及目前 Phase 03 Review Package：

```text
Infantry_Phase03_SecondaryForms_Review_v001/
```

特別查看：

```text
Detail_Waist
Detail_Shield_Back
Detail_Boot
Detail_Shoulder
Detail_Chest
L1_vs_v004_Front
P02R1_vs_v004_3Q
```

不要只依文字修改。

---

# 2. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
CHR_Infantry_A_v003.blend
CHR_Infantry_A_v003_P02R1.blend
CHR_Infantry_A_v004.blend
```

必須建立：

```text
CHR_Infantry_A_v004_P03R1.blend
```

`v004` 保留為：

```text
PHASE03 INITIAL CANDIDATE
```

`v004_P03R1` 才是：

```text
PHASE03 REVISION CANDIDATE
```

---

# 3. 本 Revision 禁止大改的項目

以下目前已 PASS。

除非為了解決 clipping 或 attachment 必須做極小幅調整，否則禁止重建：

```text
Head
Helmet overall silhouette
Plume direction
Shoulder armor 3-layer design
Chest lamellar main rows
Chest overall volume
Character height
Body proportions
Shield front outline
Shield rim
Shield boss main scale
Sword length
Sword blade
Sword guard
Sword grip
Leg overall proportion
```

如果 Agent 認為其中任一項必須大改：

不要自行執行。

在：

```text
03_Open_Issues.md
```

標記原因。

---

# 4. Revision 目標 A — Front Waist Cloth

## 4.1 Current Problem

目前 Front Waist Cloth 雖已有 taper，但 Clay View 仍偏向：

```text
rigid flat plate
```

而不是：

```text
hanging cloth
```

主要問題：

- 表面過平。
- 側面厚度感偏硬。
- 下緣過於規則。
- 缺乏布料重量。
- 與 Belt 的 attachment 不夠自然。

---

# 5. Front Waist Cloth Target

必須讓它讀成：

> 被腰帶固定、自然垂下的厚布 / Team Color cloth。

建議結構：

```text
Belt
  ↓
compressed upper cloth
  ↓
main center fold
  ↓
left/right secondary plane
  ↓
slightly irregular lower edge
```

---

# 6. Front Waist Cloth Geometry

至少建立：

```text
1 main fold
+
1–2 broad secondary folds / planes
```

不要做：

```text
10+ micro folds
```

這仍然是 Secondary Forms。

---

# 7. Front Waist Cloth Thickness

目前若像 armor plate：

降低視覺厚度。

但不能完全 zero-thickness。

應具有：

```text
cloth edge thickness
```

而不是：

```text
metal plate thickness
```

---

# 8. Front Waist Cloth Lower Edge

避免完全：

```text
────────
```

可改為：

```text
slightly angled
slightly curved
subtle asymmetry
```

保持 L1 clean stylization。

不要做破布 / torn cloth。

---

# 9. Front Waist Cloth Team Color Role

仍保留為主要 Team Color Region。

不要改變：

```text
Team Color coverage logic
```

Final Color 留 Phase 04。

---

# 10. Revision 目標 B — Scarf / Neck Cloth

## 10.1 Current Problem

目前胸前 Scarf 在 Clay View 偏向：

```text
rounded tube / hose
```

而不是：

```text
broad cloth band
```

這會產生玩具 / 塑膠感。

---

# 11. Scarf Cross Section

將截面從接近：

```text
○
```

改為：

```text
╭────╮
╰────╯
```

也就是：

> 扁寬、略有厚度的布料。

不要變成完全平面 Plane。

---

# 12. Scarf Width / Thickness

保持目前 L1 視覺存在感。

但：

```text
Width > Thickness
```

明顯成立。

目標：

```text
broad sash / scarf
```

不是：

```text
rope
```

---

# 13. Scarf Major Folds

只建立：

```text
1 main fold
+
1 secondary compression / turn
```

建議位置：

- shoulder / neck transition
- chest diagonal transition

不要做大量細皺。

---

# 14. Scarf Chest Contact

Scarf 不應整條浮在 Chest Armor 上方。

需要有部分：

```text
cloth approaching / resting against armor
```

但避免穿模。

---

# 15. Scarf Termination

前後尾端需要合理收束。

不要：

```text
perfect tube cut
```

應具有：

- taper
- broad cloth termination
- simple fold

---

# 16. Revision 目標 C — Shield Back Construction

## 16.1 Current Problem

Shield Front：

```text
PASS
```

Shield Back：

目前有多個 brace / grip element，但視覺邏輯不夠清楚。

Reviewer 無法立即理解：

> 手臂如何固定，以及手如何握住盾牌。

這是本 Revision 高優先項。

---

# 17. Shield Back Functional Readability

不要求真實軍事考據級精確。

但至少一眼看出：

```text
Forearm support
+
Hand grip
+
Structural reinforcement
```

---

# 18. Recommended Shield Back Layout

建議：

```text
        Structural brace
     ─────────────────

         Forearm strap
        ╭──────────╮
        │          │
        ╰──────────╯

             Grip
              │
              │

        Secondary brace
```

實際角度依左手 A-Pose 與盾牌 orientation 調整。

---

# 19. Forearm Strap

建立：

- broad strap
- enough arm clearance
- attachment points

不要：

```text
thin wire
```

也不要巨大到像機械支架。

材質預計：

```text
Leather
```

Final material 留 Phase 04。

---

# 20. Hand Grip

Grip 必須：

- 手可以合理握住
- 不切入 palm
- 不穿 forearm
- 有左右 attachment / base

Grip 本體可為：

```text
wood / leather-wrapped bar
```

---

# 21. Shield Brace Simplification

如果目前背面 brace 太多：

可以刪減。

目標是：

```text
clear hierarchy
```

不是：

```text
more parts = more detail
```

建議最多：

```text
1 major brace
+
1 secondary brace
+
strap
+
grip
```

除非 L1 / structural need 支持更多。

---

# 22. Shield Back Silhouette

Side / Back Detail 必須能看出：

- shield body
- rim
- strap
- grip
- brace

不要全部疊成一團。

---

# 23. Revision 目標 D — Upper Arm Cloth Form

## 23.1 Current Problem

Shoulder Armor 已 PASS。

但：

```text
Shoulder Armor
↓
UpperArm
```

之間的 Upper Arm 仍偏：

```text
perfect rounded cylinder
```

造成 Action Figure / inflatable feeling。

---

# 24. Upper Arm Revision

不要改 Upper Arm 整體比例。

只修改外形表面節奏。

建立：

```text
shoulder compression
+
upper sleeve mass
+
elbow direction
+
1–2 broad folds
```

---

# 25. Upper Arm Cloth Shape

避免：

```text
perfect circular cross section
```

加入：

- front plane
- side plane
- slight compression beneath shoulder armor

讓它更像：

```text
cloth sleeve around arm
```

---

# 26. Upper Arm Folds

最多：

```text
2 major folds
```

位置可在：

- shoulder armor 下方
- elbow 上方

不要做 tiny wrinkles。

---

# 27. Elbow Direction

即使 Neutral A-Pose：

也應該能從 geometry 輕微看出 elbow direction。

目的：

打破：

```text
straight tube
```

---

# 28. Bracer Transition

Upper Arm / sleeve → elbow → forearm → bracer：

必須有層次。

Bracer 已有 Secondary Structure，原則上保留。

---

# 29. Revision 目標 E — Boots Integration

## 29.1 Current Problem

Boots 已比 Phase 02 好。

目前有：

- sole
- heel
- toe panel
- upper panel

但局部看起來仍像：

```text
base boot
+
separate pieces attached on surface
```

而非一體設計。

---

# 30. Boot Revision

不重做 Boot。

只做：

```text
surface integration
```

---

# 31. Toe Panel

讓 Toe Panel：

- follows boot curvature
- transitions naturally
- edge not floating

不要像：

```text
armor plate glued to shoe
```

除非本來就是金屬 toe armor；目前 Infantry L1 不需要強化成 plated boot。

---

# 32. Upper Boot Panel

讓：

```text
Upper panel
↓
ankle
↓
instep
```

形成連續 transition。

避免：

```text
separate shell sitting on top
```

---

# 33. Sole

Sole：

```text
PRESERVE
```

只確認：

- thickness readable
- no floating gap
- heel connection clean

---

# 34. Boot Material Logic

幾何上應偏：

```text
Leather / cloth military boot
```

不是：

```text
metal armored fantasy boot
```

Final material Phase 04。

---

# 35. Revision 目標 F — Unity RTS Preview

本次 Revision 完成後：

**Unity RTS Preview 為 Required。**

不再只是 Optional。

---

# 36. Unity Safety

禁止直接替換正式：

```text
PF_Unit_Infantry
```

可以：

```text
PF_Unit_Infantry_v004_P03R1_Review
```

或建立 Temporary Review Scene / Prefab。

---

# 37. Unity Import

可以 Export temporary FBX：

```text
SK_Infantry_A_v004_P03R1_Review.fbx
```

用途僅：

```text
visual review
```

不要正式取代 Runtime production asset。

---

# 38. Unity Review Material

可以使用：

### Option A
Neutral Clay Material

### Option B
Material ID Preview

最好兩者都有。

本次不要求 Final Texture。

---

# 39. Required Unity Captures

至少：

```text
Unity_Close.png
Unity_RTS_Normal.png
Unity_Far.png
```

如果只允許做最少：

```text
Unity_RTS_Normal.png
```

為強制最低要求。

---

# 40. Unity RTS Normal View

必須使用：

> 接近實際遊戲正常遊玩距離與 Camera Pitch。

不要：

- 極低角度
- 角色英雄特寫
- 過度 zoom
- cinematic camera

---

# 41. Unity Review 必須看什麼

從 Unity_RTS_Normal 確認：

- Helmet readable
- Shoulder armor readable
- Chest mass readable
- Shield readable
- Sword readable
- Waist cloth 不消失
- Scarf / team-region location readable
- Secondary Forms 不造成 noise
- silhouette clean
- ground scale correct

---

# 42. Unity Lighting

使用目前遊戲常見：

```text
URP scene lighting
```

或 neutral gameplay lighting。

不要特別打 dramatic rim light。

---

# 43. Unity Scale

確認：

```text
Character world height ≈ current gameplay baseline
```

Review Prefab 不得因 FBX Import Scale 改變大小。

---

# 44. Unity Root / Ground

確認：

- feet on ground
- pivot correct
- no floating
- no sinking

---

# 45. Unity Screenshot Report

建立：

```text
04_Unity_Review_Status.md
```

內容：

```text
Unity Version:
Scene:
Review Prefab:
FBX:
Material:
Camera:
Scale:
Capture Files:
Known Differences From Production:
```

---

# 46. Geometry Budget

目前 v004：

```text
≈ 33,898 tris
```

本 Revision 目標：

```text
approximately 32K–36K
```

不要求增加。

如果修改後下降：

也沒問題。

---

# 47. 不增加無意義 Geometry

Revision 不需要：

- more chest plates
- more shoulder plates
- more shield ornaments
- more sword detail

重點是：

```text
shape quality
+
construction logic
```

---

# 48. Normals / Shading

修改後重新檢查：

- flipped normals
- hard/smooth edge
- shading artifacts
- intersecting geometry

Clay View 不得因 shading 問題產生假輪廓。

---

# 49. Non-manifold / Loose Geometry

再次檢查：

```text
Non-manifold
Loose edges
Zero-area faces
```

若不是 0：

在報告記錄。

---

# 50. A-Pose Check

修改 Scarf / Arm / Shield Back / Waist Cloth 後：

必須重新確認：

- Scarf 不穿 chest
- Upper arm 不穿 shoulder armor
- Waist cloth 不穿 thigh
- Shield strap/grip 不穿 hand/forearm
- Boots grounded

---

# 51. Shield Hand Contact

本次必須特別截圖：

```text
Detail_Shield_Back_WithArm.png
```

如果技術上可行。

Reviewer 要看：

> 左手到底怎麼持盾。

---

# 52. Revision Clay Captures

輸出：

```text
01_Clay_Front.png
02_Clay_Left.png
03_Clay_Back.png
04_Clay_3Q_Front.png
05_Clay_3Q_Back.png
```

---

# 53. Required Detail Captures

至少：

```text
Detail_WaistCloth.png
Detail_Scarf.png
Detail_UpperArm.png
Detail_Shield_Back.png
Detail_Shield_Back_WithArm.png
Detail_Boot.png
```

---

# 54. Comparison Captures

建立：

```text
v004_vs_P03R1_Front.png
v004_vs_P03R1_3Q.png
```

另外：

```text
v004_vs_P03R1_Waist.png
v004_vs_P03R1_Scarf.png
v004_vs_P03R1_ShieldBack.png
v004_vs_P03R1_Boot.png
```

非常有價值。

---

# 55. L1 Comparison

至少：

```text
L1_vs_P03R1_Front.png
L1_vs_P03R1_3Q.png
```

確認 Revision 沒有偏離角色身份。

---

# 56. Silhouette

重新輸出：

```text
Silhouette_Front
Silhouette_Left
Silhouette_Back
Silhouette_3Q
```

Secondary Revision 不應破壞已通過 Primary silhouette。

---

# 57. Screen Size

重新確認：

```text
128 px
64 px
32 px
```

尤其：

```text
64 px
```

---

# 58. Material ID Preview

保留既有 Material ID Plan。

不需要重新設計。

輸出：

```text
MaterialID_Front
MaterialID_3Q
MaterialID_Back
```

如果 Shield Back material 分區新增：

同步更新 Material ID Plan。

---

# 59. Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_Revision01_Review/
```

---

# 60. Review Package Structure

```text
Infantry_Phase03_Revision01_Review/
│
├─ README.md
├─ 00_Revision_Report.md
├─ 01_Geometry_Stats.md
├─ 02_Revision_Change_List.md
├─ 03_Open_Issues.md
├─ 04_Unity_Review_Status.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004_P03R1.blend
│
├─ RuntimeReview/
│  └─ SK_Infantry_A_v004_P03R1_Review.fbx
│
├─ Screenshots/
│  ├─ Clay/
│  ├─ Detail/
│  ├─ Comparison/
│  ├─ L1Comparison/
│  ├─ Silhouette/
│  ├─ ScreenSize/
│  ├─ MaterialID/
│  └─ Unity/
│
└─ Manifests/
```

若沒有必要，不需 RuntimeReview FBX 包含動畫。

---

# 61. Geometry Stats

記錄：

```text
Height
Vertices
Triangles
Mesh Count
Material Slots
Bones
Non-manifold
Loose
Zero-area
```

以及修改部位：

```text
WaistCloth
Scarf
UpperArm
Shield
Boot
```

---

# 62. Revision Change List

分類：

```text
PRESERVED
MODIFIED
DEFERRED
```

不要標：

```text
REBUILT
```

除非真的整個部件重建。

---

# 63. Open Issues

如果有：

- Unity automation failure
- FBX material issue
- clipping
- shader mismatch
- shield grip uncertainty

全部集中。

不要偷偷忽略。

---

# 64. Phase 03 Revision Gate

Reviewer 會逐項檢查：

- [ ] Front Waist Cloth 讀成布料，不是硬板。
- [ ] Waist Cloth 有 1–3 個 broad folds。
- [ ] Waist Cloth Belt attachment 自然。
- [ ] Scarf 不再像圓管。
- [ ] Scarf 寬扁且有布料結構。
- [ ] Upper Arm 不再是完美圓柱。
- [ ] Upper Arm 有 broad cloth form。
- [ ] Shield Back 可以看懂 forearm strap + hand grip。
- [ ] Shield Back brace hierarchy 清楚。
- [ ] Boots panels 已融入 boot 主體。
- [ ] Boots 保持 leather/cloth military boot 語彙。
- [ ] Chest / Helmet / Shoulder / Sword 沒被不必要大改。
- [ ] Primary silhouette 保持。
- [ ] L1 identity 保持。
- [ ] 64px readability 保持。
- [ ] Unity RTS Normal capture 已提供。
- [ ] Unity 尺度正確。
- [ ] 原 v004 未被覆寫。

---

# 65. PASS 後的下一階段

若 Reviewer 判定 PASS：

```text
Phase 03 = APPROVED
```

下一步：

```text
Phase 04
UV
+
Texture
+
Material
+
Team Color
```

Phase 04 將第一次正式建立：

```text
2K Texture Set
BaseColor
Normal
Roughness
Metallic
AO
Team Color Mask / runtime strategy
```

---

# 66. 不准提前 Phase 04

本次即使 Revision 完成得很好：

禁止：

- Final UV unwrap
- Final bake
- BaseColor painting
- Final normal map
- Final ORM
- Team Color texture
- final material

只允許既有 temporary / Material-ID setup。

---

# 67. 不准自行宣告 Phase 03 PASS

Agent 最終只回：

```text
READY FOR PHASE03 REVISION REVIEW
```

是否 PASS 由 Reviewer / 使用者決定。

---

# 68. ZIP

建立：

```text
Infantry_Phase03_Revision01_Review.zip
```

ZIP 只包含：

```text
Infantry_Phase03_Revision01_Review/
```

不要包含整個 Repo。

---

# 69. ZIP 驗證

確認：

- ZIP exists
- ZIP > 0 bytes
- README exists
- v004_P03R1 Blender exists
- Waist detail exists
- Scarf detail exists
- Shield back detail exists
- Boot detail exists
- Comparison exists
- Unity_RTS_Normal exists
- Unity Review Status exists

---

# 70. Git Rule

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

# 71. Agent 最終回覆格式

## Revision Source

```text
Input:
Output:
```

## Geometry

```text
Height:
Triangles:
Meshes:
```

## Revision Result

```text
Waist Cloth:
Scarf:
Upper Arm:
Shield Back:
Boot:
```

## Unity Review

```text
Unity Close:
Unity RTS Normal:
Unity Far:
```

## Package

```text
Folder:
ZIP:
```

## Open Issues

列出實際問題。

## Status

```text
READY FOR PHASE03 REVISION REVIEW
```

---

# 72. 核心原則

本次 Revision 不是：

> 增加更多細節。

而是：

> **讓已經成立的 Secondary Forms 從「有裝備結構」提升成「裝備穿戴與材質結構邏輯可信」，並在真正 Unity RTS Camera 下確認它仍然成立。**

---

# 73. 立即執行

請直接：

```text
Read Phase03 Review
↓
Preserve v004
↓
Create v004_P03R1
↓
Fix Front Waist Cloth
↓
Fix Scarf
↓
Fix Upper Arm Cloth Form
↓
Fix Shield Back Functional Structure
↓
Integrate Boot Panels
↓
Re-check A-Pose / clipping
↓
Generate Blender Evidence
↓
Export Review FBX
↓
Create Unity Review Prefab / Scene
↓
Capture Unity_RTS_Normal
↓
Package Review ZIP
↓
Report READY FOR PHASE03 REVISION REVIEW
```

不要進 Phase 04。
不要 final texture。
不要 final UV。
不要 animation polish。
不要正式替換 Runtime Prefab。
不要 git commit。
不要 git push。
