# AI Agent 總執行規則

Root：

```text
C:\projects\Unity\AegisRTS.FrameworkLab
```

任何工作前讀：00、03、04、09、27、`DevelopmentProgress.md`、目前 Phase，以及受影響 API docs。

原則：不硬編碼世界觀、不重寫正常模組、不建立 God Manager、Definition/Runtime/View 分離、Player/AI 共用 Command、UI 不改 gameplay、domain logic 有 tests、功能有 debug、不任意升級 Unity packages、不修改 Library/Temp/obj。

任何修改 repository 的工作在結束前都必須依 `docs/09_DevelopmentProgress_開發進度紀錄規範.md` 更新根目錄 `DevelopmentProgress.md`。紀錄必須反映實際測試與 Git 狀態，不得把未執行項目寫成通過。

完成報告：Changed、Architecture、Files、API、Tests、Validation、Known Issues、Next。
