# 兵種交戰模式規範

## 目的

兵種單位必須具有可查詢、可下令、可存檔的交戰模式。模式只控制「沒有明確攻擊指令時」的自主索敵與追擊；Player、AI、Scenario 與 Test 明確派送的 `AttackTargetCommand` 具有較高優先權，不會被反擊模式阻擋。

## 四種模式

| 模式 | 防守範圍 | 主動索敵 | 受擊反應 | 追擊規則 |
| --- | ---: | --- | --- | --- |
| 堅守陣地 `HoldGround` | 攻擊距離 × 0.5 | 是 | 可正常交戰 | 只追擊仍位於防守原點半徑內的目標。 |
| 普通 `Normal` | 攻擊距離 × 1.0 | 是 | 可正常交戰 | 只追擊仍位於防守原點半徑內的目標。 |
| 攻擊 `Aggressive` | 攻擊距離 × 1.5 | 是 | 可正常交戰 | 只追擊仍位於防守原點半徑內的目標。 |
| 反擊 `Retaliate` | 無主動索敵半徑 | 否 | 受到敵方傷害後鎖定傷害來源 | 可追擊該攻擊者；攻擊者死亡或失效後返回防守原點。 |

前三種模式只會選取存活、敵對且符合 `AttackProfile.TargetTags` 的目標。多個合法目標同時存在時，先選距離最近者；距離相同時以較小 `EntityId` 決定，確保 deterministic。

## 防守原點

`EngagementOrigin` 是自主追擊的 leash 中心：

1. 單位註冊時為出生位置。
2. 切換交戰模式時更新為當下位置。
3. 非 queue 的 Move order 接受後更新為移動目的地，並取消現有攻擊目標。
4. Stop／Hold order 更新為當下位置，並取消現有攻擊目標。
5. 自主目標離開 leash 後，單位清除目標並由 `CombatMovementCoordinator` 返回原點。

明確 `AttackTargetCommand` 標記為 `ManualOrder`，不套用自主索敵 leash；目標死亡或失效後才清除。反擊取得的目標標記為 `Retaliation`，與主動索敵取得的 `Proactive` 可在 snapshot、UI、AI 與測試中區分。

## 架構與資料流

```text
Player / AI / Scenario / Test
→ SetUnitEngagementModeCommand
→ CommandBus validator + handler
→ CombatSystem (mode、origin、target、reason、leash truth)
→ CombatMovementCoordinator (只把追擊／返回 intent 轉成 Movement order)
→ MovementSystem
→ Navigation adapter / Unity view
```

- `CombatSystem` 是 mode、target、origin 與 return flag 的 authoritative owner。
- `CombatMovementCoordinator` 不保存第二份單位狀態，只讀 Combat／Movement snapshots。
- HUD 只派送 command 並顯示 snapshot，不直接改模式。
- Prototype 透過 `UnityRtsInputAdapter.SetPointerBlocker` 保留 HUD pointer 區域，避免按姿態按鈕的同一點擊穿透到世界 selection／context command。
- `PrototypeEntitySaveData` 保存 mode、target reason 與 origin；舊版 v3 存檔缺少欄位時以 `Normal` 和存檔位置補值。
- Framework 新註冊 combatant 採向後相容的 `Retaliate` 預設；Playable Prototype 在 composition 註冊完成後明確設為 `Normal`。

## 公開 API

```csharp
combat.SetEngagementMode(new SetUnitEngagementModeCommand(
    selectedUnitIds,
    UnitEngagementMode.Aggressive));

combat.TryGetState(unitId, out CombatantSnapshot state);
// state.EngagementMode / DefenseRange / EngagementOrigin
// state.TargetReason / ShouldReturnToOrigin
```

事件：

- `UnitEngagementModeChangedEvent`
- `EngagementTargetChangedEvent`
- 既有 `DamageAppliedEvent` 會驅動反擊模式鎖定傷害來源。

## 驗收條件

1. 三種主動模式分別只在 0.5／1.0／1.5 倍攻擊距離內取得目標。
2. 反擊模式在未受擊前沒有目標，受擊後目標必須是傷害來源。
3. 明確攻擊指令在反擊模式仍可執行。
4. 主動追擊目標越過 leash 時清除目標並返回原點。
5. UI 可對已選我方單位切換四種模式，選取面板可看見目前模式。
6. Save／Load 後 mode、origin、target reason 與 combat target 一致。
7. Player 與 AI 不得各自實作另一套倍率或索敵規則。

## 已知限制

- 目前 hostile 判斷延續 Combat 的 faction 不同判斷；完整外交關係（Neutral／Hostile／War）尚未注入 autonomous targeting query。
- 目前是全量 snapshot 掃描，300 單位煙霧測試可接受；更大規模戰場應改用 spatial partition，但不得改變 deterministic 選擇規則。
- queue movement 目前不會立刻改寫正在執行的攻擊目標與防守原點；完整 combat order queue 將由後續 order scheduler 統一處理。
