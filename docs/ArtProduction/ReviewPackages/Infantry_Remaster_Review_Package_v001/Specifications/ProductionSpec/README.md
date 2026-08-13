# AegisRTS RTS Asset Production Specification v1

- Created Date：2026-08-13
- Agent：Codex（本次執行環境未提供可驗證的模型識別碼）
- Project：AegisRTS.FrameworkLab
- Specification Version：1.0
- Document Status：`CURRENT SPECIFICATION`

## 用途

本文件包把目前可玩的 Infantry／Archer Prototype L3 與既有 ArtSpecs，整理為可長期沿用的 **Stylized Fantasy RTS Production Asset Specification System**。它同時服務 AI 生成、人工 DCC 製作、Unity 整合、QA、授權追溯與後續量產。

本版只建立規格與稽核，不修改既有 Unity、Blender、FBX、GLB、Texture、Material、Animator、Animation、Prefab 或 Scene。現有步兵與弓兵是 `GOLDEN_SAMPLE_CANDIDATE`，不是 `PRODUCTION_READY`。

## 狀態與強制詞彙

- `REQUIRED`：進入下一 Gate 或標示 Production Ready 前必須完成。
- `RECOMMENDED`：預設採用；偏離時需在交付報告說明理由。
- `OPTIONAL`：依角色或平台需要選用。
- `CURRENT`：repository 目前實際存在且可由檔案、metadata 或既有測試證據確認。
- `PROPOSED`：本規格的新目標，尚未代表程式、Shader 或資產已實作。
- `VERIFIED`：由檔案、Unity YAML、DCC 只讀稽核或既有結果直接確認。
- `INFERRED`：由路徑、名稱或文件推論，已附依據。
- `UNKNOWN`：repository 無足夠證據；禁止自行補造。

統一製作狀態：

```text
BLOCKOUT → WIP_MODEL → WIP_TEXTURE → WIP_RIG → WIP_ANIMATION
→ INTEGRATION → GOLDEN_SAMPLE_CANDIDATE → PRODUCTION_READY
```

不接受的交付標為 `REJECTED`。只有 Visual Quality、Silhouette、Material、Rig、Animation、Unity Compatibility、RTS Readability、Performance、Documentation 全部通過，才可標示 `PRODUCTION_READY`。

## 核心決策

```text
Visual Identity
→ Silhouette
→ Production Quality
→ Animation Readability
→ Unity Compatibility
→ Performance Optimization
```

效能策略是高品質 LOD0 加上 LOD、Culling、Material Sharing、Shared Skeleton、Animation Optimization、GPU Instancing 與必要時 Impostor；不可因 RTS 鏡頭較遠，就把 LOD0 直接做成粗糙 Prototype。

## Golden Sample Gate

`CHR_Infantry_A_v002` 與 `CHR_Archer_A_v001` 是目前唯一 Golden Sample 候選。兩者都已完成可玩的 Prototype L3／Unity L4 技術整合，但正式造型、材質、蒙皮／動畫 polish、完整來源權利與 production QA 尚未通過。

> **DO NOT MASS PRODUCE**：在 Infantry 與 Archer 都通過本包的 Golden Sample Gate 前，Spearman、Heavy Infantry、Cavalry、Mage、Elite、Special、Hero 與 Buildings 可做 Concept／Backlog／Design，但不得大量進入 Production L3。

## 建議閱讀順序

1. `README.md`
2. `01_Project_Asset_Audit.md`
3. `02_Asset_Pipeline_L1_L4.md`
4. `03_Character_Production_Quality_Standard.md`
5. `04_RTS_Silhouette_and_Readability_Standard.md`
6. `06_Texture_Material_TeamColor_Standard.md`
7. `07_Rig_Skinning_Animation_Standard.md`
8. `08_Golden_Sample_Infantry_Archer.md`
9. `09_Existing_Infantry_Archer_Remaster_Audit.md`
10. `15_Unity_RTS_Asset_Acceptance_Checklist.md`
11. `16_Master_Production_Checklist.md`

## 文件分類

