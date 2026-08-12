# PlayablePrototype_01 — 架構與維護

## 責任邊界

`PlayablePrototype_01` 是產品整合層，不是新的 Framework façade。它負責把既有 systems 組成一局可操作遊戲，但 gameplay truth 仍由 package Runtime 的 domain systems 擁有。

| 路徑／類型 | 責任 | 不應負責 |
| --- | --- | --- |
| `PrototypeSystemComposition` | 建立 services、router、query adapter、固定 tick、spawn／death／restore orchestration。 | 在 HUD 或 MonoBehaviour 內複製 domain rules。 |
| `PlayablePrototypeBootstrap` | Unity scene、input、selection、camera、primitive views、HUD、session lifecycle。 | 直接修改 resource、HP、owner、objective。 |
| `PrototypeEntityRegistry` | 同一 `EntityId` 的 definition、faction、spawn、alive metadata。 | 取代 Movement／Combat／Faction authoritative snapshots。 |
| `PrototypeNavigationAdapter` | 純 C# deterministic navigation，供 composition／domain tests 使用。 | Unity scene obstacle baking。 |
| `PrototypeUnityNavigationAdapter` | runtime `NavMeshSurface`／`NavMeshAgent`、path acceptance、gate breach 後 rebuild 與目的地重派。 | 擁有 Movement／Combat gameplay truth。 |
| `PrototypeGameStateAdapter` | `GameStateCoordinator` envelope、JSON extension DTO、checksum／compatibility、single slot、fingerprint。 | 序列化 Unity object reference。 |
| `PrototypeNeutral` | world-neutral units、heroes、buildings、tech、resources、scenario、theme。 | 正式世界觀或 production balance。 |
| package Runtime | Commands、validators、systems、events、queries、snapshots。 | 引用 Prototype／產品專屬名稱或 Unity scene。 |

## Command 與狀態流程

Player input、HUD、AI 與 tests 應走同一條 mutation path：

```text
Intent
→ ICommand
→ CommandBus validator
→ Domain router / system
→ Authoritative state mutation
→ Domain event / snapshot / query
→ HUD and Unity views refresh
```

Siege 的 Prototype commands 也是 CommandBus commands；Attack／Repair／Enter／Capture 在 dispatch 前驗證 faction、repairable tag、gate、area 與 capture conditions。`fortified-city` 的牆段不註冊到 SiegeSystem，因此不是可受擊 runtime structure。UI 只顯示 accepted／rejected 與原因。

Selection 驅動的 command panel 屬於 Presentation policy：`SelectionService.Revision` 表示 selected set 是否真的改變，`SelectionCommandContextResolver` 將 descriptors 投影成 Domestic／UnitSettings／Siege，Bootstrap 只負責映射到 `PrototypeCommandTab`。完整規則見 [`44_Selection_Driven_Command_Panel_選取驅動指揮面板.md`](44_Selection_Driven_Command_Panel_選取驅動指揮面板.md)。

## 固定 Tick Order

`PrototypeSystemComposition.Tick` 的順序不可任意調換：

```text
Economy
→ Buildings
→ Technologies
→ Recruitment
→ Navigation
→ Movement
→ Combat position synchronization
→ Combat
→ CombatMovementCoordinator（追擊／返回 intent）
→ death cleanup
→ Siege
→ Scenario
→ AI cadence
```

原因：Recruitment 必須先完成 atomic spawn；Navigation 必須先提供位置；Combat 讀取同步後的位置；death cleanup 必須在同 tick 移除 Movement／Army／View eligibility；Scenario 讀 authoritative siege／owner facts；AI 最後讀取本 tick 已完成的世界狀態並派送下一批 commands。

Pause、Victory、Defeat 時 Bootstrap 不再呼叫 simulation tick。Restart／Return to Menu 必須先 dispose handlers／subscriptions，再銷毀 views、selection、input 與 camera session objects。

## Entity Lifecycle

### Spawn

同一個 unit 只建立一個 `EntityId`，原子式註冊順序為：

1. Registry／Faction metadata。
2. Navigation／Movement。
3. Combat。
4. Hero／Army eligibility（適用時）。
5. Unity selectable／view／health bar。

任一步失敗時不得留下只有部分 components 的 entity。Recruitment 必須經 `IUnitSpawnSink`，不能從 HUD instantiate 後再補資料。

### Death

`UnitDiedEvent` 觸發統一 cleanup：

1. Army membership 與 commander reference。
2. Movement／navigation registration。
3. Registry alive／entity entry。
4. Selection eligibility 與 Unity view。
5. 其他 units 的 invalid target 由 system snapshot／validation 排除。

本次也修正 package `ArmySystem.UnregisterMember`：已分配 member 現在會從 Army snapshot、Hero ArmyId 與 Combat ArmyId 一致移除；若死亡者是 commander，commander reference 同步清除。

## Position Authority

Gameplay position 由 navigation snapshot 回報並同步到 Movement／Combat read state；Unity Transform 是 presentation，不是 authoritative state。Bootstrap 每 frame 只把 snapshots 投影到 primitive views。

純 C# tests 使用 `PrototypeNavigationAdapter` 的 deterministic simulation；Unity scene 由 `PrototypeUnityNavigationAdapter` 使用 runtime-baked NavMesh。封閉 Gate 會讓內院 path incomplete，破門事件會停用 Gate、重建 surface、重新綁定 agents 並重派目的地；守方從 0 HP 修復 Gate 時會重新啟用 blocker、封閉 breach 並再次重建 navigation。共同約束如下：

