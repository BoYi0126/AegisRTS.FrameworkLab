# Phase 01 Approval Checklist

## Status Rules

- `READY`：規格值、證據與執行方式已完整，可由使用者批准。
- `APPROVED`：只能在使用者明確批准後填入。
- `CHANGE REQUESTED`：使用者指定修改值後重新稽核。

目前整體狀態：`APPROVED / PHASE 02 AUTHORIZED`。

| # | Approval decision | Locked target | Readiness | User decision |
|---:|---|---|---|---|
| 1 | Art direction | 東亞古代重裝步兵＋Stylized Fantasy RTS readability | `READY` | `APPROVED` |
| 2 | Equipment role | 左盾＋右手單手短劍 | `READY` | `APPROVED` |
| 3 | World height | target 1.83 m；allowed 1.80–1.85 m | `READY` | `APPROVED` |
| 4 | Body proportion | 5.2–5.4 heads；preferred約5.3 | `READY` | `APPROVED` |
| 5 | Silhouette emphasis | wide armored shoulders、tapered waist、stable lower body | `READY` | `APPROVED` |
| 6 | Helmet | curved dome＋metal rim＋top mount＋short plume＋readable thickness | `READY` | `APPROVED` |
| 7 | Shoulder armor | 2–3 readable layers；upper larger、lower smaller；separate left/right source objects | `READY` | `APPROVED` |
| 8 | Chest armor | shaped lamellar-inspired shell＋selected raised plates；不以平貼格線取代體積 | `READY` | `APPROVED` |
| 9 | Waist／limbs／boots | belt＋layered panels＋cloth；thigh/knee/calf/ankle/boot rhythm；非方塊腳 | `READY` | `APPROVED` |
| 10 | Shield construction | wood body＋metal rim＋boss＋reinforcement＋thickness＋back grip＋team region | `READY` | `APPROVED` |
| 11 | Sword construction | tapered blade＋edge/spine thickness＋guard＋grip＋pommel | `READY` | `APPROVED` |
| 12 | Team Color | visible coverage 15–25%；scarf／waist cloth／shield優先 | `READY` | `APPROVED` |
| 13 | v003 LOD0 budget | 20K–30K triangles；preferred 24K–27K | `READY` | `APPROVED` |
| 14 | Skinning strategy | soft body smooth skinning、最多4 influences；selected armor/equipment rigid | `READY` | `APPROVED` |
| 15 | Version protection | v002永久保留為Prototype baseline；Phase 02只建立v003 candidate | `READY` | `APPROVED` |

## Approval Record

```text
Decision: APPROVED
Approved / Change Requested By: User instruction authorizing direct Phase 02 execution
Role: Project owner / requester
Date: 2026-08-13
Approved target revision: Phase01_Production_L2_v001 / source SHA-256 DBD67A4021ED8FD56AD7F4F2B197BB430185B1817647445460753DCDB5316540
Exceptions: Phase 02 remains review-only; no Production Ready or runtime replacement approval
Expiry / revisit trigger: Any user-directed art-direction or construction change request
```

## Exit Rule

只有 15 項均由使用者接受，且 Approval Record 的 `Decision` 更新為 `APPROVED`，才可宣告：

```text
PHASE 01 = APPROVED
```

未明確批准不等於拒絕，但會阻止 Phase 02 production work開始。
