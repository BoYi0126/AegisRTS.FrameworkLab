# Unit_03 步兵 — L2 遊戲模型交付報告

Asset ID: `unit.infantry`  
版本: `CHR_Infantry_A_v001`  
交付等級: **L2**

## 座標與尺寸
- 1 Unity Unit = 1 公尺
- Y-up / Z-forward / X-right
- 腳底中心 Pivot / GroundContact = `(0,0,0)`
- Root Scale = `(1,1,1)`（模型資料以公尺建立，無匯入縮放補救）
- 角色設計高度：1.80 m；頭盔飾帶最高約 1.82 m，仍在 1.75–1.85 m 包絡內
- 盾牌：0.60 × 0.86 × 0.07 m
- 短劍：總長 1.00 m；刀刃最小寬度約 0.065 m
- SelectionAnchor Y = 0.02 m
- HealthBarAnchor Y = 2.10 m

## 網格
- LOD0 triangles: **4376**
- LOD1 triangles: **1512**
- LOD0 bounds: [-0.65, 0.0, -0.205] → [0.5845, 1.83, 0.295] m
- LOD1 bounds: [-0.65, 0.0, -0.195] → [0.5845, 1.83, 0.295] m

> Bounds 包含手持盾與斜向短劍，因此 X/Z 包絡大於純身體寬深；Gameplay NavMeshAgent 仍依規格保持 radius 0.38 m / height 2.0 m。

## 材質與 UV
- 2 個材質：`MAT_Infantry_Base`、`MAT_Infantry_TeamColor`
- 1024×1024 BaseColor atlas
- TeamColor 可替換；提供 Blue `#4AA3D8`、Red `#D94A45` 兩張 1K 貼圖
- 提供 Flat Normal 與 ORM（AO=1、Roughness≈0.8、Metallic=0）作為 Unity 匯入基準
- 所有幾何皆有 UV0；主材質 UV 分配至 atlas 大色塊，TeamColor 使用獨立 UV0。

## GLB
- `CHR_Infantry_A_v001_LOD0_Blue.glb`
- `CHR_Infantry_A_v001_LOD0_Red.glb`
- `CHR_Infantry_A_v001_LOD1_Blue.glb`
- `CHR_Infantry_A_v001_LOD1_Red.glb`

藍／紅 GLB 共享同一幾何設計，差異僅在 TeamColor 材質。正式 Unity Prefab 建議只保留一套網格，執行期替換 `MAT_Infantry_TeamColor`。

## Unity 匯入
- Scale Factor: 1
- Convert Units: On
- Generate Colliders: Off
- Mesh Compression: Off（驗收階段）
- Read/Write: Off
- 本 L2 為概念幾何與材質交付，**未含骨架與動畫**；Humanoid rig / animation 屬後續 L3。

## 預覽
- 本包內 PNG 為依 Unity 相機契約數學投影產生的尺寸預覽，不冒充 Unity 引擎截圖。
- 另附 `Unity/Editor/InfantryL2Validator.cs`，匯入模型後可在 Unity 內一鍵產生正式驗收截圖。
- 960×540 / 31 m / Pitch 55° / FOV 60°（Blue + Red）
- 1920×1080 / 31 m / Pitch 55° / FOV 60°（Blue + Red）
- 960×540 / 40 m（最遠辨識）
- 960×540 / 8 m（近距檢查）
- 正面尺寸參考圖

## 已知限制
- 此交付由程式化低多邊形建模產生，沒有使用 Blender/Maya 人工雕模；因此造型是可驗證比例、輪廓與 Unity 尺度的 production-blockout / low-poly L2，而不是精修角色雕刻。
- 執行環境沒有 FBX exporter，因此本次主格式為 **GLB**；規格允許 FBX 或 GLB。
- L2 不含骨架、蒙皮、動畫與 LOD2；依規格，量產前可於 L3 再補。
