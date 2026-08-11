# Development Progress 開發進度紀錄規範

## 唯一進度檔

專案的正式開發進度固定記錄在 Repository Root：

```text
C:\projects\Unity\AegisRTS.FrameworkLab\DevelopmentProgress.md
```

不要在其他資料夾建立第二份進度檔，也不要以聊天紀錄、Issue 或 commit message 取代它。Issue、PR 與 commit 可以提供細節，但 `DevelopmentProgress.md` 必須保留可在本機直接閱讀的整體脈絡。

## 何時必須更新

每一個會修改 repository 的開發任務，都必須在結束前更新 `DevelopmentProgress.md`，包含：

- C#、測試、工具或 Editor script。
- Unity scene、prefab、asset、設定或 package。
- 架構、API、資料模型或 Content Pack。
- 文件、建置流程、CI 或版本控制規則。
- Bug fix、refactor、效能改善或技術債處理。

只有純問答、唯讀調查或沒有修改 repository 的工作可以不新增紀錄。

開始工作前先讀取最新進度。延續同一個尚未完成的任務時，更新原有紀錄；新的獨立任務才新增一筆，避免同一件事被拆成大量重複紀錄。

## 每筆紀錄必填內容

每筆紀錄至少包含：

1. 日期、Phase／工作項目與狀態：`Planned`、`In Progress`、`Blocked` 或 `Completed`。
2. Goal：這次要解決的問題與範圍。
3. Changed：實際修改的程式、資產、設定與文件。
4. Architecture / API / Data：依賴方向、公開介面或資料格式的影響；沒有則寫 `N/A` 並說明。
5. Tests / Validation：實際執行的命令、測試、Unity 操作與結果；未執行不可寫成通過。
6. Known Issues / Risks：已知限制、外部阻塞、未驗證項目與相容性風險。
7. Next：下一個可執行步驟。

## 詳細紀錄最低標準

從 Playable Prototype 起，所有 repository 修改都必須留下足以讓未參與實作的人重建脈絡、重現驗證並繼續工作的詳細紀錄。禁止只寫「完成某功能」、「修正問題」、「tests passed」或只列 commit message。

每筆紀錄依任務性質必須具體包含：

1. **Baseline**：開始時 branch、Git 狀態、相關既有功能與最近可信測試結果。
2. **Scope**：本次包含與刻意不包含的內容，避免把 Deferred 項目誤認為遺漏。
3. **Changed Files / Assets**：列出主要新增、修改、搬移或刪除的路徑，以及每個檔案／模組的責任；大量機械性檔案可按資料夾分組。
4. **Behavior**：描述修改前後的實際行為、使用者操作流程與錯誤處理，不只描述類別名稱。
5. **Architecture / Dependency**：composition、state ownership、Command／Event／Query flow、asmdef dependency、Unity adapter boundary 與 disposal lifecycle 的影響。
6. **API / Data**：列出新增、變更、移除的 public API／command／event／query／JSON field／save schema；若無影響必須寫 `N/A` 與原因。
7. **Tests**：記錄實際命令或 Unity 操作、test platform、filter／scope、總數、passed／failed／skipped、結果檔或 log，以及沒有執行的驗證與原因。
8. **Acceptance Matrix**：逐項列出 Acceptance Criteria 的 `PASS`／`PARTIAL`／`FAIL`／`NOT RUN` 與證據。
9. **Completed / Not Completed**：明確分開已完成、部分完成、未完成與 Deferred，不能用整體敘述掩蓋缺口。
10. **Known Issues / Risks**：包含 reproduction condition、影響、暫時措施與建議優先級；沒有則寫 `None`。
11. **Git**：結束時 branch、tracked／untracked 狀態、是否 commit／push；不得把未提交變更寫成已推送。
12. **Next**：只列具體、可立即執行且有順序的下一步；第一項應是最高優先級。

若任務跨越多次工作階段，在同一筆紀錄維持 `In Progress` 並持續補充，不要為同一任務建立互相矛盾的多筆完成紀錄。只有所有要求與驗證完成後才能改成 `Completed`。

## Current Status 維護規則

`DevelopmentProgress.md` 頂部 `Current Status` 必須隨每次開發更新，至少反映：

- Current Phase／Milestone 與狀態。
- Active Branch。
- 最近一次可信 EditMode／PlayMode／build 結果。
- 目前最高優先級工作。
- 實際 blocker 或重要規格差異。

如果該次只修改文件或規格，不得沿用舊測試結果暗示新程式已經通過；必須標示「本次未修改 runtime」及本次實際完成的文件驗證。

## 撰寫與 Git 規則

- 使用繁體中文，技術名稱、類型與 API 保留原文。
- 最新紀錄放在最上方；保留歷史，不覆寫或刪除已完成紀錄。
- 內容必須與 repository、測試結果及 Git 狀態一致，不記錄尚未發生的成果。
- 文件或設定工作若沒有 API／測試，必須明確寫 `N/A` 與原因。
- 不記錄密碼、Token、私鑰、個資或其他 secret。
- `DevelopmentProgress.md` 必須與對應變更放在同一個 commit 或 PR；commit 無法記錄自身 hash，可寫 commit message、PR 編號或「本筆紀錄所在 commit」。
- 發現舊紀錄錯誤時新增更正說明，不竄改歷史結果。

## 紀錄範本

```markdown
## YYYY-MM-DD — <Phase / 工作項目>

- Status：In Progress
- Goal：<目標與範圍>
- Baseline：
  - Branch／Git：<開始狀態>
  - Existing Evidence：<相關既有功能與最近測試>
- Scope：
  - In：<本次包含>
  - Out／Deferred：<本次不包含>
- Changed：
  - `<path>`：<責任與實際變更>
- Behavior：
  - Before：<修改前>
  - After：<修改後>
- Architecture / API / Data：
  - Architecture：<ownership／flow／dependency>
  - API：<新增／變更／移除；無則 N/A>
  - Data：<schema／content／save；無則 N/A>
- Tests / Validation：
  - `<命令或 Unity 操作>`：<platform／scope／result count／PASS 或 FAIL>
  - Logs／Results：<路徑或摘要>
- Acceptance：
  - `<criterion>`：<PASS／PARTIAL／FAIL／NOT RUN> — <證據>
- Completed：
  - <已完成項目>
- Not Completed / Deferred：
  - <未完成項目、原因與優先級>
- Known Issues / Risks：
  - <reproduction／impact／workaround／priority；沒有則 None>
- Git：
  - Branch：<branch>
  - Working Tree：<clean 或具體未提交檔案>
  - Commit／Push：<實際狀態>
- Next：
  1. <最高優先級且可立即執行的下一步>
```
