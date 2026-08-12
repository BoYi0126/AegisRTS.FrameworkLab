# 單位美術規格：步兵

## 任務摘要

製作目前原型最優先的基礎近戰步兵。它必須是最容易量產、效能穩定、遠距辨識清楚的普通單位，使用盾與短兵器建立厚實輪廓。

## 技術條件

- Asset ID：`unit.infantry`。
- 建議檔名：`CHR_Infantry_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；腳底中心 Pivot；Scale 1。
- 身高 `1.75–1.85 m`，寬 `0.60–0.72 m`，深 `0.45–0.65 m`。
- 遊戲足跡與目前 Agent：半徑 `0.38 m`、高度 `2.0 m`。
- `HealthBarAnchor` Y=`2.10 m`；`SelectionAnchor` Y=`0.02 m`，選取圈直徑約 `0.9 m`。

## 外觀與輪廓

- 5–5.5 頭身；普通成年士兵，不做英雄比例。
- 左手中型盾，寬 `0.55–0.65 m`、高 `0.75–0.95 m`。
- 右手短劍／刀，長 `0.9–1.1 m`。第一版不要同時做盾、槍、劍三套混合輪廓。
- 頭盔、胸甲、盾是三個主要形狀；小配件最多 3 件。
- 盾面與肩布承載陣營色，從前、後、側面均至少有一處可見。
- 可另提供無盾長槍 Variant，但須作為獨立 Variant，不可讓基礎步兵辨識失焦。

## 材質與預算

- 中性布衣、簡化札甲、木盾或包皮盾、暗鐵武器。
- 15–25% 可見面積為隊伍色。
- 最多 2 材質槽；1024×1024 atlas。
- LOD0 2,500–6,000 triangles；LOD1 1,000–2,500；LOD2 250–700。
- 盾牌內側不做高密度細節；遠距不可見部位簡化。

## 骨架與動畫

- Unity Humanoid；Root Motion 關閉。
- Idle 2–4 s、Move 0.8–1.0 s、Attack_A 0.7–1.0 s、Hit 0.25–0.4 s、Death 1.0–1.4 s。
- Attack_A 使用由右上向左下的明確斬擊，`AttackImpact` 在武器通過身體前方時觸發。
- Idle 中盾牌不得遮住整個身體；Move 不可讓盾穿腿。
- 必要 Socket：雙手、武器、盾、受擊中心、血條、選取。

## RTS 可讀性

- 960×540、31 m 距離時，盾牌至少形成角色寬度 25% 的可見輪廓。
- 與弓兵並排時，即使轉灰階也能由盾與較厚甲片分辨。
- 不依賴劍刃細節；劍厚度可略誇張，最薄處至少 `0.06 m`。

## 可直接給 AI 的提示詞

```text
Create a production-ready stylized low-poly 3D basic melee infantry unit for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral, not a historical or copyrighted character. Scale: 1 unit = 1 meter, Y-up, Z-forward, foot-center pivot. Height 1.75–1.85 m, width 0.60–0.72 m. Give the soldier simplified medium armor, a 0.55–0.65 m wide and 0.75–0.95 m tall shield in the left hand, and a 0.9–1.1 m sword in the right hand. Use a robust compact silhouette, 5–5.5 heads tall, slightly oversized helmet, hands, shield and weapon. Team-color areas cover 15–25%, visible from front, side and back: #4AA3D8 friendly, #D94A45 enemy. Must remain identifiable at 960x540 with a 55-degree camera at 31 m. Maximum 2 material slots, 1024 atlas, 2.5k–6k LOD0 triangles. No text, logo, watermark, firearm, photorealism, root motion, or existing IP.
```

## 驗收

- [ ] 尺寸可直接替換目前 0.8 m 寬、1.6 m 高的 Capsule Placeholder，不影響半徑 0.38 Agent。
- [ ] 盾與短兵器輪廓在最遠 Zoom 可辨識。
- [ ] 兩個材質槽以內，隊伍色可替換。
- [ ] 動畫無腳滑、盾牌穿腿與 Root Motion。

