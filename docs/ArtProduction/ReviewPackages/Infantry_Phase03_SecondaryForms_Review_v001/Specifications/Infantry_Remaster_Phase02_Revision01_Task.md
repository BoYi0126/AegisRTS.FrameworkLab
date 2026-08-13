# Infantry Remaster — Phase 02 Revision 01 (Primary Forms Gate Fix)

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A`  
> **Stage:** Phase 02 — Revision 01  
> **Current Candidate:** `CHR_Infantry_A_v003.blend`  
> **Revision Output:** `CHR_Infantry_A_v003_P02R1.blend`  
> **Decision:** `CHANGE REQUESTED`  
> **Purpose:** 修正仍屬於 Primary Forms 層級的比例、造型語彙與 L1 一致性問題。  
> **Important:** 不進入 Phase 03、不做 Final Texture、不做 Final UV、不做 Final Skinning、不做 Animation Polish、不做正式 LOD。

---

# 1. Reviewer 結論

目前 `v003` 已經成功脫離 `v002` 的純方塊 Prototype。

以下項目已確認改善：

- 角色高度 1.830 m 正確。
- v002 未被覆寫。
- v003 為獨立 Blender Source。
- Triangle Count 28,138 在 Phase 02 暫定 20K–30K 範圍內。
- Helmet 已具有 dome / rim。
- Shoulder 已建立多層結構。
- Shield 已具有 rim / boss / thickness。
- Sword 已具有 blade / guard / grip。
- 128 / 64 / 32 px 可辨認為 Shield + Sword Infantry。
- 整體 silhouette 已比 v002 有明顯質變。

因此：

> 不需要回到 v002，也不需要整隻重做。

但是目前仍有數個屬於 **Primary Forms** 的問題。

這些問題必須在 Phase 03 Secondary Forms 前修正。

---

# 2. 不通過 Phase 02 的主要原因

目前 v003 在 Clay View 下仍帶有：

- Toy-like / action-figure 感
- 過度圓管化的手臂
- 過度球形的 Helmet / Head
- Face 像拼裝零件
- Chest Armor 像水平浮動板條
- Waist Armor 仍偏大塊矩形
- Boots 偏鐘形 / 卡通鞋
- Leg wrap 偏圓環堆疊
- 與 L1 的東亞古代重裝步兵造型仍有明顯距離

這些不能留給 Texture 解決。

---

# 3. Revision 原則

不要：

```text
全部重建
```

要：

```text
Preserve good v003 forms
+
Correct critical primary forms
```

---

# 4. Version Safety

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
CHR_Infantry_A_v003.blend
```

建立：

```text
CHR_Infantry_A_v003_P02R1.blend
```

原始 v003 作為：

```text
Phase02 Initial Candidate
```

保留供比較。

---

# 5. L1 仍為最高視覺來源

Revision 必須重新對照：

```text
Unit_03_Infantry_L1_Concept_Final.png
```

以及：

```text
Infantry_Phase01_Production_L2_Remaster_Target.md
```

不能只在目前 v003 上自由發揮。

---

# 6. Critical Fix A — Head / Helmet Proportion

## Current Problem

目前正面與 3/4：

- Helmet 整體偏球形。
- Helmet / Head 橫向體積過大。
- 臉部由 Nose / Brow / Cheek 等獨立 primitive-like pieces 組成，像面具或玩具零件。
- Plume 目前更像頂部小球，而非 L1 的向後羽飾。

## Required Change

### Helmet width

相對目前 v003：

```text
Reduce approximately 8–12%
```

不是硬性數字，以 L1 silhouette 為準。

### Helmet dome

改成：

```text
slightly narrower top
+
wider lower dome
+
clear metal rim
```

避免近似完整球體。

### Helmet vertical proportion

保持頭盔具有厚重感，但不要讓：

```text
Helmet + Head
```

成為全身最誇張的大球。

---

# 7. Plume — REBUILD

