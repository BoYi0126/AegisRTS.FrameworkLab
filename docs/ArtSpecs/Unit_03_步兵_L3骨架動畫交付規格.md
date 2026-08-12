# Unit_03 步兵 — L3 骨架、動畫與 Unity 交付規格

> 本文件可直接交給 3D AI、動畫 AI 或外包美術執行。交付者必須同時取得本文件列出的 L2 輸入檔；不可只看文字重新生成另一名角色。

## 1. 任務目標

將現有 `CHR_Infantry_A_v001` 靜態 L2 步兵升級為可供 Unity 6 URP 使用的 L3 完整資產。必須保留目前已驗收的角色外觀、比例、盾牌、短劍、UV 與 Team Color 配置，新增：

- Unity Humanoid 相容骨架與乾淨蒙皮。
- Idle、Move、Attack_A、Hit、Death 五個必要動畫。
- LOD2。
- 動畫事件時間表、Socket、Anchor 與碰撞代理說明。
- 可追溯的來源、生成過程與商用授權紀錄。
- Unity 匯入設定及實際 Game View 驗收證據。

這是既有 L2 的升級任務，不是新角色設計任務。Asset ID 保持 `unit.infantry`；L3 版本使用 `CHR_Infantry_A_v002`，不可覆寫或刪除 v001 原始交付。

## 2. 必須提供給製作者的輸入檔

至少附上：

1. 本文件。
2. `CHR_Infantry_A_v001_LOD0_Blue.glb`。
3. `CHR_Infantry_A_v001_LOD1_Blue.glb`。
4. `T_Infantry_A_BaseColor_1K.png`。
5. `T_Infantry_A_Normal_1K.png`。
6. `T_Infantry_A_ORM_1K.png`。
7. `Unit_03_Infantry_L1_Concept_Final.png`。
8. 原始 `L2_DELIVERY_REPORT.md`。

Red GLB 與紅／藍 Team Color 貼圖可作對照，但不得拿來建立另一套重複的紅色網格。Runtime 必須只有一套幾何，陣營色由材質參數切換。

如果使用的 AI／工具無法讀取、修改、Rig 或匯出現有 3D 模型，必須明確回覆 `Blocked - Tool Cannot Rig Existing Mesh`，不可用新生成角色假裝完成 L3。

## 3. 不可改變的 L2 契約

| 項目 | 必須保持 |
| --- | --- |
| Asset ID | `unit.infantry` |
| L3 名稱 | `CHR_Infantry_A_v002` |
| 世界比例 | 1 Unity Unit = 1 公尺 |
| 軸向 | Y-up、Z-forward、X-right |
| Pivot | 腳底中心 `(0,0,0)` |
| Root Transform | Position `(0,0,0)`、Rotation `(0,0,0)`、Scale `(1,1,1)` |
| 角色高度 | 設計 1.80 m；所有幾何最高不得超過 1.85 m |
| 現有 bounds 參考 | X `-0.65–0.5845`、Y `0–1.83`、Z `-0.205–0.295 m` |
| 盾牌 | 約 `0.60 × 0.86 × 0.07 m`，左手 |
| 短劍 | 總長約 `1.00 m`，右手，最薄可見寬度至少 `0.06 m` |
| Gameplay footprint | Capsule radius `0.38 m`、height `2.0 m` |
| Selection Anchor | `(0,0.02,0)` |
| Health Bar Anchor | `(0,2.10,0)` |
| 材質槽 | 最多 2 個：Base、Team Color |
| LOD0 | 目前 4,376 triangles；允許 2,500–6,000 |
| LOD1 | 目前 1,512 triangles；允許 1,000–2,500 |
| LOD2 | 必須新增 250–700 triangles |

不得擅自改變盔甲造型、盾牌尺寸、武器種類、角色身高、頭身比、主要色塊或 UV。為避免關節變形而增加肘、膝、肩附近的必要拓樸可以接受，但 LOD0 外觀與三角面數仍須落在表內，並在交付報告列出修改處。

## 4. 輸出格式決策

