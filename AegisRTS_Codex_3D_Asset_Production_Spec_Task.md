# AegisRTS 3D 資產 Production 規格整理與重建任務

> **用途**：將本文件直接交給 Codex Agent / Coding Agent 執行。  
> **任務類型**：現有專案盤點 + 3D 資產規格系統建立 + L1/L2/L3/L4 Pipeline 重整 + Golden Sample 規格 + 文件封裝。  
> **預設專案根目錄**：`C:\projects\Unity\AegisRTS.FrameworkLab`  
> 若目前 Agent 工作目錄已經是正確專案根目錄，優先使用目前 Workspace；不要因路徑不同而中止。  
> **重要原則**：本任務以「整理、分析、建立規格文件」為主。除非本文件明確要求，**不要修改、刪除或覆寫現有 Unity / Blender / FBX / Texture / Animation 資產**。

---

# 0. Agent 最終目標

請完整掃描目前 RTS 專案內與以下內容相關的文件與資產：

- L1 Concept
- L2 Character Sheet / Turnaround
- L3 3D Model
- L4 Unity Integration
- Infantry / 步兵
- Archer / 弓兵
- Character
- Unit
- Hero
- Special Unit
- Building
- Animation
- Rig
- FBX
- Blender
- Material
- Texture
- Team Color
- LOD
- Prefab
- Animator
- Weapon
- Shield
- Bow
- Projectiles

然後建立一套可以長期沿用的：

> **Stylized Fantasy RTS Production Asset Specification System**

這套規格未來必須能支援：

1. 一般兵種
2. 遠程兵種
3. 騎兵
4. 法師
5. 重裝單位
6. 攻城單位
7. 特殊單位
8. Hero / 英雄
9. 非人型單位
10. 建築物
11. 後續新陣營
12. AI 生成與人工製作並存的資產流程

目前專案已有的：

- Infantry / 步兵
- Archer / 弓兵

必須視為：

> **Golden Sample 候選資產**

不要立刻大量建立其他兵種。

先利用這兩種單位把整套 Production Pipeline 與品質標準建立完成。

---

# 1. 執行前規則

## 1.1 不得破壞現有專案

除非文件明確要求，禁止：

- 刪除現有 `.blend`
- 刪除現有 `.fbx`
- 刪除 Texture
- 修改 Prefab
- 修改 Scene
- 修改 Animator
- 修改 Shader
- 修改現有 C# 程式
- 改名現有資產
- 移動現有資產
- 自動重新 Import 大量 FBX
- 自動修改 Rig 設定
- 自動覆蓋原始文件

本次主要工作：

> **Inspect → Analyze → Document → Package**

---

## 1.2 不得假設不存在的資訊

如果沒有找到某項資料，例如：

- L2
- UV
- Rig
- Texture
- Polygon Count
- Team Color Mask
- Animation Event

請標記：

```text
STATUS: NOT FOUND
```

或：

```text
STATUS: CANNOT VERIFY
```

不要自行假裝已存在。

---

## 1.3 所有推論必須標示

Agent 若根據：

- 檔名
- 資料夾名稱
- Unity meta
- Prefab Reference
- FBX Importer
- Material 名稱
- 文件內容

進行推論，請標記：

```text
INFERRED
```

並說明推論依據。

---

# 2. 掃描範圍

請遞迴檢查專案。

優先掃描：

```text
Assets/
Docs/
Documentation/
Art/
ArtSource/
Characters/
Units/
Buildings/
Animations/
Materials/
Textures/
Prefabs/
Models/
ModelsSource/
FBX/
Blender/
Design/
Specifications/
Specs/
```

若實際路徑不同，以專案實際結構為準。

也請搜尋以下副檔名：

```text
.md
.txt
.json
.yaml
.yml
.asset
.prefab
.controller
.anim
.fbx
.blend
.obj
.png
.jpg
.jpeg
.tga
.psd
.exr
.mat
.meta
```

對大型 Binary 檔案：

- 不需要暴力解析所有內容。
- 優先使用檔名、路徑、Unity Reference、Metadata 與可用工具讀取。
- 不要破壞 Binary 檔案。

