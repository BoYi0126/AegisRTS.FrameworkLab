# Infantry Remaster — Phase 02 Primary Forms Task

> **Project:** AegisRTS.FrameworkLab  
> **Asset:** `CHR_Infantry_A` / Infantry / `unit.infantry`  
> **Phase:** 02 — Primary Forms Remaster  
> **Input Baseline:** `CHR_Infantry_A_v002`  
> **Output Candidate:** `CHR_Infantry_A_v003`  
> **Primary Reference:** `Infantry_Phase01_Production_L2_Remaster_Target.md`  
> **Task Type:** Blender source duplication + primary-form rebuild + review evidence generation  
> **Important:** 本階段只處理「Primary Forms / Silhouette / Major Geometry」。不要進行 Final Texture、Final UV、Animation Polish、LOD Finalization 或 Shader 重寫。

---

# 0. 任務目的

本階段的唯一核心目標：

> 將目前 `CHR_Infantry_A_v002` 從 Prototype / blocky low-poly 外觀，升級成具有正式 RTS Production 潛力的 `CHR_Infantry_A_v003` Primary Forms Candidate。

本階段不追求最終細節。

必須先解決：

- 方塊人體
- 方塊肩甲
- 方塊胸甲
- 方塊腰甲
- 方塊鞋
- 低面數頭部
- Primitive Helmet
- 平板盾牌
- 棍狀短劍
- 不清楚的 Infantry silhouette

---

# 1. 執行前必讀文件

開始前必須完整閱讀：

```text
Infantry_Phase01_Production_L2_Remaster_Target.md
```

同時讀取專案內現有：

```text
RTS_Asset_Production_Spec_v1/
```

至少：

```text
03_Character_Production_Quality_Standard.md
04_RTS_Silhouette_and_Readability_Standard.md
05_LOD_and_Performance_Standard.md
07_Rig_Skinning_Animation_Standard.md
08_Golden_Sample_Infantry_Archer.md
09_Existing_Infantry_Archer_Remaster_Audit.md
15_Unity_RTS_Asset_Acceptance_Checklist.md
```

如果文件名稱或路徑不同，以實際 Repo 為準。

---

# 2. Phase 01 誤執行安全檢查

使用者可能已經讓 Agent 執行過 Phase 01。

因此開始 Phase 02 前先確認：

```text
git status
```

並檢查最近新增/修改檔案。

目的：

確認 Phase 01 是否只修改：

```text
.md
Review package
Docs
```

若發現 Phase 01 已修改：

```text
.blend
.fbx
.prefab
.mat
.png
.tga
.controller
.anim
.shader
.cs
```

不要直接覆蓋或回復。

請：

1. 列出變更。
2. 判斷是否與 Phase 01 任務無關。
3. 保留現況。
4. 在 Phase 02 Report 中標記。
5. 不執行 `git reset --hard`。
6. 不自動刪除使用者修改。

---

# 3. 絕對版本規則

禁止覆寫：

```text
CHR_Infantry_A_v002.blend
SK_Infantry_A_v002.fbx
CHR_Infantry_A_v002*.fbx
```

禁止將 v003 儲存成 v002。

Phase 02 必須建立：

```text
CHR_Infantry_A_v003.blend
```

若另有輸出 FBX：

```text
SK_Infantry_A_v003_PrimaryForms.fbx
```

或依現有命名規格等價命名。

---

# 4. v002 定義

`v002` 視為：

> Prototype Baseline

用途：

- 對比
- 技術參考
- Skeleton 參考
- Socket 參考
- Gameplay scale 參考
- Runtime integration 參考

不可刪除。

---

# 5. v003 定義

`v003` 本階段是：

> Primary Forms Candidate

還不是：

> Production Ready Final Asset

狀態應標記：

```text
WIP_MODEL
```

或專案等價狀態。

---

# 6. 必須保留的技術契約

Phase 02 不得任意破壞：