### 4.1 首選：FBX

正式交付首選 FBX，因為目前 Unity 整合目標是 Humanoid Avatar、可重現 Clip 設定與 Animation Event：

- Binary FBX 2018 或 Unity 可穩定讀取的相容版本。
- 單位使用公尺輸出；不可依賴 Unity Scale Factor 100／0.01 修正。
- `SK_Infantry_A_v002.fbx` 保存骨架、Bind Pose、LOD0／LOD1／LOD2 Skinned Mesh。
- 每個動畫另交獨立 FBX，所有檔案必須使用相同骨架名稱、Bind Pose 與 Avatar。
- 可額外提供一個包含所有 clips 的整合 FBX，但不能取代獨立動畫檔。

### 4.2 GLB 備選

只有在工具確實無法輸出 FBX 時才接受 GLB。GLB 仍須保留相同骨架、skin、clips、命名與 Root Motion 契約，並附上實際 Unity 6 + glTFast 6.19.0 的匯入證據。

GLB 交付不得宣稱已完成 Unity Humanoid Avatar，除非 Unity Inspector 實際顯示 Avatar Valid。若只能以 Generic Rig 使用，必須標記 `Conditional - Generic Rig Only`，由整合方決定是否接受；不可把 Generic 說成 Humanoid。

### 4.3 必須附原始製作檔

至少提供一種可編輯來源：`.blend`、`.ma`、`.mb` 或工具原生工程。來源檔必須包含：

- 最終骨架與蒙皮。
- 動畫時間軸與命名。
- 未套用前的必要控制器可以保留，但輸出層不能依賴專用 plugin。
- LOD0／LOD1／LOD2。
- 材質與貼圖路徑可重新連結。

## 5. 骨架規格

### 5.1 Rig 類型

- Unity Animation Type：`Humanoid`。
- 根骨固定命名 `Root`，骨盆命名 `Hips`。
- 必須能在 Unity Avatar Configuration 通過，狀態為 Valid／綠色。
- 可使用標準 T-Pose 或輕微 A-Pose作為 Bind Pose；需另附正面、側面 Bind Pose 截圖。
- 不需要臉部骨、頭髮物理骨或布料模擬骨。
- 變形骨建議不超過 65；含 Socket 的總 Transform 建議不超過 75。
- 不可輸出 IK controller、約束器、曲線控制器、燈光、相機或背景平面。

### 5.2 最低骨架階層

命名可依 DCC／Humanoid 標準微調，但 Unity Humanoid 對應必須完整：

```text
Root
└─ Hips
   ├─ Spine
   │  └─ Chest
   │     ├─ UpperChest（可選，但建議）
   │     │  ├─ Neck
   │     │  │  └─ Head
   │     │  ├─ LeftShoulder
   │     │  │  └─ LeftUpperArm → LeftLowerArm → LeftHand
   │     │  └─ RightShoulder
   │     │     └─ RightUpperArm → RightLowerArm → RightHand
   ├─ LeftUpperLeg → LeftLowerLeg → LeftFoot → LeftToes
   └─ RightUpperLeg → RightLowerLeg → RightFoot → RightToes
```

手指骨不是必要條件。若提供手指骨，不得造成武器 grip 或 Humanoid mapping 不穩定。Twist bones 最多每條手臂／腿各一個，並須正確加入 skin weights，但不映射為 Humanoid 必要骨。

### 5.3 Root 與 Hips

- `Root` 全程保持世界原點、朝 Z+，Scale 永遠為 `(1,1,1)`。
- 所有 clips 的 Root X／Z 位移絕對值不得超過 `0.01 m`。
- Root Y 漂移不得超過 `0.02 m`，Root 旋轉不得超過 `0.5°`。
- 自然上下起伏放在 Hips，不放在 Root。
- 動畫不得靠 Root Motion 推動角色；世界位移與旋轉由遊戲的導航／戰鬥 presentation 控制。
- 不得含非均勻縮放或骨骼 Scale animation keys。

## 6. 蒙皮規格

