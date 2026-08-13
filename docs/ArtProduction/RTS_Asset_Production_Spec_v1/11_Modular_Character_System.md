# 11 — Modular Character System

- Specification Version：1.0

## Current Assessment

- `CURRENT / VERIFIED` Infantry與Archer共用相同23-bone Humanoid naming；Archer body由Infantry pipeline衍生。
- `CURRENT` 沒有正式`Human_Base_Male`／`Human_Base_Female`、模組資料schema、equipment compatibility validator或runtime modular assembler。
- `INFERRED` 技術上可建立共用Human family，但現在直接量產會放大Infantry-derived同質化與single-weight skinning缺陷。

因此模組系統標為`PROPOSED`，必須在Golden Sample視覺與technical lock後啟用。

## Proposed Bases

```text
Human_Base_Male
Human_Base_Female
```

可共用Skeleton Family、core topology rules、UV/texel-density policy、generic locomotion；不要求兩者共用完全相同body mesh或所有armor fit。

## 可替換模組

Head、Helmet、Shoulder、Chest、Gloves、Belt、Leg、Boots、Weapon、Shield、Cape、Accessory。

每個module manifest `REQUIRED`：

- Asset／Version／Skeleton Family／body base compatibility。
- Parent socket或skinned bind contract。
- Bounds、triangles、materials、texture set／atlas。
- Required hidden body regions，避免z-fighting與穿甲。
- LOD0～3對應物；不得只有LOD0 module。
- Team Color channel與provenance。

## Silhouette Families

Modular不等於同一人換裝。每個Unit Class仍需不同：Silhouette、Armor Weight、Body Emphasis、Weapon Size、Pose、Shape Language。

- Infantry：heavy shoulder、shield-side mass、strong lower body。
- Archer：narrow shoulder、open arm space、bow/quiver negative space。
- Heavy Infantry：可使用`SKEL_Human_Heavy`與不同torso/leg base。
- Hero：允許共用skeleton／generic locomotion，但visible unique mesh ratio依Tier H。

## Authoring／Runtime Boundary

- `RECOMMENDED` DCC先組合approved套裝，再輸出單一optimizedSkinnedMesh版本供大軍單位；不要假設每個士兵runtime由12個renderer組裝。
- Hero／少量單位可使用runtime modular assembly，但必須量測draw calls、material count與Animator cost。
- Gameplay Definition只引用stable visual variant ID；UI不能直接改combat state，modular visual也不持有authoritative equipment stats。

## Acceptance

- [ ] 10種跨module組合無seam、z-fighting、skin gap或穿模。
- [ ] 各class黑剪影blind test通過，不靠head／texture區分。
- [ ] 所有modules有完整LOD chain與socket／hidden-region資料。
- [ ] 100／300 units renderer／material／memory profile在target hardware完成。