---

# 3. 第一階段：Project Asset Audit

建立：

```text
01_Project_Asset_Audit.md
```

至少整理以下內容。

---

## 3.1 現有角色清單

建立表格：

| Asset | Type | L1 | L2 | L3 | L4 | Rig | Animation | Texture | Team Color | Status |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| Infantry | Standard Unit | ? | ? | ? | ? | ? | ? | ? | ? | ? |
| Archer | Standard Unit | ? | ? | ? | ? | ? | ? | ? | ? | ? |

如果掃描到其他單位也加入。

---

## 3.2 找出現有步兵所有相關檔案

列出：

- 檔名
- 完整相對路徑
- 檔案類型
- 可能用途
- 是否 Production Ready
- 是否需要重新檢查

例如：

```text
Infantry
├─ Concept
├─ Model
├─ Rig
├─ Animation
├─ Texture
├─ Material
└─ Unity
```

---

## 3.3 找出現有弓兵所有相關檔案

同上。

---

## 3.4 現有規格文件

列出所有與：

- L1
- L2
- L3
- Character
- Unit
- Art
- Asset
- Rig
- Animation

相關的 `.md` 文件。

對每份文件說明：

```text
File:
Purpose:
Current Relevance:
Conflict:
Missing Information:
Recommendation:
```

---

# 4. 第二階段：建立正式 L1 → L4 Pipeline

建立：

```text
02_Asset_Pipeline_L1_L4.md
```

必須重新明確定義：

---

## L1 — Concept Design

回答：

> 角色「應該長什麼樣子」。

至少定義：

- Faction
- Unit Type
- Role
- Fantasy
- Body Proportion
- Silhouette
- Equipment
- Weapon
- Color Palette
- Team Color Placement
- Materials
- Visual Motif

L1 可以具有藝術表現。

但不得成為唯一 3D 建模基準。

---

## L2 — Production Character Sheet

L2 必須正式定義成：

> **3D Production Reference**

不是第二張 Concept Art。

至少包含：

- Front
- Side
- Back
- 3/4
- A-Pose 或 Neutral Pose
- Weapon Breakdown
- Equipment Breakdown
- Color Palette
- Material Guide
- Team Color Area
- Height / Proportion
- Silhouette Note

若 AI 無法可靠產出完全一致 Turnaround：

文件必須寫出人工修正流程。

---

## L3 — Production 3D Character

L3 必須包含：

```text
Modeling
Retopology
UV
Texture
Material
Rigging
Skinning
Animation
Export Validation
```

不得只定義成：

> 可以丟進 Unity 的 FBX。

---

## L4 — Unity Integration

正式分離成：

```text
Prefab
Animator
Material
Shader
Team Color
LOD
Collider
Selection
Animation Event
VFX Socket
Weapon Socket
Projectile Socket
Unit Scale
RTS Camera Validation
```

---

# 5. 第三階段：建立角色 Production Quality Standard

建立：

```text
03_Character_Production_Quality_Standard.md
```

目標品質：

> **Stylized Fantasy RTS Production Character**

風格目標：

- Warcraft-like RTS readability
- Heroic proportions
- Chunky silhouette
- Exaggerated equipment
- Clear color separation
- Stylized hand-painted surface
- Strong material readability
- Strong shape language

注意：

請描述「視覺特徵」，不要只寫：

```text
Make it Warcraft style.
```

因為這不夠具體。

---

# 6. 禁止的最終品質

正式模型禁止呈現：

- Primitive character
- Cube 拼接
- Cylinder 拼接
- Voxel
- Minecraft-like
- Greybox
- Placeholder
- Prototype
- Generic low-poly blockout
- Flat Color 測試模型
- 無裝備結構層次
- 嚴重 faceted shading
- 法線錯誤造成方塊感

Primitive：

```text
Cube
Cylinder
Sphere
Capsule
```

允許作為：

> Blockout

但不得直接成為顯眼 Final Form。

---

# 7. Primary / Secondary / Tertiary Forms

