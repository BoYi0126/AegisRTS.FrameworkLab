# 動畫、VFX 與 Prefab 接點

## 1. 動畫共同規則

- 遊戲位移由 NavMesh／遊戲系統控制，所有動畫 Root Motion 關閉。
- 動畫預設 30 fps；可在 60 fps 製作，但匯出後事件時間必須一致。
- Loop 動畫首尾姿勢與速度連續，不可跳幀。
- 攻擊動畫拆成 `Windup → Impact/Release → Recover`，事件點明確。
- 受擊動畫不得改變角色面向超過 15°，不得讓根節點離開原位。
- 死亡動畫結束後保持最後姿勢；屍體消失與回收由遊戲系統控制。
- 每個 clip 單獨命名，禁止只交一條未切割的長時間軸。

## 2. 動畫命名

```text
AN_<Asset>_Idle
AN_<Asset>_Move
AN_<Asset>_Attack_A
AN_<Asset>_Attack_B
AN_<Asset>_Hit
AN_<Asset>_Death
AN_<Asset>_Ability_<Name>
```

必要事件：

- `AttackImpact`：近戰傷害命中幀。
- `ProjectileRelease`：箭矢或投射物離手幀。
- `Footstep_L`、`Footstep_R`：需要腳步音效時使用。
- `DeathSettled`：死亡姿勢落定，可開始淡出或生成屍體代理。

## 3. 雙足單位基礎動畫

L3 最低交付：

| 動畫 | 建議長度 | Loop | 備註 |
|---|---:|---|---|
| Idle | 2–4 s | 是 | 小幅呼吸，不要大幅搖晃武器 |
| Move | 0.7–1.1 s | 是 | 原地循環，Root Motion 0 |
| Attack_A | 0.7–1.2 s | 否 | 清楚前搖與命中幀 |
| Hit | 0.25–0.45 s | 否 | 短促，不中斷辨識太久 |
| Death | 1.0–1.6 s | 否 | 不翻滾超過 1 m |

指揮官與副官另需 Ability／Command 手勢；弓兵另需射擊；各自細節依個別文件。

## 4. 弓箭與投射物

弓箭不是弓兵本體動畫的一部分，而是獨立遊戲物件：

- 弓兵動畫負責拉弓、放箭與手部動作。
- `ProjectileRelease` 事件生成箭矢 Prefab。
- 箭矢沿遊戲系統指定軌跡移動，命中時生成小型 VFX。
- Placeholder 階段可用長條色塊或簡單低模箭矢，這完全可行。
- 箭矢建議尺寸：長 `0.75–0.9 m`、桿直徑 `0.025–0.04 m`；為遠距可讀性可視覺放大到實物的 1.5–2 倍。
- 箭尾可加短暫暖黃／白色拖尾，但不可像雷射。

## 5. VFX 規格

- Placeholder VFX 優先使用色塊、簡單 Mesh、Line Renderer、短粒子與地面圓環。
- 近戰命中：0.15–0.25 s，小型白黃斬擊／火花，世界尺寸不超過 `0.8 m`。
- 箭矢命中：0.15–0.3 s，世界尺寸不超過 `0.5 m`。
- 攻城命中：0.4–0.8 s，木屑／塵土範圍 `1.5–2.5 m`。
- 技能預警：地面半透明形狀，顏色與陣營色分離；敵方危險以橙紅，友方指令以藍青。
- 所有 VFX 必須能物件池化，不依靠 Destroy 每次建立大量物件。

## 6. Socket 命名

- `Socket_R_Hand`、`Socket_L_Hand`：武器與盾。
- `Socket_Projectile`：箭矢／投射物生成點。
- `Socket_Head`：頭頂效果，不放血條。
- `FX_Hit_Center`：主要受擊中心。
- `FX_Foot_L`、`FX_Foot_R`：腳步塵土。
- `HealthBarAnchor`：角色最高點上方至少 `0.15 m`。
- `SelectionAnchor`：地面中心，Y=`0.02 m`。

## 7. Prefab 與遊戲狀態

- `GameplayRoot` 掛選取、導航、戰鬥 View；`VisualRoot` 掛 Animator 與 Renderer。
- 不能把生命值、攻擊力或陣營寫進美術 Prefab；它們來自 Content Pack 與遊戲狀態。
- 隊伍顏色由 MaterialPropertyBlock 或指定材質參數切換。
- 選取圈不烘焙進模型；由獨立 Prefab／Renderer 顯示。
- 血條永遠面向相機，由程式控制；美術只提供 Anchor。
- 城門開關、破壞、修復與主堡占領均由遊戲狀態驅動，不可只靠動畫自行改狀態。

## 8. 動畫驗收

- Unity Game View 中播放 10 次無抖動、無腳滑、無穿模。
- 960×540 預設視角看得出攻擊前搖、出手與收招。
- 攻擊事件與武器接觸／放箭幀誤差不超過 1 個 30 fps frame。
- 動畫中 Collider／Agent 根節點保持穩定。
- 我方與敵方同時大量播放時，不因材質實例化造成明顯額外負擔。

