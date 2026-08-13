# Infantry Remaster — Phase 03 Secondary Forms Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Phase:** 03 — Secondary Forms  
> **Approved Baseline:** `CHR_Infantry_A_v003_P02R1`  
> **Output Candidate:** `CHR_Infantry_A_v004`  
> **Primary References:**  
> - `Infantry_Phase01_Production_L2_Remaster_Target.md`  
> - `Infantry_Remaster_Phase02_PrimaryForms_Task.md`  
> - `Infantry_Remaster_Phase02_Revision01_Task.md`  
> - Approved Phase 02 Review Package  
> **Task Type:** Blender secondary-form modeling + review evidence + optional Unity review preview  
> **Status Goal:** READY FOR PHASE03 REVIEW  
> **Important:** 本階段不做 Final Texture、Final UV、Final Skinning、Animation Polish、正式 LOD 或正式 Runtime Prefab 替換。

---

# 0. Phase 03 核心目標

Phase 02 已經證明：

- Silhouette 成立
- Primary Forms 成立
- 角色不再是方塊 Prototype
- Heavy Shield Infantry 身份可辨識
- L1 東亞古代重裝步兵語彙保留
- Geometry Density 合理

Phase 03 的任務不是：

> 再把模型變得更高模。

而是：

> **把已通過的 Primary Forms 變成具有可信結構、裝備層次與 RTS 商業模型感的 Secondary Forms。**

---

# 1. 本階段要解決的主要問題

目前 P02R1 雖然 Primary Forms 已通過，但仍偏：

```text
clean blockout
+
large armor masses
+
minimal equipment construction
```

Phase 03 必須加入：

- Armor layering
- Plate overlap logic
- Edge thickness
- Belt / strap logic
- Cloth transitions
- Equipment attachment
- Shield construction
- Sword construction
- Boot / leg wrap structure
- Bracer / glove structure

---

# 2. 本階段不做什麼

嚴格禁止：

- Final BaseColor
- Final Normal Map
- Final ORM
- Final AO bake
- Final Team Color Mask
- Final UV polish
- Micro scratches
- Rust
- Cloth fiber
- Leather grain
- Small decorative etching
- Facial micro detail
- Final skin weights
- Animation polish
- Final LOD chain
- Shader rewrite
- VFX
- Runtime Production Prefab replacement

---

