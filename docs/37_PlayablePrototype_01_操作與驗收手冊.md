# PlayablePrototype_01 — 操作與驗收手冊

## 文件目的

本文件是 `PlayablePrototype_01` 的實際操作、驗收與交接入口。規格與原始缺口請分別閱讀：

1. `34_PlayablePrototype_01_總覽與範圍.md`
2. `35_PlayablePrototype_01_分階段實作計畫.md`
3. `36_PlayablePrototype_01_現況缺口與驗收矩陣.md`
4. `38_PlayablePrototype_01_架構與維護.md`

`36` 保留實作前的 baseline；本文件記錄實作後的現況。正式歷史與每次變更證據仍以 Repository Root 的 `DevelopmentProgress.md` 為唯一來源。

## 目前結論

`PlayablePrototype_01` 的 PP00～PP08 程式、Content、scene、HUD、Save／Load、自動測試與 Windows Development Build 已建立。自動化流程可完成 New Game → Economy → Recruit → Army → Battle → Siege → Capture → Victory，另有可重現的 Defeat path。步兵已使用 Humanoid L3 Prefab 與 Idle／Move／Attack／Hit／Death；停止後應保持雙腿在身體下方的直立防守姿勢。英雄與其他兵種仍使用 primitive placeholder。

目前可宣稱 PP00～PP08 的系統優先 Prototype 已完成，但不可宣稱正式遊戲或 G01～G12 完成，原因如下：

- Windows executable 已由 Agent 直接啟動並以 UI 完成 Economy → Army → Siege → Victory；也已實際驗證 Save／Load、正常 Defeat 與從結果視窗 Restart。Unity Editor 路徑由 PlayMode 30/30 覆蓋。
- Prototype 在 Unity Player 使用 runtime-baked NavMesh；封閉城門會阻擋內院，破門後會重建 NavMesh 並重新派送目的地。純 C# composition tests 仍使用 deterministic adapter，以維持快速且可重現的 domain 測試。
- Save／Load 仍是單一 Prototype slot，但已透過 `GameStateCoordinator` 保存並驗證 checksum、版本與 Content／Scenario 相容性；進行中的建造、科技、招募、移動、戰鬥、Army、AI cadence 與隨機狀態均可還原。
- 正式世界觀、production art、動畫、VFX、SFX、教學、localization、accessibility 與正式效能預算仍屬 Deferred。

因此目前狀態是：PP00～PP08 與實機可玩性 gate 均為 `PASS／Completed`。仍待後續處理的是操作手感、正式 UI、美術與內容，而不是「能否完成一局」。

## 如何在 Unity Editor 啟動

1. 使用 Unity `6000.5.7f1` 開啟 Repository Root。
2. 開啟 `Assets/AegisRTS/Demo/PlayablePrototype/PlayablePrototype_01.unity`。
3. 確認 Console 沒有 compile error 或 missing script。
4. 按 Play。Scene 預設直接建立 New Game；也可按 `Menu` 回主選單測試 New Game／Load Game。
5. 左側 HUD 顯示 session、objective、systems、resources、population、queues、combat、siege、AI 與最近命令；右側顯示 notifications。

主選單提供「開始新遊戲」、「載入進度」與「離開遊戲」。Windows Player 按「離開遊戲」會先清理目前 Prototype session，再正常結束程式；Unity Editor 中同一按鈕會停止 Play Mode，不會關閉 Unity Editor。

Unity Editor 使用目前 Game View 尺寸，不會搶占桌面全螢幕。Windows executable 啟動時會自動切換成主顯示器原生解析度的無邊框全螢幕；例如螢幕為 2560×1440，遊戲就使用 2560×1440。設定面板會顯示實際 `Screen.width × Screen.height` 與目前 Display mode。

如要單獨重驗步兵美術接線，使用 Unity menu：

```text
AegisRTS → Playable Prototype → Run Infantry Smoke Validation
```

它會以 additive scene 進入 Play Mode，驗證雙方步兵、LOD、Team Color renderers 與 anchors，將截圖輸出至同層的 `AegisRTS.BuildValidation/Infantry_GameView.png`，完成後回復原本 Editor scene。

新手說明是 modal：顯示時底層 HUD 與 RTS input 都不可互動。按「我了解了，開始遊戲」、`Enter`、數字鍵盤 `Enter` 或 `Esc` 可關閉；`F1` 可再次開啟／關閉。

如果 scene asset 遺失或引用失效，可執行 Unity menu：

```text
AegisRTS → Playable Prototype → Rebuild Scene
```

Rebuild 會重新建立 scene、綁定 PrototypeNeutral Content／Scenario／Theme，並把 scene 放到 Build Settings 第一順位。

## 鍵鼠操作

