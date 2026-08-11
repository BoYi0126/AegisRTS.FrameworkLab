# PlayablePrototype_01 — 現況、缺口與驗收矩陣

## 狀態定義

- `Completed`：功能已有 runtime、tests，且在現有 Sandbox／Vertical Slice 可驗證。
- `Partial`：底層功能完成，但尚未接進同一個玩家可操作 Prototype。
- `Missing`：Prototype 所需的產品層 adapter、UI、scene 或 lifecycle 尚不存在。
- `Deferred`：刻意延後，不阻擋 System-first Prototype。

## 現況矩陣

| 能力 | 現有證據 | 狀態 | 尚缺內容 | 優先級 |
| --- | --- | --- | --- | --- |
| Core IDs／Clock／Random／Command／Event | Core runtime、EditMode tests、debug summaries | Completed | Prototype composition 需共用單一 clock／seed／buses。 | P0 |
| Content Pack load／validation | 三套 Demo packs、package consumer tests | Completed | 缺 `PrototypeNeutral` 完整 pack 與 scenario binding。 | P0 |
| Camera／Selection／Control Groups | `Sandbox_RTS` 可操作 | Partial | 尚未與同 scene Combat、Economy、Army、Siege entities 整合。 | P0 |
| Movement／NavMesh／Formation | 50-unit Sandbox、PlayMode tests | Partial | 需要與 Combat position、death cleanup、gate navigation refresh 統一。 | P0 |
| Combat／Ability／Status／Death | `Sandbox_Combat` 與 tests | Partial | `Sandbox_RTS` 的 Attack 目前只記錄 command；缺真實 selection-to-combat flow。 | P0 |
| Unit spawn lifecycle | Recruitment sink、Vertical Slice composition | Partial | 缺產品層原子 registration、View 建立與 rollback。 | P0 |
| Economy／Population／Production | Economy systems／tests／sandbox | Partial | 缺玩家 HUD、真實 account 與同 scene tick。 | P0 |
| Building／Technology／Recruitment | Command routers、tests、sandbox | Partial | 缺可點擊 UI、command feedback、spawn 到玩家場景。 | P0 |
| Hero／Army／Orders | Army runtime、tests、sandbox | Partial | 缺 selection-to-create-army UI 與死亡／load membership lifecycle。 | P0 |
| Faction／Settlement／Territory | Runtime、capture tests、sandbox | Partial | 缺 graybox map 中的 authoritative ownership／visibility presentation。 | P0 |
| Siege／Gate／Capture | Siege runtime、tests、sandbox、sample | Partial | 缺玩家 gate targeting、navigation coupling、objective feedback。 | P0 |
| Utility AI | AI runtime、tests、autonomous sandbox | Partial | 缺對 Prototype 真實 state 的 world query／executor。 | P0 |
| Scenario／Objective／Victory | Scenario runtime、4 modes、Vertical Slice | Partial | 缺玩家行為驅動 facts／signals 與 session end screen。 | P0 |
| HUD／Theme／Notification | HUD model、presenter、theme sandbox | Partial | 缺 Prototype aggregate query、command sink 與完整 panels。 | P1 |
| Pause／Restart／Settings | `GameSessionController`、Vertical Slice UI | Partial | 缺完整 Prototype teardown／rebuild 與 menu flow。 | P1 |
| Save／Load／Replay | Persistence runtime、tests、sandbox | Partial | 缺整合全部 Prototype systems 的 capture／restore adapter。 | P1 |
| Automated Vertical Slice | Three Kingdoms／Fantasy 皆 PASS | Completed | 自動 stage progression 不能代替玩家操作，只作 regression oracle。 | P0 constraint |
| Single player-driven full loop | 無單一 scene／test | Missing | 需要 PP00～PP08。 | P0 |
| Windows playable build | 尚未建立正式 Prototype build | Missing | PP08 建立 Development Build smoke。 | P1 |
| Production art／animation／audio | 目前為 primitive／IMGUI | Deferred | G08 與 Art Bible 後處理。 | P3 |
| World／Faction lore | Demo data only | Deferred | Prototype 完成後進入 G01／G02。 | P3 |
| Tutorial／Localization／Accessibility | Framework／Demo 非產品完成狀態 | Deferred | G10／G11。 | P3 |

## 最先處理的阻塞缺口

### 1. Entity lifecycle

同一單位必須只使用一個 `EntityId`，並明確追蹤它已註冊到哪些 systems。Spawn／death／load 任何一步失敗時，不可留下：

- 可被選取但不能移動的 View。
- 有 Combat state 但沒有 Movement／Faction state 的 entity。
- 已死亡但仍存在 Army／AI target／control group 的 ID。
- 重複 event subscription 或重複 navigation agent。

### 2. Position synchronization

NavMesh agent、Movement snapshot、Combat position 與 Unity Transform 不可各自成為真實來源。Prototype 必須記錄固定同步方向與順序，並用 test 驗證 frame rate 不改變 gameplay outcome。

### 3. Command routing

Player input、HUD、AI、Scenario 與 tests 應進入同一 CommandBus。Prototype adapter 只轉換 intent，不得跳過 validator 直接呼叫 internal mutation。

### 4. Tick order

Economy、Production、Recruitment、Movement、Combat、Siege、Scenario 與 AI 的 tick order 必須固定。Pause、Save 與 Victory 後不可繼續推進 simulation。

### 5. Restore order