# 3. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
CHR_Infantry_A_v003.blend
CHR_Infantry_A_v003_P02R1.blend
```

建立：

```text
CHR_Infantry_A_v004.blend
```

P02R1 視為：

```text
APPROVED PRIMARY FORMS BASELINE
```

---

# 4. 保留 Phase 02 已通過的 Primary Forms

以下不得任意大改：

- Character height ≈ 1.83 m
- Head/body ratio ≈ 5.2–5.4 heads
- Helmet overall silhouette
- Shoulder width
- Shoulder armor 3-layer concept
- Chest mass
- Waist taper
- Leg proportion
- Boot overall size range
- Shield overall size / outline
- Sword length / role
- Heavy infantry silhouette
- Right-hand sword
- Left-hand shield

如果 Secondary Form 建模導致 Primary silhouette 明顯改變：

需要回報。

---

# 5. Art Direction Lock

保留：

> 東亞古代重裝步兵 + Stylized Fantasy RTS

關鍵語彙：

- Lamellar-inspired armor
- Layered shoulder armor
- Curved helmet
- Short backward plume
- Team-colored neck cloth
- Waist cloth
- Wood + metal shield
- Short single-handed sword
- Cloth leg wraps
- Leather / dark boots

禁止改成：

- Western plate knight
- Samurai
- Sci-fi soldier
- generic barbarian
- MMO hero armor
- excessive fantasy ornament

---

# 6. Secondary Forms 定義

Secondary Forms 是：

> 角色在中近距離時能看出「裝備怎麼組成」，但不依賴微小表面細節。

例如：

```text
Shoulder armor layers
Chest armor rows
Belt
Waist plates
Shield rim
Shield boss support
Bracer edges
Boot panels
Sword guard / grip structure
```

---

# 7. Primary / Secondary / Tertiary 分界

## Primary

已在 Phase 02 完成：

- 大輪廓
- 頭盔大形
- 肩甲大形
- 胸甲大形
- 腰甲大形
- 腿 / Boots
- Shield
- Sword

## Secondary

本 Phase：

- Armor row segmentation
- Plate overlap
- Belt / strap
- Edge thickness
- Attachment logic
- Cloth folds / large seams
- Shield construction
- Weapon construction

## Tertiary

Phase 04 之後：

- scratches
- tiny rivets
- wood grain
- leather grain
- micro stitching
- dents
- surface wear

---

# 8. Chest Armor — Highest Priority

Chest Armor 是 Phase 03 第一優先。

目前 P02R1 已建立：

```text
underlying chest shell
+
overlapping armor rows
```

現在要讓它真正讀成：

> Lamellar-inspired heavy armor

---

# 9. Chest Armor Row Structure

建議：

```text
Upper chest support
↓
Row 1
↓
Row 2
↓
Row 3
↓
Lower torso transition
```

每一 row 應具有：

- overlap
- thickness
- slight vertical segmentation
- slight curvature around torso

---

# 10. 不建立大量獨立甲片

禁止：

```text
hundreds of separate armor objects
```

建議：

```text
1–3 major meshes
+
modeled row structure
+
selected raised plate divisions
```

原因：

- 維護
- Skinning
- Performance
- Runtime batching
- LOD

---

# 11. Chest Plate Segmentation

每 row 可以表現：

```text
3–6 major visual plate groups
```

不需真的每片拆開。

目標：

遠看：

```text
solid armored chest
```

中近距：

```text
lamellar construction readable
```

---

# 12. Chest Edge Thickness

目前若 edge 太薄：

增加：

```text
small but readable edge thickness
```

避免：

```text
paper armor
```

不要過度厚重到：

```text
tank armor
```

---

# 13. Chest Side Transition

正面 row 必須自然轉向：

```text
rib / side torso
```

避免：

- front armor abruptly ending
- side exposed like mannequin
- armor floating in front only

---

# 14. Shoulder Armor

Phase 02 三層結構：

```text
PRESERVE
```

Phase 03 增加：

- outer hard edge
- plate overlap
- inner mounting logic
- chest connection
- thickness hierarchy

---

# 15. Shoulder Plate Hierarchy

每側：

```text
Top plate
Mid plate
Lower plate
```

建議：

- Top 最硬、最靠近肩
- Mid 提供主要外擴
- Lower 包覆 upper arm

不要全部同尺寸。

---

# 16. Shoulder Armor Connection

避免：

```text
floating shoulder pieces
```

至少建立：

- inner anchor
- strap
- under-plate
- armor support

其中一種。

不需要真的做完整機械連接。

只要視覺合理。

---

# 17. Shoulder Edge Treatment

外緣：

- 清楚 hard-surface
- slight bevel
- 不過度圓滑

避免：

```text
inflatable armor
```

---

# 18. Helmet Secondary Forms

Phase 02 Helmet Primary：

```text
PASS
```

Phase 03 加：

- rim thickness
- brow band
- side / rear protection indication
- plume mount
- optional small rear neck guard

不要加入華麗 crown。

---

# 19. Plume

保留 Phase 02 向後 silhouette。

Secondary Forms：

- base mount
- one or two broad feather / cloth mass divisions

不要做：

- individual feather strands
- hair cards

---

# 20. Face

Phase 03 不做細節臉。

只確認：

- brow
- nose
- cheek
- jaw
- chin

保持 clean stylized planes。

可以加入簡單：

- eye socket indication
- mouth line groove

但不是 final。

---

# 21. Neck Cloth / Scarf

Team identity 高優先。

Secondary Form 需要：

- wrap around neck
- overlapping cloth
- front diagonal fold
- back termination
- cloth thickness

---

# 22. Scarf 不做高頻皺褶

只做：

```text
2–4 major folds
```

不要：

- sculpt dozens micro folds
- fabric noise

---

# 23. Bracer

建立：

```text
forearm guard
+
inner attachment / strap
+
edge
```

形狀需：

- curved
- conform to forearm
- not rectangular tube

---

# 24. Gloves / Hands

本 Phase：

- palm volume
- thumb
- weapon grip
- wrist transition

可以不做完整五指分離。

但手握 Sword 必須可信。

---

# 25. Sword Grip Contact

確認：

- hand 不穿 grip
- guard 不切入 hand
- grip 長度足夠
- sword pivot / attachment 合理

---

# 26. Belt

新增明確腰帶。

功能：

- 連接 torso armor
- 支撐 waist plates
- 視覺上分開胸與腿

建議：

```text
main belt
+
central buckle / clasp form
```

buckle 不需要精雕。

---

# 27. Front Waist Cloth

Phase 02 Primary 已有 taper。

Phase 03：

- cloth thickness
- 1–3 major folds
- bottom edge shape
- belt attachment
- team color region

---

# 28. Side Waist Armor

每側建議：

```text
2–3 overlapping armor plates
```

需要：

- upper attachment
- overlap
- lower taper

避免：

```text
flat boxes
```

---

# 29. Rear Waist

建立：

- rear cloth
- rear armor layer
- belt continuation

Back silhouette 不可空。

---

# 30. Leg Armor

視 L1 保留：

- upper thigh armor
- cloth pants
- leg wraps

Secondary Form 需要讓三種材質讀開。

---

# 31. Leg Wraps

Phase 02 已從 donut 改善。

Phase 03：

建立：

```text
broad cloth wraps
+
slight overlap
+
termination logic
```

可以加入：

- subtle diagonal wrap
- simple knot / tucked end

但不要做細繩。

---

# 32. Knee Transition

不能像：

```text
thigh cylinder
↓
empty gap
↓
calf cylinder
```

需有：

- knee mass
- cloth compression / armor boundary

不用加 Knee Guard 除非 L1 支持。

---

# 33. Boots

Primary silhouette 已通過。

Phase 03 增加：

- boot opening
- upper leather panel
- instep seam / division
- sole separation
- heel block
- toe panel

---

# 34. Boots 不做微小縫線

本 Phase 只做 major panel structure。

Stitching 留 Texture / Normal。

---

# 35. Shield — High Priority

Shield 在 RTS 視角佔畫面很大。

Phase 03 必須讓 Shield 從：

```text
good primary shield
```

變成：

```text
credible wood + metal military shield
```

---

# 36. Shield Front Construction

建立：

1. Wood main body
2. Metal outer rim
3. Center boss
4. Cross / structural reinforcement
5. Selected metal fastening forms
6. Team-color panel logic

---

# 37. Wood Panel Logic

可選：

```text
3–5 broad plank divisions
```

或：

```text
single wood board + major structural seams
```

不要現在做木紋。

---

# 38. Shield Rim

Rim：

- 有實體厚度
- 沿 outer contour
- corner transition 清楚
- 不要只是黑色材質線

---

# 39. Shield Boss

Boss 已通過比例。

Phase 03：

- base ring
- boss dome
- attachment logic

不要變成 oversized ornament。

---

# 40. Shield Back

必須建立：

- grip
- forearm support / strap
- major structural brace

因為後續動畫與側面會看到。

---

# 41. Shield Curvature

保留輕微 convex curvature。

Side silhouette：

- board
- rim
- boss
- back grip

需可讀。

---

# 42. Sword

Phase 02 已通過。

Phase 03 增加：

- blade bevel
- blade spine
- guard profile
- grip wrap major form
- pommel

---

# 43. Sword 不 Hero 化

普通 Infantry Sword 禁止：

- huge gem
- dragon ornament
- giant crossguard
- oversized fantasy blade

保持軍用短劍。

---

# 44. Cloth vs Armor Separation

Geometry 層面就必須可讀：

```text
Hard armor
vs
Soft cloth
```

Armor：

- clearer edges
- plate layering
- thickness

Cloth：

- softer transition
- larger folds
- flexible silhouette

---

# 45. Leather

Leather secondary forms 可透過：

- straps
- belt
- boot panels
- shield grip

表現。

表面紋理留 Phase 04。

---

# 46. Team Color Geometry Regions

本 Phase 不做 final Team Color Mask。

但需要確定幾何區域：

```text
Neck scarf
Front waist cloth
Rear waist cloth
Shield selected panel
Optional plume accent
```

不得改成整片 armor。

---

# 47. Material Slot Planning

可以建立暫時 Material ID：

```text
MATID_Metal
MATID_Wood
MATID_Leather
MATID_Cloth
MATID_Skin
MATID_Team
```

用途：

> Preview material separation

不是 Final Material。

---

# 48. Geometry Budget

Phase 02 R1：

```text
≈ 25,106 tris
```

Phase 03 建議：

```text
28K–38K tris
```

Preferred：

```text
≈ 30K–34K
```

不是硬限制。

---

# 49. Polygon 增量必須合理

增加主要花在：

- chest plate structure
- shoulder edges
- waist plates
- shield construction
- boot panels
- bracer / glove
- sword bevel

不要大量用在：

- unseen backfaces
- tiny rivets
- micro folds

---

# 50. Mesh Count

Source Blender：

可以保持 modular。

但避免：

```text
hundreds of tiny objects
```

Recommended logical objects：

```text
Body_Base
Head
Helmet
Plume
ShoulderArmor_L
ShoulderArmor_R
ChestArmor
Scarf
Belt
WaistArmor_L
WaistArmor_R
WaistCloth_Front
WaistCloth_Back
Bracer_L
Bracer_R
Hand_L
Hand_R
LegWrap_L
LegWrap_R
Boot_L
Boot_R
Shield
Sword
```

---

# 51. Object Naming

禁止：

```text
Cube.001
Cube.002
Cylinder.043
```

作為最終 Source 命名。

使用：

```text
GEO_Infantry_ChestArmor
GEO_Infantry_ShoulderArmor_L
...
```

或專案既有規則。

---

# 52. Collection Structure

建議：

```text
CHR_Infantry_A_v004
├─ GEO_BODY
├─ GEO_ARMOR
├─ GEO_CLOTH
├─ GEO_WEAPONS
├─ RIG
├─ REVIEW
└─ TEMP
```

`TEMP` 最終 Review 前清理。

---

# 53. Normals / Shading

Review 前：

- no flipped normals
- no obvious faceting
- correct hard/smooth logic
- armor edge readable

可使用：

- Bevel
- Weighted Normal
- Shade Smooth
- Auto Smooth equivalent

---

# 54. Non-manifold

Review 前至少檢查：

```text
Non-manifold edges
Loose geometry
Zero-area faces
```

盡量為 0。

如果因 cloth open mesh 等設計存在：

需記錄理由。

---

# 55. Retopology

本 Phase 不要求 Final Animation Retopo。

但 Secondary Forms 完成後：

不可完全無視 deformation。

尤其：

- shoulder
- elbow
- hip
- knee

避免把 rigid armor topology 直接焊死在柔軟人體上。

---

# 56. Rig 本階段

保持 existing 23-bone Humanoid skeleton。

不要新增大量 bones。

本 Phase 只需要：

- source alignment
- weapon attachment
- basic pose check

Final weight polish 留 Phase 05。

---

# 57. A-Pose Review

輸出 A-Pose。

檢查：

- shoulder armor clearance
- bracer clearance
- waist armor not penetrating thigh
- shield not intersecting torso excessively
- sword not intersecting leg
- boots grounded

---

# 58. Review Material — Clay

必須有：

```text
Neutral Clay
```

用來驗收 Geometry。

不要只給 Material-ID 彩色圖。

---

# 59. Material-ID Preview

額外提供：

```text
MaterialID_Front
MaterialID_3Q
```

建議用不同灰階/簡單色區分：

- metal
- wood
- cloth
- leather
- skin
- team

目的是結構檢查。

---

# 60. Clay Capture

至少：

```text
01_Clay_Front.png
02_Clay_Left.png
03_Clay_Back.png
04_Clay_3Q_Front.png
05_Clay_3Q_Back.png
```

---

# 61. Silhouette Capture

至少：

```text
Silhouette_Front.png
Silhouette_Left.png
Silhouette_Back.png
Silhouette_3Q.png
```

確認 Secondary Forms 沒破壞 Primary silhouette。

---

# 62. Wireframe

至少：

```text
Wireframe_Front.png
Wireframe_3Q.png
Wireframe_Back.png
```

Reviewer 要看：

- polygon usage
- unnecessary density
- armor construction
- shield geometry

---

# 63. Close Detail Capture

Phase 03 新增：

```text
Detail_Chest.png
Detail_Shoulder.png
Detail_Waist.png
Detail_Shield_Front.png
Detail_Shield_Back.png
Detail_Boot.png
Detail_Sword.png
```

---

# 64. L1 Comparison

建立：

```text
L1_vs_v004_Front.png
L1_vs_v004_3Q.png
L1_vs_v004_Back.png
```

盡量 normalize：

- character height
- similar camera angle

目的：

檢查設計語彙。

---

# 65. P02R1 vs v004 Comparison

建立：

```text
P02R1_vs_v004_Front.png
P02R1_vs_v004_3Q.png
```

Reviewer 要判斷：

> Secondary Forms 是否真有提升，而不是只是 mesh count 上升。

---

# 66. Screen Size Review

重新輸出：

```text
128px
64px
32px
```

至少：

- Clay
- Silhouette

64px 必須仍清楚。

---

# 67. Unity Preview — Strongly Recommended

Phase 03 完成後：

建議這次必須做一次 Unity Preview。

如果能安全執行：

建立：

```text
PF_Unit_Infantry_v004_Review
```

或 temporary preview。

禁止替換正式：

```text
PF_Unit_Infantry
```

---

# 68. Unity Review Captures

至少：

```text
Unity_Close.png
Unity_Medium.png
Unity_RTS_Normal.png
Unity_Far.png
```

---

# 69. Unity Review Lighting

使用：

- 目前遊戲常用 Lighting
- URP current project lighting

不要用過度戲劇化 Review Light 掩蓋問題。

---

# 70. Unity RTS Normal Distance

這張是 Phase 03 重要 Gate。

檢查：

- shoulder layers
- shield construction
- chest mass
- waist silhouette
- sword readability
- team-region placeholder readability

---

# 71. 若 Unity Capture 無法自動完成

不要中止 Phase。

必須：

```text
MANUAL UNITY REVIEW REQUIRED
```

並附人工操作步驟。

但 Phase 03 最終 PASS 前，建議補拍至少一張：

```text
Unity_RTS_Normal
```

---

# 72. No Final UV

本 Phase 可以：

- temporary UV
- preserve old UV for viewport

但不投入大量時間整理 Final UV。

Final UV 在 Phase 04。

---

# 73. No Final Texture

禁止：

> 用漂亮 BaseColor 讓 Secondary Form 看起來比較好。

Clay 必須先過。

---

# 74. No Micro Detail

如果 detail 在：

```text
64 px
```

完全不可能讀到，且不影響材料分區：

多半不是 Phase 03 的優先項。

---

# 75. Secondary Form Pass Standard

Phase 03 完成後：

角色近看應該從：

```text
good stylized blockout
```

升級成：

```text
credible game character construction
```

即使完全沒有 Texture，也能看出：

- armor is layered
- cloth is attached
- shield is built
- sword is built
- boots have construction
- equipment has logic

---

# 76. FAIL — Chest

若胸甲仍像：

```text
simple smooth slabs
```

或：

```text
floating bars
```

Fail。

---

# 77. FAIL — Shoulder

若 shoulder armor：

- 浮在空中
- 完全沒有 attachment
- 像柔軟泡棉

Fail。

---

# 78. FAIL — Waist

若腰部仍：

```text
flat front rectangle
+
side boxes
```

Fail。

---

# 79. FAIL — Shield

若 Shield 仍只有：

```text
outline
+
boss
```

而沒有合理：

- wood body
- metal rim
- back grip

Fail。

---

# 80. FAIL — Boots

若 Boots 仍像：

- simple rounded mass
- no sole
- no ankle transition

Fail。

---

# 81. Phase 03 Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase03_SecondaryForms_Review_v001/
```

