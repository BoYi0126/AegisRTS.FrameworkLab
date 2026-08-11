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

Framework 原始碼在 `Assets/AegisRTS/Framework`，世界觀內容在 `Assets/AegisRTS/Content`，示範與 Sandbox 在 `Assets/AegisRTS/Demo`，測試在 `Assets/AegisRTS/Tests`。

