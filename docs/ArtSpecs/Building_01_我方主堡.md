# 建築美術規格：我方主堡

## 任務摘要

製作目前 `Player City` Graybox 的替換資產。這是三國霸業式玩法中的我方主要據點，可同時承擔主堡、訓練與內政入口，不需要先另外建兵營。它可被攻擊，規則上依 GameMode 決定摧毀或占領。

## 技術條件

- Asset ID：`settlement.player-city`。
- 建議檔名：`BLD_PlayerStronghold_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 固定足跡：寬 X=`5.0 m`、深 Z=`7.0 m`。
- 視覺高度 `4.5–5.5 m`；屋簷可在 X/Z 各超出足跡最多 `0.25 m`，Collider 仍以足跡為準。
- 主要入口朝 Z-，入口淨寬 `1.4–1.8 m`、淨高 `2.0–2.4 m`。
- `HealthBarAnchor` 在最高點上 `0.6 m`；`SelectionAnchor` 位於地面中心；`CapturePoint` 位於主庭／入口內側。

## 外觀與輪廓

- 二層木石混合主樓，使用一個主屋頂與一個較低前廳，俯視時層級清楚。
- 不做完整城牆環繞，避免與「要塞主堡＋固定城牆」模式混淆。
- 主入口、旗幟、屋頂高低差必須從 55° 相機清楚可見。
- 造型穩重、寬大，輪廓大於敵方核心樓；不要堆太多小亭子。
- 可使用木柱、夯土／石台基、深色瓦頂；禁止特定朝代文字匾額。
- 預留前方集結空地的視覺方向，入口前不可有永久突出物阻礙單位。

## 隊伍色與狀態

- 旗幟、簷布、門楣布與側面盾徽面積合計 8–15%，使用可替換隊伍色。
- 我方預覽 `#4AA3D8`，敵方占領預覽 `#D94A45`。
- 不在 BaseColor 烘焙「我方」文字或藍色屋瓦；占領後只換材質即可成立。
- 提供 `Damage_25`、`Damage_50`、`Damage_75` Socket；第一版可只供掛點，不需破壞網格。
- 占領狀態由程式換旗色／材質，不直接更換整棟模型。

## 模型預算

- LOD0 15,000–30,000 triangles；LOD1 6,000–12,000；LOD2 1,500–3,500。
- 最多 4 個材質槽；2048 atlas。
- 建議碰撞：主體 2–4 個 Box Collider，屋簷不參與導航碰撞。
- 不使用整棟 Mesh Collider 建 NavMesh。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly 3D main stronghold/town center for a Unity 6 URP top-down RTS, ancient East-Asian-inspired but world-neutral. It represents a fortified administrative and recruitment center, not a full walled castle. Scale 1 meter per Unity unit, Y-up, Z-forward, bottom-center pivot. Exact gameplay footprint: 5.0 m wide on X and 7.0 m deep on Z. Visual height 4.5–5.5 m; eaves may exceed the footprint by at most 0.25 m. Main entrance faces Z- and is 1.4–1.8 m wide. Use a two-level wood-and-stone structure, one strong main roof, a lower front hall, clear entrance and readable roof hierarchy from a 55-degree RTS camera. Team-color flags and cloth cover 8–15%, switchable between #4AA3D8 and #D94A45. No surrounding wall, no text signs, logo, watermark, photorealism, existing IP, or fixed faction symbols. Provide simple box-collider plan, anchors and LODs.
```

## 驗收

- [ ] 5×7 m 足跡不可更改，入口朝 Z-。
- [ ] 960×540 可辨識主入口與主屋頂。
- [ ] 占領後只換隊伍色仍合理。
- [ ] Collider 不含屋簷，不阻擋入口集結區。