Save DTO 已存在，但 Prototype load 必須先還原 owner／accounts／definitions，再建立 units／heroes／armies／views。錯誤順序容易產生 missing owner、invalid commander、重複扣除 population 或失效 navigation。

## 優先級判斷

### P0 — 先做

- PP00 scene／content／composition／registry。
- PP01 玩家 selection／movement／真實 combat。
- PP02 economy／recruitment／spawn。
- PP03 Hero／Army／Army orders。
- PP04 使用真實 Prototype state 與共用 commands 的 AI。
- PP05 siege／objective／victory。

完成以上即可證明最核心的玩家循環。PP00～PP05 應依序執行；若需要更早展示，PP01 完成時即可先交付 First Manual Battle，不應為了展示而跳過後續 Army／AI acceptance 或先做美術。

### P1 — 核心循環成立後做

- PP06 HUD／session。
- PP07 Save／Load。
- PP08 build／soak／performance gate。

### P2 — 可選改善

- 更多 abilities／technology branches。
- 多種 formation presets。
- 進階 AI personality 與 difficulty parameters。
- 更完整的 debug timeline／replay UI。

### P3 — 延後

- 正式世界觀、勢力 lore、正式名稱。
- Production art、animation、VFX、SFX、BGM。
- Campaign、tutorial、localization、accessibility polish。

## End-to-End Acceptance Matrix

| ID | Given | When | Then | 自動化 |
| --- | --- | --- | --- | --- |
| PA-01 | Prototype scene 與 valid neutral pack | New Game | 建立兩 factions、三 settlements、player units、AI units、resources、gate、objective。 | PlayMode |
| PA-02 | Player units 存活 | 框選並右鍵地面 | 單位沿合法 path 以 formation 移動，selection 不遺失。 | PlayMode＋Manual |
| PA-03 | Player 與 hostile unit 在場 | 右鍵 hostile target | 真實 Combat pipeline 造成 damage／death，不只是 log。 | PlayMode |
| PA-04 | Player City 有足夠資源／capacity | 點擊 Recruit | 扣除 cost、reserve population、完成 queue、spawn 可操作 unit。 | EditMode＋PlayMode |
| PA-05 | 選取 Hero 與同 faction units | Create Army | 產生 valid Army snapshot，membership 在 Hero／Combat 一致。 | EditMode＋PlayMode |
| PA-06 | AI 有 economy 與目標 | 經過 decision cadence | AI 使用共用 commands 招募、集結並至少攻擊一次 Player。 | PlayMode |
| PA-07 | Gate 未摧毀 | Player 嘗試 capture | Command 被拒絕並顯示缺少條件。 | EditMode＋PlayMode |
| PA-08 | Siege 已開始且 attackers 合法 | 攻擊 Gate | Gate HP 歸零、breach event 發布、navigation refresh。 | PlayMode |
| PA-09 | Capture conditions 已完成 | 進入 objective 並 capture | Settlement／Territory owner 改變，Objective 完成，Session Victory。 | PlayMode |
| PA-10 | Player City defeat condition 成立 | AI 完成攻擊／capture | Objective failure，Session Defeat，simulation 停止。 | PlayMode |
| PA-11 | Session 正在 Playing | Pause／Resume | Pause 時 systems 不 tick；Resume 後 deterministic 繼續。 | EditMode＋PlayMode |
| PA-12 | 戰鬥／招募／攻城中狀態 | Save → mutate → Load | 核心 fingerprint、views、navigation、memberships 還原。 | EditMode＋PlayMode |
| PA-13 | Victory／Defeat session | Restart | 舊 services／views／subscriptions 清除並建立乾淨 New Game。 | PlayMode |
| PA-14 | 相同 seed 與 command sequence | 執行兩次 | 核心 outcome／fingerprint 相同。 | EditMode |
| PA-15 | Package Runtime | world-specific string scan | 沒有 Prototype、三國、Fantasy 或產品專屬名稱。 | Static audit |

## Debug 必備資訊

Prototype Debug overlay 至少顯示：

- Session state、scenario／objective state、simulation tick、seed。
- Entity count，依 Faction／Unit／Hero／Army／Dead 分類。
- Selected IDs、current order、formation、target。
- Resources、population、building／research／recruit queues。
- Combatants、projectiles、recent damage／death events。
- Siege state、gate HP／state、entered areas、capture conditions。
- AI goal、action、score、target、route、stalled count。
- Last command、validation result、last exception／diagnostic entry。
- Save slot、last save／load result、state fingerprint。

Debug 資訊只讀取 query／snapshot／events，不得成為 gameplay state owner。

## 測試最低要求

每個 PP Phase 至少包含：

1. Pure C# EditMode tests：新增的 composition decision、mapping、ordering 或 rollback。
2. PlayMode smoke：scene 可載入且對應玩家流程可執行。
3. Existing regression：FrameworkLab 全部 EditMode／PlayMode。
4. Static audit：asmdef、world hardcode、GUID／meta、JSON、Git diff。
5. Manual check：實際操作步驟、預期與實際結果；未執行要明寫。

## 完成判斷

- 所有 PA-01～PA-15 都有 PASS 證據才是 `Completed`。
- 自動 Vertical Slice PASS 但玩家流程未完成時，最多是 `Partial`。
- 只有 compile PASS、scene 可開啟或 UI 有按鈕，不能宣稱 Playable Prototype 完成。
- Production art／world lore 是 Deferred，不阻擋 Prototype；但不能把 placeholder 宣稱為 G08 完成。
