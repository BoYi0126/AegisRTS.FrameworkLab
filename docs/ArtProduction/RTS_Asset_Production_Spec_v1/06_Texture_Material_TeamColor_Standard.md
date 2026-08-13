# 06 — Texture, Material and Team Color Standard

- Specification Version：1.0

## Current Shader Audit

| 項目 | 狀態 | 證據 |
| --- | --- | --- |
| Render Pipeline | `CURRENT / VERIFIED` | URP 17.5.0 |
| Character Shader | `CURRENT / VERIFIED` | URP Lit；無repository自訂Shader／Shader Graph |
| Infantry Base maps | `CURRENT / VERIFIED` | BaseColor與Normal已連接；1K |
| Infantry ORM／Team Mask | `CURRENT / VERIFIED BUT UNUSED` | Texture存在且Importer配置；Material無reference |
| Archer textures | `STATUS: NOT FOUND` | 三個materials皆為constant color，無texture reference |
| Team Color | `CURRENT / VERIFIED` | separate `TeamColor` material slots＋`MaterialPropertyBlock` `_BaseColor/_Color` |
| Single-material mask shader | `PROPOSED` | 尚未實作，不得宣稱CURRENT |

## Texture Budget

- Standard／Elite Unit：`RECOMMENDED 2048×2048` master set。
- Special：`RECOMMENDED 2048`；只有畫面占比與獨特材質需要時用4096。
- Hero：2048或4096，需texel-density與memory review。
- LOD與平台用Unity importer／mip streaming降有效解析度，不另畫紅藍多套。
- Source可保存16-bit／lossless master；runtime compression需依target platform驗證。

## Required Authoring Maps

- Base Color
- Normal
- Roughness
- Metallic
- Ambient Occlusion
- Team Color Mask

所有map必須在delivery manifest標色彩空間：BaseColor=sRGB；Normal／Roughness／Metallic／AO／Mask=Linear。

## Proposed Channel Packing

目前URP Lit沒有直接消費下列packing，故此方案標記為 `PROPOSED`，必須先完成Shader Graph與Unity tests才可啟用：

```text
TEX_<Asset>_MRA_Team
R = Metallic
G = Ambient Occlusion
B = Roughness
A = Team Color Mask
```

Shader需明確以 `Smoothness = 1 - Roughness` 轉換B channel。若保持CURRENT URP Lit，不得把上述texture直接塞入`_MetallicGlossMap`；應使用separate maps或由import/build step產生符合URP Lit metallic/smoothness layout的runtime texture，並保留authoring source。

## Stylized／Hand-painted Base Color

BaseColor不得只用Armor Gray、Leather Brown、Skin Color平塗。`REQUIRED`：

- 大形Value Variation與painted shadow支援視線方向，但不烘焙scene lighting。
- edge highlight表達厚度；不在所有邊等寬套線。
- wear只在接觸／磨損區；以可讀大形為主。
- Metal、Leather、Cloth、Wood、Skin至少有value/hue/roughness intent的明顯分離。
- 在低對比PBR lighting與灰階下仍可辨材質。

## Team Color

`REQUIRED`：Single Mesh＋runtime replaceable Team Color。禁止Blue／Red／Green Infantry重複mesh。

- 適合區域：cloth、shield panel、banner、shoulder decoration、cape、waist cloth。
- Standard unit可見面積建議15–25%；building 8–15%。
- 四方向可見；避免全身染色。
- CURRENT separate-slot方案可保留作Golden Sample technical baseline。
- PROPOSED mask-shader方案需支援Base材質保留、team hue/tint、selection highlight優先序、SRP Batcher／instancing profile。

## Material Acceptance

- [ ] Material count與shader名稱列入manifest。
- [ ] 所有texture有source、resolution、bit depth、color space、packing說明。
- [ ] Normal orientation在Unity實測正確，無green-channel誤反。
- [ ] Metal／cloth／wood在Neutral與Game-like lighting都可辨。
- [ ] Team Color四向、藍／紅／中立、selection highlight都不污染Base slots。
- [ ] Windows Development Build無pink material或shader stripping。

