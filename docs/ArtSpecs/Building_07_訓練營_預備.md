# 建築美術規格：訓練營（預備資產）

## 任務摘要

製作 `building.recruitment` 的預備美術規格。玩家目前優先採用「主堡直接訓練兵種」的城池模式，因此訓練營不是第一批必做資產；它保留給未來類世紀帝國的 constructed-base 模式。可先做 L1 概念，不建議早於主堡、城門與主要兵種製作 L3。

## 技術條件

- Asset ID：`building.recruitment`。
- 建議檔名：`BLD_RecruitmentCamp_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 目標足跡 X=`4.5–5.0 m`、Z=`4.5–5.5 m`；高度 `3.5–4.5 m`。
- 入口朝 Z-，淨寬至少 `1.4 m`。
- `RallyPoint` 位於入口前 1.2 m；`SpawnPoint` 位於入口內側；二者連線不可被 Collider 阻擋。
- 精確建築格需等 constructed-base 放置規則開發時再次鎖定。

## 外觀與輪廓

- 主營房、開放操練棚與武器架構成軍事訓練輪廓。
- 不做高塔，不比主堡高；也不做完整圍牆。
- 武器架只用大型盾、槍束等簡單大形，不生成大量細小武器。
- 8–15% 可見面積為隊伍色，放在營旗與棚布。
- 不在招牌或旗幟生成文字。

## 預算

- LOD0 10,000–22,000 triangles；LOD1 4,000–9,000；LOD2 900–2,500。
- 最多 3 材質槽；2048 atlas。
- 2–4 個 Box Collider；SpawnPoint、RallyPoint 與入口無阻擋。

## 可直接給 AI 的提示詞

```text
Create a stylized low-poly 3D recruitment camp/barracks concept for a future constructed-base mode in a Unity 6 URP top-down RTS. This asset is specified but not yet required by the current fortified-city mode. Ancient East-Asian-inspired, world-neutral. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivot. Target footprint 4.5–5.0 m wide by 4.5–5.5 m deep, height 3.5–4.5 m, entrance facing Z-. Use one main barracks hall, an open training awning, and a few large readable weapon racks. It must look military but remain clearly smaller and lower than the main stronghold. Team-color flags/awning cover 8–15%, switchable between #4AA3D8 and #D94A45. No full wall, tower, generated text, logo, watermark, photorealism, or existing IP. Mark spawn, rally, interaction and health anchors.
```

## 驗收

- [ ] 明確標記為 constructed-base 模式的延後資產。
- [ ] 與主堡、經濟中心的輪廓與用途不同。
- [ ] SpawnPoint 到 RallyPoint 路徑無碰撞。
- [ ] 沒有文字、固定勢力徽記或不明素材。

