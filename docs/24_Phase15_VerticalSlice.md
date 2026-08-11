# Phase 15 — Vertical Slice

地圖：Player City → Village → Enemy Fortress。

2 Faction、各 1 Hero；Infantry/Archer/Cavalry/SiegeUnit；2 Resources；基礎 recruitment/economy buildings。

完整 Loop：Start→Income→Recruit→Army→Move→Field Battle→Siege→Break Gate→Enter→Capture→Victory。

AI 可反攻玩家主城。

具備 New Game、Load、Pause、Settings minimum、Victory/Defeat/Restart。

最後建立 DemoThreeKingdoms 與 DemoFantasy，禁止複製 Combat/Siege/AI 核心。

## Implementation Acceptance

- `VerticalSlice_01` 已加入 Build Settings，預設載入 Three Kingdoms，並可在同一 runtime 切換 Fantasy 後 Restart。
- 兩套 `VerticalSliceContentPack.json` 均提供 2 Resources、4 unit roles、2 Heroes、economy／recruitment buildings、3 settlements、gate 與 counterattack AI profile。
- 共用流程固定為 Start→Income→Recruit→Army→Move→Field Battle→Siege→Break Gate→Enter→Capture→Victory。
- `VerticalSliceSimulation` 僅協調既有 Faction／Territory／Settlement／Economy／Recruitment／Army／Combat／Siege／AI；未建立世界觀專屬 Combat、Siege 或 AI 核心。
- `GameSessionController` 提供 New Game、Load、Pause／Resume、minimum Settings、Victory／Defeat 與 Restart 狀態轉換。
- Unity 6000.5.7f1 驗證：EditMode 154/154、PlayMode 19/19 通過。
