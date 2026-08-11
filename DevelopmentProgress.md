# Development Progress

此檔案是 AegisRTS FrameworkLab 的唯一正式開發進度紀錄。格式與更新規則見 [`docs/09_DevelopmentProgress_開發進度紀錄規範.md`](docs/09_DevelopmentProgress_開發進度紀錄規範.md)。最新紀錄置頂。

## Current Status

- Current Phase：Phase 07 Faction / Settlement / Territory 完成；Unity EditMode 72/72、PlayMode 8/8 通過。
- Active Branch：`main`
- Unity Project Version：`6000.5.7f1`
- Specification Baseline：專案使用 Unity `6000.5.7f1`；文件標示的 Unity 6.3 LTS 與實際版本命名對應仍待確認。

## 2026-08-11 — Phase 07 Faction / Settlement / Territory

- Status：Completed
- Goal：完成 Faction runtime state、Settlement ownership／capture、Territory graph／visibility／value，並驗收三座 settlement 變更 owner 後 Faction territory 自動更新。
- Changed：
  - 新增 `FactionSystem`、Faction profiles／snapshots、resources、technology、diplomacy、AI profile 與 ownership indices。
  - 新增 `FactionArmyEventBridge`，從 Army create／split／merge events 維護 Faction army index。
  - 新增 `TerritorySystem`、territory node／connection、owner、visibility、value 與 settlement mapping。
  - 新增 `SettlementSystem`、settlement runtime state、五種 capture rules、`CaptureSettlementCommand` 與 router。
  - 新增 `SettlementArmyTargetValidator`，補強 AttackSettlement 的 existence／ownership／diplomacy validation。
  - `SettlementDefinition`、JSON loader／validator 與三個 Content Packs 新增 population、defense、capture rule／conditions。
  - `Sandbox_Siege` 新增三座 settlement、三個 connected territory nodes、自動 capture acceptance 與 debug HUD。
  - 新增 12 個 Phase 07 EditMode cases、2 個 PlayMode cases，更新 07、16、26 與本進度文件。
- Architecture / API / Data：
  - Settlement capture 是 ownership transaction entry；Settlement、Faction settlement index、Territory、Faction territory index 依序同步。
  - Faction／Settlement／Territory 都是 Pure C#，Gameplay 保持 `noEngineReferences=true`；Sandbox 只負責 composition 與 visual。
  - Capture conditions 由上游 Combat／Siege 系統提供 flags；capture rule 不依賴世界觀名稱。
  - Army settlement target validation 經 interface 注入，不讓 ArmySystem 直接依賴 SettlementSystem concrete type。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，72/72 passed、0 failed；Phase 07 新增 12 cases。
  - Unity PlayMode Test Runner：PASS，8/8 passed、0 failed；Phase 07 新增 2 Sandbox_Siege cases。
  - 三 settlement ownership acceptance：PASS；Faction A 的 settlement／territory 清空，Faction B 自動取得三座 settlement 與三個 territory nodes。
  - Capture rules、invalid command、Faction state、territory graph／visibility、settlement state、army bridge、AttackSettlement diplomacy validation：PASS。
- Known Issues / Risks：
  - Settlement resources／buildings／recruitment 尚未執行成本、時間或產出規則，待 Phase 08。
  - Territory visibility 是明確設定的狀態，尚未串 Fog of War 探索／視野傳播。
  - Capture completed conditions 尚未由實際 Siege objectives 產生，待 Phase 09。
- Next：
  - 進入 Phase 08 Economy / Recruit / Build / Tech，使用既有 Faction／Settlement runtime state 實作成本與生產流程。

## 2026-08-11 — Phase 06 Hero / Army / Command

- Status：Completed
- Goal：以 unit entity 上的 Hero component 建立 leadership／ability 資料，完成 Army composition、commander、optional morale／supply 與九種共用 commands，驗收 Hero + 20 infantry 建軍、拆分、合併與換 commander。
- Changed：
  - 新增 `HeroProfile`、`HeroSnapshot`、`IHeroQuery` 與 `HeroSystem`，不建立第二套 Combat。
  - `HeroDefinition`、JSON loader、validator 與三個 demo Content Packs 新增 world-neutral `leadership`。
  - 新增 `ArmySystem`、army models／snapshots／events、unit membership、commander 與 optional morale／supply。
  - 新增 Create／Merge／Split／AssignCommander／Move／Attack／AttackSettlement／Defend／Retreat commands 與 `ArmyCommandRouter`。
  - 新增 `IArmyOrderExecutor`、`GameplayArmyOrderExecutor`，串接既有 Movement／Combat API。
  - 新增 `IArmyMembershipSink` 與 `CombatArmyMembershipSink`，讓軍團拆分／合併後 Combat snapshot 的 ArmyId 保持同步。
  - `Sandbox_Combat` 加入獨立 `ArmySandboxBootstrap`、21 actors、command/event counters 與 debug HUD。
  - 新增 8 個 Phase 06 EditMode cases、2 個 PlayMode cases，更新 07、15、26 與本進度文件。