- 每個 vertex 最多 4 個 bone influences，權重必須 normalize。
- 不得有 unweighted vertices、負權重或總和不為 1 的權重。
- 肘、肩、膝、髖在極端動作下不得塌陷或明顯穿甲。
- 胸甲、頭盔等硬質部位應使用接近剛性的權重，不得像橡膠變形。
- 盾與短劍優先作為獨立 rigid mesh，分別掛在左／右手 Socket；若必須納入 Skinned Mesh，權重必須 100% 綁定單一手骨，不可彎曲。
- LOD0、LOD1、LOD2 共用同一骨架、Bind Pose 與 Avatar，不得每個 LOD 建一套骨架。
- 切換 LOD 時輪廓、武器、盾牌與 Team Color 不得跳位。

## 7. 必要 Socket 與 Anchor

### 7.1 動畫骨架內 Socket

| 名稱 | Parent | 用途與位置 |
| --- | --- | --- |
| `Socket_R_Hand` | `RightHand` | 短劍握把；局部軸向須讓武器正向一致 |
| `Socket_L_Hand` | `LeftHand` | 盾牌握把／固定點 |
| `Socket_WeaponTip` | 武器或 `Socket_R_Hand` | 劍尖 VFX／軌跡終點 |
| `Socket_Head` | `Head` | 頭頂狀態效果，不放血條 |
| `FX_Hit_Center` | `Chest` 或 `UpperChest` | 主要受擊 VFX，約角色胸口中央 |
| `FX_Foot_L` | `LeftFoot` | 左腳落地塵土 |
| `FX_Foot_R` | `RightFoot` | 右腳落地塵土 |

所有 Socket 的 local Scale 必須為 `(1,1,1)`，不可透過負 Scale 翻轉。武器與盾的 local axes、offset、rotation 必須寫入交付報告。

### 7.2 不可跟骨架上下晃動的 Anchor

以下節點必須是美術 Prefab root 的直接子物件或同等穩定節點，不可 parent 到 Head／Hips：

- `SelectionAnchor`：local position `(0,0.02,0)`。
- `HealthBarAnchor`：local position `(0,2.10,0)`。
- `GroundContact`：local position `(0,0,0)`。

血條和選取圈由遊戲程式建立；模型內不要烘焙圓圈、血條或文字。

## 8. 必要動畫清單

所有動畫以 30 fps 驗收；60 fps 製作可以接受，但輸出後事件必須對齊等價的 30 fps frame。每個 clip 必須獨立命名、獨立可播放，不接受只有一條未切割長時間軸。

| Clip | 檔名 | 長度／Frames | Loop | 必要事件 |
| --- | --- | ---: | --- | --- |
| Idle | `AN_Infantry_Idle` | 3.0 s／90 frames | 是 | 無 |
| Move | `AN_Infantry_Move` | 0.8 s／25 frames（0–24） | 是 | `Footstep_L`、`Footstep_R` |
| Attack_A | `AN_Infantry_Attack_A` | 0.9 s／27 frames | 否 | `AttackImpact` |
| Hit | `AN_Infantry_Hit` | 0.33 s／10 frames | 否 | 無 |
| Death | `AN_Infantry_Death` | 1.30 s／39 frames | 否 | `DeathSettled` |

允許長度誤差：Idle ±0.5 s；Move ±0.1 s；Attack_A ±0.1 s；Hit ±0.07 s；Death ±0.15 s。若因動作品質需要超出，必須先提出理由，不可自行變更。

### 8.1 Idle

- 保持戰鬥準備姿勢，盾在左前側，短劍在右側。
- 只做小幅呼吸、重心轉移與武器微動；頭、盾、劍的擺幅不可影響 RTS 輪廓。
- 盾不可長時間遮住整個身體，劍不可刺入腿部、地面或盾牌。
- 首尾姿勢、Hips 速度與所有骨骼旋轉必須連續；連播 10 次不可看出跳格。
- 腳掌不可滑動，至少一腳始終穩定接地。

### 8.2 Move

