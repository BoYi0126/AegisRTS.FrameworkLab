# 建築美術規格：經濟中心

## 任務摘要

製作日後內政用的經濟中心／市場型建築。這個建築目前尚未放入 Playable Prototype Graybox，因此本資產屬「第二優先」，先完成規格與概念，不應宣稱已完成遊戲整合。

## 技術條件

- Asset ID：`building.economy`。
- 建議檔名：`BLD_EconomyCenter_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 目標足跡 X/Z=`4.0–4.5 m`；高度 `3.5–4.5 m`。
- 入口朝 Z-，淨寬至少 `1.3 m`。
- `RallyPoint` 在入口前 Z- `1.0 m`；`InteractionPoint` 在入口；`HealthBarAnchor` 最高點上 `0.4 m`。
- 最終精確建築格與放置規則需等 Build System UI／地圖網格確認後鎖定。

## 外觀與輪廓

- 開放式前廊、貨箱／布棚與一個高位招牌框構成市場輪廓。
- 不使用文字招牌；招牌只保留純色圖形區或可替換圖示面。
- 屋頂低於主堡，寬度與開放入口強調經濟用途。
- 道具最多 4 組，每組需合併或易於關閉；避免大量小件造成 Draw Call。
- 8–12% 隊伍色放在棚布與旗帶，可切換藍、紅、中立。

## 預算與碰撞

- LOD0 8,000–18,000 triangles；LOD1 3,000–8,000；LOD2 700–2,000。
- 最多 3 材質槽，2048 atlas。
- 主體使用 2–3 個 Box Collider；入口、RallyPoint 與貨物出入方向不可被堵住。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly economy center/market building concept for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral. This asset is specified but not yet integrated into gameplay. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivot. Target footprint 4.0–4.5 m square, height 3.5–4.5 m, entrance facing Z- with at least 1.3 m clearance. Use a broad low roof, open front arcade, a few grouped crates, and cloth awnings to communicate economy/trade rather than military defense. Team-color awnings cover 8–12% and switch among #4AA3D8, #D94A45 and #C8A842. Any signboard must be blank or use a replaceable icon area, never generated text. No fortification, people, animals, watermark, logo, photorealism or existing IP. Provide simple box-collider plan and rally/interaction/health anchors.
```

## 驗收

- [ ] 文件與交付清楚標記為尚未遊戲整合。
- [ ] 看輪廓能理解是市場／經濟，而非主堡或兵營。
- [ ] 無生成文字，招牌可替換。
- [ ] 足跡與入口方向資料完整。
