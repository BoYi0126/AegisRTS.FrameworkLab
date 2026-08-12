# 建築美術規格：中立村莊

## 任務摘要

製作目前 `Neutral Village` Graybox 的替換資產。它是可占領／互動的中立據點，不是軍事主堡；應以生活與資源感和要塞建築區分。

## 技術條件

- Asset ID：`settlement.village`。
- 建議檔名：`BLD_NeutralVillage_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 整體足跡 X=`4.0 m`、Z=`4.0 m`；高度 `3.0–3.8 m`。
- 可由 2–3 間小屋與中央地標組成，但必須作為單一可選取據點，外包絡不可超過 4×4 m。
- 主入口／道路接口朝 Z-，至少留 `1.2 m` 清楚通道。
- `CapturePoint` 在中央空地；`HealthBarAnchor` 最高點上 `0.4 m`。

## 外觀與輪廓

- 一間主屋、較小棚屋、資源堆／井／小旗中選兩種，不要塞滿所有元素。
- 屋頂低於主堡，無大型塔樓與垛口。
- 生活色彩偏暖木、土牆、稻草或深瓦；輪廓親和、非軍事。
- 中立時使用黃褐 `#C8A842` 小旗；占領後可切換藍或紅。
- 旗幟面積 6–10%；隊伍色只是所有權提示，不壓過村莊本體。
- 不生成農民群像或動物，避免占領狀態與動畫管理複雜。

## 預算與碰撞

- LOD0 8,000–18,000 triangles；LOD1 3,000–8,000；LOD2 700–2,000。
- 最多 3 材質槽；2048 atlas。
- 使用 2–4 個 Box Collider 包住各棟主體；中央 CapturePoint 不可被 Collider 擋住。

## 可直接給 AI 的提示詞

```text
Create a compact game-ready stylized low-poly neutral village capture point for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral, civilian and non-military. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivot. Entire group must fit within an exact 4.0 m by 4.0 m footprint and be 3.0–3.8 m tall. Use one main house, one smaller shed, and at most two readable village props such as a well, resource stack, or simple flag. Leave a clear 1.2 m approach from Z- to a central capture point. Neutral color #C8A842, switchable after capture to #4AA3D8 or #D94A45, covering only 6–10% of visible area. No fortification, tower, battlements, people, animals, text, logo, watermark, photorealism or existing IP. Provide simple box colliders and capture/health anchors.
```

## 驗收

- [ ] 全部模型含突出物位於 4×4 m 包絡。
- [ ] 與主堡、要塞一眼可由非軍事輪廓區分。
- [ ] 中立、藍、紅三色切換不需換模型。
- [ ] CapturePoint 與入口路徑無碰撞阻擋。
