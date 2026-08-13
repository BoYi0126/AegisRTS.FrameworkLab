# 02 — Asset Pipeline L1 → L4

- Specification Version：1.0
- Status：`CURRENT WORKFLOW TARGET`

## Gate 原則

每一層都有獨立輸入、輸出、Owner 與退件條件。上一層未通過，不得用下一層的技術成果掩蓋設計缺口。

```text
L1 Concept Design
→ L1 Review
→ L2 Production Character Sheet
→ L2 Review
→ L3 Production 3D Asset
→ DCC QA
→ L4 Unity Integration
→ Unity/RTS QA
→ GOLDEN_SAMPLE_CANDIDATE / PRODUCTION_READY
```

## L1 — Concept Design

回答「角色應該長什麼樣子」。

### REQUIRED 輸出

- Faction（未定時用 `TBD`／world-neutral，不自行發明 lore）。
- Unit Type、Gameplay Role、Fantasy／power fantasy。
- Body Proportion、Primary Silhouette、Armor Weight。
- Equipment、Weapon、Team Color Placement。
- Color Palette、Material intent、Visual Motif、shape-language notes。
- Front／Side／Back／3/4、黑色 Silhouette、至少一張 RTS 俯視縮小測試。
- 同一 design ID／version；所有視圖的頭盔、裝備、比例一致。

### Gate

- 不看文字可辨識 Unit Class、Weapon、Armor Weight。
- 不靠 Team Color 仍與相鄰兵種不同。
- 主要尺寸與 `docs/ArtSpecs/06_尺寸總表.md` 不衝突；偏離有 approval。
- L1 可以有藝術表現，但不是唯一建模尺寸基準。

`CURRENT` Infantry 有 L1 concept，但解析度 1254×1254且為持盾姿態；Archer L1 `NOT FOUND`。

## L2 — Production Character Sheet

L2 在本版正式定義為 **3D Production Reference**，不是第二張 Concept，也不是 Game Model。

### REQUIRED 輸出

- Front、Left/Right Side、Back、3/4 Front、3/4 Back。
- A-Pose 或 rig-neutral pose；正投影基準圖不得使用誇張透視。
- Weapon、Shield、Bow、Quiver、Armor layer、removable module breakdown。
- Height、shoulder width、foot position、weapon dimensions與比例標線。
- Color palette、material callout、Team Color area／coverage。
- Silhouette notes；Primary／Secondary／Tertiary forms 分層。
- 正／側／背視圖疊圖驗證，關鍵 landmarks 誤差不得超過角色高度 2%。

### AI 一致性修正流程

1. 先選一張 master front與固定 design tokens。
2. 生成其他視圖後，以耳、肩、肘、骨盆、膝、腳底、武器端點對齊。
3. 人工 paint-over 修正裝備數量、左右手、層次、紋樣與比例。
4. 以正投影線稿重建可量測 views；AI beauty render只能保留作 mood reference。
5. 差異仍無法消除時標記 `L2 REJECTED`，不得直接交給建模者自行猜測。

`CURRENT` 舊文件把 L2 定義成 Game Model。既有 GLB／FBX 不會被刪除，但在新流程只能叫 `Legacy Prototype Model`；Infantry 與 Archer 的正式 L2 都是 `NOT FOUND`。

## L3 — Production 3D Asset

### REQUIRED 工作包

1. Modeling：依 approved L2建立 Primary→Secondary→Tertiary forms。
2. Retopology：可變形關節 edge flow、硬甲獨立或合理拓撲。
3. UV：有效 texel density、鏡像／重疊有文件、無意外 overlap。
4. Texture：BaseColor、Normal、Metallic、Roughness、AO、Team Color authoring source。
5. Material：材質辨識、slot／shader contract、packing manifest。
6. Rigging：Skeleton Family、Humanoid／Generic 選擇、sockets。
7. Skinning：extreme-pose review、rigid armor strategy。
8. Animation：clip list、fps、event frame、Root Motion contract。
9. LOD：LOD0～3與 optional impostor；不可只有 LOD0。
10. Export Validation：Scale、axis、pivot、bounds、triangles、materials、bones、hashes。

### Gate

- DCC neutral lighting與 game-like lighting renders齊全。
- LOD0符合 production target；不得把「可丟進 Unity 的 FBX」當完成定義。
- 可編輯 source、exported runtime、license/provenance都存在。

## L4 — Unity Integration

### REQUIRED 組成

- Stable Prefab ID與Resources／Addressable mapping。
- Animator／Avatar、Apply Root Motion Off。
- Material／Shader、Team Color與selection highlight互動。
- LODGroup、renderer／material count、bounds。
- Collider、SelectionAnchor、HealthBarAnchor、GroundContact。
- Weapon／Shield／Projectile／VFX sockets。
- Animation Events只連 presentation timing；gameplay damage仍由 domain system擁有。
- Unit scale、Y-up／Z-forward、grounding。
- Close／Medium／Normal RTS／Far及128／64／32 px readback。
- Windows Development Build與Player log。

### 現有 stable contracts

- Infantry：`PF_Unit_Infantry`、`AttackImpact`、`Socket_R_Hand`、`Socket_L_Hand`、`Socket_WeaponTip`。
- Archer：`PF_Unit_Archer`、`ProjectileRelease`、`Socket_Projectile`、arrow local Z+。
- 共用 Animator parameters：`Speed`、`MoveRate`、`AttackRate`、`Attack`、`Hit`、`Die`、`IsDead`。

Remaster 應維持這些 L4 contract；若必要變更，先建立 adapter／migration test，不能讓美術直接改 gameplay truth。

## Gate 失敗處理

- L1 fail：改設計，不進 turnaround。
- L2 fail：人工對齊，不讓 modeler自行解讀矛盾視圖。
- L3 visual fail：回到對應 form／material／skin／animation工序；不得以 Unity post-process遮掩。
- L4 fail：判斷是 source、import、shader或presentation integration；不得直接覆寫 source檔嘗試碰運氣。
- License fail：允許本地 Prototype時標 `Release Blocked`，禁止 `PRODUCTION_READY`。