```text
Asset identity
World scale
Ground contact
Root convention
Humanoid skeleton hierarchy
Weapon_R / Weapon_L 或既有 socket naming
Sword right-hand role
Shield left-hand role
AttackImpact contract
Animator parameter contract
In-place locomotion contract
Root Motion Off contract
Prefab/content binding contract
SelectionAnchor
HealthBarAnchor
```

若模型重建需要暫時脫離 Prefab：

可以。

但不得改變上述設計契約。

---

# 7. 不得直接修改 Runtime Production Prefab

本階段：

不要直接把正式：

```text
PF_Unit_Infantry
```

永久替換成 v003。

可以建立：

```text
PF_Unit_Infantry_v003_Review
```

或其他：

> Review-only temporary prefab

用於截圖。

正式 Runtime 替換留到後續 Phase。

---

# 8. Phase 02 禁止事項

不要做：

- Final BaseColor
- Final Normal Map
- Final ORM
- Final Team Color Mask
- Final UV polish
- High-frequency scratches
- Tiny rivets
- Cloth fiber
- Final animation polish
- Final skinning polish
- Final LOD chain
- Shader architecture rewrite
- VFX
- Destruction
- Hero-level face detail
- Facial rig

---

# 9. 建模方式原則

可使用：

- Blender manual modeling
- Blender Python
- Modifiers
- Sculpt + retopo
- Existing v002 geometry as proportion/reference
- New mesh creation

但最終 Primary Forms 不得只是：

```text
Cube
Cylinder
Sphere
Capsule
```

直接縮放後當完成品。

Primitive 可以做 Blockout。

完成前需要：

- reshape
- bevel
- contour
- curvature
- silhouette refinement

---

# 10. L1 身份鎖定

不得重新設計 Infantry 身份。

必須保持：

> 東亞古代重裝步兵 / Stylized Fantasy RTS

核心：

- 圓頂頭盔
- 短 plume
- 層疊肩甲
- 札甲語彙胸甲
- 藍/紅 team cloth
- 木盾
- 短劍
- 腿部綁帶
- 厚重穩定 silhouette

---

# 11. 世界高度

v003 Target：

```text
1.80–1.85 m
Preferred ≈ 1.83 m
```

不得因重建導致：

```text
1.6 m
2.0 m
```

之類比例漂移。

---

# 12. 頭身比

Target：

```text
5.2–5.4 heads
Preferred ≈ 5.3
```

不要使用：

```text
7.5–8 heads realistic male
```

---

# 13. Silhouette 目標

Front：

```text
Helmet/plume
↓
Wide shoulder
↓
Heavy chest
↓
Tapered waist
↓
Stable legs
↓
Large shield + short sword
```

黑色 silhouette 狀態仍必須可以辨認：

> Heavy Shield Infantry

---

# 14. Body_Base — REBUILD / MODIFY

可以沿用 v002 作比例參考。

但 Production Candidate 必須具備：

- 頭
- 頸
- 胸腔
- 腰
- upper arm
- elbow
- forearm
- hand
- thigh
- knee
- calf
- ankle
- foot

不同節奏。

禁止：

```text
rectangular torso
+
cylinder limbs
+
cube boots
```

作為 Review 結果。

---

# 15. Chest / Shoulder Proportion

Armored shoulder width Target：

```text
0.64–0.70 m
```

不用硬達指定數值。

重點：

- 上半身比 v002 更有倒梯形
- 肩甲與胸腔共同形成寬肩
- 不靠單純加大方塊肩甲

---

# 16. Head — MODIFY / REBUILD

頭部必須從 faceted sphere 升級。

至少建立：

- forehead plane
- brow
- nose
- cheek
- jaw
- chin

不需要：

- eye sockets 高精度
- teeth
- tongue
- facial rig

Close Review 必須看起來是：

> stylized human head

而不是：

> polygon ball

---

# 17. Helmet — REBUILD

建立邏輯：

```text
Curved dome
Metal rim
Top mount
Short plume
Rear/side volume
Thickness
```

Helmet 應明顯呈現：

```text
smooth curved hard-surface
```

禁止 final：