---

# 82. Package Structure

```text
Infantry_Phase03_SecondaryForms_Review_v001/
│
├─ README.md
├─ 00_Phase03_Report.md
├─ 01_Geometry_Stats.md
├─ 02_Secondary_Form_Change_List.md
├─ 03_Material_ID_Plan.md
├─ 04_Unity_Review_Status.md
├─ 05_Open_Issues.md
│
├─ Blender/
│  └─ CHR_Infantry_A_v004.blend
│
├─ Screenshots/
│  ├─ Clay/
│  ├─ MaterialID/
│  ├─ Silhouette/
│  ├─ Wireframe/
│  ├─ Detail/
│  ├─ Comparison/
│  ├─ ScreenSize/
│  └─ Unity/
│
└─ Manifests/
```

---

# 83. Geometry Stats

至少：

```text
Height
Vertices
Triangles
Mesh Count
Material Slots
Bone Count
Non-manifold
Loose edges
```

分部位：

```text
Head
Helmet
Shoulder
Chest
Scarf
Belt
Waist
Bracer
Hands
LegWrap
Boots
Shield
Sword
```

---

# 84. Secondary Form Change List

分類：

```text
PRESERVED
MODIFIED
ADDED
REBUILT
DEFERRED
```

---

# 85. Material ID Plan