明確定義建模順序。

## Primary

決定角色輪廓：

- Body
- Helmet
- Shoulder
- Chest Armor
- Shield
- Sword
- Bow
- Large Weapon

## Secondary

建立裝備層次：

- Belt
- Bracer
- Knee Guard
- Boots
- Waist Armor
- Quiver
- Armor Plates

## Tertiary

小細節：

- Scratch
- Wear
- Small Rivet
- Leather Grain
- Small Groove
- Surface Damage

規格必須強調：

> Primary Forms 未達標前，不應花大量時間製作 Tertiary Details。

---

# 8. Silhouette Standard

建立：

```text
04_RTS_Silhouette_and_Readability_Standard.md
```

Silhouette 應視為 RTS 角色最高優先級之一。

角色即使：

- 關閉 Texture
- 顯示純黑
- 縮小

仍應看得出：

- Unit Class
- Weapon Type
- Armor Weight
- Character Identity

---

## 8.1 Infantry

至少辨識：

- Shield
- Sword
- Heavy Armor
- Helmet
- Strong Shoulder
- Strong Lower Body

---

## 8.2 Archer

至少辨識：

- Bow
- Quiver
- Lighter Armor
- Slimmer Silhouette
- Different Shoulder Shape

禁止：

> Infantry 換 Bow 就當 Archer。

不同兵種必須具有不同 Silhouette。

---

# 9. Polygon / Geometry Budget

文件必須區分：

## Standard Unit

建議 LOD0：

```text
20K – 35K triangles
```

## Elite Unit

```text
25K – 45K
```

## Special Unit

```text
30K – 50K
```

## Hero

```text
40K – 70K
```

以上為：

> 初始 Production Guideline

不要當成絕對硬限制。

更重要的是：

- Silhouette
- Deformation
- Screen Size
- Draw Call
- LOD

禁止為了「RTS 要很多人」而直接將 LOD0 做成非常粗糙。

效能應由：

```text
LOD
Culling
GPU Instancing
Material Sharing
Animation Optimization
Impostor
```

處理。

---

# 10. LOD Standard

建立：

```text
05_LOD_and_Performance_Standard.md
```

至少定義：

```text
LOD0
LOD1
LOD2
LOD3
Optional Impostor
```

需說明：

- 各級目的
- 何時切換
- Geometry 簡化原則
- 不可破壞主要 Silhouette
- 武器 / 盾牌何時可以簡化
- 材質是否降解析度
- 骨骼是否需要簡化

禁止：

> 因為最終會縮小，所以一開始就把 LOD0 做粗糙。

---

# 11. Texture / Material Standard

建立：

```text
06_Texture_Material_TeamColor_Standard.md
```

普通單位：

建議：

```text
2048 x 2048
```

Hero：

依需求：

```text
2048 / 4096
```

至少規劃：

- Base Color
- Normal
- Roughness
- Metallic
- AO
- Team Color Mask

若採用 Channel Packing：

必須明確定義每個 Channel。

例如：

```text
R = Metallic
G = AO
B = Roughness
A = Reserved
```

實際方案需依目前專案 Shader 判斷。

若目前 Shader 尚未實作：

標記：

```text
PROPOSED
```

不要假裝已存在。

---

# 12. Hand-painted / Stylized Texture

Base Color 不得只是：

```text
Armor = Gray
Leather = Brown
Skin = Skin Color
```

必須定義：

- Value Variation
- Painted Shadow
- Edge Highlight
- Wear
- Color Variation
- Material Separation

讓：

- Metal
- Leather
- Cloth
- Wood
- Skin

即使在較弱的 PBR Lighting 下仍可辨認。

---

# 13. Team Color

禁止建立：

```text
Blue Infantry
Red Infantry
Green Infantry
```

多套重複 Mesh。

應建立：

```text
Single Mesh
+
Team Color Mask
+
Shader Parameter
```

Team Color 適合：

- Cloth
- Shield
- Banner
- Shoulder Decoration
- Cape
- Waist Cloth

避免：

> 全身直接染成 Team Color。

---

