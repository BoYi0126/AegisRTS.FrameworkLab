# Infantry Visual Remaster — 資料收集任務

> 用途：將本文件交給 Codex Agent / Coding Agent 執行。  
> 任務類型：**只讀掃描 + 資料收集 + 複製封裝 + 索引整理**。  
> 目標：收集足夠資料，交由後續 Visual / Technical Art Review 判斷 Infantry 要「保留、細修、局部重建或整體重建」。  
> 
> **重要：本任務禁止修改現有遊戲資產。**

---

# 0. 任務目標

目前需要針對 RTS 專案中的 Infantry / 步兵角色進行 Production Remaster 評估。

本次不要進行美術修改，也不要重新建模。

請完整收集目前 Infantry 的：

1. L1 Concept / 原始概念圖
2. L2 / Character Sheet / Turnaround（若存在）
3. L3 Blender Source
4. L3 FBX / GLB / OBJ 等模型輸出
5. Unity Prefab 與相關設定
6. Materials
7. Textures
8. Rig / Skeleton
9. Animation
10. Animation Event
11. Team Color
12. LOD
13. Shader
14. Model Import Settings
15. 現有 Preview / Screenshot / Render
16. 相關規格文件
17. 建模 / 生成 / 匯出腳本
18. 可驗證的 Mesh / Triangle / Material / Bone 等技術資訊

然後統一複製到：

```text
Docs/ArtProduction/ReviewPackages/
└─ Infantry_Remaster_Review_Package_v001/
```

最後產生：

```text
Infantry_Remaster_Review_Package_v001.zip
```

此 ZIP 將交給另一個 Review Agent / ChatGPT 進行下一階段的視覺與技術分析。

---

# 1. 專案根目錄

預設專案可能位於：

```text
C:\projects\Unity\AegisRTS.FrameworkLab
```

但：

如果目前 Codex Workspace 已經開在正確 Repo：

> 以目前 Workspace 為根目錄。

不要因絕對路徑不同而中止。

---

# 2. 核心規則

## 2.1 只收集，不修改

嚴格禁止：

- 修改 `.blend`
- 修改 `.fbx`
- 修改 `.glb`
- 修改 Texture
- 修改 Material
- 修改 Prefab
- 修改 Animator
- 修改 Animation Clip
- 修改 `.meta`
- 修改 Shader
- 修改 C#
- 修改 Scene
- 修改 Import Settings
- Reimport 並覆寫任何現有 Asset
- 自動重建 Mesh
- 自動 Retopology
- 自動重新 Bake Texture
- 自動重新 Export Production FBX
- 自動更新 LOD
- 自動調整 Rig
- 自動調整 Animation

本次只能：

```text
SCAN
READ
ANALYZE METADATA
COPY
DOCUMENT
PACKAGE
```

---

# 3. 不要刪除任何東西

禁止：

```text
delete
move
rename original asset
overwrite original asset
git clean
```

所有需要的資料：

> **Copy 到 Review Package。**

原檔保持原位。

---

# 4. 不要 Git Push

允許：

```text
git status
git log
git diff --stat
```

如果有助於判斷資產歷史。

禁止：

```text
git commit
git push
git reset --hard
```

---

# 5. 優先搜尋的 Infantry ID / 關鍵字

請全專案搜尋：

```text
Infantry
步兵
CHR_Infantry
CHR_HUM_Infantry
CHR_Infantry_A
CHR_Infantry_A_v001
CHR_Infantry_A_v002
Infantry_A
Sword
Shield
AttackImpact
```

也搜尋可能相關：

```text
L1
L2
L3
Concept
Turnaround
Character Sheet
Reference
Rig
Skeleton
Humanoid
Animator
TeamColor
Team Color
LOD
Material
Texture
Weapon
```

---

# 6. 掃描副檔名

至少包含：

```text
.md
.txt
.json
.yaml
.yml

.blend
.blend1

.fbx
.glb
.gltf
.obj

.png
.jpg
.jpeg
.tga
.psd
.exr

.mat
.shader
.shadergraph

.prefab
.controller
.anim
.avatar
.asset
.meta

.cs
.py
.ps1
.bat
```

---

# 7. 建立輸出資料夾