建立：

```text
03_Material_ID_Plan.md
```

至少記：

```text
Metal:
Wood:
Leather:
Cloth:
Skin:
Team:
```

說明每一材質分配到哪些部件。

---

# 86. Open Issues

集中列：

- unclear L1 region
- clipping risk
- possible skinning issue
- UV concern
- shader concern
- Unity preview missing

---

# 87. Phase 03 Reviewer Gate

Reviewer 會檢查：

- [ ] Primary silhouette 保持
- [ ] Chest armor 已具有可信 Lamellar secondary structure
- [ ] Shoulder armor 有 attachment / overlap
- [ ] Helmet 有 rim / band / mount
- [ ] Scarf 有合理 cloth volume
- [ ] Bracer / hand 結構成立
- [ ] Belt 存在
- [ ] Waist armor 有 attachment / overlap
- [ ] Leg wraps 有 cloth logic
- [ ] Boots 有 major construction
- [ ] Shield front/back 結構成立
- [ ] Sword secondary structure成立
- [ ] Material ID 分區合理
- [ ] Geometry density 沒失控
- [ ] L1 identity 保持
- [ ] 64px readability 保持
- [ ] Unity RTS preview 無重大問題，或已標記 manual review
- [ ] P02R1 未被覆寫

---

# 88. Phase 03 不自行宣告 PASS