目前頂部造型不符合 L1。

L1 方向：

```text
small mount
+
short backward-curved plume / feather shape
```

Revision：

- 建立向後的曲線 silhouette。
- Plume 不需要羽毛細節。
- Primary Form 只需形成清楚的 backward accent。
- 不要使用單純上下堆疊球 / cone。

---

# 8. Face — MODIFY

目前 face pieces 太突出。

Revision 目標：

> Stylized human face planes，而不是拼裝 mask。

至少：

- Brow 更融入 head。
- Nose 不要是突出長方柱。
- Cheek 不要看起來像兩個貼上去的球。
- Jaw / chin 要形成一個連續頭部 silhouette。

可以：

- 合併 Face geometry。
- 重新塑形 Head mesh。
- 使用更低頻率、更大的 facial planes。

本階段不做：

- 眼球
- 嘴唇細節
- facial rig
- wrinkle

---

# 9. Critical Fix B — Shoulder / Upper Arm

## Current Problem

v003 的肩膀雖然比 v002 好，但：

- UpperArm 過於巨大且圓管化。
- Shoulder + arm 在 A-Pose 下有「充氣玩偶」感。
- L1 是「肩甲厚、布料手臂相對收斂」，不是整條手臂同樣巨大。

## Required Change

UpperArm overall radial volume：

```text
Reduce approximately 10–15%
```

尤其：

- shoulder → mid upper arm

保持：

```text
Armor wide
Body arm narrower
```

形成層次。

---

# 10. Shoulder Armor

現有三層 shoulder 概念：

```text
PRESERVE
```

但需要：

- 層片更像裝甲。
- 外側下降角度稍微增加。
- 減少「三片柔軟香蕉片」感。
- 每片 outer edge 稍硬。
- 上層與胸甲更有連接關係。

不要增加更多層數。

---

# 11. Critical Fix C — Chest Armor

## Current Problem

目前 Front Chest：

```text
4 large horizontal separated bars
```

視覺容易讀成：

- sci-fi rib cage
- floating horizontal panels
- robot chest

而不是 L1 的：

> Lamellar-inspired armor.

這是本次 Revision 的最高優先項之一。

---

# 12. Chest Revision Target

保留：

```text
Primary Chest Shell
```

重新設計前側甲片節奏。

建議：

```text
continuous underlying chest shell
+
3–4 overlapping horizontal armor rows
+
small vertical segmentation / plate indication
```

核心：

> Rows 要 overlap / nest，而不是彼此之間存在大空隙。

遠看：

```text
one strong armored chest mass
```

近看：

```text
lamellar / layered construction
```

---

# 13. Chest Gaps

目前水平片之間的黑色空隙過強。

Revision：

```text
Reduce visible gap substantially
```

讓甲片形成：

```text
overlap
```

而不是：

```text
floating shelves
```

---

# 14. Center Chest Piece

目前中央垂直件太像大型 strap / bar。

保留 scarf / strap 概念，但：

- 減少其幾何重量。
- 不讓它切斷整個胸甲。
- L1 主要視覺仍是 Lamellar Chest，而不是中央大條板。

---

# 15. Critical Fix D — Waist Armor

## Current Problem

目前：

- Front center cloth / plate 太矩形。
- 左右大腿外側甲塊仍接近 block。
- Waist → Thigh transition 不自然。

## Required Change

Front cloth：

```text
narrower at upper region
slight widening / taper toward bottom
or subtle inverse taper according to L1
```

必須有：

- 非矩形輪廓
- 厚度
- 末端造型

---

# 16. Side Waist / Thigh Plates

每側改成：

```text
2–3 overlapping plate masses
```

或：

```text
one main plate + one secondary overlap
```

不需要增加大量小片。

目標：

> 從腰部自然垂向大腿，而不是兩個大型矩形盒貼在腿上。

---

# 17. Back Waist

Back view 目前過於平。

Revision：

