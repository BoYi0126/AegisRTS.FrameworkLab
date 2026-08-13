# Phase 02 Revision 01 Report

## Status

`READY FOR PHASE02 REVISION REVIEW`

## Baseline and Safety

- Reviewer decision entering this task: `CHANGE REQUESTED`.
- Preserved v002 source SHA-256: `5D9D93F9559D2A1608FB4B57A7BC0AC284C4F3ED99BA826F0A5E98E1D5F51632`.
- Preserved v003 initial SHA-256: `5F7C799C3B57A64B6E289A38FF2AA6E5509247C547FD4B2DE0EC47FF63AA4DBE`.
- L1 reference SHA-256: `C6BD352DE06854F8CD59AC92C891E43A0D40070D8F1AB72613358845847F461F`.
- Revision source: `CHR_Infantry_A_v003_P02R1.blend`.
- Stable runtime identity remains `unit.infantry`／`PF_Unit_Infantry`; no Runtime Prefab or Unity production asset was changed.

## Reviewer Findings Addressed

| Requested Primary Forms fix | P02R1 implementation |
|---|---|
| Helmet too spherical／wide | Crown narrowed, top taper strengthened, hard lower rim retained and lifted for face clearance. |
| Plume looked like stacked primitive | Rebuilt as a short backward-curved, laterally offset feather mass. |
| Face looked assembled | Six separate nose／brow／cheek／chin objects removed; one continuous Head mesh now carries large facial planes. |
| Upper arms too inflated | Radial upper-arm volume reduced 13%; shoulder armor remains the widest feature. |
| Shoulder layers too soft | Three layers preserved; outer drop increased and hard planar shading used. |
| Chest looked like floating bars | Rows enlarged into nested overlap, gaps reduced, lower edges and front depth modulated to indicate plate segmentation; center strap mass reduced. |
| Waist cloth／plates too box-like | Front/rear cloth rebuilt with tapered shaped hems; each side now has one main and one overlapping plate. |
| Legs／wraps／boots too toy-like | Thigh/calf receive broad directional planes; four donut rings become two broad subtly spiraled wraps; boots narrow asymmetrically at toe/ankle. |
| Shield boss too large／shield too flat | Boss diameter reduced 12.5%; board and rim receive a subtle convex bow. |
| Sword mostly acceptable | Length/taper/guard/grip preserved; pommel made less spherical. |

## Build and QA Iterations

1. First revision build produced 25,394 tris at 1.824 m with clean topology.
2. First Clay QA found the continuous face still hidden by helmet/scarf depth, wrap bands partly embedded in calf, and chest segmentation too weak.
3. Face was moved forward, scarf lowered, helmet rim lifted/reduced in depth, chest nesting/segmentation strengthened and wraps broadened.
4. Second Clay QA found three broad wraps still read as stacked rings; final revision uses two broad overlapping, visibly tilted bands.
5. Final source measures 25,106 tris／12,671 vertices／64 meshes at 1.824 m.

## Scope Boundary

本次不做Secondary Forms、ornament、UV、texture、skin、animation、LOD、FBX export、Unity import或Runtime Prefab replacement。Review materials仍只是Clay/ID用途。

本報告不記錄Revision PASS；最終決定由reviewer填入`04_Review_Checklist.md`。
