# 攻擊節奏、移動取消與拉打規範

本規範定義所有可戰鬥角色共用的攻擊時間軸，以及玩家／AI 如何用移動命令取消攻擊前搖或後搖。目標是讓弓兵可進行「射擊、移動、再射擊」的拉打操作，同時不因取消動畫而突破攻速上限。

## 1. 名詞與公式

- **攻擊間隔（Attack Interval）**：相鄰兩次攻擊開始之間的最短秒數；目前對應 `AttackProfile.CooldownSeconds`／`AttackIntervalSeconds`。
- **每秒攻擊次數（APS）**：`1 / Attack Interval`，不是動畫播放速度。
- **前搖／出手時間（Windup / Attack Point）**：攻擊開始到近戰傷害成立或遠程投射物生成的時間；對應 `WindupSeconds`。
- **命中／發射（Impact / Release）**：authoritative gameplay event。近戰在此套用傷害；遠程在此建立投射物，之後的移動不能收回已發射投射物。
- **後搖／恢復（Backswing / Recovery）**：`Attack Interval - Windup`。這段動畫可以被移動取消，但剩餘冷卻不會消失。
- **移動切換淡化（Move Cancel Blend）**：取消後由 Attack 切到 Move 的 presentation cross-fade 秒數，只影響觀感。
- 所有秒數使用 simulation seconds；`WindupSeconds` 必須介於 0 與 `AttackIntervalSeconds` 之間。

## 2. 共用取消規則

1. 單位在出手點以前收到非排隊 Move／Defend／Retreat 命令：立即取消本次攻擊，清除目標並開始移動；不得產生傷害或投射物，本次未成立的攻擊不消耗 Attack Interval，之後重新下 Attack 會重新跑完整 Windup。
2. 單位在出手點當下或以後收到上述命令：已成立的近戰傷害或投射物保持有效；只取消剩餘 Attack 動畫並快速切入 Move。
3. 取消後搖不清除 `AttackCooldownRemaining`。再次攻擊仍須等完整 Attack Interval，因此拉打只增加操作與走位空間，不增加理論 APS。
4. 非排隊 Stop／Hold 也會中止目前目標與尚未完成的前搖；單位維持原地。
5. Queue=true 的移動命令不取消正在進行的攻擊，等目前命令完成後才執行。
6. Player、AI、Scenario 與 army order 必須走共用 Command／Movement／Combat 流程。不得在 UI 或 Animator 內直接修改冷卻、傷害或投射物狀態。
7. Animator 的 Attack clip 依實際 clip event frame 對齊 gameplay `WindupSeconds`；動畫長度可以不同，但出手事件不得漂移。

## 3. Prototype 各角色初始參數

這些值是目前可玩原型的 world-neutral baseline，集中於 `PrototypeCombatTuning`。它們不是世界觀或最終平衡資料；正式 Content Pack 可在維持相同語意下覆寫。

| 角色 | 攻擊間隔 | APS | 前搖／出手點 | 可取消後搖 | Move Blend | 操作定位 |
|---|---:|---:|---:|---:|---:|---|
| Hero | 0.80 s | 1.2500 | 0.25 s | 0.55 s | 0.06 s | 反應最快的指揮角色 |
| Infantry | 0.95 s | 1.0526 | 0.30 s | 0.65 s | 0.07 s | 穩定近戰；出手前移動會取消揮砍 |
| Archer | 1.10 s | 0.9091 | 0.38 s | 0.72 s | 0.06 s | 拉打基準；箭離弦後可立刻移動 |
| Cavalry | 1.25 s | 0.8000 | 0.40 s | 0.85 s | 0.08 s | 較重的近戰節奏，保留衝擊感 |
| Siege | 2.20 s | 0.4545 | 1.05 s | 1.15 s | 0.12 s | 最長蓄力與恢復，不允許取消前搖仍出傷害 |

## 4. 動畫事件對齊

- 步兵 `AttackImpact`：frame 13 @ 30 FPS，即 clip event time 0.4333 s；Animator 使用 `AttackRate` 將事件縮放到 authoritative 0.30 s attack point。
- 弓兵 `ProjectileRelease`：frame 22 @ 30 FPS，即 clip event time 0.7333 s；Animator 使用 `AttackRate` 將事件縮放到 authoritative 0.38 s release point。
- `AttackRate = clip event time / authoritative windup`；目前限制在 0.25–4.0，避免無效資料造成極端播放速度。
- 其他角色在取得正式 Attack clip 後，必須記錄 event frame、FPS 與 event time，再用相同方式對齊；不得以完整 clip 播完時間代替 attack point。

## 5. 架構責任

- `AttackProfile`：authoritative cadence 與唯讀衍生值 `AttackIntervalSeconds`、`AttacksPerSecond`、`RecoverySeconds`、`MoveCancelableBackswingSeconds`。
- `CombatSystem`：決定前搖、命中／發射、冷卻與 `NotifyMoveOrder` 的取消結果。
- `MovementSystem`／`GameplayArmyOrderExecutor`：接受移動後通知 Combat；軍團 Move、Defend、Retreat 與個別 Move 使用相同取消語意。
- `PrototypeUnitAnimatorView`：只依 Combat／Movement snapshot 播放與 cross-fade；不得生成 gameplay 傷害或清除 cooldown。
- `PrototypeCombatTuning`：原型角色的集中數值來源，禁止在 composition、prefab builder 或 UI 再複製一套數字。

## 6. 驗收案例

- 出手前 0.01 s 下 Move：零傷害、零投射物、立即進入移動、attack cooldown 歸零；重新 Attack 必須重跑完整前搖。
- 出手事件後下 Move：傷害或投射物保留、Attack 動畫取消並切入 Move。
- 取消後立刻再次 Attack：冷卻未完成前不能再次命中／發射。
- 連續 Attack-Move：長時間平均 APS 不得高於 profile 的 `AttacksPerSecond`（允許 frame delta 誤差）。
- 個別單位與整個軍團執行相同行為；AI 透過同一 command path 得到相同結果。
- 步兵 Move 畫面必須保持 Humanoid 直立，Head 高於雙腳且 renderer 主要高度軸為世界 Y。

## 7. 後續平衡流程

1. 先調整 Attack Interval，決定輸出頻率與手感。
2. 再調整 Windup，決定玩家需要承諾攻擊多久；不得大於 interval。
3. Recovery 由兩者相減，不另存重複狀態。
4. 最後微調 Move Blend，只改善視覺，不用來掩蓋 gameplay event 漂移。
5. 每次改值必須同步更新本表、相關 Content／tuning source、automated tests 與 `DevelopmentProgress.md`。