```text
sphere + thin cylinder
```

---

# 18. Shoulder Armor — REBUILD

每側建立：

```text
2–3 readable major layers
```

可使用單一 mesh 表現多層，不要求每片獨立。

目標：

- 上層短
- 中/下層較寬
- 外側自然向下包覆
- 有厚度
- 前後視角都有 readable contour

---

# 19. Chest Armor — REBUILD

建立：

```text
Primary armor shell
+
Lamellar-inspired visual rhythm
```

Primary Forms 階段只需做：

- 主胸甲外殼
- 厚度
- 上下分層
- 中央 / 側面結構

不要現在做：

- 上百甲片
- 小鉚釘
- 小刮痕

---

# 20. Scarf / Neck Cloth

建立可讀布料體積：

- 圍繞 neck
- 前胸斜向
- 可保留 team-color region

不要使用：

```text
flat rectangular collar
```

---

# 21. Arms

從 shoulder 到 hand 必須有：

```text
UpperArm
Elbow
Forearm
Bracer
Hand
```

節奏。

本 Phase 先建 Geometry。

完整 skin weighting 留 Phase 05。

---

# 22. Bracer

Bracer 必須：

- curved
- wrap forearm
- visible thickness
- avoid rectangular tube

---

# 23. Hands

手不需要五指高精度。

但需要：

- palm
- thumb indication
- weapon grip volume

Hand scale 可比寫實：

```text
+5–10%
```

---

# 24. Waist — REBUILD

至少建立：

```text
Belt
Front cloth
Side armor panels
Rear cloth/armor
```

Front cloth：

- slight taper
- thickness
- non-rectangular contour

---

# 25. Legs

必須可以看出：

```text
Thigh
Knee
Calf
Ankle
```

不要做兩根同粗柱體。

---

# 26. Wraps

Leg wrap 可在本 Phase 做：

- main silhouette
- layered band indication

不需要 Final texture。

---

# 27. Boots — REBUILD

建立：

```text
toe
instep
heel
sole
ankle transition
```

鞋頭可比寫實：

```text
+8–12%
```

增加遠距離 readability。

---

# 28. Shield — REBUILD PRIMARY FORM

Scale：

```text
Height ≈ 0.86 m
Width ≈ 0.60 m
Thickness ≈ 0.05–0.07 m
```

以 Phase 01 為準。

建立：

1. Wood body
2. Metal rim
3. Center boss
4. Main reinforcement
5. Thickness
6. Back grip / attachment indication

---

# 29. Shield Silhouette

避免：

```text
rectangle
```

應接近 L1 多邊形 / taper shield。

Front：

具有辨識性的外輪廓。

Side：

必須可看到：

- board thickness
- rim
- center boss protrusion

---

# 30. Sword — REBUILD PRIMARY FORM

Target：

```text
≈ 0.98–1.02 m overall
```

建立：

```text
Blade
Blade taper
Thickness
Guard
Grip
Pommel
```

不需要雕刻。

---

# 31. Sword Readability

Blade：

- 不能太薄
- 不能像長方形棍子
- tip 明確
- guard 明顯

RTS 64 px 時仍應看得出是一把 sword。

---

# 32. Source Object Structure

建議 Source 保持：

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

可以增加：

```text
LegWrap_L
LegWrap_R
```

不要為了 Source modularity 強迫 Runtime 產生大量 Renderer。

---

# 33. Geometry Budget

Current v002：

```text
LOD0 ≈ 4,376 triangles
```

v003 Primary Forms 建議：

```text
20K–30K triangles
Preferred 24K–27K
```

本階段：

> 可暫時高於 preferred target。

如果 Primary Forms 達 30K–35K 但合理：

先不要為了數字破壞造型。

Optimization 在後續處理。

---

# 34. Polygon 使用優先順序

優先花在：

1. Head curvature
2. Shoulder contour
3. Helmet
4. Chest armor curvature
5. Waist transition
6. Joint topology
7. Boots
8. Shield bevel / boss
9. Sword silhouette

