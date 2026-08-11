# Phase 04 — Movement / Navigation / Formation

Move Command → Movement System → Navigation Adapter → View。

使用 NavMesh/AI Navigation：destination validation、unreachable、repath、stuck detection、local avoidance。

Formation：Line、Box；Group move 不能把所有單位送往同一座標。

Debug：path、destination、velocity、formation slot、stuck。

Acceptance：50 Unit 穿越障礙並形成隊形，不大量永久卡死。
