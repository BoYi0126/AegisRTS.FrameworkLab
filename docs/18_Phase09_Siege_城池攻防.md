# Phase 09 — Siege / 城池攻防

SiegeArea：OuterArea、Walls、Gates、Towers、Breach、InnerArea、CaptureObjective。

DefenseStructure：Wall/Gate/Tower/Barricade/Trap/Core + extension。

Gate state：Closed/Opening/Open/Closing/Destroyed。

Siege Unit 仍是 Unit + Tags + AttackProfile。

Breach：structure destroyed→event→navigation refresh→new path。

支援 assault、defense、wave defense、survival、escort siege、boss siege。

Acceptance：Attacker 破門→入城→capture→owner change。

## 完成內容

- 新增 Pure C# `SiegeSystem`，管理 Preparing／Active／Breached／InnerAreaContested／CaptureAvailable／Completed／Failed 狀態。
- 完成 OuterArea、Walls、Gates、Towers、Breach、InnerArea、CaptureObjective 區域模型與進入規則。
- 新增 `DefenseStructureDefinition` 與 runtime `DefenseStructureProfile`／snapshot，支援 Wall、Gate、Tower、Barricade、Trap、Core 與自訂 extension type。
- Gate 支援 Closed→Opening→Open→Closing→Closed 狀態轉移；摧毀後進入 Destroyed terminal state。
- Siege attacker 經 `CombatSiegeAttackerQuery` 讀取既有 Unit 的 Faction、Tags 與 `AttackProfile`，不建立 SiegeUnit 繼承體系。
- Gate／Wall 摧毀後發布 `BreachCreatedEvent`，並透過 `ISiegeNavigationSink` 要求 navigation backend refresh。
- Core destroyed、commander killed、defenders cleared、capture objective controlled 轉成既有 `CaptureCondition` flags。
- `SettlementSiegeCaptureSink` 重用 Phase 07 capture transaction，同步 Settlement、Faction、Territory ownership indices。
- 提供 Assault、Defense、WaveDefense、Survival、EscortSiege、BossSiege mode data；Wave／Survival 有完成條件與 time limit flow。
- 新增七種 Siege commands、`SiegeCommandRouter`、events、query、rule／navigation／capture extension interfaces。
- 三個 Content Packs 各新增一個世界觀專屬 gate definition；Fantasy 使用 Arcane Gate 而不修改 Siege 核心。
- `Sandbox_Siege` 新增破門→navigation refresh→入城→佔領→owner change 自動驗收與 debug visual／HUD。

## 驗收結果

- Unity EditMode：95/95 passed；Phase 09 新增 15 cases。
- Unity PlayMode：11/11 passed；Phase 09 新增 2 Sandbox_Siege cases。
- Attacker 破門→Breach event→Navigation refresh→InnerArea→CaptureObjective→Settlement／Territory owner change：PASS。
- Gate transition、armor damage、target tags、Combat death bridge、capture conditions、六種 mode、CommandBus routing：PASS。
- Gameplay `UnityEngine` reference scan：0；三個 Content Pack JSON parse／load／validate：PASS。

## 後續擴充界線

- `ISiegeNavigationSink` 已定義破口刷新邊界；Sandbox 使用 recording adapter，正式 NavMesh carve／surface rebuild 待 production scene adapter。
- Gate Opening／Closing 目前由 command 驅動狀態轉移；動畫時間與 collider 切換由 Presentation adapter 接入。
- Trap trigger、Tower auto-target、Escort payload movement 與 Boss ability 邏輯由既有 Combat／Movement／Scenario 系統組合，不複製到 SiegeSystem。
