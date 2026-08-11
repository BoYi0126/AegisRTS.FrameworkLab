# Agent Code Review / 驗收 Prompt

對 `C:\projects\Unity\AegisRTS.FrameworkLab` 進行架構與功能驗收。

檢查 compile、Console、asmdef、Core dependency、world hardcode、Definition/Runtime/View、UI direct mutation、Player/AI duplicate flow、Save Unity object、God Manager、tests、Acceptance、dead code、API docs，以及 `DevelopmentProgress.md` 是否與實際變更和驗證證據一致。

能安全修正就直接修正並重新驗證。

如果 review 有修改 repository，結束前必須依 `docs/09_DevelopmentProgress_開發進度紀錄規範.md` 更新 `DevelopmentProgress.md`。

最後只可給 PASS / PARTIAL / FAIL，並列證據與未完成項目。