- 原地循環，不產生 Root X／Z 位移。
- 表現為持盾快走／小跑，適配目前 gameplay movement speed `4.5 m/s`；實際 Animator speed 由程式依移動速度調整。
- v002 實際左右腳接地事件：`Footstep_L` frame 1、`Footstep_R` frame 13；事件必須對齊接地 pose，誤差不得超過 1 frame。
- 盾保持朝前偏左，不能穿過大腿、胸口或右臂。
- 短劍保持可讀，不可在每一步大幅甩動或完全藏到身後。
- 頭部、胸部不能上下跳動過大；相機距離 31 m 時輪廓應穩定。

### 8.3 Attack_A

- 動作類型：右上向左下的單次斬擊，盾持續保護左前方。
- Frame 0–8：Windup；清楚蓄力，但不得背向目標。
- Frame 9–15：Swing／Impact；武器通過角色正前方。
- `AttackImpact` 建議放在 frame 13（0.433 s），最終必須對齊劍刃通過前方命中區的畫面，誤差不超過 1 frame。
- Frame 16–27：Recover；回到可混回 Idle 的防守姿勢。
- 命中幀劍尖應位於 Root 前方 Z+ 約 `0.55–1.00 m`，不可穿過自身軀幹。
- 攻擊結束後角色面向差不得超過 5°；任何時刻 Root 都不能位移。
- `AttackImpact` 只標記視覺接觸時間，不得在模型或動畫內寫死傷害、血量或遊戲邏輯。

### 8.4 Hit

- 短促的上半身受擊反應，盾與身體後縮但不倒地。
- 角色面向偏移不得超過 15°，Root 不移動。
- 盾、劍不可穿過頭、軀幹或地面。
- 結束姿勢必須能在短時間內混回 Idle／Move。

### 8.5 Death

- 建議向後側或右後側倒下，保留盾牌與步兵輪廓。
- 不可翻滾超過 Root 周圍 `1.0 m`；頭、盾、劍不得穿地。
- Root 固定，倒地位移由骨盆與肢體完成。
- `DeathSettled` 建議 frame 35（1.167 s），放在身體已停止主要運動的幀。
- Clip 結尾必須保持最後姿勢，不可自動彈回 Bind Pose／Idle。
- 屍體淡出、回收或 Collider 關閉由遊戲程式控制，不可內建到動畫。

## 9. 動畫 Clip 與 Unity Import 設定

每個 clip 的交付報告必須列出 Start／End frame、Loop 與事件時間。Unity 目標設定：

| 設定 | Idle | Move | Attack_A | Hit | Death |
| --- | --- | --- | --- | --- | --- |
| Loop Time | On | On | Off | Off | Off |
| Loop Pose | On | On | Off | Off | Off |
| Root Transform Rotation | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose |
| Root Position Y | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose |
| Root Position XZ | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose | Bake Into Pose |
| Based Upon | Original | Original | Original | Original | Original |

FBX 本身不一定保存 Unity Animation Event，因此必須另交 `ANIMATION_EVENTS.json`：

```json
{
  "fps": 30,
  "clips": [
    {
      "name": "AN_Infantry_Move",
      "events": [
        { "name": "Footstep_L", "frame": 1 },
        { "name": "Footstep_R", "frame": 13 }
      ]
    },
    {
      "name": "AN_Infantry_Attack_A",
      "events": [
        { "name": "AttackImpact", "frame": 13 }
      ]
    },
    {
      "name": "AN_Infantry_Death",
      "events": [
        { "name": "DeathSettled", "frame": 35 }
      ]
    }
  ]
}
```

事件 frame 可依最終動作微調；JSON、交付報告與實際動畫必須一致。

## 10. LOD 規格

