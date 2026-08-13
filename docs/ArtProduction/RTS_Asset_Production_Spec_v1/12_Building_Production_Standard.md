# 12 — Building Production Standard

- Specification Version：1.0
- Scope：Stylized RTS buildings；不得直接套用Character L3 checklist

## Required Building Brief

| 項目 | REQUIRED內容 |
| --- | --- |
| Building Footprint | 精確X/Z公尺外包絡、屋簷overhang與grid anchor |
| Grid Occupancy | cell size、旋轉、placement clearance；未知時`TBD` |
| Pivot | bottom center或modular snap pivot，整套一致 |
| Entrance | 朝向、淨寬／高、approach corridor |
| Rally Point | local position、可達性與spawn path |
| Selection Bounds | ground outline、health/selection anchors |
| Collision | 少量Box／Capsule proxy；禁止整棟Mesh Collider作navigation |
| Navigation Blocking | static／dynamic、gate blocker、rebuild／carve policy |
| Construction State | foundation／frame／complete或明確Deferred |
| Damage State | 25／50／75% sockets或mesh/decal方案 |
| Destruction | gameplay規則、animation、collapse bounds、cleanup |
| Rubble | 是否阻擋navigation、生命周期、LOD |
| LOD | LOD0～3／optional impostor、module seam |
| Team Color | 8–15% flags／cloth／trim，runtime replaceable |
| Lighting／Material | URP、lightmap UV、normal／roughness、emission policy |
| Animation | gate／production／mechanism，Root/Pivot不漂移 |
| VFX Socket | hit、fire、repair、capture、spawn、chimney等stable points |

## Stylized Visual Standard

遠距功能辨識優先於小雕花。Roof、Tower、Entrance、Weapon Platform、Footprint與faction shape language必須形成Primary silhouette。Barracks、Archery Range、Stable、Mage Tower、Town Center即使純黑也要有合理辨識度，不能只換Texture／招牌。

## Current Project Building Backlog

| Existing ID／Spec | Current State | Key Contract |
| --- | --- | --- |
| `settlement.player-city` | Placeholder | 5×7 m、入口Z-、主堡直募 |
| `settlement.enemy-fortress`／`structure.stronghold-core` | Placeholder | 2.4×3.6 m stronghold、capture保留building |
| `structure.gate` | Placeholder | 穿越X、寬沿Z、Closed/Damaged/Breached/Repairing |
| `structure.wall.fixed` | Scene-only fixed wall concept | 不註冊Siege HP、不可摧毀 |
| `settlement.village` | Placeholder | 4×4 m、neutral/captured team state |
| `building.economy` | Placeholder／optional | footprint尚未由grid鎖定 |
| `building.recruitment` | Deferred for constructed-base | current fortified-city不要求 |

不得將`structure.wall.fixed`誤做可破壞牆；不得讓Gate debris重新堵住breach；Stronghold Core HP歸零是壓制／capture condition，不代表銷毀整棟主堡。

## Geometry／Materials

- Building LOD0按complexity與screen size另訂；現有個別ArtSpecs的5K–35K是Prototype/legacy參考，production可在visual review後調整。
- 重複wall/roof/module優先shared atlas與trim sheet。
- Modular seams需在10連段、90°轉角、正反光照下無裂縫／亮縫。
- Team Color與faction architecture分離；占領只換team layer仍需合理。

## Acceptance

- [ ] Graybox footprint overlay完全吻合，entrance／rally／selection不被Collider擋住。
- [ ] Closed／Breached navigation與visual state同步。
- [ ] 純黑、32／64 px與四方向可辨功能。
- [ ] Neutral／friendly／enemy切換不複製整棟mesh。
- [ ] Construction／damage／destruction／rubble每項有`CURRENT/PROPOSED/DEFERRED`，不可留模糊。
- [ ] 100+ visible modules的draw call、LOD、shadow與lightmap profile完成。

