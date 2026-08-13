# 07 — Visual Evidence Index

## 1. Concept and construction reference

| Image | Type | View | Source | Notes |
|---|---|---|---|---|
| `L1_Concept/v001/Unit_03_Infantry_L1_Concept_Final.png` | L1 concept | multi-view／blue-red／silhouette | v001 ArtSource | 1254²; main identity reference; not production L2 |
| `L1_Concept/v001/Unit_03_Infantry_L1_Concept_Alternate.png` | L1 alternate | multi-view | v001 ArtSource | 1254²; historical comparison |
| `L1_Concept/v002_LinkedReference/Unit_03_Infantry_L1_Concept_Final.png` | linked L1 copy | same as final | v002 Reference | retained to prove current source lineage |
| `L2_Reference/v001/PREVIEW_Dimensions_Front.png` | dimension preview | front | v001 ArtSource | 1400×1000; supporting only |
| `L2_Reference/v001/UV_Infantry_A_Base_LOD0.png` | UV | base UV | v001 ArtSource | 1024² technical reference |
| `L2_Reference/v001/UV_Infantry_A_TeamColor_LOD0.png` | UV | team UV | v001 ArtSource | 1024² technical reference |
| **MISSING** | L2 production sheet | neutral front/side/back/3Q A/T pose | — | `NOT FOUND`; do not substitute concept/preview |

## 2. Existing legacy camera previews

All six files below are mathematical/generated camera-contract previews from v001. They are useful for intended framing but **are not Unity screenshots**.

| Image | Type | View | Source | Notes |
|---|---|---|---|---|
| `Screenshots/Existing/LegacyCamera/PREVIEW_CameraContract_1920x1080_31m_Blue.png` | legacy preview | 31 m blue | v001 ArtSource | 1920×1080 |
| `.../PREVIEW_CameraContract_1920x1080_31m_Red.png` | legacy preview | 31 m red | v001 ArtSource | 1920×1080 |
| `.../PREVIEW_CameraContract_960x540_31m_Blue.png` | legacy preview | 31 m blue | v001 ArtSource | 960×540 |
| `.../PREVIEW_CameraContract_960x540_31m_Red.png` | legacy preview | 31 m red | v001 ArtSource | 960×540 |
| `.../PREVIEW_CameraContract_960x540_40m_Blue.png` | legacy preview | 40 m blue | v001 ArtSource | 960×540 |
| `.../PREVIEW_CameraContract_960x540_8m_Blue.png` | legacy preview | 8 m blue | v001 ArtSource | 960×540 |

## 3. Existing Unity validation evidence

Originals were outside the repository under `C:\projects\Unity\AegisRTS.BuildValidation`. They were copied because current progress/source reports explicitly describe them as visual validation evidence. They are marked historical: source commit/camera metadata are incomplete and Unity was not rerun.

| Image | Type | View | Source | Notes |
|---|---|---|---|---|
| `Screenshots/Existing/Unity/Infantry_GameView.png` | Unity game view | broad RTS scene | external validation, 2026-08-12 | 2560×1440; character small; not isolated normal-distance acceptance |
| `.../Detail/InfantryDetail_Front.png` | Unity detail | front | external validation, 2026-08-13 | 960×540; base/team material visible |
| `.../Detail/InfantryDetail_Side.png` | Unity detail | side | external validation | shield/body depth and pose evidence |
| `.../Detail/InfantryDetail_Back.png` | Unity detail | back | external validation | rear silhouette/material evidence |
| `.../Detail/InfantryMovePose_00.png` | Unity animation | Move phase 0 | external validation | static phase sample |
| `.../Detail/InfantryMovePose_01.png` | Unity animation | Move phase 1 | external validation | static phase sample |
| `.../Detail/InfantryMovePose_02.png` | Unity animation | Move phase 2 | external validation | static phase sample |
| `.../Detail/InfantryMovePose_03.png` | Unity animation | Move phase 3 | external validation | static phase sample |
| `.../Movement/Move_00.png` | Unity actual movement | sequence 0 | external validation, 2026-08-13 | 960×540; gameplay movement sequence |
| `.../Movement/Move_01.png` | Unity actual movement | sequence 1 | external validation | same sequence |
| `.../Movement/Move_02.png` | Unity actual movement | sequence 2 | external validation | same sequence |
| `.../Movement/Move_03.png` | Unity actual movement | sequence 3 | external validation | same sequence |
| `.../Movement/Move_04.png` | Unity actual movement | sequence 4 | external validation | same sequence |
| `.../Movement/Move_05.png` | Unity actual movement | sequence 5 | external validation | same sequence |
| `.../Movement/Move_06.png` | Unity actual movement | sequence 6 | external validation | same sequence |
| `.../Movement/Move_07.png` | Unity actual movement | sequence 7 | external validation | same sequence |