- LOD0：2,500–6,000 triangles；目標沿用或微調目前 4,376。
- LOD1：1,000–2,500 triangles；目標沿用目前 1,512。
- LOD2：250–700 triangles；必須保留頭盔、盾、短劍與 Team Color 大輪廓。
- 三個 LOD 共用骨架、Animator、Avatar、材質命名與 Team Color 流程。
- LOD2 可以簡化手指、甲片、盾內側、劍柄與臉部，但不可移除盾或武器。
- 每個 LOD 最多 2 個 SkinnedMeshRenderer：Base、Team Color。
- 不可因換 LOD 讓角色高度改變超過 `0.03 m`、腳底漂浮或武器跳位。
- Unity LODGroup 初始建議門檻：LOD0 `0.04`、LOD1 `0.012`、LOD2 `0.003`；最終由整合方依 8／31／40 m Game View 調整。

## 11. 材質與貼圖

- 材質名稱固定：`MAT_Infantry_Base`、`MAT_Infantry_TeamColor`。
- 最多 2 個材質槽，不得為藍／紅各建一套 material 或 mesh。
- BaseColor 使用 sRGB；Normal／ORM 使用 Linear。
- Team Color mesh／mask 在四個觀看方向都要可見，佔角色可見面積 15–25%。
- Unity Runtime 會透過 `MaterialPropertyBlock` 設定 `_BaseColor` 或 `_Color`；Team Color 基準材質應為白色，不能把藍／紅永久烘焙。
- 若沿用 v001 UV，不得造成現有色帶位置錯亂。
- 若重新整理貼圖，仍限制 1024×1024，並同時交付新 UV layout 與變更說明。
- 不得使用只有特定 DCC 才能運作的程序材質；所有必要貼圖必須實際包含在交付包。

## 12. Unity Prefab 目標

目前遊戲會在外層建立 gameplay entity root，再把 `PF_Unit_Infantry` 當作 Visual 子物件，因此交付的美術 Prefab 不要再建立第二套 NavMeshAgent 或 gameplay state。

目標層級：

```text
PF_Unit_Infantry
├─ VisualRoot                         Animator + LODGroup
│  ├─ Armature                       Root/Hips/Humanoid bones
│  ├─ LOD0
│  │  ├─ SK_Infantry_A_LOD0_Base
│  │  └─ SK_Infantry_A_LOD0_Team
│  ├─ LOD1
│  │  ├─ SK_Infantry_A_LOD1_Base
│  │  └─ SK_Infantry_A_LOD1_Team
│  └─ LOD2
│     ├─ SK_Infantry_A_LOD2_Base
│     └─ SK_Infantry_A_LOD2_Team
├─ SelectionAnchor                   (0, 0.02, 0)
├─ HealthBarAnchor                   (0, 2.10, 0)
└─ GroundContact                     (0, 0, 0)
```

- `PF_Unit_Infantry` 與 `VisualRoot` Scale 必須為 1。
- Animator 放在 `VisualRoot`，`Apply Root Motion` 關閉，Culling Mode 建議 `Cull Update Transforms`。
- SkinnedMeshRenderer 的 `Update When Offscreen` 關閉。
- LODGroup 放在 `VisualRoot`。
- 碰撞代理建議 Capsule radius `0.38`、height `1.8–2.0`、center Y `0.9–1.0`；最終 collider 由整合方建立。
- 不得放入生命值、攻擊力、陣營、NavMeshAgent、選取程式或傷害程式。

交付者若無法製作 Unity Prefab，可只交正確 FBX、clips、Socket／Anchor 座標與匯入文件，由本專案整合；但必須清楚標記 `Unity Prefab: Not Included`，不可交空 Prefab 冒充完成。

## 13. 建議 Animator 介面

這是整合目標，不要求美術 AI 寫 gameplay code。若交付 Animator Controller，參數固定：

| Parameter | Type | 用途 |
| --- | --- | --- |
| `Speed` | Float | 0 為 Idle，大於門檻進 Move |
| `Attack` | Trigger | 播放 Attack_A |
| `Hit` | Trigger | 播放 Hit |
| `Die` | Trigger | 播放 Death |
| `IsDead` | Bool | 阻止回到 locomotion |

