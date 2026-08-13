# 09 — Existing Infantry／Archer Remaster Audit

- Specification Version：1.0
- Evidence Level：檔案／Unity YAML／Blender 5.2只讀結構＋既有測試紀錄；未在本次重跑Unity或產生新render。

2026-08-13 Phase 02 Revision 01 addendum：Initial `CHR_Infantry_A_v003` 的人工結果是`CHANGE REQUESTED`（toy-like head/arms/chest/waist/wrap/boots）。已建立不覆寫initial的 `CHR_Infantry_A_v003_P02R1`（1.824 m、25,106 tris、64 meshes），以局部Primary Forms reshape／rebuild修正並提供initial與L1比較。Status為`READY FOR PHASE02 REVISION REVIEW`；review、Secondary Forms、final topology／UV／texture／skin／animation／LOD／Unity integration均未通過，因此production acceptance判定不變。

## 評分定義

`PASS` 可直接保留；`NEEDS IMPROVEMENT` 可修改；`REBUILD` 現有部分不適合production；`CANNOT VERIFY` repository證據不足。

## Infantry Audit

| Area | Result | Evidence／Decision |
| --- | --- | --- |
| Model Geometry | `NEEDS IMPROVEMENT` | 4376-tri legacy production-blockout；保留尺寸與identity，提升body／armor forms與joint topology |
| Proportion | `PASS` | 1.80 m target、Unity bounds／grounding已有gate；production sheet仍缺 |
| Silhouette | `NEEDS IMPROVEMENT` | 盾劍可辨，但production blind-test evidence不足，shoulder／lower-body需強化 |
| Armor | `REBUILD` | 目前為程式化低模層次；partial rebuild helmet／shoulder／chest／boots／waist armor |
| Weapon | `NEEDS IMPROVEMENT` | separate shield／sword contract可保留；surface、grip、proportion由L2重驗 |
| Material | `NEEDS IMPROVEMENT` | URP Lit可用；Base＋Team slot保留，formal shader／material separation未完成 |
| Texture | `REBUILD` | 1K palette／flat Normal／mean ORM是placeholder；需2K production set |
| Normals | `CANNOT VERIFY` | 本次無repository內neutral/game-like production render；需DCC／Unity normal QA |
| Rig | `PASS` | 23-bone valid Humanoid、stable sockets；可作Skeleton Family baseline |
| Skinning | `NEEDS IMPROVEMENT` | body max influences=1；production joints需smooth-deformation review |
| Animation | `NEEDS IMPROVEMENT` | 5 clips可玩且events通過；professional polish未完成；source `.blend` 0 Actions |
| Unity Integration | `PASS` | Prefab、Animator、LOD、Team Color、anchors、Attack-Move與tests已有證據 |

### Infantry Remaster Decision

- **A Preserve**：Asset ID、1.80 m scale、Prefab ID、Humanoid mapping、sockets、Animator parameters、events、source/runtime boundary、builders與gameplay integration。
- **B Modify**：body joint topology／weights、pose、animation curves、LOD chain、material hookup。
- **C Rebuild Partially**：helmet、shoulder、chest armor、waist armor、boots、shield surface與production texture set。
- **D Rebuild Character**：目前不直接採用。只有approved L2證明base proportion/topology無法有效修正，或deformation spike仍失敗時才升級D；不得只因「不好看」整體砍掉。

## Archer Audit

| Area | Result | Evidence／Decision |
| --- | --- | --- |
| Model Geometry | `NEEDS IMPROVEMENT` | 3344-tri Infantry derivative；保留technical base但需class-specific production forms |
| Proportion | `PASS` | 1.78 m、Unity standing gate通過；正式L2缺失 |
| Silhouette | `NEEDS IMPROVEMENT` | bow／quiver可辨；body仍高度沿用Infantry，需更窄肩／輕甲／拉弓negative space |
| Armor | `REBUILD` | 移除重甲的prototype差異不足以成production Archer |
| Weapon | `REBUILD` | bow/string rigid visual；需production bow limbs、string draw、hand/arrow alignment |
| Material | `REBUILD` | URP Lit constant colors，沒有production surface |
| Texture | `REBUILD` | Texture set `NOT FOUND` |
| Normals | `CANNOT VERIFY` | repository無Archer neutral/game-like render與normal map |
| Rig | `PASS` | 23-bone valid Humanoid，contracts與Infantry一致 |
| Skinning | `NEEDS IMPROVEMENT` | max influences=1；bow draw肩／肘／腕需要專門deformation test |
| Animation | `NEEDS IMPROVEMENT` | 5 clips、release frame22可玩；無deform bowstring，professional review未完成，`.blend` 0 Actions |
| Unity Integration | `PASS` | Prefab、Animator、LOD、socket、pooled projectile／impact與tests已有證據 |

### Archer Remaster Decision

- **A Preserve**：Content／Prefab IDs、Humanoid family、Animator/event/socket／arrow Z+、projectile presentation boundary、pooling。
- **B Modify**：body proportions、shoulder clearance、weights、animation timing、LOD／materials。
- **C Rebuild Partially**：helmet、light chest／shoulder、bracer、quiver、bow／string／arrow-hand setup與完整texture set。
- **D Rebuild Character**：先不採用；若production L2 silhouette與現有Infantry-derived body無法在partial rebuild達標，再以documented spike決定。

## Existing Spec Conflicts

| Old Requirement | New Requirement | Conflict | Recommended Resolution | Migration Impact |
| --- | --- | --- | --- | --- |
| L2=Game Model | L2=Production Character Sheet | stage語意不同 | 舊模型標`Legacy Prototype Model`；補正式L2 | 文件／backlog狀態調整，資產不刪除 |
| Standard LOD0 2.5–6K | Production Standard 20–35K | visual target不同 | 舊budget留給Prototype／LOD2；Production LOD0用新range | 兩個Golden Sample需remaster |
| Standard 1K texture | Production 2K | surface budget不同 | 1K保留Prototype；production source升2K | 增加memory/profile gate |
| Stylized low-poly | Stylized production quality | primitive/faceted容許度不同 | 保留shape language，禁止blockout final | 需formal art review |
| TeamColor mask宣稱 | CURRENT slot-based tint | mask texture未被shader取樣 | 文件改分CURRENT／PROPOSED | 需Shader Graph task或明確保留slots |
| `.blend`保存actions | 實檔重開0 Actions | source truth矛盾 | actions持久化並新增reopen test | 不影響現有FBX runtime；影響DCC交接 |

完整遷移順序見 `Legacy_Spec_Migration.md`。