不要浪費在：

- 看不到的小凹槽
- tiny rivets
- scratches

---

# 35. Normals / Shading

Primary Forms Review 前必須處理到：

- 沒有明顯錯誤 flipped normals
- 大曲面沒有嚴重 faceting
- Hard surface edge 邏輯合理

可以使用：

- Shade Smooth
- Auto Smooth / equivalent
- Weighted Normal
- Bevel

依目前 Blender 版本。

---

# 36. Rig Strategy

本 Phase 可以：

### Option A
保留既有 Armature 並將新 mesh 綁定作初步 Review。

### Option B
先以 A-Pose Static Candidate 完成主要幾何，再做 minimal temporary binding。

優先避免：

> 因 rig 技術細節阻塞 Primary Forms。

但 v003 `.blend` 最終至少要：

- 保留可用 Armature
- Mesh 與骨架位置一致
- Sword / Shield attachment 可驗證

---

# 37. Skinning 本階段要求

Phase 02 只要求：

> 足以產生基本 A-Pose / Unity review。

不要花大量時間 fine tune：

- shoulder weights
- elbow deformation
- knee deformation

正式 Skinning 是 Phase 05。

---

# 38. A-Pose

輸出 Primary Candidate：

```text
A-Pose
```

需確認：

- shoulder armor 不嚴重穿體
- elbow 有空間
- shield/sword attachment 邏輯正確
- feet ground contact 正確

---

# 39. Material 本階段

使用簡單 Review Material。

至少：

```text
Clay Gray
```

可另外使用少量 Material ID 色彩做部件辨認。

但：

> Final Acceptance 必須包含 Clay。

---

# 40. 不使用漂亮材質掩蓋問題

至少輸出一組：

```text
100% neutral gray clay
```

所有 body / armor / weapon 都使用中性 review material。

目的：

單純判斷：

- Geometry
- Proportion
- Silhouette

---

# 41. Blender Review Capture

必須輸出：

```text
01_Clay_Front.png
02_Clay_Left.png
03_Clay_Back.png
04_Clay_3Q_Front.png
05_Clay_3Q_Back.png
```

如果可以：

```text
06_Clay_Right.png
```

---

# 42. Black Silhouette Capture

輸出：

```text
Silhouette_Front.png
Silhouette_Left.png
Silhouette_Back.png
Silhouette_3Q.png
```

角色全部純黑。

背景：

- 白
- 淺灰

保持高對比。

---

# 43. Wireframe Capture

至少：

```text
Wireframe_Front.png
Wireframe_3Q.png
```

最好：

```text
Wireframe_Side.png
```

必須可判讀 topology 分布。

---

# 44. Comparison Capture

建立：

```text
v002_vs_v003_Front.png
v002_vs_v003_3Q.png
```

如果方便。

兩隻：

- 相同角色高度
- 相似 Camera
- 相同 Lighting / Clay

目的是確認：

> v003 是否真的改善，而不是只是加 polygon。

---

# 45. Unity Temporary Preview

如果可以安全做到：

建立 Review-only v003 Preview。

不要覆寫正式 Prefab。

輸出：

```text
Unity_v003_Close.png
Unity_v003_RTS_Normal.png
```

如果可以：

```text
Unity_v002_vs_v003_RTS.png
```

---

# 46. Unity Preview 不得阻塞

如果 Unity batch / screenshot 工具不可用：

不要為了截圖拖垮整個 Phase。

標記：

```text
UNITY REVIEW CAPTURE: MANUAL REQUIRED
```

Blender Evidence 仍然完成。

---

# 47. Screen-size Review

如能自動產出：

```text
128px
64px
32px
```

角色高度截圖。

至少 64 px silhouette 有價值。

---

# 48. Phase 02 Review Package

建立：

```text
Docs/
└─ ArtProduction/
   └─ ReviewPackages/
      └─ Infantry_Phase02_PrimaryForms_Review_v001/
```

結構：