# 14. Rig Standard

建立：

```text
07_Rig_Skinning_Animation_Standard.md
```

一般人型：

優先：

> Unity Humanoid Compatible

至少包含：

```text
Root
Pelvis
Spine
Chest
Neck
Head

UpperArm_L/R
LowerArm_L/R
Hand_L/R

UpperLeg_L/R
LowerLeg_L/R
Foot_L/R
```

視需求：

- Toe
- Twist
- Finger
- Shoulder

---

# 15. Shared Skeleton Strategy

需要分析目前 Infantry 與 Archer 是否：

- 可以共用 Skeleton
- 可以共用 Base Human
- 可以 Retarget Animation

建立 Skeleton Family 概念：

```text
SKEL_Human_A
SKEL_Human_Heavy
SKEL_Quadruped
SKEL_Giant
SKEL_Flying
```

不要強迫：

- Monster
- Horse
- Dragon
- Machine

使用 Humanoid。

---

# 16. Skinning Quality

明確驗收：

禁止：

- Shoulder collapse
- Elbow collapse
- Knee collapse
- Armor rubber deformation
- Shield penetrating body
- Weapon penetrating hand
- Cape severe clipping
- Waist armor collapse

Rigid Armor 應使用合適 Weight Strategy。

---

# 17. Weapon System

Weapon 應優先：

> Separate Object / Separate Asset

不要永久 Weld 到 Character Mesh。

至少規劃：

```text
Weapon_R
Weapon_L
Shield_L
ProjectileSpawn
VFX_Weapon
```

依專案需求命名。

如果專案已有 Socket Naming：

以現有規則為主。

---

# 18. Animation Standard

普通單位至少：

```text
Idle
Move
Attack_A
Hit
Death
```

Melee：

```text
AttackImpact
```

Ranged：

```text
ProjectileSpawn
```

如有需要：

```text
Attack_B
Attack_C
Block
Stun
Cast
Skill
Victory
```

---

# 19. Animation Style

RTS 動畫必須具備：

- Readable Anticipation
- Clear Impact
- Clear Recovery
- Exaggerated Motion
- Strong Pose

避免：

> 很寫實但鏡頭拉遠後完全看不出攻擊。

---

# 20. Root Motion

一般 RTS Locomotion：

```text
In Place = Required
Root Motion = Off
```

除非專案特定 Skill 另有需求。

---

# 21. Golden Sample

建立：

```text
08_Golden_Sample_Infantry_Archer.md
```

這份文件必須非常重要。

目前：

```text
Infantry
Archer
```

是 Golden Sample。

---

## Infantry 用來驗證

- Heavy Armor
- Sword
- Shield
- Melee Animation
- Heavy Silhouette
- Hard Surface Armor
- Team Color
- Skinning

---

## Archer 用來驗證

- Light Armor
- Bow
- Arrow
- Quiver
- Projectile Spawn
- Ranged Animation
- Thin Silhouette
- Arm Deformation

---

# 22. 不得先大量生產其他單位

文件必須明確寫：

在 Golden Sample 通過前：

```text
DO NOT MASS PRODUCE
```

以下資產：

- Spearman
- Cavalry
- Mage
- Elite
- Special
- Hero

可以：

- Concept
- Backlog
- Design

但不要大量做 Production L3。

---

# 23. 現有 Infantry / Archer Remaster Audit

建立：

```text
09_Existing_Infantry_Archer_Remaster_Audit.md
```

如果能從目前專案可靠分析，對兩者分別評估：

```text
Model Geometry
Proportion
Silhouette
Armor
Weapon
Material
Texture
Normals
Rig
Skinning
Animation
Unity Integration
```

每一項標記：

```text
PASS
NEEDS IMPROVEMENT
REBUILD
CANNOT VERIFY
```

---

# 24. Remaster Decision

對每個問題判斷：

## A. Preserve

可以直接保留。

## B. Modify

現有 Mesh 可以細修。

## C. Rebuild Partially

只重建：

- Helmet
- Shoulder
- Shield
- Chest
- Boots

等部分。