- Architecture / API / Data：
  - Hero 是 unit entity 的 supplementary component；Combat／Movement state 仍由既有系統擁有。
  - ArmySystem 是 composition authoritative owner，跨系統同步透過 sink，不直接持有 Unity object。
  - 所有 army commands 走同一個 CommandBus validator／handler flow；非法跨 faction merge、非 hero commander、重複 membership 在 mutation 前拒絕。
  - Army order execution 經 `IArmyOrderExecutor` adapter；state-only tests／sandbox 與 production Movement＋Combat coordinator 可替換。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，60/60 passed、0 failed；Phase 06 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，6/6 passed、0 failed；Phase 06 新增 2 Sandbox cases。
  - Hero + 20 infantry acceptance：PASS；21 members 建軍、10 members 拆分、合併回 21 members、commander change、membership propagation 全部通過。
  - 九種 command routing、invalid validation、optional morale／supply、event flow、router disposal：PASS。
- Known Issues / Risks：
  - Defend 保存 defense order 並移動至指定點；arrival 後 hold／engagement policy 尚待 coordinator。
  - Morale／Supply 消耗與潰退門檻尚未接 Economy／AI／Scenario rules。
  - AttackSettlement 目前由 Phase 05 Combat target 處理；settlement type／ownership validation 待 Phase 07。
- Next：
  - 進入 Phase 07 Faction / Settlement / Territory，建立 ownership query 並補強 AttackSettlement validation。

## 2026-08-11 — Phase 05 Unit Combat / Ability

- Status：Completed
- Goal：完成 unit runtime combat state、近戰／遠程攻擊、projectile／splash、傷害管線、能力目標與啟動分類、status effects 與 death flow。
- Changed：
  - 新增 Pure C# `CombatSystem`、`ICombatQuery`、combat profiles／snapshots 與 combat events。
  - 完成 Base → modifier → armor → resistance → shield → final damage → HP → death pipeline。
  - 完成 melee range／windup／cooldown、ranged projectile travel、enemy-only splash 與 target tag filtering。
  - 完成 buff、debuff、stun、slow、root、shield、DoT；修正 DoT 與 shield 同 tick 改動狀態清單的安全性。
  - 新增 `AbilityProfile`、ability target／activation enums、`UseAbilityCommand` 與 `AbilityUsedEvent`；Active／Toggle 支援手動施放與 cooldown。
  - 新增 `UnityCombatDriver`、`UnityCombatView`，提供 event-driven projectile visual、血條、受傷與死亡外觀。
  - 完成 `Sandbox_Combat` composition root 與自動 acceptance scenario，共 6 個 combatants。
  - 新增 8 個 EditMode combat／ability tests 與 2 個 PlayMode Sandbox tests。
  - 更新 07、14、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay 保持 `noEngineReferences=true` 且只依賴 Core；authoritative HP、status、cooldown、projectile 全部位於 `CombatSystem`。
  - Unity view 只讀取 `CombatantSnapshot`，不持有 HP truth；projectile GameObject 是 simulation event 的短期視覺回饋。
  - FactionId 用於敵我篩選，ArmyId 保留歸屬；不提前耦合 Phase 06／07 service。
  - 大 delta 下先推進既有 projectile，再建立新 projectile，避免新投射物同 tick 消耗完整 delta 而瞬移命中。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，52/52 passed、0 failed；Phase 05 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，4/4 passed、0 failed，含既有 50-unit movement acceptance 與新 combat scene acceptance。
  - Combat acceptance：PASS；melee、ranged、projectile、splash、target tags、status、ability cooldown、DoT、death event 均有測試。
  - Unity scripts compile：PASS；Gameplay forbidden `UnityEngine` reference scan：PASS；`git diff --check`：PASS。
