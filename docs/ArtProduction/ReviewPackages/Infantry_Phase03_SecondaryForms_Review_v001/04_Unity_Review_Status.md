# Unity Review Status

Status: `MANUAL UNITY REVIEW REQUIRED`

本階段未建立或替換正式 `PF_Unit_Infantry`，也未將 v004 匯入 production runtime folder。這符合使用者的 Runtime Prefab 禁止事項，但 Unity Close／Medium／RTS Normal／Far captures 尚未執行。

## Safe manual review procedure

1. 將 `CHR_Infantry_A_v004` 匯入隔離的 Review-only folder，禁止覆蓋既有 FBX／Prefab。
2. 建立 temporary `PF_Unit_Infantry_v004_Review`，不改 `PF_Unit_Infantry` reference。
3. 套用 current URP lighting與簡單 Material-ID／Clay，不套 final textures。
4. 驗證 1 Unit = 1 m、A-Pose、左右武器、grounding、shoulder／waist／shield clearance。
5. 拍攝 `Unity_Close.png`、`Unity_Medium.png`、`Unity_RTS_Normal.png`、`Unity_Far.png`。
6. 記錄 importer、Console、camera distance與結果後再由 reviewer 決定。

在 manual review完成前，Unity gate只能標為 `NOT RUN / MANUAL REQUIRED`。
