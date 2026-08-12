# Game Mode、據點與武將分配規則

## 文件目的

本文件定義正式產品的兩種對局模式與兩種據點規則。Framework 只提供世界中立的規則能力；故事勢力、指揮官、武將名單、地圖與平衡值由 Content Pack／Scenario 提供，不得硬寫進 package Runtime。

目前的首要實作是 `fortified-city`（要塞城市）。`constructed-base` 與隨機武將分發只完成規格，尚未排入目前可玩原型。

## 對局模式

### `story-grand-war`：劇情超大地圖大亂鬥

- 地圖、勢力數量、勢力起始位置與外交關係由故事 Scenario 固定。
- 每個勢力使用作者指定的指揮官、武將池、兵種、城池與初始領土。
- 同一 Scenario 在相同版本與 seed 下必須可重現；不得在開局任意交換故事武將所屬勢力。
- 勝負條件可依劇情定義為統一、存活、指定城池、護送、限時或多目標。
- 適合正式世界觀、戰役與大型沙盒；在 G01 世界觀完成前，可先用中立 ID 和 placeholder content 驗證系統。

### `random-commander-war`：隨機指揮官模式

- 開局先建立參戰指揮官，再從本局允許的武將池分發武將到各指揮官陣營。
- 分發必須使用可保存、可重播的 deterministic seed。
- 預設規則：同一武將不可重複、指揮官本人不可被分發、每方人數差不超過 1，並保留陣營／角色／稀有度平衡擴充點。
- 結果必須寫入 runtime roster 與存檔；Definition 不因本局抽選而改變。
- 後續可加入玩家選指揮官、禁用池、權重、保底與蛇形選秀，但不得直接修改英雄 Definition 的固定資料。

## 據點類型

### `constructed-base`：建造型基地

參考傳統基地建造 RTS：開局只有指揮中心，玩家自行建造經濟、生產、科技與防禦建築。所有玩家建築皆可受傷、摧毀與重建；指揮中心被摧毀後的失敗／重建規則由 GameMode 決定。

此類型目前只有既有 Building／Technology／Recruitment／Combat 基礎 API，尚未完成完整可玩模式、建築放置、全建築受擊、基地摧毀勝負與建造 UI。

### `fortified-city`：要塞城市（目前首選）

每座主要城市由地圖預先配置主堡、城牆、城門與內城空間：

| 元素 | 規則 | Authoritative owner |
| --- | --- | --- |
| 主堡 | 城市的招募與科技入口；不要求另建兵營。敵軍進入內城後攻擊主堡核心，核心歸零代表防禦被壓制，隨即執行城市與領土所有權轉移；主堡 GameObject 不銷毀。 | Settlement／Siege／Economy／Recruitment |
| 城門 | 有生命值與護甲，可被合法攻城攻擊破壞；被破壞時開啟導航通道。守方可用 `RepairDefenseStructureCommand` 修復，生命值從 0 恢復時城門關閉並封回外圈缺口。 | SiegeSystem |
| 城牆 | 固定地圖與導航物件，不註冊為可受擊 DefenseStructure；因此無生命值、不可選為攻擊目標、不可摧毀。 | Scene／navigation presentation |
| 生產 | 一般兵種直接由主堡招募。攻城兵器可仍受科技前置限制，但不得要求訓練營建築。 | RecruitmentSystem／TechnologySystem |
| 佔領 | 破門 → 進入內城 → 控制主堡區域 → 壓制主堡核心 → Settlement／Territory owner transaction。不是刪除舊城再生成新城。 | SiegeSystem → SettlementSiegeCaptureSink |

## 目前 Prototype 行為

`PrototypeNeutral` 使用以下資料規則：

```json
{
  "settlementArchetypeId": "fortified-city",
  "destructibleWalls": false,
  "gateRepairEnabled": true,
  "strongholdRecruitmentEnabled": true,
  "captureStrongholdInsteadOfDestroy": true
}
```

可玩流程：

```text
主堡直接招募一般兵種
→ 研究攻城科技
→ 主堡製造攻城兵器
→ 建立英雄軍團
→ 開始攻城
→ 破壞城門（守軍 8 秒後可修復，每次 45 HP）
→ 在修復前進入內城
→ 攻擊主堡核心
→ 城市與領土所有權轉移
→ Victory
```

城牆只有 scene collider／NavMesh 阻擋責任。若未來 Content 將 `destructibleWalls` 設為 true，必須另提供可受擊牆段 Definition、破口導航與修復／重建規則；不可只切 boolean 卻沒有 runtime 實作。

## 尚未完成

- 超大地圖的正式 Scenario、固定故事勢力與勝負條件。
- 隨機模式的指揮官選擇、武將池驗證、deterministic 分發與 roster save／replay。
- 玩家自有城市遭 AI 正式攻城時的守城操作、維修資源成本與工程單位。
- 城門修復動畫、施工中斷、多人同步與平衡值。
- `constructed-base` 的建築放置、全建築受擊與摧毀勝負閉環。
- 正式世界觀名稱、武將、兵種、美術與音訊。

## 實作順序

1. 先以 `fortified-city` 完成可理解、可攻、可修、可佔領的系統閉環。
2. 補上玩家守城與 AI 攻城，讓城門修復不只由原型守軍計時器示範。
3. 建立 Match Setup／Roster Assignment 純 C# 模型與 deterministic tests。
4. 完成 `random-commander-war` 的最小選角／分發畫面。
5. 世界觀確定後才建立 `story-grand-war` 正式勢力與大型地圖資料。
6. 最後再做 `constructed-base`，避免同時維護兩套尚未穩定的建造／佔領循環。

## 驗收條件

- Content 可明確辨識目前據點類型，且 package Runtime 不含產品世界觀名稱。
- 一般單位在未建兵營時仍可由主堡招募；攻城兵器只受資料定義的科技限制。
- 城牆沒有可被攻擊的 runtime structure；城門可破壞並可由守方命令修復。
- 城門由 0 HP 修回正值時恢復 Closed，外圈 breach 回到封閉狀態。
- 主堡核心未被壓制前不得佔領；核心歸零後 Settlement 與 Territory owner 一致轉移，主堡視圖保留並換成新 owner 顏色。
- Save／Load 能保存 gate HP、repair countdown、stronghold HP、siege area 與 capture result。
- Player、AI、Scenario 與 tests 的修改意圖仍經 CommandBus／domain API，不由 HUD 直接寫 HP 或 owner。

## 變更紀錄規範

任何修改本文件所述模式、據點、武將分發、攻城、招募或佔領規則的工作，都必須在同一次變更更新 Repository Root 的 `DevelopmentProgress.md`，至少記錄 Baseline、Scope、Behavior、Architecture、API／Data、Tests、Acceptance、Known Issues、Git 與 Next。聊天內容或 commit message 不能取代正式紀錄。