- Known Issues / Risks：
  - Passive／Aura／Triggered 目前定義 activation type，但自動觸發／refresh policy 留給 Hero／Army／AI 規則層注入。
  - Direction target 已保存方向資料；cone／line shape 尚待 spatial query。
  - Combat out-of-range state 會維持 `Targeting`；追擊至攻擊距離仍需 Movement／Combat coordinator。
  - Projectile simulation 追蹤移動目標；目前 Unity projectile visual 使用發射當下 destination，production VFX 可改為追蹤 view。
- Next：
  - 進入 Phase 06 Hero / Army / Command，並建立 Movement／Combat 的上層協調與能力觸發來源。

## 2026-08-11 — Phase 04 Movement / Navigation / Formation

- Status：Completed
- Goal：建立 destination validation、unreachable、queue、repath、stuck detection、local avoidance 與 Line／Box formation，並驗證 50 units 可繞過障礙且不大量永久卡死。
- Changed：
  - 新增 Pure C# `FormationPlanner`、Line／Box formation types 與 deterministic `FormationSlot`。
  - 新增 `INavigationAdapter`、navigation result／snapshot contracts、`MovementSystem` 與 movement state snapshots。
  - `MoveUnitsCommand` 新增 optional `FormationType`；context resolver 與 input adapter 支援 formation，`Tab` 切換 Box／Line。
  - 新增 `NavMeshMovementAdapter` 與 `UnityMovementDriver`，完成 destination sampling、complete-path validation、NavMeshAgent local avoidance、repath／stuck feedback 與 path／destination／velocity gizmos。
  - `Sandbox_RTS` 改為 runtime NavMeshSurface、3 組 obstacle、50 friendly agents、movement HUD 與可由測試派發的 acceptance command。
  - Demo asmdef 新增 `Unity.AI.Navigation` reference；package 版本未調整。
  - 新增 7 個 Phase 04 EditMode cases，並將 PlayMode 擴充為 50-agent composition 與跨障礙 acceptance，共 2 cases。
  - Unity Editor 首次載入產生標準 `ProjectSettings/SceneTemplateSettings.json`，納入版本控制以維持 ProjectSettings 完整性。
  - 更新 13、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay 只依賴 Core 且保持 `noEngineReferences=true`；`MovementSystem` 不持有 Transform、GameObject、NavMeshAgent。
  - `INavigationAdapter` 讓未來 grid／flow-field／server navigation 可替換 Unity NavMesh，不改 command 或 movement state API。
  - formation assignment 依 EntityId 排序且每 actor 使用 distinct slot；Box 避免大量單位集中同一 destination。
  - Bootstrap 只組合 runtime NavMesh、services 與 demo actors；frame tick 由獨立 `UnityMovementDriver` 負責。
  - local avoidance 留在 Unity adapter；order queue、arrival、unreachable、repath 與 stuck transition 由 Gameplay 擁有。
- Tests / Validation：
  - Unity 6000.5.7f1 netstandard 2.1 compatibility build：PASS；Core、Gameplay、Presentation、Demo、全部 EditMode／PlayMode source，0 warnings、0 errors。
  - `dotnet format ... --verify-no-changes`：PASS；`git diff --check`：PASS。
  - Unity EditMode Test Runner：PASS，44/44 passed、0 failed；Phase 04 新增 7 cases，涵蓋 Line／Box、50 distinct slots、unreachable、queue、repath／stuck、stop／hold。
  - Unity PlayMode Test Runner：PASS，2/2 passed、0 failed，總時間約 15.69 秒。
  - 50-agent acceptance：PASS；所有 agents 收到 distinct Box destinations，15 秒後至少 40 個已跨過中央障礙，`Stuck`／`Unreachable` 不超過 5。
  - 靜態 dependency／asset validation：PASS；Gameplay Unity references=0、Gameplay asmdef 只依賴 Core、Demo 明確依賴 `Unity.AI.Navigation`、177 unique asset GUIDs。
- Known Issues / Risks：
  - Sandbox 為驗收方便在 runtime synchronous build NavMesh；大型 production map 應使用 pre-baked data 或受控 async update，避免載入尖峰。
  - Unity NavMeshAgent local avoidance 並非 deterministic simulation；Replay／lockstep 策略需在 Phase 13 明確界定記錄層級。
  - Attack／Follow／Interact 目前仍只派發 intent；追擊、跟隨與接近互動目標會在 Combat／Army phase 接入 MovementSystem。
  - 50-unit Line formation 可能超出小型可走區域而被 adapter 拒絕；production formation planner 後續可加入 bounds-aware wrapping。
  - Unity 專案版本 `6000.5.7f1` 與文件中的 Unity 6.3 LTS 名稱仍需確認是否為同一發行基線。
