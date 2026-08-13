# CHR_Infantry_A_v003 — Phase 02 Primary Forms

Status: `WIP_MODEL`  
Review state: `READY FOR REVIEW`

這是由保留的 `CHR_Infantry_A_v002` contract baseline 建立的新版本。內容只處理 Primary Forms、silhouette 與 major geometry；它不是 Production Ready 資產，也未取代任何 Unity Runtime Prefab。

## Contents

- `Source/CHR_Infantry_A_v003.blend`：Blender 5.2.0 LTS Primary Forms source。
- `Source/CHR_Infantry_A_v003_P02R1.blend`：依 reviewer `CHANGE REQUESTED` 建立的Phase 02 Revision 01 source；不覆寫initial candidate。
- `Source/build_infantry_v003_primary_forms.py`：由唯讀 v002 baseline 重建 v003 的 deterministic script。
- `Source/revise_infantry_v003_p02r1.py`：從指定SHA-256的v003 initial建立P02R1，局部修正Primary Forms。
- `Source/render_infantry_v003_primary_forms_review.py`：產生 Clay、Silhouette、Wireframe、screen-size 與 manifests；不儲存輸入 blend。
- `Source/render_infantry_v003_p02r1_review.py`：產生P02R1 review evidence；不儲存輸入blend。
- `Source/compose_primary_forms_comparison.ps1`：以既有 v002 review captures 與新 v003 captures 合成並排比較圖。
- `Source/compose_p02r1_comparisons.ps1`：合成v003 initial／P02R1與L1／P02R1比較圖。
- `Documentation/BUILD_RESULT.json`：建模輸出與量測。
- `Documentation/P02R1_BUILD_RESULT.json`：Revision 01輸出、hash、量測與deferred work。

## Deliberately Deferred

Final UV、Final Texture、Team Color Mask、Final Skinning、Animation Polish、正式 LOD chain、shader rewrite、FBX/Unity import 與正式 Runtime Prefab replacement 全部不屬於 Phase 02。

## Reproduce

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background `
  'ArtSource\Units\Infantry\CHR_Infantry_A\v002\Source\CHR_Infantry_A_v002.blend' `
  --python 'ArtSource\Units\Infantry\CHR_Infantry_A\v003\Source\build_infantry_v003_primary_forms.py' -- `
  --repo-root 'C:\projects\Unity\AegisRTS.FrameworkLab'
```

執行前應先確認 v002 SHA-256 為 `5D9D93F9559D2A1608FB4B57A7BC0AC284C4F3ED99BA826F0A5E98E1D5F51632`。腳本只允許輸出到 v003 路徑。
