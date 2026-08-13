# Phase 01 Open Issues

## Blocking the Phase 01 Exit

| ID | Priority | Issue | Required closure |
|---|---|---|---|
| INF-P01-001 | P0 | 15項方向尚未取得使用者明確批准。 | 使用者批准 `02_Approval_Checklist.md`，填寫revision與exceptions。 |
| INF-P01-002 | P1 | 正式Design／Art／Technical Art reviewer與簽核責任未登記。 | Producer指定owners；最遲在Phase 02 review前完成。 |

## Blocking Phase 02 Acceptance or Later Production Ready

| ID | Priority | Issue | Phase impact / closure |
|---|---|---|---|
| INF-P01-003 | P1 | 尚無v003 exact-geometry orthographic Clay／Silhouette／Wireframe。 | Phase 02從new v003 source輸出Front／Side／Back／3/4、128／64／32 px evidence；批准後補完3D production reference。 |
| INF-P01-004 | P0 Release | v001→v002來源與commercial/distribution rights不完整。 | Producer／Legal完成provenance registry；未完成不得Production Ready。 |
| INF-P01-005 | P1 | v002 `.blend`重開為0 Actions；獨立FBX存在但editable animation source不durable。 | 在後續versioned source保留Actions/NLA並reopen validate；不改v002。 |
| INF-P01-006 | P1 | Production Team Color shader／packed-channel contract未實作；current為named slots＋MPB。 | Technical Art選擇保留slots或versioned mask shader，通過mip／batching／selection highlight測試。 |
| INF-P01-007 | P1 | v003 LOD1／LOD2／LOD3／Impostor budget未凍結。 | Primary Forms批准後依screen-size與profile建立完整LOD plan。 |
| INF-P01-008 | P2 | Target hardware、battle count、camera envelope及frame/memory budget未批准。 | Product／Engineering建立benchmark matrix；不得把規劃triangle budget當performance PASS。 |
| INF-P01-009 | P1 | v003 topology、UV、2K textures、surface、deformation與animation polish尚未實作。 | 分別在後續L3 phases完成並依Master Checklist驗收。 |
| INF-P01-010 | P1 | Current standardized Unity Close／Medium／Normal／Far與128／64／32 px captures缺失。 | 從exact candidate revision產生含camera/LOD/team/hash metadata的capture set。 |
| INF-P01-011 | P1 | Legacy `Unit_03_步兵` 的2.5K–6K LOD0與v003 production 20K–30K目標語意衝突。 | v003依本Phase target；legacy值只適用v002 Prototype／較低LOD參考。同步文件已加狀態註記。 |

## Non-Issues / Explicit Decisions

- 不需要再生成AI turnaround；既有L1與本施工規格是Phase 01輸入。
- 不因v002視覺品質不足而直接整體刪除重做；目前決策仍是Preserve contracts＋Partial Rebuild forms。
- Phase 02不修改runtime Prefab，不製作final texture，不宣告Golden Sample lock。
- `CANNOT VERIFY`、`PENDING`與`NOT RUN`都不計為PASS。

## Closure Rule

關閉任一項都要記錄owner、日期、證據路徑、受影響revision與例外；同時更新本文件、相關Golden Sample／Master Checklist及`DevelopmentProgress.md`。口頭決定或聊天內容若未落入repository紀錄，不視為關閉。
