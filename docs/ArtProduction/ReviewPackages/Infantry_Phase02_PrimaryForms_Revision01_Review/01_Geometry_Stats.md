# Geometry Stats

Status: `READY FOR PHASE02 REVISION REVIEW`

## Candidate Metrics

| Metric | v003 initial | P02R1 | Revision target |
|---|---:|---:|---:|
| Height | 1.830 m | 1.824 m | 1.80–1.85 m |
| Meshes | 72 | 64 | maintainable source |
| Vertices | 14,211 | 12,671 | informational |
| Triangles | 28,138 | 25,106 | 24K–30K |
| Materials | 6 | 6 | review-only |
| Armature／Bones | 1／23 | 1／23 | preserve |
| Empty sockets／anchors | 10 | 10 | preserve |
| Saved Actions | 0 | 0 | animation deferred |

Triangle delta: `-3,032` (`-10.78%`). Density was removed mainly by deleting pasted face pieces and replacing eight high-density ring objects with four broader wrap objects; this is redistribution during Primary Forms, not formal retopology.

## Exclusive Triangle Groups

| Group | Triangles |
|---|---:|
| Body／limbs excluding head and boots | 5,800 |
| Head／integrated face | 1,512 |
| Helmet／rim／mount／plume | 1,908 |
| Shoulder armor | 5,544 |
| Chest armor／lamellar | 3,988 |
| Waist／scarf | 1,760 |
| Leg wraps | 576 |
| Boots／soles | 768 |
| Shield | 2,372 |
| Sword | 542 |
| Bracers／other armor | 336 |
| **Total** | **25,106** |

## Bounds and Topology

- Min: `(-0.890000, -0.280720, 0.000000)` m
- Max: `(1.028345, 0.199229, 1.824011)` m
- Non-manifold edges: `0`
- Boundary edges: `0`
- Loose edges: `0`
- Zero-area faces: `0`

Topology counts certify closed Primary Forms geometry only. Final deformation edge flow、UV seams與LOD suitability尚未驗收。
