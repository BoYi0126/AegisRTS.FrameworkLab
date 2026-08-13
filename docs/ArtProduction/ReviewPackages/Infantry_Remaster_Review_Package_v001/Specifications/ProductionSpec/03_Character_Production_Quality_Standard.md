# 03 — Character Production Quality Standard

- Specification Version：1.0
- Target：`Stylized Fantasy RTS Production Character`

## 視覺目標

`REQUIRED`：英雄式但可量產的比例、厚實清楚的輪廓、誇張但合理的裝備、大色塊與強材質分離、受控手繪表面、可辨識 shape language。不得只寫「Warcraft-like」；實作上要求：

- 頭、手、肩、武器在正常 RTS 距離仍形成可讀形狀。
- Armor／Cloth／Leather／Wood／Skin至少以 value、roughness intent與edge treatment中的兩項區分。
- 裝備層次從身體向外有清楚 overlap；不以貼圖假裝不存在的甲片厚度。
- 背面、側面也有角色 identity，不只正面漂亮。
- 角色在弱 PBR lighting下仍可藉 BaseColor value設計辨材質。

## 禁止作為 Final

Primitive character、Cube／Cylinder／Capsule拼接、Voxel／Minecraft-like、Graybox、Placeholder、Prototype、generic blockout、flat-color test model、無裝備層次、嚴重 faceted shading、錯誤 normals造成方塊感，一律不得標 `PRODUCTION_READY`。Primitive只允許在 `BLOCKOUT`。

## Form 製作順序

### Primary Forms — 先通過

Body、Helmet、Shoulder、Chest Armor、Shield、Sword、Bow、Large Weapon。檢查黑色剪影、正側背比例與RTS縮圖。

### Secondary Forms — 建立層次

Belt、Bracer、Knee Guard、Boots、Waist Armor、Quiver、Armor Plates。檢查 overlap、功能與動畫空間。

### Tertiary Details — 最後處理

Scratch、Wear、Small Rivet、Leather Grain、Small Groove、Surface Damage。只在 Primary／Secondary全部 approved後投入。

> `REQUIRED`：Primary Forms 未達標前，不得大量製作 Tertiary Details。

## Production LOD0 Geometry Guideline

| Tier | 建議 triangles |
| --- | ---: |
| Standard Unit | 20K–35K |
| Elite Unit | 25K–45K |
| Special Unit | 30K–50K |
| Hero | 40K–70K |

這是初始 guideline，不是硬上限。可依 screen size、deformation、material/draw calls與target hardware以review結果調整。偏離超過25%時，交付報告須說明 profile證據與視覺影響。

`CURRENT` Infantry 4,376 tris、Archer 3,344 tris符合舊 Prototype budget，不符合本版 Production LOD0 target；它們是技術 Golden Sample候選，不是視覺 Golden Sample。

## Geometry QA

- `REQUIRED` 無 non-manifold defect、重疊面、零面積面、孤立垃圾、反向 normal。
- `REQUIRED` silhouette edges獲得足夠 segments；相機看不到的內部面才可移除。
- `REQUIRED` 肩、肘、髖、膝有變形需要的 edge flow。
- `RECOMMENDED` rigid armor為 separate shells或使用可控 hard weights；不將全身拆成無法平滑變形的單權重碎片。
- `REQUIRED` 武器與盾牌保持 separate objects／assets，除非 asset-specific review明確允許。

## Surface QA

- 大色塊先建立三階 value；micro-noise不得破壞單位輪廓。
- edge highlight需支援形體，不可每條邊同強度。
- wear集中在合理接觸點；一般士兵不做成全身鏽蝕。
- faction與team color是兩層概念：faction shape/material identity可固定，match team color必須runtime可換。

## 品質退件

- 只能近距辨識兵種。
- Hero只是普通士兵換貼圖。
- Archer只是Infantry換弓。
- Team Color覆蓋全身或只剩不可見小點。
- 用高解析貼圖掩飾扁平幾何／錯誤normal。
- 動作真實但在32–64 px完全看不出 anticipation／impact。