## D. Rebuild Character

只有在基礎比例 / Topology / Rig 完全不適合時才建議整體重做。

禁止：

> 一看到不好看就直接全部重做。

應先判斷是否能有效 Remaster。

---

# 25. Hero Standard

建立：

```text
10_Hero_and_Special_Unit_Standard.md
```

Hero 不得只是：

> 普通士兵換一套 Texture。

Hero 應有：

- Unique Silhouette
- Unique Head
- Unique Armor
- Unique Weapon
- Unique Color Identity
- Unique Idle Personality
- Unique Skill Animation

可以共用：

- Skeleton
- Base Topology
- Generic Locomotion

但外觀與表現必須具有高度識別性。

---

# 26. Special Unit

特殊單位介於：

```text
Standard Unit
↓
Elite
↓
Special
↓
Hero
```

應定義不同資產 Tier：

```text
Tier A = Standard
Tier A+ = Elite
Tier S = Special
Tier H = Hero
```

每 Tier 定義：

- Triangle Budget
- Texture Budget
- Animation Budget
- Unique Mesh Ratio
- VFX Complexity
- Material Complexity

---

# 27. Modular Character System

建立：

```text
11_Modular_Character_System.md
```

建議分析是否可以建立：

```text
Human_Base_Male
Human_Base_Female
```

共用：

- Skeleton
- Base Topology
- Core Animation

可替換：

```text
Head
Helmet
Shoulder
Chest
Gloves
Belt
Leg
Boots
Weapon
Shield
Cape
Accessory
```

---

# 28. Modular 限制

禁止讓所有角色變成：

> 同一個人換裝。

不同 Unit Class 應擁有不同：

- Silhouette
- Armor Weight
- Body Emphasis
- Weapon Size
- Pose
- Shape Language

---

# 29. Building 規格必須獨立

建立：

```text
12_Building_Production_Standard.md
```

不要直接沿用 Character L3。

建築需額外定義：

- Building Footprint
- Grid Occupancy
- Pivot
- Entrance
- Rally Point
- Selection Bounds
- Collision
- Navigation Blocking
- Construction State
- Damage State
- Destruction
- Rubble
- LOD
- Team Color
- Lighting
- Material
- Animation
- VFX Socket

---

# 30. Building Visual Standard

也需保持 Stylized RTS。

重要：

- 遠距辨識
- 建築功能辨識
- 陣營形狀語言
- 大型輪廓
- 屋頂
- 塔
- 門口
- 武器平台

建築不能只靠 Texture 區分。

例如：

```text
Barracks
Archery Range
Stable
Mage Tower
Town Center
```

即使純黑剪影，也應有一定辨識度。

---

# 31. Naming Standard

建立：

```text
13_Asset_Naming_and_Folder_Standard.md
```

若目前專案已有命名規則：

優先沿用。

否則提出建議，例如：

```text
CHR_[Faction]_[Unit]_[Variant]_v###
BLD_[Faction]_[Building]_[Variant]_v###
WPN_[Faction]_[Type]_[Variant]_v###
SKEL_[Type]_[Variant]
ANM_[Unit]_[Action]_[Variant]
MAT_[Asset]_[Type]
TEX_[Asset]_[MapType]
PFB_[Asset]
```

例如：

```text
CHR_HUM_Infantry_A_v001
CHR_HUM_Archer_A_v001
CHR_HUM_Hero_KnightKing_v001
```

---

# 32. Source / Runtime 分離

規範：

```text
Source Art
↓
Export
↓
Unity Runtime Asset
```

避免所有：

- `.blend`
- Photoshop Source
- Export FBX
- Unity Runtime

全部混在同一資料夾。

請依目前 Repo 結構提出合理方案。

---

# 33. AI Asset Provenance

建立：

```text
14_AI_Asset_Provenance_and_License_Standard.md
```

每個 AI 生成資產必須記錄：

- Tool
- Tool Version
- Model
- Prompt
- Negative Prompt
- Seed
- Job ID
- Date
- Human Modification
- Input Reference
- Third-party Asset
- License
- Commercial Use

