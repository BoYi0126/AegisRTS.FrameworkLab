# Geometry Stats

Status: `READY FOR REVIEW`

## Candidate Metrics

| Metric | Result | Phase 02 target |
|---|---:|---:|
| World height | 1.830 m | 1.80–1.85 m |
| Mesh objects | 72 | review decomposition |
| Vertices | 14,211 | informational |
| Triangles | 28,138 | 20K–30K; preferred 24K–27K |
| Review materials | 6 | temporary only |
| Armatures | 1 | preserve contract |
| Bones | 23 | preserve contract |
| Saved Actions | 0 | animation work deferred |

28,138 triangles are inside the approved Phase 02 range and 1,138 above the preferred band. This is reviewable, not an optimization or production-LOD sign-off.

## Exclusive Triangle Groups

| Group | Triangles |
|---|---:|
| Body base／limbs | 5,800 |
| Head／face | 3,396 |
| Boots | 768 |
| Helmet | 1,896 |
| Shoulders | 5,544 |
| Chest | 3,988 |
| Waist／scarf | 1,256 |
| Bracers | 336 |
| Leg wraps | 2,240 |
| Shield | 2,372 |
| Sword | 542 |
| **Total** | **28,138** |

## Bounds

- Min: `(-0.890000, -0.286000, 0.000000)` m
- Max: `(1.028345, 0.163000, 1.830000)` m
- Height Z: `1.830000` m

The X/Y bounds include equipped shield and sword; height is the character/equipment scene bound required by the task.

## Topology Audit

| Check | Count |
|---|---:|
| Non-manifold edges | 0 |
| Boundary edges | 0 |
| Loose edges | 0 |
| Zero-area faces | 0 |

These counts verify closed generated review forms. They do not certify final animation topology, UV seams, deformation loops or LOD suitability.
