# Definition of Done

每個 Phase：

1. Compile 無 Error。
2. Console 無未處理 Exception。
3. Acceptance Criteria 完成。
4. Public API 有 docs。
5. 新核心邏輯有 tests。
6. 有 debug 方法。
7. 無世界觀硬編碼。
8. 無錯誤 asmdef dependency。
9. 不破壞既有 Sandbox。
10. 可清楚 Git review。
11. `DevelopmentProgress.md` 已記錄實際變更、驗證結果、已知問題與下一步，並會與對應變更一起提交。

Framework 最終：兩種背景、攻城與守城、Save/Load、AI 完整循環、Package 可安裝到第二個 Unity project。

## 2026-08-11 Final Validation

- 兩種背景：Three Kingdoms／Fantasy 共用同一 Vertical Slice runtime，PASS。
- 攻城與守城：Sandbox Siege modes 與 Basic Siege sample，PASS。
- Save／Load：Phase 13 persistence acceptance 與 Phase 15 session load path，PASS。
- AI 完整循環與反攻：Sandbox AI／Vertical Slice，PASS。
- Package 第二專案安裝、sample import／compile／play、自製 Content Pack：PASS。
- 原專案 Unity tests：EditMode 156/156、PlayMode 19/19，PASS。
- 乾淨驗證專案 Unity tests：EditMode 3/3、PlayMode 3/3，PASS。
