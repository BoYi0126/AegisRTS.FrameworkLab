# 10 — Hero and Special Unit Standard

- Specification Version：1.0
- Status：`PROPOSED`；目前所有Hero／Special仍為Placeholder或Backlog

## Hero

Hero不得只是普通士兵換Texture。每名Hero `REQUIRED`：Unique Silhouette、Unique Head、Unique Armor、Unique Weapon、Unique Color Identity、Unique Idle Personality、Unique Skill Animation。

可以共用Skeleton、Base Topology、Generic Locomotion；但至少三個Primary silhouette tokens、主武器、頭／肩輪廓與Idle pose必須在64 px不看名字可辨識。Hero match Team Color與character identity palette必須分層，不能將整個unique palette染成team hue。

`CURRENT` Content只有world-neutral `hero.commander`、`hero.lieutenant`、`hero.opponent`，皆綁`PF_Hero_Placeholder`；正式角色姓名、faction與visual identity為`TBD`。

## Asset Tiers

| Tier | 類型 | LOD0 tris | Texture | Animation Budget | Unique Mesh Ratio | VFX／Material |
| --- | --- | ---: | --- | --- | ---: | --- |
| Tier A | Standard | 20–35K | 2K | 5 required＋0–2 variants | 20–40% | simple hit/attack；1–2 materials |
| Tier A+ | Elite | 25–45K | 2K | 6–8 clips | 40–60% | moderate accent VFX；≤3 materials |
| Tier S | Special | 30–50K | 2K，必要時4K | 8–12 clips | 60–80% | unique attack/skill VFX；≤3 materials |
| Tier H | Hero | 40–70K | 2K/4K | 12+含unique idle/skill/victory | 80–100% visible identity | character-specific VFX；≤4 materials |

Triangle與ratio是initial guideline。Unique Mesh Ratio指從正常RTS視角可見且不只是材質換色的獨特mesh面積估計；不應用隱藏幾何灌數字。

## Special Unit

Special位於Standard→Elite→Special→Hero之間。它需有獨特戰術role、primary silhouette、animation timing與VFX語言，但不必擁有Hero級face／portrait／personality clips。

`CURRENT` 可確認的Special候選只有`unit.siege`，個別ArtSpecs將其設計為light battering ram；在Golden Sample lock前只可做L1／technical footprint spike，不進大量L3。

## Acceptance

- [ ] Tier、budget與reuse來源寫入asset brief。
- [ ] 64／32 px blind test不與Standard混淆。
- [ ] Hero unique idle／skill不影響gameplay authority或Root Motion contract。
- [ ] Shared skeleton／topology沒有讓所有角色變成同一人換裝。
- [ ] VFX可pool、可team-readable且不遮蔽戰場。