請建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Remaster_Review_Package_v001/
```

建議結構：

```text
Infantry_Remaster_Review_Package_v001/
│
├─ README.md
├─ 00_Collection_Report.md
├─ 01_Asset_Inventory.md
├─ 02_Missing_Data.md
├─ 03_Unity_Technical_Summary.md
├─ 04_Blender_Model_Summary.md
├─ 05_Animation_Summary.md
├─ 06_Material_Texture_Summary.md
├─ 07_Visual_Evidence_Index.md
├─ 08_Source_Spec_Index.md
│
├─ L1_Concept/
├─ L2_Reference/
├─ L3_Source/
├─ Runtime_Models/
├─ Unity/
├─ Materials/
├─ Textures/
├─ Animations/
├─ Scripts/
├─ Specifications/
├─ Screenshots/
└─ Manifests/
```

如有更合理的子目錄可以增加。

但不要刪除上述核心分類。

---

# 8. L1 Concept 收集

搜尋 Infantry 原始概念圖。

可能包含：

```text
Concept
Character Concept
Infantry Concept
L1
Reference
Design
Front Art
```

若找到：

複製到：

```text
L1_Concept/
```

並在：

```text
01_Asset_Inventory.md
```

紀錄：

```text
File:
Original Path:
Copied Path:
Resolution:
Format:
Likely Role:
Verified / Inferred:
```

---

# 9. L2 Production Reference 收集

搜尋：

- Front
- Side
- Back
- 3/4
- Turnaround
- Character Sheet
- Orthographic
- A-Pose
- T-Pose
- Equipment Breakdown

若找到：

複製到：

```text
L2_Reference/
```

如果完全不存在：

在 `02_Missing_Data.md` 明確寫：

```text
L2 Production Character Sheet: NOT FOUND
```

不要把普通 Concept Art 誤標成 Production L2。

---

# 10. L3 Blender Source

搜尋所有 Infantry `.blend`。

例如可能：

```text
CHR_Infantry_A_v001.blend
CHR_Infantry_A_v002.blend
```

全部列入 Inventory。

優先將：

> 最新且實際對應目前遊戲模型的 `.blend`

複製到：

```text
L3_Source/
```

如果容量合理，可同時收集上一版作為比較。

---

# 11. Blender Source 判斷

若 Agent 環境可以安全地以 **Read Only / 非覆寫方式** 取得 Blender 資訊，可整理：

- Blender Version
- Object Count
- Mesh Count
- Armature Count
- Material Count
- Texture References
- Animation Actions
- Bone Count
- Triangle Count
- Vertex Count
- UV Map Count
- Shape Keys
- Modifiers
- Object Names
- Armature Name
- Main Mesh Names
- Weapon Object Names

若無法安全解析：

標記：

```text
CANNOT VERIFY WITHOUT BLENDER INSPECTION
```

不要修改 `.blend` 來取得資訊。

---

# 12. 建立 Blender Summary

建立：

```text
04_Blender_Model_Summary.md
```

格式至少包含：

```text
Primary Blender:
Original Path:
File Size:
Modified Date:

Objects:
Meshes:
Armatures:
Materials:
Actions:

Character Height:
Triangle Count:
Vertex Count:
Bone Count:
UV Sets:

Main Character Mesh:
Helmet:
Shoulder Armor:
Chest Armor:
Waist Armor:
Bracer:
Leg Armor:
Boots:
Shield:
Sword:

