# L1 vs 3D Landmark Report

L1 數值來自 friendly-blue front figure pixel landmarks，角色高度以 plume top 至 ground 的 322 px uniform normalization。裝甲遮住的 joint 均標為 estimated；3D 使用 Blender armature bone head／tail world coordinate與 mesh bounds。

## Critical vertical landmarks

| Metric | L1 Normalized | 3D Before | 3D After | Difference After | Confidence | Action |
|---|---:|---:|---:|---:|---|---|
| Ankle Y | 0.0932 | 0.0752 | 0.0752 | -1.79% H | Medium | WITHIN_TOLERANCE／preserved ground-foot system |
| Knee Y | 0.2578 | 0.2859 | 0.2495 | -0.83% H | Medium | PROPORTION_MISMATCH corrected |
| Hip Y | 0.3634 | 0.4414 | 0.3564 | -0.70% H | Low | PROPORTION_MISMATCH corrected |
| Belt Y | 0.5590 | 0.5743 | 0.5743 | +1.53% H | High | WITHIN_TOLERANCE |
| Chest Center Y | 0.6894 | 0.6927 | 0.6927 | +0.33% H | Medium | WITHIN_TOLERANCE |
| Shoulder Joint Y | 0.7795 | 0.7575 | 0.7575 | -2.20% H | Medium | WITHIN_TOLERANCE；不抬高胸肩 |
| Head Top Y | 0.9441 | 0.9622 | 0.9556 | +1.15% H | Low | localized head correction |
| Helmet Top Y | 0.9689 | 0.9730 | 0.9730 | +0.41% H | High | WITHIN_TOLERANCE／preserved |

## Widths

| Metric | L1 Normalized | Before | After | Relative gap after | Confidence | Action |
|---|---:|---:|---:|---:|---|---|
| Head width | 0.1118 | 0.1362 | 0.1267 | +13.3% | Medium | controlled −7% mesh adjustment; reviewer check |
| Helmet width | 0.1553 | 0.1577 | 0.1577 | +1.5% | High | WITHIN_TOLERANCE／PRESERVED |
| Anatomical shoulder width | 0.2453 | 0.2609 | 0.2609 | +6.4% | Medium | WITHIN_TOLERANCE／skeleton preserved |
| Armored shoulder width | 0.3789 | 0.3988 | 0.3974 | +4.9% | High | ARMOR_MISMATCH minor refit only |
| Chest width | 0.2671 | 0.2939 | 0.2762 | +3.4% | Medium | PROPORTION_MISMATCH corrected |
| Hand width | 0.0497 | 0.0746 | 0.0694 | +39.6% | Medium | controlled −7%; RTS readability retained; reviewer check |
| Boot width | 0.1056 | 0.1177 | 0.1107 | +4.9% | High | corrected |

## Segment ratios

| Metric | L1 Estimate | Before | After | Relative gap after | Confidence | Action |
|---|---:|---:|---:|---:|---|---|
| UpperArm | 0.1861 | 0.1549 | 0.1549 | −16.8% | Medium | UNCERTAIN：concept joint estimate／armor occlusion; pose evidence says PRESERVE |
| Forearm | 0.1688 | 0.1549 | 0.1549 | −8.2% | Medium | threshold warning, but no length correction after pose diagnosis |
| Torso shoulder→hip | 0.4161 | 0.3160 | 0.4011 | −3.6% | Low | corrected through hip landmark |
| UpperLeg | 0.1056 | 0.1556 | 0.1070 | +1.4% | Low | corrected |
| LowerLeg ankle→knee | 0.1646 | 0.2107 | 0.1742 | +5.8% | Medium | corrected |
| Hip→Ground | 0.3634 | 0.4414 | 0.3564 | −1.9% | Low | corrected |

Shield 實測 0.600 × 0.862 m，在 L1 0.55–0.65 × 0.75–0.95 m 範圍；Sword projected overall length 1.061 m，在 0.90–1.10 m 範圍，兩者尺寸均 PRESERVED。