```text
Infantry_Phase02_PrimaryForms_Review_v001/
│
├─ README.md
├─ 00_Phase02_Report.md
├─ 01_Geometry_Stats.md
├─ 02_Changes_From_v002.md
├─ 03_Open_Issues.md
├─ Blender/
├─ Screenshots/
│  ├─ Clay/
│  ├─ Silhouette/
│  ├─ Wireframe/
│  ├─ Comparison/
│  └─ Unity/
└─ Manifests/
```

---

# 49. Blender Copy in Review Package

Review Package 中可以放：

```text
CHR_Infantry_A_v003.blend
```

副本。

如果檔案太大：

至少提供：

- 實際 Project path
- File size
- SHA256

但建議 ZIP 內包含，方便外部 Reviewer。

---

# 50. Geometry Stats

建立：

```text
01_Geometry_Stats.md
```

至少：

```text
Character Height:
Total Vertices:
Total Triangles:
Mesh Count:
Material Count:
Armature:
Bone Count:

Body:
Head:
Helmet:
Shoulder:
Chest:
Waist:
Boots:
Shield:
Sword:
```

可以為每部位列 triangle count。

---

# 51. v002 → v003 變更清單

建立：

```text
02_Changes_From_v002.md
```

分類：

```text
PRESERVED
MODIFIED
REBUILT
NOT YET ADDRESSED
```

---

# 52. 必須明確列出尚未處理事項

例如：

```text
Final UV
Final Texture
Final Team Color Mask
Final Skinning
Animation Polish
LOD
```

不要讓 Reviewer 誤以為 v003 已 Production Ready。

---

# 53. Open Issues

建立：

```text
03_Open_Issues.md
```

所有不確定問題集中記錄。

例如：

- L1 某角度不明
- shield grip 不清楚
- shoulder clipping risk
- topology still temporary

---

# 54. Primary Forms PASS 條件

下列全部必須達標：

- [ ] 不再有明顯方塊人體
- [ ] Head 不再像 faceted sphere
- [ ] Helmet 有曲面 / rim / plume
- [ ] Shoulder armor 有 readable layers
- [ ] Chest armor 有厚度與大型結構
- [ ] Waist 不再是單一 box
- [ ] Leg 有 thigh/knee/calf
- [ ] Boots 具真正鞋型
- [ ] Shield 有厚度/rim/boss
- [ ] Sword 有 taper/guard/grip
- [ ] Front silhouette 明顯是 Heavy Infantry
- [ ] Side silhouette 可讀
- [ ] 3/4 silhouette 清楚
- [ ] 角色保持約 1.83 m
- [ ] 武器尺寸沒有嚴重漂移
- [ ] v002 沒被覆寫

---

# 55. FAIL 條件

如果發生以下任一項：

不得宣布 Phase 02 通過：

- v003 仍像 Minecraft / Roblox / primitive mannequin
- 只是對 v002 Subdivide
- 只是加 Bevel 沒改 silhouette
- Shoulder 仍是 box
- Boots 仍是 cube
- Shield 仍是一塊平板
- Sword 仍是 rectangular stick
- L1 身份丟失
- 改成西方 Knight
- 比例變成寫實細長人體
- 只靠 Texture 假裝改善
- v002 被覆寫

---

# 56. 不允許「只 Subdivision」

特別禁止：

```text
v002
↓
Subdivision Surface
↓
稱為 v003
```

原因：

> Subdivision 只能增加面數，不能自動建立正確造型語彙。

必須真的重建 / reshape Primary Forms。

---

# 57. 不允許盲目 Remesh

如果使用：

- voxel remesh
- auto retopo
- sculpt remesh

仍需要人工 / 程式化檢查：

- silhouette
- armor edge
- deformation topology

不能只因 mesh 變密就視為改善。

---

# 58. Source Maintainability

`v003.blend` 必須：

- Object naming 清楚
- 不留下大量無意義 Cube.001 ...
- 不留下 dozens unused duplicate objects
- 不留下無用 hidden geometry
- 不留下未命名臨時 collection

---

# 59. Collection 建議

