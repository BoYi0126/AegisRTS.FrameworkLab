# 單位美術規格：攻城兵器

## 任務摘要

製作目前 `unit.siege` 的第一版：可移動的輕型衝車。選擇衝車是因為目前核心玩法是破壞城門、攻入城塞再占領主堡；它比投石機更直接對應現有系統。

## 技術條件

- Asset ID：`unit.siege`；視覺型別：`siege.ram.light`。
- 建議檔名：`VEH_SiegeRam_Light_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 高 `1.65–1.90 m`，寬 `1.30–1.55 m`，長 `2.50–3.00 m`。
- 目標碰撞盒約 `2.6×1.35 m`；目前原型仍用半徑 0.38、高 2.0 的 Agent，正式整合前必須新增大型單位 Agent／避障規格。
- `HealthBarAnchor` Y=`2.10 m`；選取範圍約 `2.8×1.6 m`。
- 車頭／撞擊方向為 Z+。

## 外觀與輪廓

- 四輪低重心木製車架，中央明顯撞木，前端包暗鐵撞頭。
- 可有簡化棚頂，但不可遮住撞木動作；棚頂高度不超過 1.9 m。
- 不顯示固定操作士兵，以免增加骨架與死亡狀態同步；第一版視為抽象化自走攻城隊。
- 車輪直徑 `0.55–0.75 m`，寬度至少 `0.12 m`，遠距可見。
- 旗帶、棚布或側盾提供 12–20% 可替換隊伍色。
- 撞木前端需和普通車身有明度差，讓玩家知道攻擊方向。

## 材質與預算

- 主體木材、暗鐵補強、粗繩、隊伍色棚布。
- 最多 3 材質槽；1024 或 2048 atlas。
- LOD0 5,000–12,000 triangles；LOD1 2,000–5,000；LOD2 500–1,200。
- 繩索最小幾何厚度 `0.06 m`；更細繩索改用貼圖。

## 骨架與動畫

- Generic Rig，Root Motion 關閉。
- Idle、Move、Attack_Ram、Hit、Death/Disabled。
- Move：輪子依速度旋轉；若由程式驅動輪轉，文件中需標明輪軸 Socket。
- Attack_Ram 建議 1.4–2.0 s：後拉 0.4–0.7 s、前撞、`AttackImpact`、回復。
- 撞木位移相對 VisualRoot，不推動 GameplayRoot。
- Disabled 可以是棚布破損、車體下沉 `0.1–0.2 m`，不可突然縮成扁平色塊。

## 攻擊 VFX

- 撞城門時在 `Socket_RamTip` 生成木屑、塵土與小火花。
- VFX 0.4–0.8 s、直徑 `1.5–2.5 m`，不得遮住整座城門。
- 音效事件命名建議 `SiegeRamImpact`，傷害仍以 `AttackImpact` 的遊戲事件為準。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly light battering ram for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral. 1 unit = 1 meter, Y-up, Z-forward, bottom-center pivot, ram attacks toward Z+. Dimensions: 1.65–1.90 m tall, 1.30–1.55 m wide, 2.50–3.00 m long. Use a low four-wheel wooden chassis, a clearly visible central suspended ram with a dark iron head, and an optional simple roof. No visible crew in this first version. Wheels must be 0.55–0.75 m diameter and readable from the RTS camera. Team-color roof cloth or side shields cover 12–20%: #4AA3D8 friendly, #D94A45 enemy. Generic rig, stable root, no root motion; provide Idle, Move, Ram Attack, Hit and Disabled. The attack uses AttackImpact at the forward strike and includes a RamTip socket. 5k–12k LOD0 triangles, max 3 materials. No text, logo, watermark, gunpowder weapon, photorealism, existing IP, or hidden crew.
```

## 驗收

- [ ] Z+ 攻擊方向、Pivot、撞木與輪軸清楚。
- [ ] 與騎兵、建築相比尺寸合理。
- [ ] 撞木動畫不移動 GameplayRoot。
- [ ] 文件與交付明確指出大型 Agent 尚需程式整合。