- Next：
  - 進入 Phase 05 Unit Combat / Ability，將 attack range approach 與 movement stop conditions 接入共用 command flow。

## 2026-08-11 — Phase 03 RTS Input / Selection / Camera

- Status：Completed
- Goal：完成 RTS 相機、click／box selection、Shift add/remove、double-click same type、control groups 與 context command，並建立 20 debug units 的可操作驗收場景。
- Changed：
  - 在 Gameplay 新增 Unity-independent `WorldPoint` 與 Move／Attack／Follow／Interact／Stop／Hold commands，維持 Player／AI 共用 `CommandBus`。
  - 在 Presentation 新增 pure C# `SelectionService`、`ContextCommandResolver`、`RtsCameraRigModel`，以及 Unity selectable、input、camera adapters。
  - 新增 `AegisRTS_RTS.inputactions`，包含 Point、Select、AddSelection、Command、CameraMove、CameraZoom、ControlGroup、QueueCommand、Stop、Hold 與 FocusSelected。
  - 將 Presentation asmdef 加入 `Unity.InputSystem`，EditMode tests 加入 Presentation reference。
  - `Sandbox_RTS` 新增 composition bootstrap，runtime 建立 ground、20 friendly debug units、friendly／enemy／settlement context targets、camera、input 與 command diagnostics UI。
  - 新增 10 個 Phase 03 EditMode test cases 與 1 個 PlayMode scene composition test；更新 12、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay commands 不依賴 Unity；選取狀態與相機 bounds 可脫離 MonoBehaviour 測試。
  - `UnityRtsInputAdapter` 專責 Input System、screen-space box、raycast 與 adapter dispatch；`RtsSandboxBootstrap` 僅負責 composition 與 acceptance actors。
  - Context mapping 固定為 Ground→Move、Enemy→Attack、Friendly→Follow、Settlement→Interact；Stop／Hold 走同一 CommandBus。
  - Control groups 保存 EntityId snapshot，recall 時自動忽略已 unregister 的 entity。
  - 本次正式 Unity EditMode 結果涵蓋 Phase 01–03，因此關閉 Phase 01／02 舊紀錄中的 Test Runner 阻塞；舊紀錄保留當時狀態，不回寫歷史。
- Tests / Validation：
  - Pure C# validation harness：PASS，smoke 9/9；反射執行全部實際 EditMode NUnit cases：PASS，37/37，其中 Phase 03 為 10 cases。
  - Unity 6000.5.7f1 netstandard 2.1 compatibility build：PASS；Core、Gameplay、Presentation、Demo、全部 EditMode／PlayMode test source，0 warnings、0 errors。
  - `dotnet format ... --verify-no-changes`：PASS。
  - Input Actions JSON 與 asmdef JSON parse：PASS；11 actions（含額外 FocusSelected）與 10 個必要 action names 齊全。
  - 靜態 scene／asset acceptance：PASS；`Sandbox_RTS` 引用 bootstrap GUID、scene 位於 Build Settings、170 unique asset GUIDs、inputactions importer GUID 正確。
  - Unity EditMode Test Runner：PASS，37/37 passed、0 failed／skipped／inconclusive；同時正式複驗 Phase 01／02 cases。
  - Unity PlayMode Test Runner：PASS，`SandboxRts_ComposesTwentyDebugUnitsSelectionInputAndCamera` 1/1 passed、0 failed；場景載入後沒有未處理 exception。
  - 初次 Unity batch import 曾卡在 Bee ScriptAssemblies rebuild，已停止程序；完成 import cache 後重跑即正常編譯並完成兩種 Test Runner。
- Known Issues / Risks：
  - 自動測試已覆蓋 selection／command／camera domain 與 Sandbox composition；滑鼠框選、edge pan 與 middle drag 的實際操作手感仍建議在互動式 Editor 做 exploratory tuning。
  - 目前 command handler 只顯示已派發的 debug summary；實際 pathfinding、movement 與 formation execution 屬 Phase 04。
  - Input action asset 是 authoring contract；Sandbox runtime 建立同名 action map 以免依賴 Inspector wiring。後續若調整 bindings，兩者需同步，或導入 generated wrapper 作單一來源。
  - Unity 專案版本 `6000.5.7f1` 與文件中的 Unity 6.3 LTS 名稱仍需確認是否為同一發行基線。
- Next：
  - 可先在互動式 Unity Editor 做 Sandbox 操作手感複驗，再進入 Phase 04 Movement / Pathfinding / Formation。