```text
CHR_Infantry_A_v003
├─ GEO_BODY
├─ GEO_ARMOR
├─ GEO_WEAPON
├─ RIG
└─ REVIEW
```

如果專案已有 Blender Collection 規則：

沿用現有規則。

---

# 60. Coordinate / Export

保持目前 Unity pipeline：

- world axis
- scale
- ground
- facing

不要在 Phase 02 任意更改。

如現有 Export 規格明確：

依現有規格。

---

# 61. Phase 02 不正式建立 LOD

不要在這個階段產：

```text
LOD1
LOD2
LOD3
```

最終鏈。

因為 Primary Forms 還可能改。

如果測試 Unity 必須：

只使用：

```text
v003 temporary LOD0
```

---

# 62. Phase 02 不正式重做 Texture

可以保留 v002 Texture 作 temporary viewport reference。

但 Clay Review 必須獨立存在。

不要製作 final 2K texture。

---

# 63. Phase 02 不 Animation Polish

可以測：

- A-Pose
- basic bind
- weapon sockets

不需要 polish：

```text
Idle
Move
Attack
Hit
Death
```

---

# 64. Reviewer 的後續決策

Phase 02 完成後 Reviewer 會判斷：

### PASS
進入：

```text
Phase 03 Secondary Forms
```

### CONDITIONAL PASS
針對少量：

- helmet
- shoulder
- boots
- shield

修一輪。

### FAIL
Primary Forms 再做一輪。

不要自己跳 Phase 03。

---

# 65. ZIP

完成後建立：

```text
Infantry_Phase02_PrimaryForms_Review_v001.zip
```

ZIP 內只包含：

```text
Infantry_Phase02_PrimaryForms_Review_v001/
```

不要塞整個 Unity Project。

---

# 66. ZIP Verification

必須確認：

- ZIP exists
- ZIP > 0 bytes
- README exists
- Report exists
- Geometry Stats exists
- v003 Blender exists或有 manifest
- Clay Front exists
- Clay 3Q exists
- Silhouette exists
- Wireframe exists
- v002 intact

---

# 67. Git Rule

不要：

```text
git commit
git push
```

本階段完成後只回報：

```text
git status
```

讓使用者自行決定。

---

# 68. README

Review README 開頭寫：

```text
This package contains the Phase 02 Primary Forms candidate for CHR_Infantry_A_v003.

This is not a final Production Ready character.
Final UV, textures, skinning, animation polish and LOD are intentionally deferred.
```

並使用繁體中文補充說明。

---

# 69. Agent 最終回覆

完成後回覆：

## Phase 01 Safety Check

```text
Unexpected production changes found:
YES / NO
```

若 YES 列出。

## v003

```text
Path:
Height:
Triangles:
Meshes:
```

## Review Package

```text
Folder:
ZIP:
```

## Visual Evidence

```text
Clay:
Silhouette:
Wireframe:
Unity:
```

## Known Issues

列 3–10 個實際問題。

## Phase Result

只標：

```text
READY FOR REVIEW
```

不要自行標：

```text
PASS
```

最終 PASS 由 Reviewer / 使用者決定。

---

# 70. 核心原則

Phase 02 的成功不是：

> 面數從 4,376 增加到 25,000。

成功是：

> **即使完全拿掉 Texture，只看灰模和黑色 Silhouette，也已經像一個正式 Stylized RTS Heavy Infantry，而不是可動的方塊 Prototype。**

---

# 71. 立即執行

請不要只回覆計畫。

直接：

```text
Read Phase 01
↓
Safety Check
↓
Preserve v002
↓
Create v003
↓
Rebuild Primary Forms
↓
Clay Review
↓
Silhouette Review
↓
Wireframe Evidence
↓
Optional Unity Preview
↓
Package
↓
Verify
↓
Report READY FOR REVIEW
```

任何最終美術決策不清楚時：

優先遵循：

```text
Existing L1 Concept
+
Infantry_Phase01_Production_L2_Remaster_Target.md
```

不要自行重新設計角色。