- 保留 rear cloth / plate。
- 增加輕微 taper。
- 讓 rear silhouette 與 Front 有一致 armor language。

---

# 18. Critical Fix E — Legs

## Current Problem

腿已比 v002 有 anatomy rhythm，但：

- Thigh / calf 仍偏圓柱。
- Leg wraps 看起來像數個 donut ring。
- Boot 和 calf transition 偏玩具。

## Required Change

Leg shape：

保持：

```text
Thigh > Knee > Calf > Ankle
```

但增加：

- front/back plane
- side plane
- slight asymmetry / human leg direction

不要改成高寫實 anatomy。

---

# 19. Leg Wraps

目前四個環狀 objects：

需要改成：

```text
2–3 broader wrap bands
```

或具有：

```text
slight spiral / overlapping cloth indication
```

避免：

```text
stacked rubber rings
```

本階段只做主要造型。

---

# 20. Critical Fix F — Boots

## Current Problem

v003 Boots 比 v002 方塊鞋好很多。

但是正面仍偏：

- bell-shaped
- oversized toy boots

## Required Change

相對目前：

```text
Reduce visual width approximately 8–12%
```

尤其：

- toe outer width

保留：

- sole
- heel
- instep
- toe

---

# 21. Boot Shape

Front：

- toe 不要完全左右對稱鐘形。
- 輕微內外側差異。
- ankle transition 收窄。

Side：

- toe 向前。
- heel 獨立。
- sole thickness 可讀。

---

# 22. Shield — MOSTLY PRESERVE

目前 Shield Primary Form：

```text
PASS WITH MINOR CHANGE
```

保留：

- overall height
- width
- polygon outline
- rim
- center boss
- cross reinforcement
- thickness

---

# 23. Shield Boss

目前 boss 偏大。

Revision 建議：

```text
Reduce diameter approximately 10–15%
```

讓：

```text
wood body
+
rim
+
boss
```

比例更接近 L1。

---

# 24. Shield Curvature

若成本合理：

加入非常輕微：

```text
convex bow
```

不要完全像一片平板。

只需要幾何 Primary Form。

---

# 25. Sword — PRESERVE WITH MINOR POLISH

Sword 整體：

```text
PASS
```

保留：

- length
- one-handed role
- taper
- guard
- grip

Minor：

- guard 可略增加 shape definition。
- pommel 不要太球狀。
- blade edge / spine 保持可讀。

不要大改尺寸。

---

# 26. Body Width Balance

Revision 後應達到：

```text
Armor shoulders = widest upper body feature
Upper arms = subordinate
Chest = solid
Waist = narrower
Legs = stable but not oversized
```

不要：

```text
head / arm / boots
```

成為比 armor 更搶眼的大型圓形。

---

# 27. Target Visual Language

Revision 最終 Clay View 應接近：

```text
Stylized RTS soldier
```

而不是：

```text
toy figurine
robot
inflatable humanoid
```

---

# 28. Triangle Budget

目前：

```text
28,138
```

Revision 不要求增加面數。

Target：

```text
24K–30K acceptable
```

如果重建後：

```text
26K–29K
```

非常合理。

不要以：

> 面數越多越好

作為改善目標。

---

# 29. Triangle Redistribution

目前 Shoulder：

```text
5,544 tris
```

Leg Wrap：

```text
2,240 tris
```

數量偏高。

本 Revision 不要求完整 Optimization。

但如果重建方便：

- Shoulder 可減少不影響 silhouette 的 subdivisions。
- Leg wrap 可顯著降低密度。
- 將預算保留給 Head / torso / deformation-ready form。

正式 Retopo / optimization 仍是後續工作。

---

# 30. Comparison Requirement

本 Revision 必須提供：

```text
v003_initial_vs_P02R1_Front.png
v003_initial_vs_P02R1_3Q.png
```

另提供：

```text
L1_vs_P02R1_Front.png
L1_vs_P02R1_3Q.png
```

L1 comparison：