- Idle ↔ Move 使用 0.10–0.15 s crossfade。
- Attack／Hit 回 locomotion 使用 0.05–0.10 s crossfade。
- Death 不可離開，不能自動回 Idle。
- Animator 不得直接修改遊戲 HP、攻擊判定或世界 Transform。
- 攻擊動畫播放速度之後會由程式對齊 authoritative combat interval；不得把傷害邏輯綁死在 0.9 s。

## 14. 交付資料夾與檔名

```text
Unit_03_Infantry_L3_v002/
├─ Source/
│  └─ CHR_Infantry_A_v002.blend 或其他可編輯來源
├─ Models/
│  └─ SK_Infantry_A_v002.fbx
├─ Animations/
│  ├─ AN_Infantry_Idle.fbx
│  ├─ AN_Infantry_Move.fbx
│  ├─ AN_Infantry_Attack_A.fbx
│  ├─ AN_Infantry_Hit.fbx
│  └─ AN_Infantry_Death.fbx
├─ Textures/
│  ├─ T_Infantry_A_BaseColor_1K.png
│  ├─ T_Infantry_A_Normal_1K.png
│  └─ T_Infantry_A_ORM_1K.png
├─ Documentation/
│  ├─ L3_DELIVERY_REPORT.md
│  ├─ ANIMATION_EVENTS.json
│  ├─ UNITY_IMPORT_SETTINGS.md
│  └─ GENERATION_AND_LICENSE_RECORD.md
└─ Previews/
   ├─ BindPose_Front.png
   ├─ BindPose_Side.png
   ├─ SkinTest_Extremes.png
   ├─ GameView_960x540_Zoom31.png
   ├─ GameView_1920x1080_Zoom31.png
   ├─ GameView_960x540_Zoom40.png
   └─ AnimationPreview.mp4 或 GIF
```

若動畫內嵌於 master FBX，仍必須在報告列出每個 clip 的名稱、frame range、Loop 與事件；獨立 clips 仍是首選。

## 15. `L3_DELIVERY_REPORT.md` 必填內容

```text
Asset ID:
Version:
交付等級: L3
來源 L2 版本:
輸出格式與 FBX 版本:
世界單位 / 軸向 / Pivot:
實測 bounds:
LOD0 / LOD1 / LOD2 triangles:
各 LOD renderer 與材質槽數:
骨架類型:
Unity Humanoid Avatar Valid: Yes / No
變形骨數 / 總 Transform 數:
每頂點最大權重:
Root Motion 測量結果:
各動畫 fps / frame range / 秒數 / Loop:
各動畫事件 frame:
Socket parent / local position / local rotation:
使用貼圖與色彩空間:
Unity 版本與匯入設定:
已知限制:
```

## 16. 生成與授權紀錄

`GENERATION_AND_LICENSE_RECORD.md` 必須填寫：

```text
生成／Rig／動畫工具與版本:
使用的模型或服務名稱:
完整 Prompt:
Seed / Job ID:
生成日期:
人工修改工具與逐步說明:
使用的第三方模型、動作、材質或貼圖:
每項第三方素材的來源網址與授權:
是否允許商業遊戲使用、修改與再散布 build:
已知限制:
```

如果任何來源或商用授權無法確認，必須標記：

```text
Release Status: Blocked - License Unverified
```

此狀態可以供本地 Prototype 測試，但不可宣稱是 production／release accepted asset。

## 17. 驗收測試

### 17.1 幾何與匯入

- [ ] Asset ID 與 v002 命名正確，v001 原始檔未被覆寫。
- [ ] Unity Import Scale 1，Prefab Transform `(0,0,0) / (0,0,0) / (1,1,1)`。
- [ ] Y-up、Z-forward、腳底中心 Pivot，沒有漂浮或穿地。
- [ ] 外觀與 v001 一致；高度、盾、短劍與 bounds 在允許範圍。
- [ ] Unity Humanoid Avatar Valid。
- [ ] LOD0／LOD1／LOD2 共用同一骨架，切換無跳位。
- [ ] 每個 LOD 不超過 2 個 renderer／材質槽，triangle budget 合格。
- [ ] 沒有相機、燈光、背景、控制器、無用骨或 hidden mesh。

### 17.2 蒙皮

