# CHR_Infantry_A — Phase 01 Production L2 / Remaster Target

**Project:** AegisRTS.FrameworkLab  
**Asset:** `CHR_Infantry_A` / `unit.infantry` / `PF_Unit_Infantry`  
**Phase:** 01 — Production L2 / Visual & Construction Target  
**Specification Version:** 1.0  
**Status:** READY FOR USER APPROVAL → PHASE 02  
**Baseline:** `CHR_Infantry_A_v002`  
**Next Candidate:** `CHR_Infantry_A_v003`  

---

# 1. 本文件目的

本文件將現有 Infantry L1 Concept 轉換為可供 Blender / Codex Agent 執行的正式 Production L2 建模目標。

本文件不是新的 Concept Art。

它的任務是回答：

> `CHR_Infantry_A_v003` 到底應該建成什麼形狀、哪些部分要保留、哪些部分要重建，以及什麼程度才算通過 Primary Form 驗收。

本文件與既有 L1 Concept 合併視為 Phase 01 的正式 L2 Production Reference。

---

# 2. Phase 01 使用的主要證據

## 2.1 L1 Visual Source

主要視覺來源：

```text
Unit_03_Infantry_L1_Concept_Final.png
```

L1 已提供：

- Front
- Left Side
- Back
- 3/4
- Blue Team
- Red Team
- Black Silhouette
- Character Height
- Shield Size
- Sword Size
- Color Palette
- Team Color Coverage

因此 Phase 01 不重新生成另一套可能前後不一致的 AI Turnaround。

---

## 2.2 Current L3 Baseline

目前 Production Candidate Baseline：

```text
CHR_Infantry_A_v002.blend
SK_Infantry_A_v002.fbx
PF_Unit_Infantry.prefab
```

已確認：

```text
Character Height: 1.830 m
LOD0: 4,376 triangles
LOD1: 1,512 triangles
LOD2: 542 triangles
Skeleton: 23 bones
Saved Blender Actions: 0
Max body bone influence: 1
```

目前 L3 技術整合可用，但外觀屬於 Prototype / low-poly block construction。

---

# 3. Art Direction Lock

正式 Infantry 不重新設計成西方騎士，也不改成 Generic Fantasy Soldier。

必須保留：

> **東亞古代軍事語彙 + Stylized Fantasy RTS Readability**

核心特徵：

- 東亞古代重裝步兵
- 層疊式札甲 / Lamellar-inspired Armor
- 圓頂金屬頭盔
- 頭頂短羽飾 / Plume
- 中大型木盾
- 單手短劍
- 布料披肩 / 領巾
- 腰部垂布
- 腿部綁帶
- 厚重穩定下盤

視覺處理採：

- Heroic proportions
- Chunky but not cubic
- Strong silhouette
- Exaggerated equipment
- Curved armor surfaces
- Clear material separation
- Stylized hand-painted / PBR-assisted surface

---

# 4. 禁止偏離的方向

禁止將 v003 改成：

- Warcraft 角色的直接複製
- 西方板甲騎士
- 日本武士
- 高寫實中國歷史復原
- Minecraft / voxel
- Roblox-like block body
- Primitive-only low poly
- 超瘦寫實人體
- 超大型 MMO Hero proportions
- Anime chibi
- Photoreal human

目標是：

> 保留目前 L1 身份，但使用商業 RTS 角色常見的造型清晰度與製作品質。

---

# 5. World Scale Lock

角色世界高度：

```text
Target: 1.83 m
Allowed: 1.80–1.85 m
```

原因：

- 目前 `v002` 已為約 1.83 m。
- L1 指定 1.75–1.85 m。
- 不需要因 Remaster 改變 Gameplay Scale。

Ground Contact：

```text
Foot sole = Y/Z world ground according to project coordinate convention
```

Pivot / root gameplay contract：

> Preserve current runtime contract.

---

# 6. Proportion Target

L1 指定約：

```text
5.0–5.5 heads
```

Production Target：

```text
5.2–5.4 heads
Preferred visual target: ≈ 5.3 heads
```

