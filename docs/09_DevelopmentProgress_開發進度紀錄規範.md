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
- Changed：
  - <實際變更>
- Architecture / API / Data：
  - <影響；沒有則寫 N/A 與原因>
- Tests / Validation：
  - `<命令或操作>`：<PASS / FAIL / 未執行與原因>
- Known Issues / Risks：
  - <限制或 None>
- Next：
  - <下一步>
```

