# Revision Change List

## PRESERVED

- `CHR_Infantry_A_v004.blend` 原檔及其 SHA-256。
- Head、helmet、plume、shoulder、chest、shield front、sword。
- Body height、major proportion、primary silhouette 與 L1 身分特徵。
- 既有 armature hierarchy、23 bones、A-pose 與六區 Material-ID 邏輯。

## MODIFIED

- Front Waist Cloth：重建指定局部 cloth object；1 個中央 broad fold、兩側主平面、薄厚度、不完全水平下擺。
- Scarf：重建指定局部 drape；寬扁截面、較少大摺、胸前接觸與乾淨終止。
- Upper Arm Cloth：縮減圓潤截面，改成可讀的平面轉折並保留 bracer transition。
- Shield Back：刪除零碎交叉件；留下清楚 brace hierarchy、broad strap 與 palm-aligned grip。
- Boots：移除 glued-on toe／upper panels，直接在 primary boot mesh 形成鞋頭、靴筒與 sole 轉折。
- Unity Review：新增隔離 review-only FBX、Prefab、Scene、材質與四張 capture；未連接 gameplay。

## DEFERRED

- Phase 04。
- Final UV unwrap、Final Texture、bake、BaseColor、Normal、ORM、Team Color texture。
- Final Skinning、Animation Polish、正式 LOD。
- 正式 Runtime Prefab 替換與 gameplay integration。

