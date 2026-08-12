# 建築美術規格：可破壞、可修復城門

## 任務摘要

製作要塞入口的城門模組。城牆不可破壞，城門可被攻城兵器打破，也可由守方修復。資產必須支援 Closed、Damaged、Breached、Repairing 四種視覺狀態。

## 技術條件

- Asset ID：`structure.gate`。
- 檔名：`BLD_FortressGate_A_v001`。
- 1 Unity Unit = 1 公尺；Y-up、Z-forward；底面中心 Pivot；Scale 1。
- 目前 Graybox 外包絡：X=`1.0 m` 厚、Y=`3.2 m` 高、Z=`3.0 m` 寬；X 為穿越方向，門面朝 X-／X+。
- 正式門樓裝飾總高可到 `4.0 m`，但可通行開口需維持 Z 寬 `2.4–3.0 m`、Y 高至少 `2.6 m`。
- 門扇／柵門關閉時形成導航阻擋；Breached 時阻擋物必須能被程式停用。
- 根 Pivot 不動；開啟、破壞與修復只改子物件或 Animator 狀態。

## 模組組成

```text
PF_FortressGate
├─ Frame_Static
├─ Door_Left 或 Door_Main
├─ Door_Right（若雙扇）
├─ Debris_Breached
├─ TeamColorFlags
├─ FX_Hit_Center
├─ FX_Repair
└─ HealthBarAnchor
```

- `Frame_Static` 與固定城牆風格一致，不因破門消失。
- 門扇採厚木板加少量暗鐵橫條；板縫要大、遠距可讀。
- 不在門上放漢字或固定徽記。
- 門洞方向、碰撞方向需在交付圖中標出。

## 狀態表現

- Closed：完整門扇，隊伍旗顯示守方顏色。
- Damaged：裂痕、歪斜木條或掛載傷痕 Decal；不可只把整門變黑。
- Breached：門扇隱藏／倒下，入口清楚打開；碎片不應形成新的導航阻擋。
- Repairing：門框／門扇上顯示簡單支架或 FX，不要求倒播破壞動畫。
- Repaired：回到 Closed，生命值由遊戲系統決定。

## 預算

- LOD0 5,000–12,000 triangles；LOD1 2,000–5,000；LOD2 500–1,200。
- 最多 3 材質槽；與城牆共用 2048 atlas 優先。
- Collider：門框 2 個 Box，門扇 1 個可啟用／停用 Box；碎片不碰撞或只作非導航碰撞。

## 可直接給 AI 的提示詞

```text
Create a modular game-ready stylized low-poly fortress gate for a Unity 6 URP top-down RTS. Ancient East-Asian-inspired, world-neutral. Scale 1 meter per unit, Y-up, Z-forward, bottom-center pivot. Current gameplay envelope: 1.0 m thick on X, 3.0 m wide on Z, 3.2 m tall; decorative frame may reach 4.0 m. The passage runs through X and needs a 2.4–3.0 m clear width and at least 2.6 m clear height. Separate static frame, destructible wooden door leaves, breached debris, team-color flags, hit/repair/health anchors. Support Closed, Damaged, Breached and Repairing visual states; in Breached state the passage must be visibly and physically clear. Team color switches between #D94A45 and #4AA3D8. No text, logo, watermark, photorealism, fixed faction symbol, or existing IP. Use simple modular box colliders and do not make debris block NavMesh.
```

## 驗收

- [ ] 穿越方向為 X，門寬沿 Z，文件與模型一致。
- [ ] Breached 後視覺和導航皆明確可通行。
- [ ] Frame 不隨門扇破壞消失。
- [ ] 修復不依賴倒播破壞動畫。
