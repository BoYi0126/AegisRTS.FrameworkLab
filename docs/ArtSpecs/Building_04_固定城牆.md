# 建築美術規格：固定城牆模組

## 任務摘要

製作不可摧毀、固定於地圖的要塞城牆套件。城牆是地圖結構與導航邊界，不顯示生命值，也不因攻擊產生可破壞洞口；玩家應把攻擊集中於城門與主堡。

## 技術條件

- Asset ID：`structure.wall.fixed`。
- 套件檔名：`BLD_FortressWall_Modular_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 標準直牆：厚 X=`1.0 m`、高 Y=`4.0 m`、長 Z=`2.5 m`。
- 長段：`1.0×4.0×7.0 m`、`1.0×4.0×9.0 m`；可由標準段拼接，不一定另做網格。
- 轉角：外包絡 `1.0×4.0×1.0 m`，需與直牆無縫。
- 牆段旋轉 90° 後尺寸與接縫仍正確。

## 必要模組

- `Wall_Straight_2p5m`。
- `Wall_Corner_90`。
- `Wall_EndCap`。
- `Wall_GateConnector_Left`、`Wall_GateConnector_Right`。
- 可選：`Wall_Straight_1m` 作細部補縫。

## 外觀

- 石／夯土牆體、木製或石製垛口、少量屋瓦壓頂。
- 俯視 55° 時需看到牆頂與外側立面，不可只像高矩形盒。
- 細節以 0.5–1.0 m 節奏重複，避免每段有不同裝飾造成接縫。
- Gate Connector 必須與城門框高度、材質、屋瓦線一致。
- 固定牆本身不承載大量隊伍色；可在規定 Socket 掛小旗，但換勢力不必換牆體。
- 不生成破壞版與 HP Bar；可有局部舊化，但不得像已能被炸開。

## 拼接與 UV

- 所有接合面精確落在 0.5 m 網格。
- 端點法線、UV 與材質邊界一致，重複拼接不出現明顯亮縫。
- Texture tiling 需支援 2.5、7、9 m 長段；避免在長牆上把石紋拉伸。
- Lightmap UV 各模組獨立、無重疊。

## 預算與碰撞

- 標準段 LOD0 2,000–6,000 triangles；LOD1 800–2,500；LOD2 200–600。
- 每模組最多 2–3 材質槽，共用 2048 atlas。
- 每直牆 1 個 Box Collider；垛口與屋瓦不拆細碰撞。

## 可直接給 AI 的提示詞

```text
Create a seamless modular stylized low-poly fixed fortress-wall kit for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired but world-neutral. These walls are permanent map geometry and cannot be destroyed. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivots. Standard straight module is exactly 1.0 m thick, 4.0 m tall, and 2.5 m long; include a 90-degree corner, end cap, and left/right gate connector. All connections snap to a 0.5 m grid and must tile without normal, UV, roofline or material seams. Use simplified rammed-earth/stone wall, readable battlements and restrained dark roof tiles. Do not provide destroyed holes, health bar, text, logo, watermark, fixed faction marks, photorealism or existing IP. Use one simple box collider per straight segment, shared 2048 atlas, and LODs.
```

## 驗收

- [ ] 2.5 m 標準段連續拼 10 次無縫、無紋理拉伸。
- [ ] 轉角與 Gate Connector 的牆頂線一致。
- [ ] Collider 簡單且完全阻擋牆體。
- [ ] 沒有任何暗示牆可破壞的狀態資產。
