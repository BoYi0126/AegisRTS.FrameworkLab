# Phase 09 — Siege / 城池攻防

SiegeArea：OuterArea、Walls、Gates、Towers、Breach、InnerArea、CaptureObjective。

DefenseStructure：Wall/Gate/Tower/Barricade/Trap/Core + extension。

Gate state：Closed/Opening/Open/Closing/Destroyed。

Siege Unit 仍是 Unit + Tags + AttackProfile。

Breach：structure destroyed→event→navigation refresh→new path。

支援 assault、defense、wave defense、survival、escort siege、boss siege。

Acceptance：Attacker 破門→入城→capture→owner change。