| 操作 | 輸入 | 結果 |
| --- | --- | --- |
| 選取單位 | 滑鼠左鍵 | 選取單一 player unit。 |
| 框選 | 按住左鍵拖曳 | 選取框內 player units。 |
| 加入選取／排隊命令 | `Shift` | 依 input context 加選或 queue command。 |
| 移動 | 對地面按右鍵 | 對選取單位派送 `MoveUnitsCommand`。 |
| 攻擊 | 對 hostile unit 按右鍵 | 對選取單位派送 `AttackTargetCommand`，進入 Movement／Combat pipeline。 |
| 切換交戰模式 | HUD「兵種設定」 | 對已選我方單位派送 `SetUnitEngagementModeCommand`；可選堅守陣地、普通、攻擊、反擊。 |
| 選取我方建築 | 左鍵點擊／框選主堡 | 指揮面板自動切到「內政」。 |
| 選取兵種／英雄 | 左鍵點擊／框選單位 | 指揮面板自動切到「兵種設定」。 |
| 選取敵方建築 | 左鍵點擊城門／敵方主堡 | 指揮面板自動切到「攻城行動」。 |
| 停止 | `X` | 派送 Stop。 |
| 原地防守 | `H` | 派送 Hold。 |
| 移動相機 | `WASD` | 平移 RTS camera。 |
| 放大／縮小 | 滑鼠滾輪 | 滾輪向上拉近、向下拉遠；預設速度為原本的 3 倍，相機距離限制在 2.5–40 m。2.5–4 m 會自動降低俯角並聚焦身體。 |
| 調整縮放速度 | `+`／`-` | `+` 加快、`-` 減慢；主鍵盤與數字鍵盤都支援，倍率限制為 ×1～×6，預設 ×3。設定面板會顯示目前倍率。 |
| 聚焦選取 | `F` | 將相機中心移到目前選取單位，再用滾輪拉近。 |

Prototype 的 HUD 按鈕是完整系統流程的可重現入口，不會直接寫 resources、HP、owner 或 objective。所有 mutation 都必須經 CommandBus、domain validator 或 authoritative event／fact。

## 建議人工通關流程

### A. 戰鬥閉環

1. 選取藍色 player units。
2. 右鍵地面，確認單位移動且 selection 保留。
3. 右鍵紅色 hostile unit，確認 HP bar 下降、notification 出現 damage，死亡後 view 消失。
4. 切換「單位姿態」，確認前三種模式會依 0.5／1.0／1.5 倍攻擊距離主動索敵；反擊模式在受擊前不主動攻擊。
5. 點擊任何姿態按鈕後，原本選取的單位必須保持選取；HUD 點擊不得穿透到世界選取或右鍵指令。
6. 分別選取我方主堡、我方兵種、敵方城門，確認頁籤依序為內政、兵種設定、攻城行動；主堡與兵種混合框選時以兵種設定優先。
4. 對已死亡 ID 再派命令時，確認它不再存在於 selection、movement、combat 或 army membership。

### B. 主堡生產／Economy／Tech／Recruit

1. 不建兵營，直接按 `Recruit Infantry`／`Recruit Archer`，確認單位由主堡完成招募。
2. 先嘗試 `Recruit Siege`，確認只因缺少攻城科技而被拒絕。
3. 按 `Research Siege`，再按 `Recruit Siege`；`Build Economy` 是可選的資源升級，不是招募前置。
4. 確認 resources／population 正確變化，新單位出現在 player spawn，並可選取、移動、戰鬥。

### C. Hero／Army／AI

1. 按 `Create Hero Army`，確認 HUD 的 Army snapshot、commander、members、morale／supply。
2. 按 `Move Army` 與 `Attack Enemy`，確認 army members 使用同一批 actor IDs 執行命令。
3. 等待 AI cadence；確認 AI notifications 至少出現 recruit 與 attack／counterattack，而不是直接修改玩家 state。

### D. Fortified City Siege／Capture／Victory

1. 在 Gate 尚未 breach 前按 `Enter Objective` 或 `Attack Stronghold`，確認命令被拒絕。
2. 按 `Start Siege`。
3. 按一次 `Breach Gate` 後等待 8 秒，確認守軍修復城門；若城門曾到 0 HP，修復後 Gate 回到 Closed 並重新封閉 NavMesh。
4. 連續攻擊城門至 0 HP，確認固定城牆仍存在且沒有可攻擊 HP，然後在修復前按 `Enter Objective`。
5. 按 `Attack Stronghold`，確認主堡核心被壓制後 Fortress 與 Territory owner 轉移、主堡視圖保留並換為玩家顏色、Objective Completed、Session Victory。

Development Build 額外提供 `Debug: Trigger Defeat`，只用於重現 Defeat UI／session stop；它不是正常玩家勝負流程的替代品。

### E. Session／Save／Load

