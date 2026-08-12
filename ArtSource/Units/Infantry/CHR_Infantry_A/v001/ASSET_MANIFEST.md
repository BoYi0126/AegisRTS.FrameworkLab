# CHR_Infantry_A v001 Asset Manifest

## 狀態

- Asset ID：`unit.infantry`
- Delivery：L1 Concept + L2 Game Model
- Current Status：`Integrated (Prototype L2) / Release Blocked`
- Received File Count：25
- Received Total Size：4,201,771 bytes
- Organized Date：2026-08-12
- L3 Rig／Animation：未交付
- Unity Prefab：已建立 `PF_Unit_Infantry`
- Runtime Integration：已接入 `PlayablePrototype_01`

2026-08-12 已由使用者採「GLB 方案 A」安裝 `glTFast 6.19.0`，GLB 匯入阻塞已解除。`Release Blocked` 僅代表交付中仍沒有可編輯 DCC 原始檔、完整生成工具／Prompt／Seed、第三方素材與商用授權紀錄，也沒有 L3 骨架動畫；不阻止本地 Prototype 使用。

## 分類

### Concepts

- `Concepts/Unit_03_Infantry_L1_Concept_Final.png`
- `Concepts/Unit_03_Infantry_L1_Concept_Alternate.png`

用途：L1 視覺參考與設計追溯，不進 Unity Runtime Assets。

### Models

- `Models/CHR_Infantry_A_v001_LOD0_Blue.glb`
- `Models/CHR_Infantry_A_v001_LOD0_Red.glb`
- `Models/CHR_Infantry_A_v001_LOD1_Blue.glb`
- `Models/CHR_Infantry_A_v001_LOD1_Red.glb`

用途：L2 模型來源。Runtime 已採 Blue 的 LOD0／LOD1 作唯一幾何來源，匯入時把 Base 與 Team Color 各自合併為一個 renderer，並以 `MaterialPropertyBlock` 切換陣營色；Red GLB 只留在 `ArtSource` 作原始交付追溯。

### Textures

- `Textures/T_Infantry_A_BaseColor_1K.png`
- `Textures/T_Infantry_A_Normal_1K.png`
- `Textures/T_Infantry_A_ORM_1K.png`
- `Textures/T_Infantry_A_TeamColor_Blue_1K.png`
- `Textures/T_Infantry_A_TeamColor_Red_1K.png`

用途：L2 材質來源。Runtime 使用 BaseColor；Team Color 採純白材質搭配 `MaterialPropertyBlock`，沒有複製紅／藍網格。Normal 與 ORM 是近乎均值的 placeholder 貼圖；目前以 URP Lit 常數（Metallic 0、Smoothness 0.2）呈現，待正式材質貼圖交付後再接入。

### Previews

- `Previews/Camera/`：960×540／1920×1080、Zoom 8／31／40 的數學投影預覽。
- `Previews/Dimensions/`：正面尺寸參考。

交付報告明確表示這些不是 Unity 引擎截圖，因此不能取代實際 Unity Game View 驗收。

### UV

- `UV/UV_Infantry_A_Base_LOD0.png`
- `UV/UV_Infantry_A_TeamColor_LOD0.png`

用途：UV 版面檢查，不進 Runtime Assets。

### Documentation

- `Documentation/DELIVERY_README.md`
- `Documentation/L2_DELIVERY_REPORT.md`
- `Documentation/L2_METRICS.json`
- `Documentation/Unity_Hierarchy.txt`

用途：保存原始交付說明、尺寸、三角面、Hierarchy 與已知限制。

### Tools

- `Tools/UnityEditor/InfantryL2Validator.cs`

用途：交付方提供的 Unity Editor 驗證工具。尚未放入 `Assets/AegisRTS/Editor`，因此目前不參與專案編譯。待模型格式可匯入後，需先補上 `AegisRTS.Tools.Editor` namespace、調整輸出目錄並 code review，再決定是否啟用。

## 已知資料

- 設計高度：1.80 m；交付報告 bounds 最大 Y 為 1.83 m。
- LOD0：4,376 triangles。
- LOD1：1,512 triangles。
- 材質目標：2 個。
- Pivot：腳底中心 `(0,0,0)`。
- 軸向：Y-up、Z-forward、X-right。
- HealthBarAnchor：Y 2.10 m。
- SelectionAnchor：Y 0.02 m。

上述數據已由 Unity 匯入與 Prefab builder 重測：LOD0 4,376 triangles、LOD1 1,512 triangles；合併後每個 LOD 為 Base／Team Color 共 2 個 renderer，bounds 高度約 1.83 m。

## 整合狀態與後續工作

1. 已完成：glTFast 6.19.0、Scale／Pivot／朝向／bounds／triangles 驗證。
2. 已完成：單一 LOD0／LOD1 幾何、Team Color runtime、LODGroup 與 URP Lit materials。
3. 已完成：`PF_Unit_Infantry`、Selection／HealthBar anchors、0.38 m collider、Content Pack prefab ID 與遊戲 View 串接。
4. 已完成：實際 Unity Play Mode 1920×1152 smoke screenshot，藍／紅雙方模型、血條、LOD／anchors 存在，Console smoke PASS。
5. 待補：完整 Prompt／Seed／Job ID、人工修改、第三方素材與商用授權紀錄；完成前不可當成可發布 production asset。
6. 待補：Rigged L3 模型與 Idle／Move／Attack／Hit／Death 動畫、武器／投射物 socket；目前移動時只旋轉靜態模型。
7. 待補：真正有細節的 Normal／ORM 與較高品質 BaseColor；現有貼圖是色帶／均值 placeholder。

## 驗收結論

此資產已通過 Prototype L2 整合，可在 `PlayablePrototype_01` 實際生成、辨識陣營、選取、移動與顯示血條。它尚未達到 Release／Production Accepted：授權追溯、正式材質與 L3 動畫仍須補齊。