如果資訊不存在：

標記：

```text
UNKNOWN
```

不要自行編造 Seed / Job ID。

---

# 34. Unity Acceptance Standard

建立：

```text
15_Unity_RTS_Asset_Acceptance_Checklist.md
```

每個角色最終需要在 Unity 測試：

```text
Close
Medium
Normal RTS Distance
Far
```

建議 Screen Height：

```text
128 px
64 px
32 px
```

至少檢查：

- Silhouette
- Weapon
- Unit Type
- Team Color
- Animation Readability
- Shadow
- Material
- Normal
- Clipping
- Scale

---

# 35. Blender / DCC Acceptance

至少要求 Render：

- Front
- Back
- Side
- 3/4 Front
- 3/4 Back

並包含：

## Neutral Lighting

檢查：

- Geometry
- Proportion
- Normal
- Material

## Game-like Lighting

檢查實際視覺效果。

---

# 36. Production Ready 定義

建立統一狀態：

```text
BLOCKOUT
WIP_MODEL
WIP_TEXTURE
WIP_RIG
WIP_ANIMATION
INTEGRATION
GOLDEN_SAMPLE_CANDIDATE
PRODUCTION_READY
REJECTED
```

只有同時通過：

```text
Visual Quality
Silhouette
Material
Rig
Animation
Unity Compatibility
RTS Readability
Performance
Documentation
```

才可標示：

```text
PRODUCTION_READY
```

---

# 37. Master Checklist

建立：

```text
16_Master_Production_Checklist.md
```

使用 Markdown Checkbox。

每個 Unit 從：

```text
L1
↓
L2
↓
L3
↓
L4
↓
QA
↓
Production Ready
```

全部可勾選。

---

# 38. 建立 Backlog

建立：

```text
17_Asset_Production_Backlog.md
```

至少分：

## Current Golden Sample

- Infantry
- Archer

## Next Standard Units

- Spearman
- Heavy Infantry
- Cavalry
- Mage

## Special

暫時依專案現有資料整理。

## Hero

如果尚未定義角色：

標記 TBD。

## Buildings

依目前設計文件整理。

不要自行發明大量遊戲內容。

---

# 39. 建立 Agent 執行說明

建立：

```text
18_Agent_Execution_Guide.md
```

說明未來如何讓另一個 Agent：

1. 讀取規格
2. 建立新 Unit
3. 檢查 L1
4. 產生 L2
5. 建立 / 修改 L3
6. Unity Integration
7. QA
8. 更新 Provenance
9. 更新 Backlog

---

# 40. 建立總覽 README

建立：

```text
README.md
```

README 必須說明：

- 這整包文件是做什麼
- 建議閱讀順序
- 哪些是核心文件
- 哪些是 Golden Sample 文件
- 哪些是 Character
- 哪些是 Building
- 哪些是 Unity
- 哪些是 AI Provenance

建議閱讀順序：

```text
README
↓
01 Project Audit
↓
02 Pipeline
↓
03 Production Quality
↓
04 Silhouette
↓
06 Texture / Material
↓
07 Rig / Animation
↓
08 Golden Sample
↓
09 Remaster Audit
↓
15 Unity Acceptance
↓
16 Master Checklist
```

---

# 41. 建議輸出資料夾

在專案內建立：

```text
Docs/
└─ ArtProduction/
   └─ RTS_Asset_Production_Spec_v1/
```

如果專案已有統一文件目錄：

可調整路徑。

但必須在最終報告中說明實際輸出位置。

---

# 42. 預期輸出檔案

最終資料夾至少包含：

