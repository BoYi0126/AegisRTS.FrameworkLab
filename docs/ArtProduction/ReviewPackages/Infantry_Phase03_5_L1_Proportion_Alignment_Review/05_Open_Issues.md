# Open Issues

1. L1 armor遮蔽hip、shield-side elbow／wrist與部分head edge；這些pixel landmarks是low-confidence estimate，Reviewer應以Overlay與3D bone資料一起判斷。
2. UpperArm ratio由L1估測為0.1861、3D為0.1549；Forearm為0.1688 vs 0.1549。因A-Pose視覺誤差很大且concept joints被armor遮擋，本次依「不機械套threshold」原則保留骨長，待Reviewer判斷。
3. Hand width在controlled −7%後仍比L1估測大，但繼續縮小可能破壞RTS readability與weapon grip；需要art／technical-art sign-off。
4. Source是static object-bound review geometry；review pose由可識別的`REVIEW_ONLY_POSE_L1_COMPARE` action與獨立review FBX表達，不是Final Skinning或deformation proof。
5. Unity L1 pose斜劍尖比ground低0.01757 m；boot ground正確。正式combat stance／weapon clearance留到Phase 06 Animation Polish。
6. Phase 04、Final UV／Texture／Shader、Final Skinning、Animation Polish與正式LOD均未開始。

本文件不宣告Phase 03.5 PASS，也不建立PRE-UV GEOMETRY LOCK。

