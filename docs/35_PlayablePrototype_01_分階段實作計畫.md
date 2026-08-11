# PlayablePrototype_01 — 分階段實作計畫

## 執行原則

- 每次只執行一個 PP Phase，通過 acceptance 才進入下一個。
- 優先完成最短的玩家可操作閉環，再增加策略層與 persistence。
- 所有新增 domain decision 先放在 Demo／Prototype composition；不先擴充 package Runtime。
- 每個 Phase 都必須更新 `DevelopmentProgress.md`，詳細規格見 `docs/09_DevelopmentProgress_開發進度紀錄規範.md`。
- 每個 Phase 完成後使用 `docs/42_Agent_CodeReview與驗收Prompt.md` 驗收，只能得到 PASS／PARTIAL／FAIL。

## Milestone 順序

| Milestone | 內容 | 玩家可看到的結果 | 優先級 |
| --- | --- | --- | --- |
| M1 First Manual Battle | PP00～PP01 | 玩家可框選、移動、實際攻擊並消滅敵人。 | P0 |
| M2 Strategic Loop | PP02～PP03 | 玩家可取得資源、招募、建立 Army 並下達 Army order。 | P0 |
| M3 Conquest Loop | PP04～PP05 | AI 反攻；玩家可破門、佔領、獲勝或戰敗。 | P0 |
| M4 Session Complete | PP06～PP07 | HUD、Pause、Restart、Save／Load 完整。 | P1 |
| M5 Prototype Gate | PP08 | 自動測試、debug、效能與 Windows build smoke 完成。 | P1 |

## PP00 — Baseline、Scene 與 Composition Skeleton

### Goal

建立 `PlayablePrototype_01.unity` 與中立 Content Pack，讓所有既有 systems 能在同一 composition 中建立、tick、查詢與正確 dispose，但尚不要求完整玩法。

### Tasks

1. 建立 `PrototypeNeutral` Content Pack、Scenario binding 與 UI theme。
2. 驗證 Content Pack references、tags、costs、technology DAG、prefab IDs。
3. 建立 graybox map：Player City、Village、Enemy Fortress、Road、Choke、Gate、Spawn points、Camera bounds。
4. 建立 `PlayablePrototypeBootstrap`，只負責組裝，不寫 gameplay rules。
5. 建立 `PrototypeSystemComposition`，持有 Core／Gameplay services 與 routers。
6. 建立固定 tick order 並寫入 debug overlay：

```text
Input intent
→ CommandBus
→ Economy / Building / Technology / Recruitment
→ Movement
→ Combat
→ Siege / Territory / Scenario
→ AI cadence
→ Events
→ Presentation refresh
```

7. 建立 entity registry、事件 subscription 與 disposal lifecycle。
8. 保留 `VerticalSliceSimulation` 作為自動 regression，不直接拿它的自動 stage progression 當玩家流程。

### Acceptance

- Scene 可進 Play Mode，Console 無 compile error／unhandled exception。
- Content Pack validation 為 PASS。
- 所有 systems／routers 可建立及 dispose，無 duplicate handler。
- Debug overlay 顯示 session、tick、entity、faction、resource 與 objective summary。
- EditMode 有 composition／content validation tests；PlayMode 有 scene boot smoke test。

## PP01 — Player Input、Movement 與真實 Combat

### Goal

完成最短的玩家戰鬥閉環：選取 → 移動 → 攻擊 → 傷害 → 死亡。

### Tasks

1. 重用 `UnityRtsInputAdapter`、`SelectionService`、`RtsCameraController` 與 `MovementSystem`。
2. 讓 `AttackTargetCommand` 不再只顯示文字，而是接到同一 scene 的 `CombatSystem`。
3. 建立同一 `EntityId` 的 Movement、Combat、Selection、Faction、View registration。
4. 明確定義 position authority：Gameplay snapshot 為可查詢狀態；Unity navigation adapter 回報位置；Combat position bridge 依固定順序同步。
5. 顯示 selection highlight、health bar、attack target、death state。
6. Unit death 時執行統一 cleanup：Selection、Movement、Army membership、target references、View。
7. 驗證 Stop／Hold／Queue／Formation 不破壞 combat order。

### Acceptance

- 玩家可選取單位並移動到目的地。
- 玩家右鍵敵方單位會進入實際 attack pipeline。
- Melee／ranged 至少各一種；damage、projectile、death event 可觀察。
- 死亡單位不能再次被選取或接收命令。
- Player 與測試派送相同 `AttackTargetCommand`。
- PlayMode 自動完成「選取／移動／攻擊／死亡」流程。

