# Shield Vertical Alignment Report

Normalization使用角色完整呈現高度 `H = 1.824010849 m`，ground為0。L1數值來自friendly-blue front figure的pixel estimate，屬medium-confidence 2D reference。

| Metric | L1 Estimate | Before P035R1 | After P035R2 | Difference After |
|---|---:|---:|---:|---:|
| Shield Top / H | 0.701863 | 0.509444 | 0.701604 | -0.000259 |
| Shield Center / H | 0.465839 | 0.273035 | 0.464339 | -0.001499 |
| Shield Bottom / H | 0.229814 | 0.036626 | 0.227074 | -0.002740 |
| Grip Center / H | N/A | 0.408019 | 0.573519 | N/A |
| Wrist / H | N/A | 0.424446 | 0.505081 | N/A |

After的盾頂位於lower chest／upper abdomen，boss位於abdomen／belt附近，盾底在knee附近；與任務的Unity RTS rules一致。2D concept受透視、盾牌yaw及裝甲遮擋影響，因此此表用於vertical relationship，不作pixel-perfect形狀判定。

Shield Size Before：`0.600000 × 0.862424 m`。

Shield Size After：`0.600000 × 0.862424 m`。

結論：尺寸相同；Top／Center／Bottom從整體偏低約`0.193H`收斂至L1估計的`0.003H`內。
