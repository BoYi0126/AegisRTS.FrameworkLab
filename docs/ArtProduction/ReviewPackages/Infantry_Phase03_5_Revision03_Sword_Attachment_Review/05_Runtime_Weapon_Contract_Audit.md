# 05 — Runtime Weapon Contract Audit

## Search scope

Repository code and production docs were searched for `WeaponSocket_R`, `Socket_R_Hand`, `RightHand`, `Sword`, `Attach`, `Equip`, `Equipment`, `Weapon`, and `AttackImpact`.

## Existing contract

- Production standards and legacy Infantry L3 delivery contract already define `Socket_R_Hand` beneath `RightHand` for short-sword attachment.
- `Socket_WeaponTip` is the separate VFX/trail endpoint contract in the legacy golden sample.
- Current prototype runtime exposes a reusable projectile socket through `PrototypeUnitArtView.ProjectileSocket` and uses it from `PlayablePrototypeBootstrap` / `PrototypeProjectileVisualController`.
- Infantry attack timing uses `AttackImpact`; it is unrelated to weapon-transform ownership.

## Finding

There is no current generic melee `Equipment`, `AttachWeapon()`, or weapon-socket runtime API to extend, and no `WeaponSocket_R` code contract. Adding an Infantry-only runtime manager or a second synonymous socket would create architecture debt. Revision 03 therefore repairs the asset/prefab transform contract only, using the already documented `Socket_R_Hand` name.

The hierarchy is reusable by later Spearman/Hero equipment work because it follows the common `Unit → hand socket → equipment root` pattern, but a generic equipment system remains explicitly deferred. No gameplay/domain/UI code was changed.
