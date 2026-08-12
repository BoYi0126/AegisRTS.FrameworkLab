# 單位美術規格：指揮官

## 任務摘要

製作一名可作為多勢力共用基底的古代東亞風格指揮官。角色需在俯視 RTS 中明顯高於普通士兵，但不要綁定特定歷史人物、國家、文字或既有 IP。

## 技術條件

- Asset ID：`hero.commander`；敵方 `hero.opponent` 第一版共用同一模型與換色材質。
- 建議檔名：`CHR_Commander_A_v001`。
- Unity 6 URP；1 Unity Unit = 1 公尺。
- Y-up、Z-forward；腳底中心 Pivot `(0,0,0)`；匯入 Scale `(1,1,1)`。
- 常態人體高度 `2.10–2.20 m`；含背旗／冠飾總高不得超過 `2.45 m`。
- 常態寬 `0.70–0.85 m`、深 `0.55–0.75 m`。
- 目標選取／導航足跡半徑 `0.48 m`；目前程式仍使用半徑 0.38 m、高 2.0 m，整合時需由程式調整。
- `HealthBarAnchor`：Y=`2.50 m`；`SelectionAnchor`：Y=`0.02 m`。

## 外觀與輪廓

- 5.5–6 頭身，威嚴、穩重、非寫實人體。
- 中型肩甲、披風或短背旗構成倒梯形輪廓。
- 主武器使用長劍／指揮刀，長 `1.10–1.25 m`；不可使用槍械。
- 裝甲比普通步兵完整，但避免滿身細碎尖刺。
- 頭部輪廓需有簡潔盔冠；臉部細節不是辨識重點。
- 背旗、肩布、腰帶或披風提供 15–25% 可替換陣營色，四面都能看到。
- 不在旗幟上生成文字；預留純色／徽記 Mask，世界觀確定後再替換。

## 配色與材質

- 中性基底：暗鐵、暖灰、深棕皮革、米灰布。
- 我方預覽：`#4AA3D8`；敵方預覽：`#D94A45`。
- 金屬以暗鐵為主，亮邊面積小於可見面積 10%。
- 最多 3 個材質槽；2048×2048 或 1024×1024 atlas。
- LOD0 4,000–10,000 triangles；LOD1 1,500–4,000；LOD2 400–1,000。

## 骨架與動畫

- Unity Humanoid，相容標準重定向；Root Motion 關閉。
- 必要：Idle、Move、Attack_A、Hit、Death、Command、Ability_A。
- Command：0.8–1.2 s，以舉劍／手勢下令，角色根節點不移動。
- Attack_A：0.8–1.1 s，清楚的 0.25–0.4 s 前搖；用 `AttackImpact` 標記事件。
- 披風與旗幟可用少量骨骼，不依賴布料模擬才能正常顯示。
- 必要 Socket：雙手、武器、頭部、受擊中心、血條、選取圈。

## RTS 可讀性

- 960×540、55° Pitch、60° FOV、31 m 距離下，不靠名字也應一眼看出「英雄／指揮官」。
- 與 1.8 m 步兵並排時，高度約多 15–20%，肩部與披風輪廓也須不同。
- 純黑剪影中，背旗／披風與長劍仍可辨識。
- 最近 Zoom 8 m 不可出現肩甲穿頭、披風穿腿或破面。

## 禁止項目

- 不要製作知名三國武將的直接外觀。
- 不要生成漢字、勢力名稱、商標或龍形既有徽記。
- 不要超長披風拖地；不可遮住選取圈或碰撞判斷。
- 不要使用 Root Motion、寫實毛髮或高成本布料模擬。

## 交付

L1：2048 PNG 四視圖、3/4 俯視、黑剪影、藍紅換色、尺寸圖。  
L2：FBX/GLB、原始檔、UV、LOD0/1、材質貼圖、Anchor/Socket。  
L3：Humanoid 骨架、全部指定動畫、LOD2、Unity 預覽與匯入設定。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly 3D ancient East-Asian-inspired RTS commander, world-neutral and not based on any historical or copyrighted character. Unity scale: 1 unit = 1 meter, Y-up, Z-forward, foot-center pivot. Body height 2.10–2.20 m, maximum height including a short back banner 2.45 m, width 0.70–0.85 m. Use a strong inverted-trapezoid silhouette, medium lamellar-inspired armor, short cape or plain banner, and a 1.10–1.25 m command sword. Exaggerate shoulder, weapon, hands and head by 10–20% for readability from a 55-degree RTS camera at 960x540. Provide replaceable team-color cloth visible from all directions: friendly #4AA3D8 and enemy #D94A45. No text, logo, watermark, firearms, photorealism, existing IP, or root motion. Follow the required L1/L2/L3 deliverables exactly.
```

## 驗收

- [ ] 尺寸、Pivot、朝向、Scale 正確。
- [ ] 2 秒內可與步兵、副官區分。
- [ ] 陣營色四面可見且可替換。
- [ ] 動畫不移動 GameplayRoot。
- [ ] 無文字、浮水印、IP 或不明素材。