## 2026-08-11 — Phase 02 Data-Driven / Content Pack

- Status：Blocked
- Goal：建立通用 Definition、GameRuleSet、JSON Content Pack、typed catalog 與完整資料驗證，證明同一 Framework 可切換三種世界觀資料。
- Changed：
  - 在 `AegisRTS.Gameplay.Content` 新增 25 個 Pure C# source files，包含 immutable definitions、GameRuleSet、JSON loader、validator、typed catalog 與 atomic pack service。
  - 將 `AegisRTS.Gameplay.asmdef` 設為 `noEngineReferences: true`，依賴維持只有 `AegisRTS.Core`。
  - 新增 `DemoNeutral`、`DemoThreeKingdoms`、`DemoFantasy` 三個 `ContentPack.json`；每個 pack 各含 7 個 definitions 與一套 rules。
  - 新增 4 個共用 placeholder prefab assets，供 prefab ID existence validation。
  - 新增 5 個 Phase 02 EditMode test files（含 test factory），並更新 02、07、08、26 文件。
  - 為所有新增 Unity folders、scripts、JSON 與 prefab 建立並驗證 `.meta`。
- Architecture / API / Data：
  - Gameplay 僅理解通用 Definition、Tag、typed reference 與 prefab asset ID；世界觀名稱、數值與 rules 只存在 JSON Content Pack data。
  - `DefinitionId` 與 `ContentTag` 正規化成穩定 lowercase value；reference 不依賴 display name。
  - `ContentPackValidator` 回報 duplicate ID、missing typed reference、invalid stat／cost、technology cycle、missing prefab／tag，不在第一個錯誤停止。
  - `ContentPackService.Load` 驗證成功才切換 `ActiveCatalog`；invalid pack 保留前一個 catalog。
  - `IContentAssetCatalog` 是 Unity asset lookup adapter boundary，definitions 不持有 GameObject。
  - Phase 01 Unity Test Runner 尚未複驗；依使用者明確指示先繼續 Phase 02，原有阻塞紀錄保留。
- Tests / Validation：
  - `dotnet build Temp/Phase01Validation/Phase01Validation.csproj --configuration Release --no-restore`：PASS；Core、Gameplay 與實際 NUnit source 一起編譯，0 warnings、0 errors。
  - Unity netstandard 2.1 compatibility build（使用 Unity 6000.5.7f1 隨附的 `System.Text.Json` reference）：PASS，0 warnings、0 errors。
  - validation harness smoke tests：PASS，9/9。
  - 反射執行實際 NUnit `[Test]`／`[TestCase]`：PASS，27/27；其中 Phase 02 為 12 cases。
  - `dotnet format ... --verify-no-changes`：PASS。
  - 三個實際 JSON packs deserialize、完整 validation、依序切換與 typed lookup：PASS。
  - 靜態 Acceptance：PASS；25 Gameplay files、3 packs、160 unique asset GUIDs，Gameplay asmdef 只依賴 Core、`noEngineReferences=true`，沒有 Unity／Demo／Presentation dependency 或世界觀 hardcode。
  - Unity EditMode Test Runner：未執行；本次工作階段中 Unity CLI 已確認在進入 runner 前受 Licensing Client IPC 逾時阻擋。
- Known Issues / Risks：
  - 需在已登入 Unity Hub 的互動式 Editor 中確認 Console 無 error，並執行全部 27 個 tests；完成前不宣稱 Phase 01／02 完整符合 Definition of Done。
  - 四個 prefab 是只有 root Transform 的資料驗證 placeholder，尚未包含正式 Visual、Collider 或 View Components。
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 仍不一致。
- Next：
  - 從 Unity Hub 開啟專案，執行全部 EditMode tests 並確認 Bootstrap Console；通過後將 Phase 01／02 Status 改為 `Completed`，再進入 Phase 03。

## 2026-08-11 — Phase 01 Core 基礎設施

- Status：Blocked
- Goal：實作 Entity ID、GameClock、Seeded Random、Command Bus、Event Bus、State Machine 與 Diagnostics，並完成 Phase 01 測試與驗收。
- Changed：
  - 在 `AegisRTS.Core` 新增 20 個 Pure C# source files，完成 Entity ID、GameClock、Seeded Random、Command Bus、Event Bus、State Machine 與 Diagnostics。
  - 將 `AegisRTS.Core.asmdef` 設為 `noEngineReferences: true`。
  - 新增 7 個 EditMode test files，共 15 個 NUnit test／test case attributes。
  - 更新 `docs/26_Framework_API_目標介面.md`，記錄 Phase 01 public API、生命週期、determinism 與 threading 邊界。
  - 為所有新增 Unity assets 建立並驗證 `.meta`。
