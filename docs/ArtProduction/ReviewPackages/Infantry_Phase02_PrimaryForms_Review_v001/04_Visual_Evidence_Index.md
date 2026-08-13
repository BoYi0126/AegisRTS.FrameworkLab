# Visual Evidence Index

| Evidence | Files | Review purpose |
|---|---:|---|
| Clay | 6 | Front／left／back／front 3Q／back 3Q／right major forms |
| Silhouette | 4 | Front／left／back／3Q outer contour and mass balance |
| Screen size | 3 | 128／64／32 px height readability |
| Wireframe | 3 | Front／side／3Q density and part construction |
| v002 comparison | 2 | Front／3Q prototype baseline versus v003 direction |
| Unity note | 1 | Explicit manual-capture deferral; Runtime Prefab unchanged |

All PNG size, byte and SHA-256 values are listed in `Manifests/Screenshot_Manifest.csv`. The two comparison sheets are generated after Blender rendering by `Blender/compose_primary_forms_comparison.ps1`.

## Suggested Review Sequence

1. Compare `v002_vs_v003_Front.png` and `v002_vs_v003_3Q.png`.
2. Review all Clay views without using wireframe density as a quality proxy.
3. Check silhouette at 128 px, then 64 px and 32 px.
4. Use wireframes only to identify obvious form-density or construction risks.
5. Record disposition in `05_Review_Checklist.md`.
