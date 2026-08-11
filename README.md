# AegisRTS FrameworkLab

AegisRTS FrameworkLab 是可重複用於不同世界觀的 RTS／領土／攻城 Unity Framework 開發與驗證專案。這個 Repository Root 同時也是唯一的 Unity Project Root。

## 開發環境

- Unity：以 `ProjectSettings/ProjectVersion.txt` 為準
- Render Pipeline：Universal Render Pipeline (URP)
- Input：Unity Input System
- Navigation：Unity AI Navigation
- 第一階段目標平台：Windows x86-64

## 開始使用

1. 使用 Unity Hub 開啟本目錄。
2. 等待 Unity 完成 Package 與 Script 匯入，確認 Console 無 error。
3. 開啟 `Assets/AegisRTS/Demo/Scenes/Bootstrap.unity` 並進入 Play Mode。
4. 所有規格與執行順序請由 [`docs/00_README_開發總覽.md`](docs/00_README_開發總覽.md) 開始閱讀。

開始任何開發前先閱讀 [`DevelopmentProgress.md`](DevelopmentProgress.md)；每次修改 repository 都必須依 [`docs/09_DevelopmentProgress_開發進度紀錄規範.md`](docs/09_DevelopmentProgress_開發進度紀錄規範.md) 同步更新進度。

Framework 原始碼與 UPM 發行包在 `Packages/com.boyi.aegis-rts`，世界觀內容在 `Assets/AegisRTS/Content`，示範與 Sandbox 在 `Assets/AegisRTS/Demo`，專案測試在 `Assets/AegisRTS/Tests`。

## 安裝 Framework Package

在另一個 Unity 專案的 Package Manager 選擇 **Add package from git URL...**，輸入：

```text
https://github.com/BoYi0126/AegisRTS.FrameworkLab.git?path=/Packages/com.boyi.aegis-rts#main
```

也可選擇 **Add package from disk...** 並指定 `Packages/com.boyi.aegis-rts/package.json`。安裝後可從 Samples 分頁匯入 Basic RTS、Basic Combat 或 Basic Siege。
