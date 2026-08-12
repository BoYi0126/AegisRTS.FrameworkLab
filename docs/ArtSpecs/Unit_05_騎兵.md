# 單位美術規格：騎兵

## 任務摘要

製作一名騎手與一匹馬組成的中型騎兵。騎兵需以長水平輪廓和較快步態呈現機動性，不能只是把普通士兵放大。

## 技術條件

- Asset ID：`unit.cavalry`。
- 建議檔名：`CHR_Cavalry_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；馬匹接地中心 Pivot；Scale 1。
- 總高 `2.15–2.35 m`，寬 `0.85–1.00 m`，長 `2.20–2.55 m`。
- 目標導航足跡為長 `2.2 m`、寬 `0.9 m` 的膠囊／近似代理；目前原型仍用半徑 0.38、高 2.0，正式換入前需程式支援依兵種 Agent 尺寸。
- `HealthBarAnchor` Y=`2.60 m`；選取橢圓約 `2.4×1.1 m`。

## 外觀與輪廓

- 馬身為主要輪廓，騎手不要做得像 2 m 巨人；騎手人體比例約 1.7–1.8 m。
- 馬匹使用風格化中型戰馬，腿部稍粗，避免過細在遠距閃爍。
- 騎手穿中型甲，使用 `2.2–2.6 m` 長槍／騎槍；Move 時斜持，避免橫向占用過大。
- 馬鞍布、騎手肩布與槍纓提供 15–25% 隊伍色；四面可見。
- 不使用大型翅膀、飄浮裝甲或現代馬具。

## 材質與預算

- 馬體可提供棕、黑、灰三種 Base Variant，共用同一骨架與 UV。
- 最多 3 材質槽；角色與馬可共用 2048 atlas 或各 1024。
- 人馬合計 LOD0 6,000–14,000 triangles；LOD1 2,500–6,000；LOD2 600–1,500。
- 馬尾與鬃毛使用低模片束或實體塊，不依賴高透明 overdraw。

## 骨架與動畫

- Generic Rig；單一 `Root` 控制整組，Root Motion 關閉。
- Idle、Move_Trot、Move_Gallop、Attack_A、Hit、Death。
- 原型可先只使用 Idle、Move_Gallop、Attack_A、Death。
- 馬蹄循環需避免腳滑；速度由程式決定，Animator Speed 可依移動速度縮放。
- Attack_A 以斜前方刺擊，`AttackImpact` 時槍尖位於馬頭前 `0.8–1.4 m`。
- Death 不可翻滾超過模型自身 1.2 倍長度，以免占據錯誤路徑區域。

## RTS 可讀性

- 960×540、31 m 時，由長馬身和高位騎手清楚分辨。
- 與步兵混編時，隊伍色至少在馬鞍布上形成大色面，不只在騎手胸口。
- 轉灰階仍能由水平輪廓辨識；最遠 Zoom 下槍可消失但人馬輪廓不可消失。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly 3D cavalry unit for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral, no historical or copyrighted character. 1 unit = 1 meter, Y-up, Z-forward, ground-center pivot beneath the horse. Combined dimensions: 2.15–2.35 m tall, 0.85–1.00 m wide, 2.20–2.55 m long. Use a sturdy stylized medium warhorse, a medium-armored rider, and a 2.2–2.6 m lance held diagonally during movement. Use large team-color saddle cloth and rider cloth visible from all sides: #4AA3D8 friendly, #D94A45 enemy. Generic rig with one stable root, no root motion. Provide Idle, Trot/Gallop, Attack, Hit and Death. 6k–14k LOD0 triangles, max 3 materials. Readable at 960x540 from a 55-degree RTS camera. No text, logo, watermark, wings, firearms, photorealism, or existing IP.
```

## 驗收

- [ ] 尺寸與足跡資料完整，並明確標記目前 Agent 需程式升級。
- [ ] 人馬共用穩定 Root，移動動畫不帶世界位移。
- [ ] 馬鞍布隊伍色從四面可見。
- [ ] 最遠視角仍可由長水平輪廓辨識。