- Normalize overall character height。
- 不要求 pose 完全一致。
- 主要比較 silhouette / proportion / equipment language。

---

# 31. Clay Captures

必須：

```text
Clay_Front
Clay_Left
Clay_Back
Clay_3Q_Front
Clay_3Q_Back
```

---

# 32. Silhouette Captures

必須：

```text
Silhouette_Front
Silhouette_Left
Silhouette_Back
Silhouette_3Q
```

---

# 33. Wireframe

至少：

```text
Wireframe_Front
Wireframe_3Q
```

---

# 34. Screen Size

重新產：

```text
Silhouette_128px
Silhouette_64px
Silhouette_32px
```

目的：

確認 Revision 沒破壞 Infantry readability。

---

# 35. Unity

本 Revision 仍然：

```text
Unity capture optional
```

不要為了 Unity 截圖修改正式 Runtime Prefab。

Phase 02 Gate 可以主要使用 Blender Review。

---

# 36. Review Package

建立：

```text
Infantry_Phase02_PrimaryForms_Revision01_Review/
```

並打包：

```text
Infantry_Phase02_PrimaryForms_Revision01_Review.zip
```

---

# 37. Review Package 必須包含

```text
README.md
00_Revision_Report.md
01_Geometry_Stats.md
02_Change_List.md
03_Open_Issues.md

Blender/
  CHR_Infantry_A_v003_P02R1.blend

Screenshots/
  Clay/
  Silhouette/
  Wireframe/
  Comparison/
  ScreenSize/
```

---

# 38. Revision PASS Gate

Reviewer 會確認：

- [ ] Helmet 不再過度球形。
- [ ] Plume 有 L1 的向後 silhouette。
- [ ] Face 不再像貼上 primitive pieces。
- [ ] UpperArm 不再像巨大圓管。
- [ ] Shoulder armor 保留三層但更像硬甲。
- [ ] Chest 不再像 floating horizontal bars。
- [ ] Lamellar chest 讀成一個完整護甲體。
- [ ] Front waist cloth 不再是矩形 slab。
- [ ] Side waist armor 不再是 box。
- [ ] Leg wraps 不像 stacked donuts。
- [ ] Boots 不再過度鐘形。
- [ ] Shield boss 比例更合理。
- [ ] Sword 保持 readable。
- [ ] L1 identity 更接近。
- [ ] 64 px Infantry readability 保留。
- [ ] v002、v003 initial 均未被覆寫。

---

# 39. 不要做 Phase 03 工作

即使 Revision 很順利：

不要自行開始：

```text
Secondary ornament
Final armor detail
Final UV
Texture
Skinning
Animation
LOD
```

完成後只回報：

```text
READY FOR PHASE02 REVISION REVIEW
```

---

# 40. Reviewer Decision

Phase 02 R1 完成後：

### 若通過

進：

```text
Phase 03 — Secondary Forms
```

### 若仍有重大比例錯誤

做：

```text
Phase 02 Revision 02
```

預期 R1 應該足以通過，不希望無限反覆。

---

# 41. 最終核心要求

不要把這次 Revision 理解成：

> 再加更多細節。

它真正的目標是：

> **把 v003 已經成功建立的高品質 Primary Forms 基礎，從「玩具化的 stylized blockout」修正為「可以承接正式 Secondary Forms 與材質的 Stylized RTS Infantry」。**

---

# 42. 立即執行

請直接：

```text
Preserve v002
Preserve v003 initial
↓
Create v003_P02R1
↓
Fix Head / Helmet / Plume
↓
Fix Arm Proportion
↓
Fix Chest Lamellar Mass
↓
Fix Waist Forms
↓
Fix Legs / Wraps / Boots
↓
Minor Shield / Sword adjustment
↓
Generate Evidence
↓
Package ZIP
↓
Report READY FOR PHASE02 REVISION REVIEW
```

不要 git commit。
不要 git push。
不要自行進 Phase 03。
