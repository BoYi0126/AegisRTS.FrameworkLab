# 單位美術規格：弓兵

## 任務摘要

製作輕甲遠程弓兵與獨立箭矢 Placeholder／正式低模。弓兵射擊動畫只負責拉弓與放箭；箭矢必須是另外的 Prefab，供遊戲系統生成與移動。

## 技術條件

- Asset ID：`unit.archer`；Projectile ID 建議 `projectile.arrow.basic`。
- 檔名：`CHR_Archer_A_v001`、`PRJ_Arrow_Basic_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；腳底中心 Pivot；Scale 1。
- 身高 `1.72–1.82 m`，寬 `0.55–0.68 m`，深含箭袋 `0.50–0.72 m`。
- Agent 半徑 `0.38 m`、高度 `2.0 m`。
- 弓高 `1.15–1.35 m`；箭長 `0.75–0.90 m`，可為遠距辨識把箭桿加粗到 `0.025–0.04 m`。
- `HealthBarAnchor` Y=`2.10 m`；`Socket_Projectile` 位於弓弦放箭位置。

## 外觀與輪廓

- 5–5.5 頭身，輕甲、較窄肩部、開放輪廓。
- 一把清楚彎曲的長弓是最重要辨識點；不可像短直棍。
- 背部箭袋與羽箭形成第二辨識點，但不可超出角色深度上限。
- 不配大型盾牌；護臂、胸帶與短披肩即可。
- 弓身、箭袋帶或肩布提供 15–22% 隊伍色。
- 手、弓弦與箭的相對位置必須合理，不要求真正物理弓弦模擬。

## 材質與預算

- 木弓、暗鐵箭頭、輕型皮布甲。
- 最多 2 材質槽；1024 atlas。
- 角色 LOD0 2,500–6,000 triangles；箭矢 40–180 triangles。
- 弓弦可用簡單 Mesh 或 Line；不得依賴只能在近距看到的 1 px 細線作辨識。

## 骨架與動畫

- Unity Humanoid，Root Motion 關閉。
- Idle、Move、Attack_Ranged、Hit、Death。
- Attack_Ranged 建議 0.9–1.3 s：取箭／抬弓 0.2–0.35 s、拉弓瞄準 0.25–0.45 s、放箭、收招。
- 放箭幀加入 `ProjectileRelease`；事件前箭可掛於手／弦，事件後隱藏手中箭並由遊戲生成 Projectile。
- 箭矢飛行與命中不是角色 Animation Clip。
- Projectile 前端朝本地 Z+；Pivot 在箭桿中心或尾端，但整個專案必須一致，建議放在中心。

## Placeholder 表現

- 若暫時不做完整箭矢，可用長 `0.8 m`、寬 `0.04 m` 的暖黃色／木色長方體。
- 飛行時可加 0.2–0.4 m 淡色拖尾。
- 命中使用 0.15–0.3 s、最大 0.5 m 的小色塊火花。
- Placeholder 也必須由 `ProjectileRelease` 生成，不可把「飛出去」畫死在弓兵 Animation 中。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly 3D archer for a Unity 6 URP top-down RTS, ancient East-Asian-inspired but world-neutral. 1 Unity unit = 1 meter, Y-up, Z-forward, foot-center pivot. Character height 1.72–1.82 m, width 0.55–0.68 m, depth including quiver no more than 0.72 m. Use light armor, a clearly curved 1.15–1.35 m bow, and a readable back quiver. No shield. Team-color cloth/straps cover 15–22% and remain visible from all sides: #4AA3D8 friendly, #D94A45 enemy. Also create a separate low-poly arrow prefab 0.75–0.90 m long with Z+ forward. The shooting animation must use a ProjectileRelease event; the projectile is not baked into the flight animation. Readable at 960x540 from 31 m. Maximum 2 materials, 1024 atlas, 2.5k–6k LOD0 triangles. No text, logo, watermark, photorealism, firearm, existing IP, or root motion.
```

## 驗收

- [x] 弓兵、步兵轉灰階後仍可立即區分（Prototype L3：弓／箭袋對盾／劍）。
- [x] 箭矢為獨立資產，方向與 Pivot 已說明並由 Unity Builder 驗證。
- [ ] `ProjectileRelease` 與弓弦放開相差不超過 1 frame。
- [ ] 手臂、弓弦、箭袋無明顯穿模。

目前可玩實作、重建方式、測試證據與正式美術替換條件見 [`Unit_04_弓兵_L3實作交付與驗收.md`](Unit_04_弓兵_L3實作交付與驗收.md)。最後兩項保留給正式弓弦動畫與人工 release review，不因 Prototype 已可運作而誤標完成。
