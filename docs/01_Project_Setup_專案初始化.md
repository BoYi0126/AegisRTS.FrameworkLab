# Project Setup — 專案初始化

## Unity Project Root

```text
C:\projects\Unity\AegisRTS.FrameworkLab
```

這就是完整 Unity Project Root。不要另外在 Visual Studio 建第二個 C# Solution 當主要專案。

C# 原始碼前期放在：

```text
C:\projects\Unity\AegisRTS.FrameworkLab\Assets\AegisRTS
```

Framework 穩定後才在 Phase 16 搬成 Unity Custom Package。

## 根目錄判斷

至少應存在：

```text
Assets/
Packages/
ProjectSettings/
```

## Package 建議

透過 `Window > Package Manager` 管理：

- Input System
- AI Navigation
- Test Framework
- URP（Universal 3D template 已建立）
- TextMeshPro（若專案已有則沿用）

不要為了追最新版任意升級；使用 Unity 6.3 專案可用的相容版本。

## Platform

第一階段：Windows x86-64。

## Scene

建立：

```text
Assets/AegisRTS/Demo/Scenes/Bootstrap.unity
Assets/AegisRTS/Demo/Scenes/Sandbox_RTS.unity
Assets/AegisRTS/Demo/Scenes/Sandbox_Combat.unity
Assets/AegisRTS/Demo/Scenes/Sandbox_Siege.unity
Assets/AegisRTS/Demo/Scenes/Sandbox_AI.unity
```

不要先做正式三國世界地圖。

## docs

所有規格文件放：

```text
C:\projects\Unity\AegisRTS.FrameworkLab\docs
```

不要放進 Assets，避免 Unity 匯入不必要文件。

## 初始驗收

- Unity 可正常開啟。
- Console 無 error。
- URP 正常。
- Visual Studio 可由 Unity 開啟 C#。
- Bootstrap 可 Play。
- Git repository 正常。
