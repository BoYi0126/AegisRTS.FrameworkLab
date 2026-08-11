# Phase 05 — Unit / Combat / Ability

## 目標

- Unit State：HP、state、target、cooldown、status、faction、army。
- AttackProfile：damage、type、range、cooldown、windup、projectile、splash、target tags。
- Damage Pipeline：Base → modifier → defense → resistance → status → final → HP → death。
- Ability Target：Self／Unit／Point／Area／Direction／Settlement。
- Ability Activation：Active／Passive／Aura／Triggered／Toggle。
- Status：buff／debuff／stun／slow／root／shield／DoT。

## 完成內容

- Pure C# `CombatSystem` 擁有所有 runtime HP、attack state、target、cooldown、status、projectile 與 death 狀態。
- `AttackProfile`、`DefenseProfile`、`CombatantProfile`、`CombatantSnapshot` 為 Unity-independent model。
- 傷害依序處理來源 buff／debuff、armor、physical／magical resistance、shield、final damage、HP 與 death event。
- 近戰支援 range、windup 與 cooldown；遠程支援有飛行時間的 projectile；impact 支援 enemy-only splash。
- target tags 可限制可攻擊目標。
- Status 支援 buff、debuff、stun、slow、root、shield、DoT；同 ID 重新套用時 refresh。
- `AbilityProfile` 支援全部 target／activation 分類；`UseAbilityCommand` 提供 Active／Toggle 的手動施放入口與 cooldown。
- `ICombatQuery` 提供 UI、AI、測試與 Presentation 唯讀 snapshot。
- `UnityCombatDriver` 與 `UnityCombatView` 只負責 transform 同步、血條、顏色、死亡外觀與 projectile visual，不保存 authoritative HP。
- `Sandbox_Combat` 會自動執行 melee、ranged projectile、splash、area damage、DoT 與 death 驗收情境。

## 驗收結果

- Unity EditMode：52/52 passed；Phase 05 新增 8 cases。
- Unity PlayMode：4/4 passed；Phase 05 新增 2 cases。
- Acceptance：melee／ranged／projectile／splash／status／death 全部通過。

## 後續擴充界線

- Passive／Aura／Triggered 已保留 activation type；自動觸發條件與 aura refresh policy 由後續 Hero／Army／AI 系統注入，不在手動 `UseAbilityCommand` 中猜測規則。
- Direction target 已保留方向資料；cone／line shape query 可在新增 spatial query 後擴充。
- 追擊至 attack range 仍需由 Movement 與 Combat 的上層 coordinator 串接。