Agent 最終只能回報：

```text
READY FOR PHASE03 REVIEW
```

不能：

```text
PHASE03 PASS
```

由 Reviewer / 使用者決定。

---

# 89. ZIP

完成後建立：

```text
Infantry_Phase03_SecondaryForms_Review_v001.zip
```

ZIP 只包含 Review Package。

不要塞整個 Repo。

---

# 90. ZIP Verification

確認：

- ZIP exists
- ZIP > 0 bytes
- README
- Report
- Geometry Stats
- v004 Blender
- Clay
- Material ID
- Detail screenshots
- Comparison
- Unity status
- Open Issues

---

# 91. Git Rule

禁止：

```text
git commit
git push
```

可以：

```text
git status
```

回報新增 / 修改檔案。

---

# 92. 最終 Agent 回報格式

## Source

```text
Input:
Output:
```

## Geometry

```text
Height:
Triangles:
Meshes:
Bones:
```

## Main Secondary Improvements

列出：

```text
Chest:
Shoulder:
Waist:
Shield:
Boots:
Sword:
```

## Unity

```text
Preview:
YES / MANUAL REQUIRED
```

## Review Package

```text
Folder:
ZIP:
```

## Known Issues

3–10 項。

## Status

```text
READY FOR PHASE03 REVIEW
```

---

# 93. 本階段核心原則

Phase 02：

> 讓角色不再是方塊人。

Phase 03：

> **讓角色的裝甲與裝備看起來是真的「被設計、被組裝、被穿戴」的。**

Phase 04 才是：

> 讓它有真正的材質與表面完成度。

---

# 94. 立即執行

請直接：

```text
Read approved Phase 01 / 02 specs
↓
Preserve P02R1
↓
Create CHR_Infantry_A_v004
↓
Build Chest Secondary Forms
↓
Build Shoulder Secondary Forms
↓
Build Helmet / Scarf
↓
Build Belt / Waist
↓
Build Bracer / Hands
↓
Build Leg Wrap / Boots
↓
Build Shield Front / Back
↓
Build Sword Secondary Forms
↓
Create Material-ID Plan
↓
Clay / Silhouette / Wireframe / Detail Review
↓
Unity Preview if possible
↓
Package ZIP
↓
Report READY FOR PHASE03 REVIEW
```

不要進 Phase 04。
不要 final texture。
不要 final UV。
不要 animation polish。
不要 git commit。
不要 git push。