這比寫實人體更厚重，也更適合 RTS。

---

# 7. Overall Body Shape

## 7.1 Shoulder / Chest

目標：

- 寬肩
- 厚胸
- 腰部收窄
- Armor 形成梯形上半身

Armored shoulder width：

```text
Target visual range: 0.64–0.70 m
```

不要使用：

```text
兩個方塊直接掛在 torso 兩側
```

肩部輪廓應由：

```text
Deltoid
+
Shoulder armor
+
Upper chest
```

形成連續造型。

---

## 7.2 Waist

腰部視覺必須明顯比胸肩窄。

目標：

```text
Shoulder-heavy
↓
Tapered waist
↓
Stable leg stance
```

避免目前接近：

```text
vertical rectangular torso
```

的方柱感。

---

## 7.3 Legs

腿部不得是兩根等寬長方柱。

必須可以辨認：

```text
Thigh
Knee
Calf
Ankle
Boot
```

不需要寫實肌肉。

主要目標：

> 讓下半身具有重量感與關節節奏。

---

# 8. Head / Face

Current Problem：

目前頭部接近低面數球體，臉部缺乏明確平面與五官結構。

Production Target：

- Stylized head
- 寬下顎
- 鼻樑明確
- 眉骨可讀
- 眼睛簡化
- 嘴部簡化
- 不做毛孔與微細節

頭部在 RTS Camera 不需要 Facial Rig。

但 Close View 不可再呈現：

> faceted sphere with skin color

---

# 9. Helmet — PARTIAL REBUILD

Helmet 是 Primary Form。

必須建立：

1. Curved dome
2. Metal brow / rim band
3. Top mount
4. Short plume
5. Rear / side volume
6. Readable thickness

目標：

```text
round dome
+
hard metal rim
+
vertical plume accent
```

禁止：

```text
sphere + cylinder
```

直接作為 final result。

Helmet 外緣必須有適度 bevel / curvature。

---

# 10. Shoulder Armor — REBUILD

肩甲是 L1 與目前 L3 差距最大的部位之一。

Production Target：

- 每側 2–3 層主要甲片視覺
- 外側略向下包覆
- 上層較短
- 下層較寬
- 有實體厚度
- 不需每一片都單獨 Object

Silhouette：

```text
Chest
  ↘
   layered shoulder
     ↘
      upper arm
```

不可：

```text
torso → square block → cylinder arm
```

---

# 11. Chest Armor — REBUILD

L1 為明顯札甲 / Lamellar-inspired Chest Armor。

Production Geometry 需至少表現：

- 胸甲主殼
- 中央分區
- 水平 / 垂直甲片節奏
- 胸口厚度
- 下胸至腰部的層級

不建議真的做上百個獨立甲片。

可採：

```text
Primary shell geometry
+
Selected raised plates
+
Normal / BaseColor detail
```

目標：

> 近看有結構，遠看是一個強烈胸甲塊面。

---

# 12. Neck Cloth / Scarf

L1 的藍 / 紅領巾是重要 Team Identity。

必須保留。

來源形狀：

- 前胸斜向布料
- 頸後布料
- 略有厚度
- 與金屬裝甲形成 Material Contrast

Team Color 的高優先區。

不可做成：

```text
flat rectangular collar
```

---

# 13. Bracers / Gloves

手臂應拆成可讀節奏：

```text
Shoulder Armor
Upper Arm
Bracer
Glove / Hand
```

Bracer：

- 有厚度
- 圓弧包覆
- 避免矩形筒

Hands：

可適度放大約：

```text
+5–10% stylization
```

以增加武器持握可讀性。

---

# 14. Waist Armor — REBUILD

腰部是另一個 Primary / Secondary 交界的重要區域。

需要：

- Belt
- Front cloth
- Side armor panels
- Rear cloth / armor
- Layered silhouette

L1 的前 / 後 Team Color 垂布必須保留。

Front cloth 不可只是：

```text
single flat rectangle
```

應有：

- taper
- thickness
- simple folds / contour

---

# 15. Leg Armor / Wraps

