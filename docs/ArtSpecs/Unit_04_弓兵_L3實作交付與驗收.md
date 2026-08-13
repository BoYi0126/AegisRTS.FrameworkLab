# Unit 04 弓兵 L3 實作交付與驗收

## 實作狀態

- 狀態：`Prototype L3 Completed`。
- Content ID：`unit.archer`。
- Runtime Prefab ID：`PF_Unit_Archer`。
- 箭矢 Prefab：`PRJ_Arrow_Basic_v001`。
- 來源層：`ArtSource/Units/Archer/CHR_Archer_A/v001`。
- Unity 層：`Assets/AegisRTS/Content/Shared/Art/Units/Archer`。
- 發布限制：角色身體沿用步兵原型交付的衍生幾何；原始素材權利與正式動畫品質未完成人工 release review，因此只可視為可玩的 Prototype，不宣稱 production-ready。

## 已交付內容

- 可重建 Blender 5.2 腳本、`.blend`、Master FBX 與 SHA-256 manifest。
- 1.78 m Humanoid 弓兵；盾牌與短劍已移除，新增弓、弓弦、箭袋與背箭。
- LOD0／LOD1／LOD2：3,344／1,280／542 triangles。
- `Idle`、`Move`、`Attack_Ranged`、`Hit`、`Death` 五個 In Place Clips；Root Motion Off。
- `Attack_Ranged` 的 `ProjectileRelease`：30 FPS、frame 22、0.733333 秒。
- 獨立 0.82 m 箭矢；Unity local Z+、中心 Pivot、無 Collider。
- Unity Humanoid Avatar、Animator Controller、LODGroup、Team Color、Selection／Health／Projectile anchors。
- ContentPack 已由 placeholder 改綁 `PF_Unit_Archer`；玩家與敵人使用同一模型，以 MaterialPropertyBlock 套用陣營色。

## Runtime 行為

```text
CombatSystem（authoritative）
  → ProjectileLaunchedEvent
  → PrototypeProjectileVisualController（presentation only）
  → 從 Socket_Projectile 租用箭矢
  → Z+ 朝飛行方向、帶小幅拋物線
  → 到達後歸還 ObjectPool 並播放 pooled impact flash
```

- 箭矢與命中特效不計算傷害、不碰撞、不推進 Combat Tick。
- 命中與 HP 仍完全由 `CombatSystem` 決定；Animation Event 只提供可觀測的視覺 timing。
- 找不到 source socket／target view 時會退回 event 的世界座標，避免 presentation 缺件阻塞 gameplay。

## Unity 重建

從 Unity 選單執行：

```text
Tools/AegisRTS/Art/Rebuild Archer L3 Prefabs
```

或 batch method：

```text
AegisRTS.Editor.ArcherL3PrefabBuilder.BuildAndValidate
```

Builder 會驗證 Humanoid、五個 Clip、事件、三段 LOD triangle count、角色 Y-up 高度、弓／箭袋存在、盾／劍不存在，以及箭矢尺寸、Z+、中心 Pivot、無 Collider。

## 驗收結果（2026-08-13）

- Blender 5.2 reproducible build：PASS。
- Unity Archer builder：PASS，Humanoid `isHuman=True`、`isValid=True`、5 clips。
- Archer targeted PlayMode：3/3 PASS（Prefab／動畫事件／Root Motion、pooled projectile／impact、近距離姿勢／陣營辨識）。
- 近距離人工畫面檢查：正面、側面、背面、4 個 Attack samples，共 7 張；初版攻擊下半身折疊已修正並重新驗證。
- 完整 EditMode：177/177 PASS。
- 完整 PlayMode：39/39 PASS。
- Windows Development Build：PASS，BuildReport 187,657,769 bytes；啟動 10 秒 process responding，Player log error scan 0 hits。

## 尚未完成／正式美術替換條件

- 弓弦尚非獨立拉伸／放弦骨架動畫；`ProjectileRelease` 已正確觸發，但「弦放開相差不超過 1 frame」仍需正式動畫人工驗收。
- 本版材質是 world-neutral 原型色，不是最終 PBR 貼圖組。
- 仍需專案擁有者在實際遊戲中主觀確認攻擊辨識度、箭速、弧度與命中特效手感。
- 正式替換不得更改 `PF_Unit_Archer`、Animator parameters、`Socket_Projectile`、`ProjectileRelease` 與 Z+ arrow contract；如此可不改 gameplay code。