1. 保持 `INavigationAdapter` 介面與 EntityId registration。
2. 明確定義 NavMeshAgent → Movement snapshot → Combat position 的同步時點。
3. Save 仍只保存純資料位置，不保存 NavMeshAgent／Transform。
4. 維持 gate closed unreachable、breach refresh、gate open reachable 的 PlayMode regression。
5. 不可讓 Unity view position 反向覆蓋 save／combat state而沒有測試。

## Save／Load

### 保存內容

Prototype save 是純 JSON DTO，涵蓋：

- schema、framework／content／scenario compatibility metadata。
- elapsed clock 與 deterministic random state。
- faction／settlement／territory owner。
- resources、population、production。
- completed buildings／technologies。
- Building／Technology／Recruitment active queues 與 remaining time（成本與人口已在 Economy snapshot 反映，不重複扣款）。
- entities 的 ID、definition、faction、position、HP、movement status／orders、combat target／cooldown。
- Hero／Army membership、commander、formation、morale、supply、current order。
- AI cadence／decision counters／last decision 與 deterministic random state。
- siege／gate／stronghold HP、gate repair countdown／entered areas。
- objective facts、Victory／Defeat state。

不包含 GameObject、Transform、Material、MonoBehaviour 或其他 Unity object。

### Restore Order

```text
Validate metadata and parse JSON
→ construct definitions and empty systems
→ restore faction / settlement / territory
→ restore progression / entities / armies
→ restore siege and gate NavMesh state
→ restore economy / population / active queues
→ restore units / heroes / navigation / combat
→ restore movement / combat / AI runtime state
→ restore objective / clock / random
→ rebuild Unity views and selection eligibility
```

Load 採用新 composition 成功建立後才替換現有 session的方式；corrupted 或 incompatible save 會回報可讀錯誤，不應先破壞目前 session。

active queues 與 mid-action state 已支援。Restore API 的契約是「工作已付款／人口已保留」；先還原 Economy，再呼叫 `RestoreQueuedJob`，不得重新走 Request 扣款。PlayMode 與 EditMode 均驗證 Save → mutate → Load 不重複扣款、remaining time 不丟失，且 spawn 失敗會回滾成本、人口、EntityId 與 random draw。

## Content 與世界觀隔離

Prototype 專屬 ID、display name、balance 與 Scenario 只存在 `Assets/AegisRTS/Content/PrototypeNeutral` 或 Demo layer。Package Runtime 不得新增 `PrototypeNeutral`、三國、Fantasy 或正式產品專屬字串。

模式與據點的正式規格見 `39_GameMode_據點與武將分配規則.md`。新增 Content 時至少驗證：

- unique Definition IDs。
- typed references 與 prefab asset IDs 存在。
- costs、stats、population 合法。
- technology graph 無 cycle。
- required tags 齊全。
- Scenario 的 factions、settlements、units、gate 與 objectives 都能解析。

Theme 只能改顯示資料，不可改 layout ownership 或 gameplay state。

## 測試與 Build Gate

任何修改 Prototype runtime／scene／Content／package bug fix，至少執行：

1. FrameworkLab 全部 EditMode tests。
2. FrameworkLab 全部 PlayMode tests。
3. 若 package Runtime 有變更，乾淨 package validation project 的 EditMode／PlayMode。
4. `git diff --check`、JSON parse、`.meta`／GUID、asmdef dependency、package world-hardcode 靜態檢查。
5. Windows Development Build。
6. 啟動 executable 並掃描 Player log 的 compile／unhandled／missing-reference errors。
7. 有 gameplay／UI／input 改動時，執行 `37` 的人工 A～E；未執行必須寫 `NOT RUN`，不能用自動測試代替。

效能 smoke 的 300 active units／120 ticks／5 秒是 regression ceiling，不是 production hardware frame budget。正式 target hardware、rendering、NavMesh 與 30-minute wall-clock profiling 需另立 G09／G11 gate。

## 變更檢查清單

每次修改前後確認：

- 是否仍為一個 EntityId、單一 state owner、共用 CommandBus。
- 是否新增 duplicate handler／subscription 或沒有 dispose 的 Unity object。
- 是否改變 tick／restore order；若有，文件與 tests 是否同步。
- 是否把 gameplay rule 寫進 Bootstrap／HUD／View。
- 是否把 Unity object 放入 save DTO。
- 是否把 Prototype／世界觀 hardcode 放進 package Runtime。
- 是否新增 public API／JSON field／save schema；相容性與 migration 是否記錄。
- 是否更新 `DevelopmentProgress.md` 的 Baseline、Scope、Changed、Behavior、Architecture、API／Data、Tests、Acceptance、Completed／Not Completed、Risks、Git、Next。

## 建議後續順序

1. 先完成 executable 人工通關與小解析度 UI 驗收。
2. 依人工結果改善 HUD interaction、selection／camera feel、NavMesh path feedback 與正常 Defeat flow。
3. Prototype gate 全 PASS 後建立 checkpoint，再進 G01／G02 世界觀；production art 依舊延後到玩法與尺寸需求穩定。