- Architecture / API / Data：
  - `AegisRTS.Core` 無 assembly references 且禁止 UnityEngine reference；Core 沒有 Gameplay／Presentation using。
  - `ICommand`／`CommandBus` 提供 Player、AI、Scenario、Test 共用的 validation 與 dispatch flow。
  - `IEvent`／`EventBus` 提供同步、依註冊順序且可安全 unsubscribe 的 event flow。
  - `IRandomSource`／`SeededRandom` 使用固定 PCG sequence，並以 reference vector test 防止演算法意外變更。
  - `IDiagnosticSink` 隔離 logging adapter；`DiagnosticBuffer` 是 bounded、thread-safe history。
- Tests / Validation：
  - `dotnet build Temp/Phase01Validation/Phase01Validation.csproj --configuration Release --no-restore`：PASS，Core 與實際 NUnit source 一起編譯，0 warnings、0 errors。
  - `dotnet run ... --no-build`：PASS，7/7 等價行為測試通過，涵蓋 Phase 01 所有指定測試面向。
  - `dotnet format ... --verify-no-changes`：PASS。
  - 靜態 Acceptance：PASS；20 Core files、7 test files、119 unique asset GUIDs，Core asmdef references=0、`noEngineReferences=true`、沒有 forbidden using。
  - Unity EditMode Test Runner：未執行；Unity 在進入 test runner 前因 Licensing Client IPC 連線逾時而持續重試，沒有產生 test result XML。
- Known Issues / Risks：
  - 需在已登入 Unity Hub 的互動式 Editor 中確認 Console 無 error，並執行 15 個 NUnit test／test cases；完成前不宣稱 Phase 01 完整符合 Definition of Done。
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 仍不一致。
- Next：
  - 從 Unity Hub 開啟專案，執行 EditMode tests 並確認 Bootstrap Console；通過後把本筆 Status 改為 `Completed`，再進入 Phase 02。

## 2026-08-11 — 建立 Development Progress 規範

- Status：Completed
- Goal：建立每次 repository 開發都必須同步留下可驗證進度的統一規範與紀錄檔。
- Changed：
  - 新增 `docs/09_DevelopmentProgress_開發進度紀錄規範.md`。
  - 新增根目錄 `DevelopmentProgress.md`。
  - 更新開發總覽、Git 規範、Definition of Done 與 Agent prompts。
- Architecture / API / Data：
  - N/A；此工作只調整開發治理與文件，不修改 runtime architecture、public API 或資料格式。
- Tests / Validation：
  - 確認 `DevelopmentProgress.md` 位於 Repository Root，且未被 `.gitignore` 排除：PASS。
  - 確認總覽、Git、DoD 與 Agent 規則都有連結或強制更新條款：PASS。
- Known Issues / Risks：
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 不一致，尚未決定升降版策略。
- Next：
  - 開始 Phase 01 前先閱讀最新進度，並在每次實作完成時同步更新本檔。

## 2026-08-11 — Project Initialization

- Status：Completed
- Goal：依 01–08 規格完成 Unity FrameworkLab 初始目錄、assembly、場景與 Git repository。
- Changed：
  - 建立 `Assets/AegisRTS` Framework、Content、Demo 與 Tests 目錄。
  - 建立 8 個 asmdef 與 5 個 Bootstrap／Sandbox 場景。
  - 建立根目錄 README、Git ignore／attributes，並初始化 `main` branch。
- Architecture / API / Data：
  - 建立 `Core → Gameplay → Presentation/Persistence → Demo/Tools` 的初始 assembly dependency。
  - 尚無 runtime public API 或核心資料模型實作。
- Tests / Validation：
  - 目錄、asmdef、scene GUID 與 Build Settings 靜態驗證：PASS。
  - `dotnet build AegisRTS.FrameworkLab.slnx --no-restore`：PASS，0 warnings、0 errors。
  - Unity CLI Console／Play Mode：未完成；本機 Licensing Client 連線逾時。
- Known Issues / Risks：
  - Unity 專案版本與規格基準不一致。
  - 仍需使用已登入 Unity Hub 的互動式 Editor 驗證 Bootstrap Play Mode。
- Next：
  - 確認 Unity 版本策略與 Editor 驗證後進入 Phase 01。
- Related Commit：`196975d Finish project init.`
