# 建築美術規格：敵方要塞主堡

## 任務摘要

製作位於固定城牆內的要塞核心主堡。玩家破壞可修復城門、進入城內並攻擊此核心；核心生命值歸零後觸發占領，而不是把固定城牆摧毀。

## 技術條件

- Asset ID：`settlement.enemy-fortress`。
- 建議檔名：`BLD_FortressStronghold_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 精確足跡 X=`2.4 m`、Z=`3.6 m`；視覺高度 `4.0–4.8 m`。
- 屋簷可超出足跡最多 `0.18 m`；不可碰到現有城牆配置。
- 入口朝 Z-，寬 `1.0–1.3 m`、高 `1.9–2.2 m`。
- `CapturePoint` 放於建築前方 Z- `0.8 m` 或入口內，由整合者選一個固定位置。
- `HealthBarAnchor` 最高點上 `0.5 m`。

## 外觀與輪廓

- 高而緊湊的兩層門樓／主廳，作為城塞內的視覺焦點。
- 與我方 5×7 m 主堡相比，足跡更小、垂直感更強。
- 使用明顯主入口、上層觀察台與 2–4 面隊伍旗。
- 不附帶城牆或城門，避免模組重疊。
- 從城門向內看時，建築正面需清楚指引玩家攻擊目標。

## 占領狀態

- 旗幟、簷布與門楣提供 10–15% 可替換隊伍色。
- 初始敵方紅 `#D94A45`；占領後我方藍 `#4AA3D8`。
- 建築本體維持中性石木色，換旗後立刻可讀。
- 提供 `FX_Capture`、`FX_Hit_Center`、四個 `DamageSocket`。
- 不做永久「敵方」造型；不能因占領而必須更換整棟 FBX。

## 預算與碰撞

- LOD0 10,000–24,000 triangles；LOD1 4,000–10,000；LOD2 1,000–2,800。
- 最多 4 材質槽；2048 atlas。
- 2–3 個 Box Collider，維持精確 2.4×3.6 m 導航阻擋。

## 可直接給 AI 的提示詞

```text
Create a game-ready stylized low-poly compact fortress stronghold core for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired, world-neutral, designed to sit inside separate fixed walls. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivot. Exact footprint 2.4 m on X by 3.6 m on Z; visual height 4.0–4.8 m; eaves overhang no more than 0.18 m. Main entrance faces Z-. Use a tall compact two-level command hall/gate-tower silhouette, clear front entrance, upper lookout, and 2–4 replaceable faction flags. The same model must switch from enemy #D94A45 to friendly #4AA3D8 after capture. Do not include walls or gate. No text, fixed enemy symbols, logo, watermark, photorealism, or existing IP. Provide capture, hit, health-bar and damage sockets plus simple box-collider plan.
```

## 驗收

- [ ] 足跡完全容納於 2.4×3.6 m。
- [ ] 從城門方向可清楚看見入口與隊伍旗。
- [ ] 敵紅切換友藍後不需換模型。
- [ ] 沒有附帶牆段或擋住城內通道的突出物。