- [ ] 每 vertex 最多 4 權重，無 unweighted vertex。
- [ ] 肩、肘、髖、膝在 Bind／Move／Attack／Death 無明顯塌陷。
- [ ] 盾與劍保持剛性，不變形、不脫手。
- [ ] 動畫播放時盾不穿腿、劍不穿身體、腳不穿地面。

### 17.3 動畫

- [ ] 五個必要 clips 都能單獨播放，名稱、長度、Loop 正確。
- [ ] Idle／Move 連播 10 次沒有跳格或明顯腳滑。
- [ ] 全部 clips 的 Root Motion 為 0，Root drift 在規格容許值內。
- [ ] `AttackImpact`、`Footstep_L/R`、`DeathSettled` 對齊實際畫面，誤差不超過 1 個 30 fps frame。
- [ ] Attack 在 960×540、Zoom 31 可辨識 Windup／Impact／Recover。
- [ ] Death 最後姿勢保持，不跳回 Idle。

### 17.4 Unity 與 RTS 畫面

- [ ] 960×540、1920×1080，Zoom 31 可辨識步兵與攻擊方向。
- [ ] 960×540，Zoom 40 仍能由盾與厚實輪廓辨識步兵。
- [ ] Zoom 8 檢查無破面、嚴重 UV 接縫與穿模。
- [ ] 同一網格可用 `#4AA3D8` 與 `#D94A45` 切換陣營色。
- [ ] 血條固定在 Y 2.10，不跟頭部動畫上下跳動。
- [ ] 選取圈位於地面中心 Y 0.02。
- [ ] 50 個單位同時播放 Move／Attack 時沒有 Console error、missing reference 或材質 instance 爆增。

### 17.5 文件與授權

- [ ] 原始製作檔、FBX／GLB、clips、貼圖、事件 JSON、報告與預覽齊全。
- [ ] 完整 Prompt、Seed／Job ID、人工修改與第三方素材來源可追溯。
- [ ] 商用授權明確；不含現有遊戲 IP、商標、浮水印或來源不明動作。

## 18. 直接退件條件

符合任一項即退件，不進 Unity Runtime：

- 用新生成角色取代 v001，而非對現有模型 Rig／升級。
- 只有動畫預覽影片，沒有可匯入的骨架、mesh 與 clips。
- Avatar Invalid、Generic 冒充 Humanoid，或每個動畫使用不同骨架／Bind Pose。
- Root Motion 推動角色、腳底不在 Y=0、Z-forward 錯誤或必須靠 100／0.01 Scale 修正。
- 缺少 Idle、Move、Attack_A、Hit、Death 任一必要動畫。
- 盾、短劍在動畫中脫手、彎曲、穿身或消失。
- 藍／紅是兩套重複網格，或 Team Color 無法 runtime 替換。
- LOD2 移除盾／武器，導致最遠距離無法辨識。
- 只交一條未切割 timeline，沒有 clip／事件 frame 表。
- 沒有原始檔、生成紀錄或素材授權來源。

## 19. 可直接貼給製作 AI 的完整任務 Prompt

