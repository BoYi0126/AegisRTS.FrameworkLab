# Phase 06 — Hero / Army / Command

## 目標

- Hero 使用既有 Unit entity，加上 Hero／Leadership／Ability component，不建立第二套 Combat。
- Army state：ArmyId、Faction、Commander、UnitIds、Formation、Morale、Supply、Order。
- Commands：Create／Merge／Split／AssignCommander／Move／Attack／AttackSettlement／Defend／Retreat。
- Morale／Supply 由 optional rule 控制。
- Acceptance：Hero + 20 infantry 可建軍、拆分、合併、換 commander。

## 完成內容

- `HeroSystem` 保存 hero-only component，包含 definition identity、Faction、Leadership、Ability IDs 與目前 ArmyId。
- `HeroDefinition` 與三個 Content Pack 新增 world-neutral `leadership`，並由 validator 驗證 finite、non-negative。
- `HeroProfile.FromDefinition` 將 immutable authoring definition 轉為指定 faction 的 runtime hero component。
- `ArmySystem` 擁有軍團 composition、commander、formation、optional morale／supply 與 current order。
- `ArmyCommandRouter` 將九種 commands 的 validator／handler 註冊到共用 `CommandBus`；非法命令在 mutation 前拒絕。
- Create／Split／Merge 會原子更新 unit membership 與 hero ArmyId；跨 faction merge、重複 membership、非 hero commander、移走 source commander 等操作會拒絕。
- `IArmyMembershipSink` 將 membership 更新傳給其他 runtime system；`CombatArmyMembershipSink` 更新 Combat snapshot 的 ArmyId。
- `IArmyOrderExecutor` 隔離 Army state 與 Movement／Combat；`GameplayArmyOrderExecutor` 將 Move／Defend／Retreat 交給 `MovementSystem`，Attack／AttackSettlement 交給 `CombatSystem`。
- `IArmyQuery` 提供 immutable army snapshots 與 unit-to-army query。
- `Sandbox_Combat` 新增獨立 `ArmySandboxBootstrap`，自動建立 Hero + 20 infantry，執行 split、merge、commander change 與 orders，並顯示 debug HUD。

## 驗收結果

- Unity EditMode：60/60 passed；Phase 06 新增 8 cases。
- Unity PlayMode：6/6 passed；Phase 06 新增 2 cases。
- Hero + 20 infantry：建立 21-member army，拆出 10-member army，再合併回 21 members 並更換 commander，PASS。
- Create／Merge／Split／AssignCommander／Move／Attack／AttackSettlement／Defend／Retreat command routing，PASS。
- Morale／Supply enabled／disabled、clamp、Combat ArmyId propagation，PASS。

## 後續擴充界線

- Defend 目前把 army 移至 defense position 並保存 Defend order；到達後 hold／engagement policy 由 Movement／Combat coordinator 擴充。
- Morale／Supply 已具備 optional state 與調整 API；消耗、恢復、潰退門檻留給 Economy／AI／Scenario rules。
- Army 不自行判斷 settlement entity type；AttackSettlement 的目標合法性將由 Phase 07 settlement query validator 補上。