L1 具有：

- 大腿護甲 / skirt plates
- 深色褲
- 小腿綁帶
- 靴子

Production Model 必須分出材質層級：

```text
Armor
Cloth
Wrap
Leather boot
```

即使使用相同 atlas，也要讓形狀清楚區分。

---

# 16. Boots — REBUILD

目前方塊鞋是 Prototype 感主要來源之一。

Boot Target：

- Toe shape
- Instep
- Heel
- Sole thickness
- Slight outward stance

鞋頭可略大：

```text
+8–12% stylization
```

但不得變成巨大卡通鞋。

---

# 17. Shield — REBUILD SURFACE, PRESERVE ROLE/SCALE

L1 Shield：

```text
Height: 0.75–0.95 m
Width: 0.55–0.65 m
```

Production Target 建議：

```text
Height: ≈ 0.86 m
Width: ≈ 0.60 m
Thickness: ≈ 0.05–0.07 m
```

保持中大型單手盾。

---

# 18. Shield Construction

Shield 必須至少包含：

1. Wood body
2. Metal outer rim
3. Center boss
4. Structural reinforcement
5. Thickness
6. Back grip / attachment logic
7. Team-color region

Front silhouette：

> 接近 L1 的多邊形木盾，而不是純長方形。

Side View：

> 必須看得出厚度與中央 boss protrusion。

---

# 19. Shield Material Layout

建議：

```text
Wood: 55–65%
Metal: 20–30%
Team Color: 10–20%
```

不要讓 Team Color 蓋住整面盾牌。

Team Color 需遠距可讀。

---

# 20. Sword — REBUILD SURFACE, PRESERVE CLASS

L1：

```text
Overall Length: 0.90–1.10 m
```

Preferred Production Target：

```text
≈ 0.98–1.02 m
```

Sword 必須包含：

- Blade
- Edge
- Spine / blade thickness
- Guard
- Grip
- Pommel

禁止：

```text
flat rectangular stick
```

---

# 21. Sword Readability

RTS Camera 下：

- Sword blade 必須比寫實稍厚
- Guard 必須形成可讀橫向形狀
- Blade taper 必須明顯
- Tip 必須有明確終點

不需要複雜雕花。

---

# 22. Team Color Coverage

沿用 L1：

```text
15–25% visible surface
```

Priority areas：

1. Neck scarf
2. Waist cloth
3. Shield panel / emblem region
4. Small armor trim / plume accent

不要使用：

- 整個 Armor 變藍 / 紅
- 整個盾牌變藍 / 紅
- 整個角色 uniform recolor

---

# 23. Material Language

主要材料：

```text
Dark steel / iron
Wood
Leather
Cloth
Skin
Wrap fabric
```

Production Target：

即使只有 BaseColor，也應能區分上述材質。

PBR Lighting 是加強，不是唯一辨識手段。

---

# 24. Geometry vs Texture Rule

以下必須使用 Geometry：

- Helmet silhouette
- Helmet rim
- Shoulder layers
- Chest main armor volume
- Waist armor silhouette
- Bracer outer shape
- Boots
- Shield rim
- Shield boss
- Sword blade/guard/grip

以下可以主要依賴 Texture / Normal：

- 細小刮痕
- 木紋
- 小鉚釘
- 甲片細線
- 皮革紋路
- 布料纖維
- 小凹槽

---

# 25. Source Object Strategy

為了 v003 後續維護，Blender Source 建議保持邏輯模組。

Minimum recommended source separation：

```text
Body_Base
Head
Helmet
ShoulderArmor_L
ShoulderArmor_R
ChestArmor
WaistArmor
Bracer_L
Bracer_R
Boot_L
Boot_R
Scarf
WaistCloth
Shield
Sword
```

注意：

> Source 可分離，不代表 Unity Runtime 一定要產生一個 Draw Call / Renderer 對應一個 Object。

Export / runtime 可依 batching/material 策略合併。

---

# 26. Skinning Strategy Target

Soft anatomy：

```text
Body
Shoulder joint
Elbow
Hip
Knee
Ankle
```

