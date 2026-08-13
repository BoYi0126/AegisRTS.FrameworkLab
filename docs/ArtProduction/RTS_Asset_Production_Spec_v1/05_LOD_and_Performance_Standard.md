# 05 — LOD and Performance Standard

- Specification Version：1.0
- Status：`PROPOSED PRODUCTION STANDARD`；現有 thresholds另列為 `CURRENT`

## 原則

LOD解決遠距成本，不是降低LOD0品質的理由。所有級別先保留Unit Class、weapon、armor weight與Team Color primary silhouette，再減少看不見的曲面、內部面與secondary details。

## LOD 階級

| Level | 目的 | Geometry target（相對LOD0） | Material／Texture | Bones |
| --- | --- | ---: | --- | --- |
| LOD0 | Close、marketing／inspection、完整deformation | 100% | full maps；Standard 2K，Hero 2K/4K | full production skeleton |
| LOD1 | Medium、主要gameplay近景 | 45–60% | 同atlas優先；可降一級mip bias | 保留主要deform/twist |
| LOD2 | Normal RTS distance／大量單位 | 15–30% | 1K有效解析度；可合併小材質 | 移除finger／minor accessory bones |
| LOD3 | Far、群集輪廓 | 5–12% | 512–1K；1 material優先 | 只保留主要body／weapon bones |
| Optional Impostor | 極遠或策略地圖 | baked cards／octahedral impostor | 256–512 atlas | none |

比例是起點；實際以pixel coverage、silhouette與profiling決定。

## 切換條件

- `REQUIRED` 以screen-relative size設置，不以固定世界距離單獨決定。
- `RECOMMENDED` 先以128 px=L0、64 px=L1、32 px=L2、16 px=L3的測試點調整；Unity LODGroup thresholds須由capture校正。
- `CURRENT` Infantry／Archer使用 `0.04 / 0.012 / 0.003` 三級後Cull；這是已可玩的Prototype設定，但沒有LOD3或Impostor，也尚無正式pixel-calibration報告。
- 任何切換不可造成高度、grounding、weapon/socket或team-color大跳；相鄰LOD截圖overlay的主要輪廓偏差應小於角色高度3%。

## 簡化順序

1. 移除永遠不可見的內部面與micro bevel。
2. 合併tertiary details進Normal／BaseColor。
3. 降低圓柱segment、布摺與背面小配件。
4. 保留helmet crest、shoulder mass、shield、bow、quiver、large weapon。
5. LOD2以前不得移除整把武器／盾；LOD3也需保留可讀proxy。
6. 簡化skin weights與bones時，重新驗證extreme poses及socket位置。

## 材質、Draw Call 與動畫

- Standard unit LOD0最多2個主要material slots；Hero／Special偏離需profile證據。
- 共享shader／atlas優先；不要為藍、紅、綠各複製mesh或material。
- GPU Instancing／SRP Batcher相容性由Unity Frame Debugger確認；`MaterialPropertyBlock`不得意外破壞預期batching而未量測。
- 遠距Animator可降低update frequency、關閉minor bones或使用pose cache；不能修改authoritative movement/combat tick。
- Culling、occlusion、shadow distance與animation culling必須在target hardware Player profile，Editor數字不能冒充正式通過。

## Acceptance

- [ ] LOD0～3與optional impostor決策有triangle／renderer／material／bone報告。
- [ ] 128／64／32／16 px capture沒有class identity pop。
- [ ] Weapon／shield／bow在規定LOD仍存在。
- [ ] LOD切換不漂浮、不穿地、socket不跳位。
- [ ] 100／300單位Player profile記錄CPU、GPU、batches、SetPass、Animator與memory。