1. 在 Playing 按 `Pause`，記錄 tick／resources；等待數秒後確認 simulation 未推進，再按 `Resume`。
2. 在至少一個建造／研究／招募 queue 或移動／戰鬥命令進行中按 `Save`，記下 fingerprint 前 12 字元。
3. 移動、戰鬥或改變進度後按 `Load`，確認 state 與 views 重建，fingerprint 回到存檔狀態。
4. 按 `Restart`，確認取得乾淨 New Game，沒有舊 entities、views 或重複 notification subscriptions。
5. Victory／Defeat 後確認 simulation 停止，但 Restart／Menu 仍可使用。

## Windows Development Build

預設建置輸出：

```text
C:\projects\Unity\AegisRTS.BuildValidation\PlayablePrototype_01.exe
```

從 Unity 執行 build method：

```text
AegisRTS.Editor.PlayablePrototypeSceneBuilder.BuildWindowsDevelopment
```

可先設定環境變數 `AEGIS_PP_BUILD_DIR` 改變輸出資料夾。此 method 會先 rebuild scene，再建立 Windows x86_64 Development Build。Build 完成後需實際啟動 `.exe`，至少完成一次本文件的 A～E；只證明 process 能啟動不等於人工 playable gate 通過。

Player Settings 與 runtime display adapter 都要求 `FullScreenWindow`＋主顯示器原生解析度；runtime log 會輸出 `[PlayablePrototype Display] Native Fullscreen · <width>×<height>`，可用來核對實際要求值。`Alt+Enter` 仍可由 Unity Player 的 Allow Fullscreen Switch 提供暫時切換，但重新啟動時會回到原生解析度全螢幕。

## PP00～PP08 狀態

| Phase | 自動化狀態 | 已完成 | 尚待人工／後續 |
| --- | --- | --- | --- |
| PP00 Composition | PASS | Neutral Content、Scenario、Theme、scene、registry、systems、routers、dispose、boot test。 | Editor 中查看 overlay 與 scene readability。 |
| PP01 Player Combat | PASS | selection/input wiring、move queue、stop／hold、formation、attack、projectile、damage、death／selection cleanup。 | 實際調整框選、camera 與戰鬥手感。 |
| PP02 Economy | PASS | 主堡直募、optional economy building、technology、recruitment、validation、atomic spawn。 | 互動式確認 queue feedback 的可讀性。 |
| PP03 Hero／Army | PASS | 雙 Hero、Army、commander、Add／Split／Merge／Change Commander、move／attack／defend／retreat、death membership fix。 | 人工確認指令面板的可理解性。 |
| PP04 AI | PASS | 真實 read model、共用 commands、建造／招募／合軍／部署／反攻與正常 Defeat path。 | 增加 personality、difficulty 與更細的 AI HUD。 |
| PP05 Siege | PASS | 固定不可破壞城牆、可破壞／修復 Gate、breach seal／rebuild、主堡壓制、owner transfer、victory／defeat tests。 | 玩家守城操作與維修資源成本留待下一輪。 |
| PP06 HUD／Session | PASS | `IHudQuery`／`IHudCommandSink`、完整 panels、notifications、theme/settings、pause、resume、restart、view rebuild。 | 兩種解析度與實際使用者體驗需人工確認。 |
| PP07 Save／Load | PASS | `GameStateCoordinator` envelope、checksum／compatibility、完整 active queues／orders／cooldowns／AI／random state、corrupt rejection、view rebind。 | 未來若要多存檔槽再另行擴充。 |
| PP08 Gate／Build | PASS | 全自動 regression、E2E、deterministic soak、300-unit smoke、Windows build；1280×720／960×540 視覺檢查及 executable 勝利、戰敗、重開、Save／Load 實機操作。 | 後續擴充 target hardware profiling 與正式 UI，不阻擋 Prototype checkpoint。 |

## 現在建議先做什麼

依優先順序：

1. 建立一個明確的 Prototype checkpoint commit／tag，保存目前已通關狀態。
2. 下一輪優先處理 selection／camera／戰鬥手感、command feedback 與正式 UI 架構。
3. 系統操作成熟後，再進行 G01 世界觀與勢力資料。
4. 世界觀與 production art 繼續 Deferred；先不要用正式美術掩蓋玩法、路徑與 UI feedback 問題。

## 人工驗收紀錄範本

執行後將結果補進 `DevelopmentProgress.md`，不要只留在聊天或 commit message：

```markdown
- Manual Playable Gate：
  - Date／Tester：<日期／測試者>
  - Build／Commit：<exe 路徑、Unity 版本、commit>
  - Resolution：<1920x1080 與較小解析度>
  - Editor A～E：<PASS／PARTIAL／FAIL＋實際結果>
  - Executable A～E：<PASS／PARTIAL／FAIL＋實際結果>
  - Console／Player.log：<0 unhandled exception 或錯誤摘要>
  - Issues：<重現步驟、影響、優先級>
```
