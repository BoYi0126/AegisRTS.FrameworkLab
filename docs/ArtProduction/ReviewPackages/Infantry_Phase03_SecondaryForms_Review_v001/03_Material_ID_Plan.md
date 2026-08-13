# Phase 03 Material ID Plan

Status: `READY FOR PHASE03 REVIEW`

這些材質只提供 viewport／review 分區，不含 texture files、final shader graph、UV acceptance 或 channel packing。

| ID | Preview assignment | Intended production family |
|---|---|---|
| `MATID_Metal` | Helmet、armor rows、shoulders、bracers、shield rim／boss／brace、sword blade | Stylized dark metal／steel |
| `MATID_Wood` | Shield main body與broad panel seams | Wood；wood grain deferred |
| `MATID_Leather` | Belt、attachment straps、boots、shield grips、sword grip | Leather；grain／stitching deferred |
| `MATID_Cloth` | Undergarment、pants、leg wraps | Neutral cloth |
| `MATID_Skin` | Head、neck、hands | Stylized skin；face detail deferred |
| `MATID_Team` | Neck scarf、front／rear waist cloth、plume accent、selected shield panel | Future Team Color mask target |

Team region保留在局部 cloth／shield accent，不覆蓋整片 armor。最終可見面積、shader與 mask 由後續材質階段驗證。