## 4. Generated Blender neutral/material views

Generated with Blender 5.2 from `L3_Source/CHR_Infantry_A_v002.blend` **package copy**, LOD0 only, orthographic camera, neutral three-light setup and ground. The script never saved a `.blend`.

| Image | Type | View | Source | Notes |
|---|---|---|---|---|
| `Screenshots/Blender/01_Front.png` | DCC actual material | front | current copied `.blend` | 768²; shield/sword primary cue |
| `Screenshots/Blender/02_Left.png` | DCC actual material | left | current copied `.blend` | 768²; body/equipment depth |
| `Screenshots/Blender/03_Right.png` | DCC actual material | right | current copied `.blend` | 768² |
| `Screenshots/Blender/04_Back.png` | DCC actual material | back | current copied `.blend` | 768² |
| `Screenshots/Blender/05_ThreeQuarter_Front.png` | DCC actual material | front 3/4 | current copied `.blend` | 768²; recommended initial review view |
| `Screenshots/Blender/06_ThreeQuarter_Back.png` | DCC actual material | rear 3/4 | current copied `.blend` | 768² |

## 5. Generated Clay and wireframe views

| Image | Type | View | Source | Notes |
|---|---|---|---|---|
| `Screenshots/Blender/Clay_Front.png` | Clay | front | current copied `.blend` | separates geometry from colour/material |
| `Screenshots/Blender/Clay_Side.png` | Clay | side | current copied `.blend` | armour/equipment thickness |
| `Screenshots/Blender/Clay_Back.png` | Clay | back | current copied `.blend` | rear construction |
| `Screenshots/Blender/Clay_3Q.png` | Clay | front 3/4 | current copied `.blend` | primary/secondary forms |
| `Screenshots/Wireframe/Wireframe_Front.png` | wire overlay | front | current copied `.blend` | topology density and primitive construction |
| `Screenshots/Wireframe/Wireframe_3Q.png` | wire overlay | front 3/4 | current copied `.blend` | joint/equipment topology |

## 6. Required-view coverage

| Required evidence | Status | Best available evidence／gap |
|---|---|---|
| DCC front／left／right／back／front 3Q／rear 3Q | GENERATED | complete, current copy, static A-pose |
| Clay front／side／back／3Q | GENERATED | complete |
| Wireframe front／3Q | GENERATED | complete |
| Unity close | HISTORICAL ONLY | Detail PNGs; no current revision/camera manifest |
| Unity medium | MISSING | MANUAL CAPTURE REQUIRED |
| Unity RTS normal | PARTIAL／HISTORICAL | broad GameView + mathematical 31 m preview; neither is a standardized current isolated capture |
| Unity far | LEGACY PREVIEW ONLY | 40 m mathematical preview, not current Unity capture |
| 128／64／32 px | MISSING | MANUAL CAPTURE REQUIRED |
| Blue/red same camera | PARTIAL | legacy mathematical 31 m pair; no current Unity pair |
| LOD0／1／2 transitions | MISSING | MANUAL CAPTURE REQUIRED |
| Idle／Move／Attack／Hit／Death review | Move partial | only static Detail/Movement sequence; other clips missing |
| Normal/material channel diagnostic | MISSING | production material review required |

No generated screenshot should be interpreted as a Golden Sample approval; it is evidence for deciding remaster scope.
