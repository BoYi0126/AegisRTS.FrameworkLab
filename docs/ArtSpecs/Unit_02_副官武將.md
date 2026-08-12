# 單位美術規格：副官武將

## 任務摘要

製作指揮官麾下的副官／武將通用基底。其質感需高於普通士兵，但在大小、旗幟與裝飾上低於主指揮官，避免戰場上角色階級混淆。

## 技術條件

- Asset ID：`hero.lieutenant`；目前已存在 Content Pack 與原型角色資料，仍使用 Hero Placeholder。
- 建議檔名：`CHR_Lieutenant_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；腳底中心 Pivot；Scale 1。
- 高 `1.95–2.08 m`，寬 `0.65–0.78 m`，深 `0.50–0.70 m`。
- 目標遊戲足跡半徑 `0.43 m`；目前原型相容 Agent 半徑 0.38、高 2.0。
- `HealthBarAnchor` Y=`2.30 m`；`SelectionAnchor` Y=`0.02 m`。

## 外觀與輪廓

- 5.5 頭身；姿態有戰鬥力但不比指揮官更華麗。
- 中輕型札甲、單側肩披或短腰旗，形成非對稱辨識點。
- 武器預設為 `1.8–2.2 m` 槍／戟；可製作劍類 Variant，但同一份交付先完成一種。
- 不使用大型背旗；可使用短背標，總高不超過 `2.25 m`。
- 15–22% 可見面積為可替換陣營色。
- 預留可替換的頭盔、肩甲、武器三個 Variant 接點，以便未來隨機武將系統生成差異。

## 材質與預算

- 暗鐵、深棕、麻布色為中性基底。
- 我方 `#4AA3D8`、敵方 `#D94A45`。
- 最多 3 材質槽，貼圖 1024 或 2048。
- LOD0 4,000–9,000 triangles；LOD1 1,500–3,500；LOD2 400–900。

## 骨架與動畫

- Unity Humanoid，Root Motion 關閉。
- Idle、Move、Attack_A、Attack_B、Hit、Death、Command_Short、Ability_A。
- 長兵器攻擊事件 `AttackImpact`；掃擊不可讓武器穿過自身軀幹。
- Command_Short 為 0.5–0.8 s，辨識度低於指揮官 Command。
- 必要 Socket：雙手、武器、頭、背部 Variant、FX_Hit_Center、HealthBarAnchor。

## RTS 可讀性

- 預設相機下比步兵高約 8–12%，有非對稱肩部和長兵器。
- 比指揮官矮、沒有大披風或大型背旗。
- 純黑剪影可同時通過「像武將」與「不是最高階指揮官」兩項判斷。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly 3D ancient East-Asian-inspired RTS lieutenant/officer, not tied to a dynasty, historical person, or existing IP. Unity scale 1 meter per unit, Y-up, Z-forward, foot-center pivot. Height 1.95–2.08 m, width 0.65–0.78 m. Use medium-light armor, one asymmetric shoulder cloth or short waist banner, and a 1.8–2.2 m spear/polearm. The silhouette must look more elite than infantry but clearly less important than a commander: no large cape and no tall back banner. Include modular helmet, shoulder and weapon attachment points. Team-color cloth covers 15–22% and is visible from all sides, friendly #4AA3D8 and enemy #D94A45. Readable at 960x540 from a 55-degree RTS camera. No text, logos, watermark, photorealism, copyrighted character, firearms, or root motion.
```

## 驗收

- [ ] 與指揮官、步兵的高度與輪廓層級正確。
- [ ] Variant 接點清楚，不須重做整個骨架即可換件。
- [ ] 武器在 Idle／Move／Attack 不明顯穿模。
- [ ] 陣營色可替換、四面可見。
