# Arm Proportion Report

L1數值沿用 `L1_Landmarks_Front.json` 的friendly-blue front pixel landmarks；Sword-side作主要比例依據。L1 joint仍屬estimated／medium confidence，但本次同時使用segment measurement、posed landmark、overlay與Unity Close，不再以confidence作為不修正16.8% mismatch的理由。

## 比例

| Metric | L1 Estimate | Before P035 | After P035R1 | Gap After |
|---|---:|---:|---:|---:|
| UpperArm / H | 0.186051 | 0.154943 | 0.176000 | −5.40% |
| Forearm / H | 0.168762 | 0.154943 | 0.165000 | −2.23% |
| Shoulder→Wrist / H | 0.354813 | 0.309886 | 0.341000 | −3.89% |
| Hand Width / H | 0.049689 | 0.069423 | 0.060652 | +22.06%（在revision target內，保留RTS stylization） |
| Hand Length / H | 0.077888 | 0.062856 | 0.057828 | −25.75% vs estimated L1；實際單次只縮8%，需Reviewer看grip |
| Head Width / H | 0.111801 | 0.126651 | 0.120000 | +7.33%（在revision target內） |

## Sword-side posed vertical landmarks

Normalized Y以ground=0、P035／P035R1 source height `1.824011 m` 為基準；數值越大代表越高。

| Landmark | L1 | Before P035 | After P035R1 | Gap After |
|---|---:|---:|---:|---:|
| Shoulder Y | 0.776398 | 0.757485 | 0.757485 | −1.89% H |
| Elbow Y | 0.596273 | 0.607784 | 0.587439 | −0.88% H |
| Wrist Y | 0.431677 | 0.454726 | 0.424446 | −0.72% H |
| Hand End Y | 0.360248 | 0.392712 | 0.367393 | +0.71% H |

Sword-hand結束位置原本比L1高約`3.25%H`，修正後只高`0.71%H`；Wrist由高`2.30%H`變為低`0.72%H`。兩者皆明顯收斂且沒有長到knee或形成猿臂。

## Confidence與證據

- L1 Sword-side shoulder／elbow／wrist／hand end：`medium / estimated`。
- 3D Before／After：直接由Blender armature rest endpoints加上同一review pose matrix計算。
- 視覺交叉檢查：`Skeleton_Arm_Landmarks_Front.png`、arm-focus overlay、P035 vs P035R1 comparison、Unity Close side-by-side。