需要 Smooth Weight。

Production LOD0 人體：

```text
Recommended max influences: 4
```

Rigid / near-rigid：

```text
Helmet
Hard shoulder plates
Shield
Sword
selected rigid armor plates
```

可以採：

- bone parenting
- rigid weights
- constrained multi-weight

依部位決定。

---

# 27. LOD0 Geometry Budget

Current：

```text
4,376 triangles
```

Production Target：

```text
20,000–30,000 triangles
```

Preferred target：

```text
≈ 24K–27K
```

不是要求刻意湊到指定數字。

目的：

> 提供足夠曲面、裝甲層次與關節 topology。

---

# 28. Suggested LOD0 Budget Distribution

可作為初始規劃：

| Region | Approx. Triangle Budget |
|---|---:|
| Body + Head | 7K–10K |
| Armor + Cloth | 8K–12K |
| Helmet | 1.5K–2.5K |
| Shield | 1.5K–2.5K |
| Sword | 0.8K–1.5K |
| Reserve / polish | 1K–3K |

不是硬性限制。

---

# 29. Primary Forms Acceptance

Phase 02 灰模完成時：

不看 Texture。

必須只用 Clay / Solid Shading 驗收。

以下全部要通過：

- 身形不再像方柱
- 頭部不再像低面球體
- Helmet 曲面清楚
- Shoulder layers 清楚
- Chest armor 有厚度
- Waist silhouette 清楚
- Legs 有 thigh/knee/calf/boot 節奏
- Boots 不再是方塊
- Shield 有厚度、rim、boss
- Sword 有 blade taper / guard / grip

---

# 30. Silhouette Acceptance

建立純黑 Silhouette：

```text
Front
Side
Back
3/4
```

Reviewer 必須不依靠顏色就能辨認：

```text
Heavy Infantry
Shield user
Sword user
Armored East-Asian-inspired soldier
```

如果關閉材質後仍像：

```text
box humanoid
```

則 Phase 02 Fail。

---

# 31. 64 px Readability

角色 Screen Height 約 64 px 時至少要辨認：

- Helmet / plume
- Large shield
- Sword
- Shoulder armor
- Heavy torso
- Team Color major block

---

# 32. 32 px Readability

約 32 px：

不要求看甲片。

至少仍要辨認：

```text
Shield infantry
vs
Archer / Mage / Cavalry
```

---

# 33. A-Pose

Production Source Neutral Pose：

```text
A-Pose
```

手臂：

- 不使用完全水平 T-Pose
- 肩膀自然下沉
- 保留 Humanoid mapping 可用性

目的：

- Shoulder armor placement
- Skinning
- Retargeting
- Neutral modeling

---

# 34. Face / Helmet Clearance

Phase 02 必須確認：

- Helmet 不穿 Head
- Forehead / eyes 可讀
- 頭盔 brim 不完全遮住臉
- Neck / scarf 不切入 jaw
- Head rotation 有基本 clearance

---

# 35. Shoulder Clearance

肩甲必須：

- 不阻塞 A-Pose
- 不嚴重穿 Chest
- UpperArm 抬起時保留可調空間

Phase 05 才做完整 deformation polish。

Phase 02 只需避免明顯結構錯誤。

---

# 36. Shield / Body Relationship

Front / 3/4：

盾牌應覆蓋角色約：

```text
roughly torso-to-knee defensive area
```

但不得：

- 完全遮掉整隻人物
- 過度貼身
- 與肩甲大量穿模

---

# 37. Sword / Body Relationship

Neutral pose：

Sword 不應與腿部平行形成一條不易閱讀的線。

持劍手需具有：

- clear hand grip
- visible guard
- blade separation from leg silhouette

---

# 38. L2 Visual Reference Policy

不要再使用 AI 重新產生一套：

```text
Front / Side / Back
```

並假設它們完全一致。

本 Phase 正式 Production Reference 為：

```text
Existing L1 multi-view concept
+
This construction specification
+
Phase 02 actual 3D orthographic clay captures
```