```text
Upgrade the attached existing CHR_Infantry_A_v001 L2 asset into CHR_Infantry_A_v002 L3 for a Unity 6 URP top-down RTS. This is a rigging, skinning, animation, LOD2, and delivery task. Do not redesign or regenerate a different character. Preserve the attached LOD0/LOD1 appearance, 1.80 m scale, 5–5.5-head proportions, left-hand 0.60 x 0.86 m shield, right-hand 1.00 m sword, UV layout, Base material, and replaceable Team Color regions.

World contract: 1 unit = 1 meter, Y-up, Z-forward, X-right, foot-center pivot at (0,0,0), all exported transforms applied, root scale (1,1,1). The runtime gameplay capsule remains radius 0.38 m and height 2.0 m. SelectionAnchor is (0,0.02,0), HealthBarAnchor is (0,2.10,0), and GroundContact is (0,0,0).

Create a Unity Humanoid-compatible skeleton with Root and Hips. Unity Avatar Configuration must be Valid. Use one shared skeleton and bind pose for LOD0, LOD1, and LOD2. Maximum 4 bone influences per vertex, normalized weights, recommended <=65 deform bones and <=75 total transforms. Root must remain at the origin with no root motion, no animated scale, X/Z drift <=0.01 m, Y drift <=0.02 m, and rotation drift <=0.5 degrees. Put natural vertical motion on Hips, not Root.

Keep shield and sword rigid. Prefer separate rigid meshes attached to Socket_L_Hand and Socket_R_Hand. Add Socket_WeaponTip, Socket_Head, FX_Hit_Center, FX_Foot_L, and FX_Foot_R. SelectionAnchor, HealthBarAnchor, and GroundContact must not be parented to animated bones.

Deliver five separate 30 fps clips using the identical skeleton/avatar:
- AN_Infantry_Idle: 3.0 s / 90 frames, looping, subtle combat-ready breathing.
- AN_Infantry_Move: 0.8 s / 25 frames (0-24), looping in place, no root motion; Footstep_L at frame 1 and Footstep_R at frame 13, aligned to exact foot contacts.
- AN_Infantry_Attack_A: 0.9 s / 27 frames, non-looping; right-high to left-low sword slash, shield stays protective; windup frames 0–8, swing frames 9–15, recover frames 16–27; AttackImpact near frame 13 at the actual visual contact frame.
- AN_Infantry_Hit: 0.33 s / 10 frames, non-looping, short upper-body reaction, facing change <=15 degrees.
- AN_Infantry_Death: 1.30 s / 39 frames, non-looping, root fixed, body stays within 1 m of root, DeathSettled near frame 35, final pose held.

No foot sliding, shield-leg penetration, sword-body penetration, ground penetration, root translation, automatic return from Death to Idle, embedded gameplay damage, cameras, lights, background, existing IP, logos, watermarks, unknown third-party animations, physics cloth, or DCC-only runtime dependencies.

Provide LOD0 at 2.5k–6k triangles (target current 4,376), LOD1 at 1k–2.5k (target current 1,512), and a new LOD2 at 250–700. Keep helmet, shield, sword, and Team Color silhouette in LOD2. Maximum two renderers/material slots per LOD: MAT_Infantry_Base and MAT_Infantry_TeamColor. Do not create separate blue/red geometry. Team Color must remain runtime-changeable through _BaseColor or _Color.

Preferred output is FBX 2018-compatible: one SK_Infantry_A_v002.fbx containing skeleton, bind pose, and all three skinned LODs, plus five individual AN_*.fbx files sharing the same avatar. Also provide the editable source file, 1K BaseColor/Normal/ORM textures, ANIMATION_EVENTS.json, UNITY_IMPORT_SETTINGS.md, L3_DELIVERY_REPORT.md, GENERATION_AND_LICENSE_RECORD.md, bind-pose screenshots, skin extreme-pose screenshot, Unity Game View screenshots at 960x540 and 1920x1080 / camera pitch 55 degrees / vertical FOV 60 / distance 31 m, a 960x540 distance 40 m screenshot, and an animation preview.

Record the exact tool/version, model/service, full prompt, seed/job ID, date, manual edits, every third-party source and license, and commercial-use status. If you cannot edit and rig the supplied mesh, respond exactly with “Blocked - Tool Cannot Rig Existing Mesh”. If commercial rights cannot be verified, mark “Release Status: Blocked - License Unverified”. Do not claim L3 completion without importable model, skeleton, clips, event manifest, and editable source.
```

## 20. 完成定義

只有當 Unity 能以 Scale 1 匯入、Humanoid Avatar Valid、五個動畫與事件正確、Root Motion 為 0、三個 LOD／Team Color／anchors 可用、Game View 驗收通過，且來源與商用授權可追溯時，才可標記：

```text
Integrated (Prototype L3)
Production Accepted
```

如果技術整合通過但授權、正式材質或來源紀錄未完成，只能標記：

```text
Integrated (Prototype L3) / Release Blocked
```