Notes:
```

未知欄位填：

```text
UNKNOWN
```

---

# 13. Runtime Models 收集

收集 Infantry：

```text
.fbx
.glb
.gltf
.obj
```

複製到：

```text
Runtime_Models/
```

需要特別辨識：

- Unity 實際使用哪一個
- Source Export
- Old Version
- Test Version
- LOD Version

不要把所有模型都當成 Current。

標示：

```text
CURRENT
LEGACY
UNKNOWN
TEST
```

---

# 14. Unity Prefab

搜尋 Infantry Prefab。

可能例如：

```text
CHR_Infantry_A_v002.prefab
Infantry.prefab
PFB_Infantry.prefab
```

複製：

```text
Unity/Prefabs/
```

同時收集對應 `.meta`。

---

# 15. Unity Model Import Settings

對目前使用的 Infantry FBX：

收集對應：

```text
.fbx.meta
```

分析：

- Scale Factor
- Mesh Compression
- Read/Write
- Optimize Mesh
- Normals
- Tangents
- Material Import
- Rig Type
- Avatar Definition
- Humanoid
- Animation Import
- Root Motion settings（如可驗證）

建立：

```text
03_Unity_Technical_Summary.md
```

不要修改 Import Settings。

---

# 16. Animator

收集 Infantry 使用的：

```text
.controller
.overrideController
```

若存在。

複製到：

```text
Unity/Animator/
```

整理 Parameters：

例如：

```text
Speed
Attack
Hit
Death
```

依實際專案。

不要自行假設。

---

# 17. Prefab Components

從 Prefab YAML / 可讀內容中整理：

- Animator
- Renderer
- SkinnedMeshRenderer
- Material
- Collider
- Selection Anchor
- Health Bar Anchor
- Weapon Socket
- Shield Socket
- Scripts
- LODGroup
- Team Color Component

對每個 Component：

```text
Component:
Status:
Reference:
Notes:
```

---

# 18. Material 收集

搜尋所有 Infantry Material：

```text
.mat
```

複製到：

```text
Materials/
```

同時找出：

- Character Material
- Armor Material
- Skin Material
- Weapon Material
- Shield Material
- Team Color Material

如果使用共用 Material：

也要複製或至少完整記錄來源。

---

# 19. Shader 收集

針對 Infantry 實際 Material 所引用 Shader：

如果是專案自訂：

複製：

```text
.shader
.shadergraph
.hlsl
```

到：

```text
Materials/Shaders/
```

如果是 Unity built-in / URP 官方 Shader：

不用複製 Unity Package 內容。

只需記錄：

```text
Shader: Universal Render Pipeline/Lit
```

或實際名稱。

---

# 20. Texture 收集

收集 Infantry 實際使用：

- BaseColor
- Albedo
- Normal
- Metallic
- Roughness
- AO
- ORM
- Mask
- Team Color Mask
- Emission

複製到：

```text
Textures/
```

在 `06_Material_Texture_Summary.md` 整理：

| Texture | Resolution | Format | Usage | Material | Current |
|---|---:|---|---|---|---|

---

# 21. Texture Resolution

務必記錄每張 Texture：

```text
Width
Height
Channels
Format
File Size
```

如能取得 Unity Import：

也記錄：

```text
Max Size
Compression
sRGB
Normal Map flag
Alpha
```

---

# 22. Team Color

完整搜尋：

```text
TeamColor
Team Color
TeamMask
FactionColor
PlayerColor
```

確認目前 Infantry 是：

### A

單一 Mesh + Team Color Mask

或

### B

Blue / Red 多套 Mesh

或

### C

尚未完成

或

### D

其他實作

建立明確說明。

---

# 23. Rig / Skeleton

整理目前：

```text
Armature Name
Root
Pelvis
Spine
Chest
Neck
Head
Arms
Hands
Legs
Feet
```

確認：

```text
Humanoid Compatible:
YES / NO / CANNOT VERIFY
```

---

# 24. Weapon / Shield

確認：

- Sword 是否獨立 Object
- Shield 是否獨立 Object
- Unity 是否可以單獨掛載
- 使用什麼 Bone / Socket
- Sword Pivot
- Shield Pivot

若存在相關 Prefab：

一併收集。

---

# 25. Animation 收集

至少搜尋：

```text
Idle
Move
Attack_A
Hit
Death
```

收集：

```text
.anim
FBX embedded clip
.controller references
```

複製可直接複製的檔案到：

```text
Animations/
```

---

# 26. Animation Summary

建立：

```text
05_Animation_Summary.md
```

至少整理：

| Clip | Source | Length | Loop | In Place | Event | Status |
|---|---|---:|---:|---:|---|---|

---

# 27. AttackImpact

專門確認：

```text
AttackImpact
```

實際位於：

- Animation Event
- StateMachineBehaviour
- Script Timer
- 其他

必須記錄：

```text
AttackImpact Source:
Clip:
Normalized Time:
Absolute Time:
Verified:
```

如果無法確認時間：

```text
CANNOT VERIFY
```

---

# 28. 相關 Script

收集與 Infantry 資產建立 / 匯出 / 驗證高度相關的腳本。

例如：

- Blender Python
- Asset Generator
- FBX Exporter
- Rig Builder
- Character Builder
- Unity Validator
- Team Color Setup
- LOD Builder

複製到：

```text
Scripts/
```

但不要把整個 C# 專案全部複製。

只取直接相關。

---

# 29. 既有規格文件

收集目前與 Infantry 有關：

- L1 Spec
- L2 Spec
- L3 Spec
- Character Production Standard
- Golden Sample
- Remaster Audit
- Naming
- Animation
- Material
- Team Color
- LOD

複製到：

```text
Specifications/
```

---

# 30. Visual Evidence — 既有圖片

全專案搜尋：

- Screenshot
- Preview
- Capture
- Render
- Infantry
- Game View
- Scene View

如果已有：

```text
Unity screenshot
Blender render
Character preview
```

全部複製到：

```text
Screenshots/
Existing/
```

---

# 31. 如果可以自動、安全取得 Unity Screenshot

只有在：

> 不修改 Production Asset、不改 Scene、不覆寫設定

的前提下執行。

優先希望得到：

```text
01_Unity_Close.png
02_Unity_Medium.png
03_Unity_RTS_Normal.png
04_Unity_Far.png
```

至少需要：

```text
RTS Normal Distance
```

---

# 32. Unity Screenshot 內容要求

如果可以產出：

必須完整看到：

- 整隻 Infantry
- Sword
- Shield
- Ground
- Lighting
- Shadow

不要只截 Inspector。

---

# 33. Screen Size 測試

如果現有工具允許：

取得角色螢幕高度約：

```text
128 px
64 px
32 px
```

的 Screenshot。

目的：

判斷：

- Silhouette
- Armor readability
- Weapon readability
- Team Color readability

---

# 34. 如果不能自動取得 Unity Screenshot

不要阻塞任務。

在：

```text
02_Missing_Data.md
```

寫：

```text
MANUAL CAPTURE REQUIRED
```

並列出使用者需要人工提供：

```text
Unity Close
Unity Medium
Unity RTS Normal
Unity Far
```

---

# 35. Blender Render

如果可以在 **不修改 Production `.blend`** 的情況下：

使用複製到 Review Package 的 `.blend` 副本進行 Render。

可以對副本：

- 開啟
- 臨時設定 Camera
- 臨時設定 Neutral Lighting
- Render

但：

> 絕對不能 Save 回 Production `.blend`。

---

# 36. 希望的 Blender Capture

若可產出：

```text
01_Front.png
02_Left.png
03_Right.png
04_Back.png
05_ThreeQuarter_Front.png
06_ThreeQuarter_Back.png
```

存入：

```text
Screenshots/Blender/
```

---

# 37. Neutral Render

Render 優先使用：

> Neutral / Review Lighting

不要靠戲劇化 Lighting 掩蓋模型問題。

目標是看：

- Proportion
- Geometry
- Silhouette
- Surface
- Normals
- Materials

---

# 38. Clay / Solid View

如果可安全產出：

額外提供：

```text
Clay_Front
Clay_Side
Clay_Back
Clay_3Q
```

非常有價值。

這可以把：

> Geometry 問題

與：

> Texture / Lighting 問題

分開判斷。

---

# 39. Wireframe Screenshot

如果可以安全產出：

```text
Wireframe_Front
Wireframe_3Q
```

存入：

```text
Screenshots/Wireframe/
```

主要用來判斷：

- Topology Density
- Primitive Construction
- Joint Topology
- Armor Geometry

---

# 40. Visual Evidence Index

建立：

```text
07_Visual_Evidence_Index.md
```

格式：

| Image | Type | View | Source | Notes |
|---|---|---|---|---|

對沒有的 View：

標 `MISSING`。

---

# 41. Triangle / Mesh 技術資訊

盡可能取得：

```text
Total Vertices
Total Triangles
Character Triangles
Sword Triangles
Shield Triangles
Mesh Count
Material Slots
Bone Count
Skinned Mesh Count
```

放進：

```text
04_Blender_Model_Summary.md
```

以及：

```text
03_Unity_Technical_Summary.md
```

---

# 42. Object Inventory

若能從 Blender 或 Source Script 取得：

建立 Object 清單：

```text
Object Name
Type
Parent
Bone
Material
Triangles
Visible
```

輸出：

```text
Manifests/Blender_Object_Manifest.csv
```

如無法可靠取得：

不需偽造。

---

# 43. File Manifest

建立：

```text
Manifests/File_Manifest.csv
```

欄位：

```text
Category
FileName
OriginalPath
CopiedPath
FileSize
ModifiedTime
Status
Notes
```

---

# 44. SHA256

對 Review Package 中重要 Binary：

例如：

```text
.blend
.fbx
.glb
.png
```

可建立：

```text
Manifests/SHA256SUMS.txt
```

方便確認上傳後檔案沒有變。

---

# 45. Source Specification Index

建立：

```text
08_Source_Spec_Index.md
```

對每份規格：

```text
Document:
Original Path:
Purpose:
Applies To Infantry:
Current / Legacy:
Conflict:
```

特別標記：

> 舊 L3 Prototype 規格

與：

> 新 Production Quality Standard

是否存在衝突。

---

# 46. Collection Report

建立：

```text
00_Collection_Report.md
```

最前面放：

```text
Project:
Asset:
Collection Version:
Collection Date:
Workspace:
```

然後提供：

## Found

```text
L1:
L2:
BLEND:
FBX:
Prefab:
Materials:
Textures:
Rig:
Animations:
Screenshots:
```

---

# 47. Missing Data

建立：

```text
02_Missing_Data.md
```

分成：

## Critical

缺了就難以 Visual Review：

- L1 Concept
- Current L3 Model
- Unity Screenshot

## Important

- Blender Source
- Materials
- Textures
- L2

## Optional

- Wireframe screenshot
- Historical versions

---

# 48. 不要因資料缺少而停止

例如：

```text
沒有 L2
```

仍然要：

- 收集 L1
- 收集 L3
- 收集 Unity
- 收集 Texture
- 打 ZIP

最後將 L2 標：

```text
NOT FOUND
```

---

# 49. README

建立：

```text
README.md
```

第一段說明：

> 此資料包用於 Infantry Visual Remaster Review。
> 不代表已修改或改善任何 Production Asset。

並列出：

```text
Review Priority:
1. L1 vs L3
2. Silhouette
3. Primary Forms
4. Armor
5. Texture
6. Skinning
7. Animation
8. Unity RTS Readability
```

---

# 50. ZIP 不要包含整個 Unity Project

非常重要。

不要把：

```text
Library/
Temp/
Logs/
obj/
.git/
Packages Cache
```

塞入 ZIP。

只收集：

> 與 Infantry Remaster Review 直接相關的必要資料。

---

# 51. ZIP 大小控制

如果 `.blend` 或 Texture 非常大：

仍優先保留 Current `.blend`。

歷史版本可以只記錄：

```text
Original Path
File Size
Version
```

而不一定全部打包。

但 Current 核心資料不得因為節省大小被省略。

---

# 52. ZIP

完成後：

保留資料夾：

```text
Docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/
```

另建立：

```text
Docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001.zip
```

---

# 53. ZIP 驗證

必須確認：

```text
ZIP exists
ZIP > 0 bytes
README exists
00 Collection Report exists
01 Asset Inventory exists
02 Missing Data exists
Current Blender or current runtime model exists
Unity information exists
Specifications exist
```

---

# 54. 不要自動修復 Missing Data

例如發現：

```text
No L2
```

本次禁止：

> 自動生成新的 L2。

例如發現 Texture 很差：

本次禁止：

> 自動重畫 Texture。

例如 Triangle 太低：

本次禁止：

> 自動加 Subdivision。

這些是下一階段 Remaster 任務。

---

# 55. Agent 最終回覆格式

完成後，只需清楚回報：

## Output Folder

```text
<實際完整路徑>
```

## ZIP

```text
<實際完整路徑>
```

## Core Asset Found

```text
L1:
L2:
BLEND:
FBX:
Prefab:
Textures:
Animations:
```

## Manual Data Still Required

例如：

```text
Unity RTS Screenshot
L2 Character Sheet
```

依實際情況。

---

# 56. Success Criteria

- [ ] 已掃描 Infantry 相關資料
- [ ] 已找到並收集 L1（若存在）
- [ ] 已搜尋 L2
- [ ] 已收集 Current L3 Blender（若存在）
- [ ] 已收集 Current runtime model
- [ ] 已收集 Unity Prefab / meta
- [ ] 已整理 Model Import Settings
- [ ] 已收集 Material
- [ ] 已收集 Texture
- [ ] 已整理 Team Color
- [ ] 已整理 Rig
- [ ] 已整理 Animation
- [ ] 已確認 AttackImpact
- [ ] 已收集相關 Scripts
- [ ] 已收集相關 Specifications
- [ ] 已收集既有 Screenshots
- [ ] 若可安全取得，已產生 Review Screenshot
- [ ] 已建立 Asset Inventory
- [ ] 已建立 Blender Summary
- [ ] 已建立 Unity Summary
- [ ] 已建立 Texture Summary
- [ ] 已建立 Animation Summary
- [ ] 已建立 Visual Evidence Index
- [ ] 已建立 Missing Data
- [ ] 已建立 README
- [ ] 已產生 ZIP
- [ ] 已驗證 ZIP
- [ ] 沒有修改 Production Asset
- [ ] 沒有 git commit
- [ ] 沒有 git push

---

# 57. 核心原則

本任務的目的不是：

> 把 Infantry 變漂亮。

而是：

> **建立一份足夠完整、可以讓 Reviewer 精準判斷 Infantry 為什麼不夠精美，以及哪些部分該 Preserve / Modify / Partial Rebuild / Rebuild 的 Review Package。**

---

# 58. 立即執行

請直接開始：

```text
Scan
↓
Identify Current Infantry
↓
Collect
↓
Copy
↓
Document
↓
Generate Safe Review Evidence
↓
Package
↓
Verify ZIP
↓
Report
```

不要只回覆執行計畫。

如果有無法自動取得的資料：

繼續完成其餘工作，

並將缺失寫入：

```text
02_Missing_Data.md
```

不要因為缺少單一資料而停止整個收集任務。