當 Phase 02 的 3D Blockout 被批准後：

> 該 3D 模型本身會成為後續完全一致的 Front/Side/Back Production Reference。

---

# 39. L1 → v003 Translation Matrix

| L1 Feature | Current v002 Problem | v003 Target |
|---|---|---|
| Rounded helmet | faceted primitive | curved dome + rim + plume |
| Layered shoulders | block / low-detail | 2–3 readable armor layers |
| Lamellar chest | flat grid block | shaped shell + plate rhythm |
| Scarf | rectangular collar | wrapped cloth volume |
| Arm protection | block/cylinder | arm → bracer → hand rhythm |
| Waist armor | flat strips | belt + layered panels + cloth |
| Wrapped lower leg | column | calf + wrap + ankle |
| Boots | cubes | stylized boot form |
| Wood shield | flat polygon slab | rim + wood + boss + thickness |
| Short sword | rectangular blade | tapered blade + guard + grip |
| Heroic silhouette | thin/rectangular | wide shoulder + tapered waist + stable legs |
| Team color | usable but flat | same coverage, better integrated forms |

---

# 40. Preserve List

以下 Phase 02 不得任意破壞：

```text
Asset ID
World scale
Root convention
Humanoid skeleton hierarchy
Socket naming
Right-hand sword role
Left-hand shield role
AttackImpact contract
Animator parameter contract
In-place locomotion
Root Motion Off
Prefab/content binding
SelectionAnchor
HealthBarAnchor
```

---

# 41. Phase 02 Versioning Rule

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
SK_Infantry_A_v002.fbx
```

下一版：

```text
CHR_Infantry_A_v003.blend
```

Phase 02 初期不必直接替換 Runtime Prefab。

先建立：

```text
v003 Primary Forms Candidate
```

通過 Review 後再進下一階段。

---

# 42. Phase 02 禁止事項

Primary Forms 階段不要先做：

- final texture
- final UV polish
- scratches
- tiny rivets
- complex normal map
- final animation polish
- final LOD chain
- shader rewrite
- VFX

Primary Forms 如果失敗：

> 不得靠 Texture 掩蓋。

---

# 43. Phase 01 Approval Checklist

進入 Phase 02 前請確認：

- [ ] 保留東亞古代重裝步兵身份
- [ ] 保留盾 + 短劍定位
- [ ] 角色高度鎖定約 1.83 m
- [ ] 5.2–5.4 頭身可接受
- [ ] 肩部比目前更寬厚可接受
- [ ] Helmet 使用圓頂 + rim + plume
- [ ] 肩甲改成層疊造型
- [ ] 胸甲重做成有厚度的札甲語彙
- [ ] Waist / boots / limbs 不再使用方柱
- [ ] Shield 重建結構但維持尺寸級別
- [ ] Sword 重建結構但維持短劍級別
- [ ] Team Color 維持 15–25%
- [ ] v003 LOD0 目標約 20K–30K triangles
- [ ] v003 採正常 smooth skinning + rigid armor 混合
- [ ] v002 永久保留做 Prototype baseline

---

# 44. Phase 01 Exit Criteria

以下條件成立後：

```text
PHASE 01 = APPROVED
```

1. 使用者接受本文件整體造型方向。
2. 不需要重新設計 L1 角色身份。
3. 尺度與裝備類型鎖定。
4. Primary Forms 的重建範圍鎖定。
5. Phase 02 可以在不猜測核心美術方向的情況下開始。

---

# 45. 下一步

Phase 01 通過後建立：

```text
Infantry_Remaster_Phase02_PrimaryForms_Task.md
```

交由 Codex Agent 執行。

Agent 任務：

```text
Preserve v002
↓
Create v003
↓
Rebuild Primary Forms
↓
Create Clay / Silhouette / Wireframe Evidence
↓
DO NOT finalize texture
↓
Package Review Evidence
```

Phase 02 完成後由 Reviewer 檢查：

```text
Front
Side
Back
3/4
Clay
Black Silhouette
Wireframe
Unity temporary preview
```

通過才進入 Phase 03。
