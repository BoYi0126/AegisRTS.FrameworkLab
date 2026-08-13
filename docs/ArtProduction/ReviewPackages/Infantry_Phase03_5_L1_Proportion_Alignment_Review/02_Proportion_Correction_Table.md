# Proportion Correction Table

| Metric | L1 | Before | After | Correction |
|---|---:|---:|---:|---|
| Overall height | ≈1.824 m | 1.824011 m | 1.824011 m | PRESERVED |
| Hip Y / H | 0.3634 | 0.4414 | 0.3564 | joint＋body piecewise vertical remap |
| Knee Y / H | 0.2578 | 0.2859 | 0.2495 | joint＋body piecewise vertical remap |
| Torso / H | 0.4161 | 0.3160 | 0.4011 | hip lowered; shoulder preserved |
| UpperLeg / H | 0.1056 | 0.1556 | 0.1070 | hip/knee landmarks aligned |
| LowerLeg / H | 0.1646 | 0.2107 | 0.1742 | knee landmark aligned; ankle preserved |
| Head width / H | 0.1118 | 0.1362 | 0.1267 | head mesh uniform ×0.93; helmet preserved |
| Anatomical shoulder width / H | 0.2453 | 0.2609 | 0.2609 | PRESERVED within width gate |
| Armored shoulder width / H | 0.3789 | 0.3988 | 0.3974 | armor fit ×0.98 only |
| Chest width / H | 0.2671 | 0.2939 | 0.2762 | body/chest armor X ×0.94 |
| Hand width / H | 0.0497 | 0.0746 | 0.0694 | hand/thumb uniform ×0.93 |
| Boot width / H | 0.1056 | 0.1177 | 0.1107 | boot/sole X/Y ×0.94 |
| Shield | 0.55–0.65 W / 0.75–0.95 H m | 0.600 / 0.862 m | same | PRESERVED |
| Sword | 0.90–1.10 m | 1.061 m | same | PRESERVED |

## Explicitly not changed

UpperArm／Forearm length、bone names、hierarchy、23-bone contract、Helmet construction／width、Chest detail language、Shoulder layer structure、Scarf／Waist Cloth form、Shield front/back geometry、Sword geometry、Material-ID plan均保留。Review-only action沒有成為正式 bind pose或 gameplay Idle。