## PP02 — Economy、Building、Technology 與 Recruitment

### Goal

讓玩家透過最小 HUD 取得資源、建造 prerequisite、研究必要科技並招募可立即操作的新單位。

### Tasks

1. 建立 Player City economy account 與 population capacity。
2. 顯示兩種資源、population、production rate、queue 與剩餘時間。
3. 建立 Construct／Research／Recruit HUD commands，全部經既有 routers／CommandBus。
4. 顯示 command validation failure，例如資源不足、缺 building、缺 technology、population full。
5. `PrototypeUnitSpawnAdapter` 完成原子式 registration：Faction、Movement、Combat、Selection、View，必要時 Hero／Army eligibility。
6. Recruitment 完成後新單位出現在合法 spawn point，可被選取、移動與戰鬥。
7. Building／Technology completion 必須更新 query／HUD，不由 UI 直接寫 state。

### Acceptance

- 資源依 deterministic tick 增加。
- 資源不足時 Recruit／Build／Research 被 validator 拒絕且 UI 顯示原因。
- 完成 prerequisite 後可招募至少 Infantry、Archer、Siege Unit。
- Cost 與 population reserve／consume 正確，不會重複扣除。
- 新招募單位通過 PP01 的全部可操作條件。
- EditMode 覆蓋成功、失敗、queue order 與 spawn rollback；PlayMode 覆蓋玩家點擊招募。

## PP03 — Hero、Army 與 Army Orders

### Goal

讓玩家把 Hero 與 units 組成 Army，並使用共用 order 流程移動、攻擊、守備與撤退。

### Tasks

1. Spawn／register 一名 player Hero 與一名 AI Hero。
2. HUD 顯示 selected units 的 army membership、commander、morale／supply rule status。
3. 透過 `CreateArmyCommand` 建立 Army；驗證 commander 是同 faction 的 registered Hero member。
4. 支援加入／拆分／合併／更換 commander 的最小 debug controls。
5. `MoveArmyCommand`／`AttackArmyCommand` 委派至既有 Movement／Combat executor。
6. Unit death、recruitment 與 load 後同步 Army membership。

### Acceptance

- 玩家可從選取單位建立 Hero-led Army。
- Army snapshot、Hero ArmyId、Combat ArmyId 一致。
- Army move／attack 使用相同 actor IDs 且 formation assignment deterministic。
- 非法 commander／跨 faction member 會被拒絕。
- 死亡 member 不殘留在 Army snapshot。

## PP04 — AI 對手與反攻

### Goal

讓 AI 讀取 Prototype 的真實 Economy／Army／Territory／Siege state，透過與 Player 相同的 commands 行動。

### Tasks

1. 建立 `IAiWorldQuery` composition，聚合真實 read models，不建立 AI 專用平行 state。
2. 建立 `IAiActionExecutor`，把 Recruit／Create Army／Move／Attack／Siege actions 派送到同一 CommandBus。
3. 使用固定 decision cadence，不在每 frame busy loop。
4. AI 至少能生產、集結、選擇目標、行軍、攻擊與在 stalled 時 Recover／Wait。
5. Debug overlay 顯示 goal、layer、action、score、target、route、stalled count。
6. AI 不直接修改 resources、HP、owner 或 objective。

### Acceptance

- AI 在沒有玩家腳本代打的情況下完成至少一次 recruit 與 attack。
- AI commands 會經過相同 validators；資源不足時等待或改變決策。
- AI 能對 Player City 發動一次可觀察反攻。
- 長時間執行沒有 command spam、deadlock 或無限 stalled loop。

## PP05 — Siege、Capture、Scenario 與 Victory／Defeat

### Goal

完成玩家從野戰進入攻城、破門、佔領敵方堡壘並結束 Scenario 的主要循環。

### Tasks

1. 將 Enemy Fortress、Gate、Inner Area、Capture Objective 接到 `SiegeSystem`。
2. 讓一般 Combat attacker 的 attack profile／tags 決定是否可傷害 defense structure。
3. Gate destroyed／opened 後通知 `ISiegeNavigationSink` 更新路徑。
4. 玩家透過 commands 進入 siege areas，不由 trigger 直接改 authoritative state。
5. Capture 使用既有 `SettlementSiegeCaptureSink`／Settlement owner transaction。
6. Scenario objectives 監看 authoritative events／facts，產生 Victory／Defeat。
7. AI 可防守堡壘；Player City 被攻陷或 commander／objective 條件失敗時可 Defeat。

