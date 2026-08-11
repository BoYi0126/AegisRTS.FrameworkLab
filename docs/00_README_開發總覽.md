# AegisRTS FrameworkLab — 完整開發總覽

## 專案根目錄

本專案唯一根目錄固定為：

```text
C:\projects\Unity\AegisRTS.FrameworkLab
```

後續所有文件提到的「專案根目錄」、「Repository Root」、「Unity Project Root」都指這個位置。

```text
C:\projects\Unity\AegisRTS.FrameworkLab
├─ Assets/
├─ Packages/
├─ ProjectSettings/
├─ UserSettings/
├─ docs/
├─ DevelopmentProgress.md
├─ .gitignore
└─ README.md
```

`Library/`、`Temp/`、`Logs/`、`obj/` 等為 Unity 產生內容，不作為核心原始碼。

## 專案定位

AegisRTS 是可重複利用於三國、中世紀、奇幻、科幻等背景的 RTS / Territory / Siege Framework，不是單一三國遊戲。

FrameworkLab 同時負責：

1. 開發 Framework。
2. 放置 Demo / Sandbox。
3. 驗證 Vertical Slice。
4. 自動化測試。
5. 驗證多 Content Pack。
6. 最後輸出可重用 Unity Package。

## 技術基準

- Unity 6.3 LTS
- Universal 3D / URP
- C#
- Visual Studio
- Unity Input System
- Unity AI Navigation / NavMesh
- Unity Test Framework
- Windows PC 優先
- Single-player 優先
- 3D world，Presentation 可使用 2D / 2.5D / 3D

## 文件閱讀順序

### 專案初始化
1. `01_Project_Setup_專案初始化.md`
2. `02_Project_Structure_完整目錄結構.md`
3. `03_Naming_Namespace_Asmdef_命名規範.md`
4. `04_Architecture_核心架構與依賴規則.md`
5. `05_Unity_Assets_Scene_Prefab_規範.md`
6. `06_Git_版本控制與提交規範.md`
7. `07_Data_Model_核心資料模型總表.md`
8. `08_ContentPack_世界觀重用規範.md`
9. `09_DevelopmentProgress_開發進度紀錄規範.md`

### Framework Phase
10. `10_Phase01_Core基礎設施.md`
11. `11_Phase02_DataDriven_ContentPack.md`
12. `12_Phase03_RTS輸入選取與相機.md`
13. `13_Phase04_移動尋路與編隊.md`
14. `14_Phase05_單位戰鬥與能力.md`
15. `15_Phase06_Hero_Army_Command.md`
16. `16_Phase07_Faction_Settlement_Territory.md`
17. `17_Phase08_Economy_Recruit_Build_Tech.md`
18. `18_Phase09_Siege_城池攻防.md`
19. `19_Phase10_AI.md`
20. `20_Phase11_GameMode_Scenario_Objective.md`
21. `21_Phase12_UI_UX.md`
22. `22_Phase13_Save_Replay_Debug_Test.md`
23. `23_Phase14_Performance.md`
24. `24_Phase15_VerticalSlice.md`
25. `25_Phase16_Package_Framework化.md`

### Game Production
26. `30_GameProduction_總覽.md`
27. `31_GamePhase_G01_G04_世界觀勢力兵種英雄.md`
28. `32_GamePhase_G05_G08_地圖劇本美術音訊.md`
29. `33_GamePhase_G09_G12_平衡教學Polish發布.md`
30. `34_PlayablePrototype_01_總覽與範圍.md`
31. `35_PlayablePrototype_01_分階段實作計畫.md`
32. `36_PlayablePrototype_01_現況缺口與驗收矩陣.md`

### Agent / Art
33. `40_Agent_總執行規則.md`
34. `41_Agent_Phase執行Prompt.md`
35. `42_Agent_CodeReview與驗收Prompt.md`
36. `50_AI_Art_Pipeline.md`
37. `51_Art_Bible_Template.md`
38. `60_第一階段實際執行順序.md`

## 最終驗證

至少用同一套 Framework 做：

```text
Demo A：三國風格攻城
Demo B：奇幻風格守城
```

兩者共用 Core、Combat、Movement、Army、Settlement、Siege、AI、Save、UI Framework；只替換 Content、Art、Audio、Rules、Scenario、Theme 與必要 Extension。
