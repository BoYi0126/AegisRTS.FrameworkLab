# Phase 16 — Framework Package 化

輸出 `com.boyi.aegis-rts`。

```text
Runtime/
Editor/
Tests/
Samples~/
Documentation~/
package.json
```

Runtime 不放某背景專用 Content。

Samples：BasicRTS、BasicCombat、BasicSiege。

使用 SemVer。

用第二個乾淨 Unity project 安裝測試：import sample、compile、play、create own content pack。

## Implementation Acceptance

- 已建立 embedded UPM package：`Packages/com.boyi.aegis-rts`，SemVer `1.0.0`。
- Runtime 已拆為 Core／Gameplay／Presentation／Persistence；Editor validation menu 與 Tests 各自位於正確 package boundary。
- Runtime 搜尋不到 Three Kingdoms／Fantasy 字串或背景 Content；所有背景資料仍在 Lab 專案的 `Assets/AegisRTS/Content`。
- `Samples~` 提供 Basic RTS、Basic Combat、Basic Siege，每個 sample 均包含獨立 asmdef、scene、bootstrap 與 README。
- Package 文件提供 disk／Git URL 安裝方式、assembly boundary 與自製 Content Pack 範例。
- 原專案 package migration regression：EditMode 156/156、PlayMode 19/19 通過。
- 第二個乾淨專案 `C:\projects\Unity\AegisRTS.PackageValidation`：本機 package install、3 samples import、compile 成功。
- 第二專案 EditMode 3/3 通過（2 package smoke＋1 consumer-authored Content Pack）；PlayMode 3/3 通過（三個 sample scenes）。
