# Phase 01 L2 Conformance Audit

## Audit Result

結果：`CONFORMING / APPROVAL PENDING`。

Phase 01 target 能轉換成可執行的 v003 Primary Forms brief；沒有發現需要在批准前猜測的核心尺寸、造型或 runtime contract。未完成項均已隔離為後續 gate，不得用來把 Phase 01 誤標為 production asset 完成。

## Target-to-Repository Matrix

| Area | Phase 01 target | Repository baseline / standard | Result |
|---|---|---|---|
| Identity | `unit.infantry`、`PF_Unit_Infantry` | Content／Prefab stable IDs 已存在 | `PRESERVE` |
| Direction | 東亞古代重裝步兵＋Stylized Fantasy RTS readability | Legacy world-neutral East-Asian-inspired；production target要求 stylized fantasy quality | `CONFORMING`；仍待使用者批准 |
| Height | target 1.83 m；允許 1.80–1.85 m | v002 1.830 m；legacy 1.75–1.85 m | `CONFORMING` |
| Proportion | 5.2–5.4 heads，preferred約5.3 | legacy 5–5.5 heads | `CONFORMING` |
| Armored width | 0.64–0.70 m | legacy overall width 0.60–0.72 m | `CONFORMING` |
| Shield | 約 0.86 × 0.60 × 0.05–0.07 m | v002／legacy約0.86 × 0.60 × 0.07 m | `CONFORMING` |
| Sword | 約 0.98–1.02 m | legacy 0.90–1.10 m；v002約1.00 m | `CONFORMING` |
| Team Color | 15–25%；scarf、waist cloth、shield、small accents | production與legacy均建議15–25% | `CONFORMING` |
| Primary Forms | helmet／shoulder／chest／waist／boots／shield／sword partial rebuild | current audit已將相同區域列為 improve／partial rebuild | `CONFORMING` |
| LOD0 | 20K–30K；preferred 24K–27K | production standard Standard Unit 20K–35K；v002 4,376 | `CONFORMING`；supersedes legacy LOD0 budget for v003 |
| Skinning | soft body最多4 influences；硬甲／裝備 rigid | production rig standard相同；v002 body max 1 influence | `CONFORMING TARGET`；尚未實作 |
| Neutral pose | A-Pose | production L2／rig規格接受A-Pose | `CONFORMING` |
| Readability | black silhouette；64／32 px；四方向 | production silhouette standard要求128／64／32 px與多方向 | `CONFORMING`；Phase 02 evidence應一併補128 px |
| Runtime | preserve skeleton、socket、events、Animator、anchors、Root Motion Off | v002 Unity整合已驗證相同 contracts | `CONFORMING / MUST PRESERVE` |
| Versioning | preserve v002；new v003 source | naming standard要求versioned source且stable Prefab ID | `CONFORMING` |

## Clarifications Locked by This Audit

1. 原始任務將「既有 L1 multi-view＋asset-specific construction specification＋Phase 02 3D orthographic clay」定義為此 remaster 的 3D production reference 路徑。Phase 01 本身不是新的 AI turnaround，也不是 completed game model。
2. 通用 production spec 的 L2 orthographic要求沒有被刪除；它改由既有 L1與 Phase 02 exact-geometry orthographic Clay 組合滿足。Phase 02 未產生且未批准前，Golden Sample 的 L2 gate仍保持 unchecked。
3. Phase 02 的 20K–30K是 production LOD0 target，不代表本階段要完成 LOD1／LOD2／LOD3／Impostor。完整 LOD chain在後續 phase另行設計與量測。
4. Production material仍應以最多兩個主要material slots為預設；Phase 02不做final texture或shader，因此現在不凍結 packed channel實作。
5. `AttackImpact`、Animator parameters、socket、anchors只可驗證保留，不可由美術改寫 gameplay damage／position authority。

## Scope Decision

- Phase 01：文件、對照、decision lock、approval gate。
- Phase 02：new versioned v003 Primary Forms source與 review captures。
- Later：topology／UV／texture／material、rig／skin、animation、LOD chain、Unity integration、performance與release approval。

## Audit Limitations

- 沒有本次 Unity reimport、tests、build或Profiler結果。
- 沒有新 DCC geometry、v003 source或v003 renders。
- 現有 v002 screenshots只能說明 baseline，不是 v003 acceptance evidence。
- Reviewer／approver身份、target hardware與commercial rights仍未完成。
