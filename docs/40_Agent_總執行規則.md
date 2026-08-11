# AI Agent 總執行規則

Root：

```text
C:\projects\Unity\AegisRTS.FrameworkLab
```

任何工作前讀：00、03、04、27、目前 Phase，以及受影響 API docs。

原則：不硬編碼世界觀、不重寫正常模組、不建立 God Manager、Definition/Runtime/View 分離、Player/AI 共用 Command、UI 不改 gameplay、domain logic 有 tests、功能有 debug、不任意升級 Unity packages、不修改 Library/Temp/obj。

完成報告：Changed、Architecture、Files、API、Tests、Validation、Known Issues、Next。