```text
RTS_Asset_Production_Spec_v1/
│
├─ README.md
│
├─ 01_Project_Asset_Audit.md
├─ 02_Asset_Pipeline_L1_L4.md
├─ 03_Character_Production_Quality_Standard.md
├─ 04_RTS_Silhouette_and_Readability_Standard.md
├─ 05_LOD_and_Performance_Standard.md
├─ 06_Texture_Material_TeamColor_Standard.md
├─ 07_Rig_Skinning_Animation_Standard.md
├─ 08_Golden_Sample_Infantry_Archer.md
├─ 09_Existing_Infantry_Archer_Remaster_Audit.md
├─ 10_Hero_and_Special_Unit_Standard.md
├─ 11_Modular_Character_System.md
├─ 12_Building_Production_Standard.md
├─ 13_Asset_Naming_and_Folder_Standard.md
├─ 14_AI_Asset_Provenance_and_License_Standard.md
├─ 15_Unity_RTS_Asset_Acceptance_Checklist.md
├─ 16_Master_Production_Checklist.md
├─ 17_Asset_Production_Backlog.md
├─ 18_Agent_Execution_Guide.md
└─ 99_Open_Issues_and_Missing_Information.md
```

可以增加必要文件。

不可隨意刪減核心文件。

---

# 43. Open Issues

建立：

```text
99_Open_Issues_and_Missing_Information.md
```

集中紀錄：

- 找不到的資產
- 不確定的規格
- 文件互相矛盾
- 缺少 L2
- 缺少 Unity Screenshot
- 缺少 `.blend`
- 缺少 Texture
- Shader 不明
- Animation Event 不明
- Licensing 不明

不要把不確定資訊散落後遺忘。

---

# 44. 文件品質要求

所有文件必須：

- 使用繁體中文
- 專有名詞保留英文
- Markdown 格式
- 層級清楚
- 可直接讓另一個 Agent 讀取
- 不使用模糊描述
- 儘量給出驗收方式
- 區分 REQUIRED / RECOMMENDED / OPTIONAL
- 區分 CURRENT / PROPOSED
- 區分 VERIFIED / INFERRED / UNKNOWN

---

# 45. 不要只複製本 Prompt

這非常重要。

你的任務不是：

> 把這份 Prompt 拆成 18 份 Markdown。

而是：

> **根據實際專案掃描結果，將本 Prompt 轉換成符合目前專案的正式 Production 文件。**

例如：

如果專案已經有：

```text
CHR_Infantry_A_v001
```

應直接引用實際命名。

如果專案目前 Shader 使用 URP Lit：

必須記錄。

如果 Team Color 已有自訂 Shader：

必須分析現況。

如果目前根本沒有 Team Color Shader：

應標記為 Proposed。

---

# 46. Existing Specification Conflict Resolution

如果現有 L3 文件與新規格衝突：

不要直接刪除舊文件。

請在 Audit 中列出：

```text
Old Requirement:
New Requirement:
Conflict:
Recommended Resolution:
Migration Impact:
```

必要時另外產生：

```text
Legacy_Spec_Migration.md
```

---

# 47. 版本控制

第一版文件標記：

```text
Specification Version: 1.0
```

README 記錄：

```text
Created Date:
Agent:
Project:
Specification Version:
```

不要偽造 Agent 型號。

能知道才寫。

---

# 48. ZIP 封裝

完成所有文件後：

保留原始資料夾。

另外產生：

```text
RTS_Asset_Production_Spec_v1.zip
```

ZIP 內容必須只包含：

```text
RTS_Asset_Production_Spec_v1/
```

完整資料夾。

---

# 49. Windows ZIP 建議

若環境為 PowerShell，可以使用：

```powershell
Compress-Archive `
  -Path ".\Docs\ArtProduction\RTS_Asset_Production_Spec_v1" `
  -DestinationPath ".\Docs\ArtProduction\RTS_Asset_Production_Spec_v1.zip" `
  -Force
```

如果實際輸出路徑不同：

請依實際路徑修改。

---

# 50. ZIP 驗證

打包後必須確認：

- ZIP 存在
- ZIP 非 0 bytes
- 可以列出內容
- README 在內
- 01~18 核心文件存在
- 99 Open Issues 存在

不要只執行 Compress-Archive 就假設成功。

---

# 51. Git 規則

本任務不要自動：

```text
git commit
git push
```

除非使用者另外明確要求。

可以執行：

```text
git status
```

用來確認新增哪些文件。

---

# 52. 最終 Agent 回報格式

完成後，請在回覆中提供：

## A. 掃描摘要

例如：

```text
Found:
- X specification files
- X character assets
- X FBX
- X Blender files
- X animation assets
```

---

## B. Golden Sample 狀態

```text
Infantry:
- Overall:
- Main Issues:

Archer:
- Overall:
- Main Issues:
```

---

## C. 產生文件

列出全部新增文件。

---

## D. 輸出路徑

例如：

```text
Folder:
C:\projects\Unity\AegisRTS.FrameworkLab\Docs\ArtProduction\RTS_Asset_Production_Spec_v1

ZIP:
C:\projects\Unity\AegisRTS.FrameworkLab\Docs\ArtProduction\RTS_Asset_Production_Spec_v1.zip
```

依實際位置回報。

---

## E. 尚未解決

列出：

- 需要 Unity Screenshot
- 需要 L1 / L2
- 需要 Blender Source
- 需要 Shader 確認
- 需要人工美術決策

等。

---

# 53. Success Criteria

本任務只有在以下條件全部成立時才算完成：

- [ ] 已掃描實際專案
- [ ] 已整理 Infantry
- [ ] 已整理 Archer
- [ ] 已確認目前 L1/L2/L3/L4 現況
- [ ] 已建立正式 L1→L4 Pipeline
- [ ] 已建立 Character Production Standard
- [ ] 已建立 Silhouette Standard
- [ ] 已建立 Texture / Material / Team Color Standard
- [ ] 已建立 Rig / Skinning / Animation Standard
- [ ] 已建立 LOD Standard
- [ ] 已建立 Golden Sample Standard
- [ ] 已建立 Infantry / Archer Remaster Audit
- [ ] 已建立 Hero / Special Unit Standard
- [ ] 已建立 Modular Character Standard
- [ ] 已建立獨立 Building Standard
- [ ] 已建立 Asset Naming / Folder Standard
- [ ] 已建立 AI Provenance / License Standard
- [ ] 已建立 Unity Acceptance Checklist
- [ ] 已建立 Master Production Checklist
- [ ] 已建立 Production Backlog
- [ ] 已建立 Agent Execution Guide
- [ ] 已建立 Open Issues
- [ ] 已建立 README
- [ ] 已保留輸出資料夾
- [ ] 已產生 ZIP
- [ ] 已驗證 ZIP
- [ ] 未破壞現有 Production Asset
- [ ] 未自動 Git Push

---

# 54. 核心決策原則

請始終遵守以下優先順序：

```text
Visual Identity
↓
Silhouette
↓
Production Quality
↓
Animation Readability
↓
Unity Compatibility
↓
Performance Optimization
```

Optimization 不代表：

> 直接降低所有 LOD0 品質。

真正策略應為：

```text
High-quality LOD0
+
Proper LOD
+
Shared Materials
+
Shared Skeleton
+
Culling
+
Animation Optimization
```

---

# 55. 本階段最重要的專案策略

目前只有 Infantry 與 Archer 已有模型。

因此：

> **不要急著把所有兵種都做完。**

先完成：

```text
Infantry Golden Sample
+
Archer Golden Sample
↓
Production Pipeline Validation
↓
Visual Standard Lock
↓
Technical Standard Lock
↓
Mass Production
```

當這兩個 Golden Sample 通過之後，才開始大量製作：

```text
Spearman
Cavalry
Mage
Elite Units
Special Units
Heroes
Buildings
```

如此可以避免：

> 大量建立低品質 L3 → 後續全部重做

造成大量 Asset Debt。

---

# 56. 立即開始

請不要只提供執行計畫。

直接：

1. 掃描專案
2. 找出現有相關規格
3. 找出 Infantry / Archer
4. 建立 Audit
5. 建立完整 Production Specification
6. 建立 Output Folder
7. 建立 ZIP
8. 驗證內容
9. 回報結果

若部分資訊不存在：

> 繼續完成所有可完成內容，並將缺失集中寫入 `99_Open_Issues_and_Missing_Information.md`。

不要因缺少單一檔案而中止整個任務。
