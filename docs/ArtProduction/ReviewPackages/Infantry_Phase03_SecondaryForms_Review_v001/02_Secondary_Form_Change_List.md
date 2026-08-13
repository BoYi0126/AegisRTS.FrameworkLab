# Secondary Form Change List

Status: `READY FOR PHASE03 REVIEW`

## PRESERVED

- P02R1 身高、頭身比、頭盔／羽飾大輪廓、肩寬、三層肩甲、胸甲 mass、腰部 taper、腿／靴比例、盾牌 outline、短劍角色與左右手配置。
- 23-bone skeleton、10 empties／anchors、`unit.infantry` 與 Runtime Prefab contract。

## MODIFIED

- 現有幾何 material slots 改為六類 preview `MATID_*`；僅供 Review。
- 既有 scarf、waist cloth 與 plume 移至 `GEO_CLOTH` collection，幾何形體不被當作 final cloth simulation。

## ADDED

- Chest raised divisions、side returns、upper support。
- Shoulder anchors、under-plates、outer edge rails。
- Helmet brow band／rear guard、plume mount ring／broad division、scarf folds。
- Bracer rims／straps、hand grip contact、belt clasp、waist tabs／fold。
- Leg-wrap tucked ends、knee cloth boundaries、boot upper／toe／heel panels。
- Shield boss base、wood seams、rear brace／grip／forearm strap、team panel。
- Sword blade spine與三個 broad grip-wrap forms。

## REBUILT

- 無 P02R1 primary object 被刪除重建；Phase 03 採 additive、modular construction，方便 reviewer 比對與後續 retopology。

## DEFERRED

- Final retopology／UV／textures／Team Color mask／skin weights／animation／LOD／Unity integration。
