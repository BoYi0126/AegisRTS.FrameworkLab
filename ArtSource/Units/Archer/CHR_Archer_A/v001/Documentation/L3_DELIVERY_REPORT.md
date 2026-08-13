# Unit_04 Archer Prototype L3 delivery report

- Asset ID: `unit.archer`
- Character: `CHR_Archer_A_v001`
- Projectile: `PRJ_Arrow_Basic_v001`
- Source: deterministic derivative of the repository's accepted Infantry v001 body geometry.
- Distinction: infantry shield/sword and heavy guards removed; curved bow, bow string, back quiver, visible quiver arrows, and light open silhouette added.
- Rig: Unity Humanoid-compatible A-Pose; Root Motion off.
- Clips: Idle, Move, Attack_Ranged, Hit, Death at 30 FPS.
- `ProjectileRelease`: frame 22 of Attack_Ranged; gameplay projectile remains authoritative.
- LOD: LOD0, LOD1, LOD2 share one skeleton and runtime team-color contract.
- Arrow: separate 0.82 m FBX, center pivot, Unity local Z+ forward.
- Materials: Base plus runtime TeamColor; no baked friendly/enemy duplicate mesh.
- Release: `Blocked - derivative source rights and final animation quality require owner review`.
