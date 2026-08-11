# Definition of Done

每個 Phase：

1. Compile 無 Error。
2. Console 無未處理 Exception。
3. Acceptance Criteria 完成。
4. Public API 有 docs。
5. 新核心邏輯有 tests。
6. 有 debug 方法。
7. 無世界觀硬編碼。
8. 無錯誤 asmdef dependency。
9. 不破壞既有 Sandbox。
10. 可清楚 Git review。
11. `DevelopmentProgress.md` 已記錄實際變更、驗證結果、已知問題與下一步，並會與對應變更一起提交。

Framework 最終：兩種背景、攻城與守城、Save/Load、AI 完整循環、Package 可安裝到第二個 Unity project。

## 2026-08-11 DoD Revalidation — `bb80db2`

本次以 Unity `6000.5.7f1` 重新執行全部 release gates。Phase 01／02 歷史紀錄中的 Unity Licensing 阻塞已由本次實際 Test Runner 結果解除；歷史紀錄保留，不回寫成當時已通過。

### Phase gates

| # | Gate | 結果 | 證據 |
| --- | --- | --- | --- |
| 1 | Compile 無 Error | PASS | FrameworkLab 與乾淨安裝專案的 EditMode／PlayMode runner 均完成，四份 logs 無 `error CS` 或 `Compilation failed`。 |
| 2 | Console 無未處理 Exception | PASS | 四份 logs 掃描 `Unhandled`、`NullReferenceException`、`Aborting batchmode`、`Test run failed`，0 hits。 |
| 3 | Acceptance Criteria 完成 | PASS | Phase 01～16 的 domain、Sandbox、Vertical Slice 與 package acceptance 均由本次測試及下方 Framework gates 覆蓋。 |
| 4 | Public API 有 docs | PASS | Repository `docs/26_Framework_API_目標介面.md` 與 package `Documentation~/FrameworkApi.md` 均存在。 |
| 5 | 新核心邏輯有 tests | PASS | FrameworkLab EditMode 159/159、PlayMode 19/19；package consumer EditMode 6/6、PlayMode 3/3。 |
| 6 | 有 debug 方法 | PASS | Runtime 有 25 個 public `GetDebugSummary()` 與 21 個 public Snapshot types，Sandbox 亦提供 diagnostics。 |
| 7 | 無世界觀硬編碼 | PASS | Package Runtime 掃描 Three Kingdoms／Fantasy／特定種族與中文世界觀詞彙，0 hits；背景內容只存在 Lab Content JSON。 |
| 8 | 無錯誤 asmdef dependency | PASS | Core 無 references；Gameplay 只依賴 Core；Persistence 只依賴 Core／Gameplay；三者 `noEngineReferences=true`。 |
| 9 | 不破壞既有 Sandbox | PASS | 六個 Demo scenes 均在 Build Settings；FrameworkLab PlayMode 19/19。 |
| 10 | 可清楚 Git review | PASS | 驗收前 `HEAD`／`origin/main` 同為 `bb80db2` 且工作樹乾淨；本次只修改 DoD 與進度文件。 |
| 11 | DevelopmentProgress 已記錄 | PASS | 根目錄 `DevelopmentProgress.md` 新增本次結果、風險與下一步，須與本文件一起提交。 |

### Framework gates

| Gate | 結果 | 證據 |
| --- | --- | --- |
| 兩種背景 | PASS | DemoThreeKingdoms／DemoFantasy 各有 Content Pack 與 Vertical Slice scenario，共用同一 `VerticalSliceSimulation`。 |
| 攻城與守城 | PASS | Siege／Vertical Slice EditMode acceptance、Sandbox Siege PlayMode 與 Basic Siege consumer sample 通過。 |
| Save／Load | PASS | Persistence／Replay EditMode acceptance 與 GameSession Load path 通過。 |
| AI 完整循環 | PASS | Utility AI、Sandbox AI、Vertical Slice AI 反攻與 stall recovery tests 通過。 |
| Package 可安裝到第二專案 | PASS | `C:\projects\Unity\AegisRTS.PackageValidation` 以 `file:` 安裝；EditMode 6/6、PlayMode 3/3，三個 imported samples 可執行。 |

### Static validation

- Package：`com.boyi.aegis-rts` `1.0.0`，最低 Unity manifest version `6000.0`，3 samples。
- Pure C# boundary：Core／Gameplay／Persistence 的 UnityEngine reference scan 為 0。
- Asset integrity：307 個 asset GUID，0 duplicates，0 missing file `.meta`。
- Architecture：package Runtime 無世界觀字串、無 `GameManager`／`FrameworkManager` 類型，舊 `Assets/AegisRTS/Framework` 不存在。

### Remaining release risks

- Package 仍為 `UNLICENSED`；公開散布前必須由擁有者選定授權。
- Git URL 範例目前追蹤 `#main`；正式 release 建議改用 immutable tag。
- Demo／samples 使用 primitive、IMGUI 與 placeholder assets，不代表 production art、UX 或最終硬體效能。
- 規格文件寫 Unity 6.3 LTS，實際驗證版本為 `6000.5.7f1`；版本命名對應仍待確認。

## 2026-08-11 Final Validation

- 兩種背景：Three Kingdoms／Fantasy 共用同一 Vertical Slice runtime，PASS。
- 攻城與守城：Sandbox Siege modes 與 Basic Siege sample，PASS。
- Save／Load：Phase 13 persistence acceptance 與 Phase 15 session load path，PASS。
- AI 完整循環與反攻：Sandbox AI／Vertical Slice，PASS。
- Package 第二專案安裝、sample import／compile／play、自製 Content Pack：PASS。
- 原專案 Unity tests：EditMode 156/156、PlayMode 19/19，PASS。
- 乾淨驗證專案 Unity tests：EditMode 3/3、PlayMode 3/3，PASS。