| 類別 | 文件 |
| --- | --- |
| Audit／Migration | `01`、`09`、`Legacy_Spec_Migration.md`、`99` |
| Core Pipeline | `02`、`03`、`04`、`05`、`06`、`07` |
| Golden Sample | `08`、`09` |
| Character／Hero | `03`、`04`、`07`、`10`、`11` |
| Building | `12` |
| Naming／Source Boundary | `13` |
| AI Provenance／License | `14` |
| Unity／DCC QA | `15`、`16` |
| Planning／Agent | `17`、`18` |

## 完整文件索引

| 檔案 | 內容 |
| --- | --- |
| `01_Project_Asset_Audit.md` | Repository、ArtSource、Unity runtime、Infantry／Archer、材質、動畫與 ContentPack 實況稽核 |
| `02_Asset_Pipeline_L1_L4.md` | L1 Concept、L2 Production Sheet、L3 DCC Source、L4 Unity Integration 的 gate 與交付物 |
| `03_Character_Production_Quality_Standard.md` | Stylized RTS 角色的形體、拓樸、品質與 tier 規範 |
| `04_RTS_Silhouette_and_Readability_Standard.md` | 正常遊戲距離、縮圖尺寸、兵種與武器辨識規範 |
| `05_LOD_and_Performance_Standard.md` | LOD0–LOD3／Impostor、幾何預算、材質與量測方法 |
| `06_Texture_Material_TeamColor_Standard.md` | Texture、URP 材質、packed map 與 team-color contract |
| `07_Rig_Skinning_Animation_Standard.md` | Skeleton family、蒙皮、socket、root motion 與 animation delivery |
| `08_Golden_Sample_Infantry_Archer.md` | Golden Sample Gate、鎖定條件與禁止量產規則 |
| `09_Existing_Infantry_Archer_Remaster_Audit.md` | 既有兩兵種逐項 PASS／改善／重建／不可驗證判定 |
| `10_Hero_and_Special_Unit_Standard.md` | Hero／Special 的視覺層級、效能與 gameplay boundary |
| `11_Modular_Character_System.md` | 模組化角色的 family、socket、材質與組合限制 |
| `12_Building_Production_Standard.md` | 建築 footprint、模組、狀態、LOD、碰撞與 Unity gate |
| `13_Asset_Naming_and_Folder_Standard.md` | Source/runtime 分離、命名、版本與證據存放規範 |
| `14_AI_Asset_Provenance_and_License_Standard.md` | AI／第三方／人工來源、授權、prompt 與修改鏈追溯 |
| `15_Unity_RTS_Asset_Acceptance_Checklist.md` | Unity／DCC 視覺、整合、效能、build 與 QA 驗收清單 |
| `16_Master_Production_Checklist.md` | 單資產 L1→L4→Production Ready master gate 與現況 |
| `17_Asset_Production_Backlog.md` | Golden Sample、後續兵種、Hero、Special、Building 與 enabler 排序 |
| `18_Agent_Execution_Guide.md` | 後續 Agent 的唯讀稽核、製作、驗證、紀錄與停止條件 |
| `99_Open_Issues_and_Missing_Information.md` | 缺失資訊、P0–P3 blockers、owner 與關閉證據 |
| `Legacy_Spec_Migration.md` | 舊 ArtSpecs 與新 production contract 的衝突、映射、遷移與 rollback |

## 現有規格的關係

`docs/ArtSpecs/` 保留為 Prototype 歷史規格與個別資產輸入，不刪除、不覆寫。若舊文件與本版衝突，以 `Legacy_Spec_Migration.md` 的分流為準：舊版的低多邊形預算仍可用於 Prototype／低階 LOD；本包的 Production LOD0、正式 L2 Character Sheet 與 Production Ready 定義是新量產 gate。

## 最終交付原則

- Source Art：`ArtSource/`，保存 `.blend`、原始輸入、Prompt、License、DCC renders 與版本紀錄。
- Unity Runtime：`Assets/AegisRTS/Content/Shared/Art/`，只放執行期需要且已通過整合 gate 的 FBX、Texture、Material、Animation、Animator、Prefab。
- Definition／Runtime／View 維持分離；美術 Prefab 不擁有 HP、Damage、Faction 或 Command truth。
- Player 與 AI 共用 gameplay Command；動畫事件只表達 presentation timing。
