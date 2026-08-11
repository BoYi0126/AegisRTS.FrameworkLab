# Development Progress

此檔案是 AegisRTS FrameworkLab 的唯一正式開發進度紀錄。格式與更新規則見 [`docs/09_DevelopmentProgress_開發進度紀錄規範.md`](docs/09_DevelopmentProgress_開發進度紀錄規範.md)。最新紀錄置頂。

## Current Status

- Current Phase：專案初始化完成，準備進入 Phase 01 Core 基礎設施。
- Active Branch：`main`
- Unity Project Version：`6000.5.7f1`
- Specification Baseline：Unity 6.3 LTS；版本差異仍需在 Phase 01 前確認。

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

