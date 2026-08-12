# PlayablePrototype_01 — 總覽與範圍

## 決策

目前採用 **System-first／Playable-first** 路線：先用 primitive、顏色、文字與最小 UI，把既有 Framework 組成玩家可以親自完成的遊戲流程；正式世界觀、角色設定與 production art 延後到 Prototype 驗證通過後。

這不代表繼續擴張底層 Framework。Phase 01～16 與 Framework DoD 已完成，接下來的重點是產品層 composition、玩家操作、跨系統同步與可玩性。

## 現況判斷

目前已經具備：

- `Sandbox_RTS`：玩家可框選、移動、編隊、控制鏡頭與建立 control groups。
- `Sandbox_Combat`：Combat、Ability、Status、Projectile、Damage、Death 可自動運作。
- Economy、Recruitment、Building、Technology、Hero、Army、Faction、Settlement、Territory、Siege、AI、Scenario、HUD、Save／Replay 均有 Pure C# system、tests 與各自 acceptance sandbox。
- `VerticalSlice_01`：可以自動跑完收入、招募、軍團、野戰、AI 反攻、攻城、佔領與勝利。
- 原專案 Unity tests：EditMode 159/159、PlayMode 19/19；乾淨 package 專案 EditMode 6/6、PlayMode 3/3。

目前尚未具備：

- 同一個 scene 內由玩家親自完成全部流程。
- Selection／Movement／Combat／Economy／Army／Siege 對同一批 runtime entities 的完整同步。
- 可操作的 Recruit／Build／Research／Create Army／Siege／Capture UI。
- 與實際玩家狀態對戰的 AI，而不是獨立 acceptance simulation。
- 完整 Prototype state 的 Save／Load round-trip。
- 可直接交付玩家的 launcher、Windows build 與 onboarding。

因此目前是「Framework 可運作、整合驗收可自動完成」，尚不是「玩家可從開局玩到勝利的遊戲」。

## Prototype 目標

玩家在 `PlayablePrototype_01` 應可親自完成：

```text
New Game
→ 查看主城、資源與目標
→ 選取／框選單位
→ 移動與編隊
→ 由主堡直接招募新單位
→ 研究科技並製造攻城兵器
→ 建立 Hero-led Army
→ 擊敗野外敵軍
→ 應對 AI 反攻
→ 攻擊城門
→ 在守軍修復前進入內城
→ 攻擊主堡並轉移據點所有權
→ Victory
```

並可使用：

- Pause／Resume。
- Restart。
- 一個 Save slot 的 Save／Load。
- Debug overlay、command result、objective status 與錯誤提示。

## Definition of Playable

只有同時滿足以下條件才可稱為 Playable Prototype：

1. 核心流程由真實玩家 input 觸發，不使用自動 stage executor 代替玩家。
2. Player 與 AI 透過相同 Gameplay commands／validators 執行行為。
3. Unit 的 EntityId、Faction、Army、Position、Health、Selection 與 View 對應一致。
4. Recruit 完成後的新單位可以被選取、移動、戰鬥、加入 Army 並被儲存。
5. Combat death 會同步移除 Selection、Movement、Army、AI target 與 Unity View。
6. Gate destroyed 會刷新 navigation；守方修復至正值時重新封閉通路。固定城牆不可攻擊或摧毀。
7. 主堡核心被壓制後，Settlement owner change 由既有 Settlement／Siege transaction 完成；主堡視圖保留。
8. Objective 只根據 authoritative state 判定 Victory／Defeat。
9. Save／Load 後核心 state fingerprint 與可見狀態一致。
10. PlayMode 可由 New Game 跑到 Victory，Console 無未處理 Exception。

## 範圍內

- 單人 3D RTS Prototype。
- 一張 graybox map。
- 一個 player faction、一個 AI faction。
- 一座玩家主城、一個中立據點、一座敵方堡壘。
- Infantry、Archer、Cavalry、Siege Unit、Hero 等最小戰術角色。
- 兩種通用資源。
- 一條主堡直募／Tech／Army／Siege／Stronghold Capture progression；經濟建築是可選升級。
- Primitive meshes、team colors、selection highlight、health bar、文字 UI。
- Keyboard／mouse 操作。
- 一個可完成的 Scenario 與明確 Victory／Defeat。

## 範圍外

- 正式世界觀與 lore。
- Production character／building／environment art。
- 大量 Hero、完整 Campaign、大型世界地圖。
- Cinematic、配音、正式 BGM、完整 localization。
- Multiplayer、商城、Mod workshop。
- 在 Prototype 階段重寫已通過的 Framework systems。
- 為單一 Prototype 把世界觀規則硬寫進 package Runtime。