### Acceptance

- 未滿足 capture conditions 時命令被拒絕並顯示原因。
- 玩家可攻擊並摧毀 Gate，navigation refresh 至少發生一次。
- 進入 Inner Area／Capture Objective 後可完成 capture。
- Settlement／Territory owner 同步轉移。
- Objective status 轉為 Completed，Session 進入 Victory。
- Defeat path 至少有一個可重現 PlayMode test。

## PP06 — HUD、Session 與操作回饋

### Goal

把前述 debug controls 整理成可理解的最小遊戲介面，不追求正式美術。

### Tasks

1. 顯示 Resources、Selection、Unit、Hero、Army、Settlement、Production、Objective、Notification、Pause panels。
2. UI 透過 `IHudQuery`／`IHudCommandSink`，禁止 direct mutation。
3. 建立 Main Menu、New Game、Pause、Resume、Restart、Return to Menu。
4. 顯示 command accepted／rejected、damage、death、recruit completed、siege、capture、victory notifications。
5. 保持 world-neutral labels；正式名稱由 Content Pack display name 提供。
6. 鍵鼠操作與按鈕狀態在 1920×1080 與至少一個較小解析度可用。

### Acceptance

- 玩家不看 Console 也能理解資源、選取、命令失敗、目前目標與勝負。
- Pause 會停止 simulation，但 UI 可操作 Resume／Settings。
- Restart 產生乾淨的新 session，不殘留 entities、subscriptions 或 views。
- Theme 切換不改 layout responsibility 或 gameplay state。

## PP07 — Save／Load

### Goal

儲存並還原同一個玩家可操作 Prototype，不只驗證獨立 persistence DTO。

### Tasks

1. `PrototypeGameStateAdapter` 聚合 Faction、Settlement、Economy、Unit、Hero、Army、Building、Technology、Objective、Clock、Random 與必要 extension state。
2. 明確定義 restore order：definitions／systems → factions／settlements → economy／tech／buildings → units／heroes → armies → siege／scenario → views。
3. Save 不包含 GameObject、Transform、Material 或其他 Unity object reference。
4. 使用 `GameStateCoordinator` 與一個 Prototype slot；Editor 可先使用 file store。
5. Load 後重建 views、navigation registration、selection eligibility 與 event subscriptions。
6. Save metadata 驗證 framework／content／scenario compatibility。

### Acceptance

- 在 recruit、battle、siege 前後至少三個狀態點可 Save／Load。
- Load 後 resources、population、HP、position、army、owner、objectives、clock、random state 一致。
- Save/load fingerprint 一致；不重複 spawn 或重複訂閱 events。
- incompatible／corrupted save 會顯示可讀錯誤且不破壞目前 session。

## PP08 — Prototype Gate、效能與 Build

### Goal

把 Prototype 固化為可重複驗證、可 review、可交給人操作的版本。

### Tasks

1. 完整 EditMode／PlayMode regression。
2. 建立 end-to-end PlayMode：New Game → Recruit → Army → Battle → Siege → Capture → Victory。
3. 建立至少 30 分鐘 soak test 或 deterministic long-run test。
4. 檢查 100～300 active units 的 frame／simulation／AI／navigation metrics；不以 production hardware budget 冒充通過。
5. 建立 Windows Development Build smoke；記錄 build path、Unity version 與實際操作結果。
6. 檢查 Console、missing references、duplicate GUID、asmdef、world hardcode、save compatibility、dead code 與 Git diff。
7. 更新 API／prototype docs 與 `DevelopmentProgress.md`。

### Acceptance

- 所有既有 tests 與 Prototype tests 通過。
- 玩家能從 executable 完成至少一局，不需要 Unity Inspector 或 Debug Console 修改 state。
- 無未處理 Exception、compile error、missing script 或 broken reference。
- Debug overlay 可關閉，關閉後不影響 gameplay。
- Code review 結果為 PASS；PARTIAL／FAIL 不可宣稱 Prototype 完成。

## 建議現在先執行

第一個實作任務應為：

```text
執行 docs/35_PlayablePrototype_01_分階段實作計畫.md 的 PP00。
只建立中立 Content、graybox scene、composition skeleton、entity registry、tick order、debug 與 tests；
不要執行 PP01～PP08，不要製作正式美術，不要修改已正常運作的 package Runtime。
```

PP00 通過後立即執行 PP01，盡快得到第一個由玩家親自操作的戰鬥閉環。