## 暫定中立內容

Prototype 使用獨立的 `PrototypeNeutral` Content Pack，ID 只描述 gameplay role：

```text
resource.material
resource.supply
unit.infantry
unit.archer
unit.cavalry
unit.siege
hero.commander
building.economy
building.recruitment
settlement.player-city
settlement.village
settlement.enemy-fortress
structure.gate
ai.prototype
scenario.prototype-conquest
```

暫定 `GameRuleSet`：

| Rule | Prototype 預設 | 原因 |
| --- | --- | --- |
| Morale | Disabled | 先驗證核心控制與戰鬥，不增加第一輪調參面向。 |
| Supply | Disabled | 避免阻擋第一個玩家循環；Army API 仍保留。 |
| Hero Capture | Disabled | Prototype 只驗證 Hero death／army membership。 |
| Hero Permanent Death | Disabled | 允許快速 Restart／重試。 |
| Population | Enabled | 驗證 Economy、capacity reservation 與 Recruitment。 |
| Fog of War | Disabled | 先確保所有狀態容易觀察與 debug。 |
| Destructible Walls | Disabled | 城牆是固定地圖物件；攻城只能走城門通道。 |
| Settlement Archetype | `fortified-city` | 主堡、固定城牆、可修城門與完整佔領交易。 |
| Gate Repair | Enabled | 守方經共用 command 修復；0 HP 回復時封閉 breach。 |
| Stronghold Recruitment | Enabled | 一般兵種不需要兵營；攻城兵器仍可要求科技。 |
| Capture Stronghold | Enabled | 主堡核心歸零代表壓制並轉移 owner，不銷毀主堡。 |

完整模式與第二種 `constructed-base` 據點規則見 `39_GameMode_據點與武將分配規則.md`。世界觀名稱仍可在 G01 之後替換，但這組據點 gameplay 是目前優先實作。

## 產品層架構

Prototype 留在 `Assets/AegisRTS/Demo/PlayablePrototype/`，使用既有 `AegisRTS.Demo` assembly，避免在 Framework package 內加入產品專屬 composition。

建議結構：

```text
Assets/AegisRTS/Demo/PlayablePrototype/
├─ Composition/
│  ├─ PlayablePrototypeBootstrap.cs
│  ├─ PrototypeSystemComposition.cs
│  └─ PrototypeTickDriver.cs
├─ Entities/
│  ├─ PrototypeEntityRegistry.cs
│  ├─ PrototypeUnitSpawnAdapter.cs
│  └─ PrototypeEntityViewBinder.cs
├─ Input/
│  └─ PrototypeCommandBridge.cs
├─ UI/
│  ├─ PrototypeHudQuery.cs
│  └─ PrototypeHudCommandSink.cs
├─ Persistence/
│  └─ PrototypeGameStateAdapter.cs
├─ Debug/
│  └─ PrototypeDebugOverlay.cs
└─ PlayablePrototype_01.unity
```

責任邊界：

- `PlayablePrototypeBootstrap` 只建立、連接與 dispose services，不擁有 domain rules。
- `PrototypeSystemComposition` 保存 system references 與明確 tick order，不複製 state。
- `PrototypeEntityRegistry` 維持同一 `EntityId` 在 systems／views 間的 registration lifecycle。
- `PrototypeUnitSpawnAdapter` 實作 `IUnitSpawnSink`，完成跨 system registration；失敗不可留下半註冊 entity。
- `PrototypeCommandBridge` 把 input／HUD intent 轉成既有 commands。
- `PrototypeHudQuery` 只聚合 query／snapshot；`PrototypeHudCommandSink` 不直接 mutation。
- `PrototypeGameStateAdapter` 負責 capture／restore ordering，不把 GameObject 寫進 save。

## 禁止事項

- 不建立同時包含 Combat、Economy、AI、Save、UI 邏輯的 God Manager。
- 不讓 UI 直接修改 resources、health、owner 或 objective state。
- 不為 Player 建一套、AI 再建另一套 command flow。
- 不讓 Transform 成為唯一 authoritative position。
- 不因 Prototype 需求直接修改 package；只有確認為通用 Framework defect，且先有 failing test，才修改 package Runtime。
- 不用自動 `VerticalSliceLoop` 冒充玩家完成流程；它可保留作 regression oracle。

## 完成後再做什麼

Playable Prototype 通過後再進入 G01～G04，決定正式遊戲名稱、世界觀、勢力、正式兵種與英雄。之後建立 Art Bible，再把 placeholder 逐步替換為 production assets。
