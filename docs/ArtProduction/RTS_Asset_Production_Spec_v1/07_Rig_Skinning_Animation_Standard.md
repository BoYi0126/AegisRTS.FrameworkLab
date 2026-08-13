# 07 — Rig, Skinning and Animation Standard

- Specification Version：1.0

## Rig 選擇

- 一般人型：`REQUIRED` Unity Humanoid compatible。
- Monster、Horse、Dragon、Machine、Siege Vehicle：依需求Generic；禁止強迫Humanoid。
- Root Motion：一般RTS locomotion `In Place = Required`、`Root Motion = Off`。只有特定Skill經gameplay／presentation設計review才可例外。

最低Humanoid bones：

```text
Root → Pelvis/Hips → Spine → Chest → Neck → Head
UpperArm_L/R → LowerArm_L/R → Hand_L/R
UpperLeg_L/R → LowerLeg_L/R → Foot_L/R
```

Toe、Twist、Finger、Shoulder依deformation與weapon需求選用。

## Skeleton Families

| Family | 用途 | Rig Type |
| --- | --- | --- |
| `SKEL_Human_A` | 一般／輕裝人型 | Humanoid |
| `SKEL_Human_Heavy` | 重甲、大肩與特殊armor clearance | Humanoid |
| `SKEL_Quadruped` | Horse／beast | Generic |
| `SKEL_Giant` | 超大型人型／非標準比例 | Generic或經驗證Humanoid |
| `SKEL_Flying` | Flying creature | Generic |

`CURRENT / VERIFIED` Infantry與Archer都為23-bone Humanoid，Archer由Infantry body pipeline衍生，骨名與Animator contracts一致；技術上可共享Skeleton Family與retarget。`INFERRED`：正式production可共用`SKEL_Human_A`，但需先比較bind pose、bone orientation、body proportion與armor clearance；重甲Infantry若肩甲干涉則升到`SKEL_Human_Heavy`，不可為追求共用而犧牲deformation。

## Current Source Caveat

- 兩個source `.blend`只讀重開皆有1 armature／23 bones，但0 Actions。
- Builder scripts與獨立FBX保存可重建動畫；`.blend`不是完整animation source of truth。
- `REQUIRED` Remaster後把actions設為Fake User、NLA stash或其他durable datablock再存檔，重開後逐一驗證clips存在。

## Skinning Quality

直接退件：Shoulder／Elbow／Knee collapse、Armor rubber deformation、Shield穿身、Weapon穿手、Cape severe clipping、Waist armor collapse。

- 每vertex最多4 influences；需要smooth deformation的joint不得一律單權重。
- Rigid armor可100%綁單骨、bone-parent、surface transfer或segment strategy，但與柔軟underlayer邊界不得裂開。
- `CURRENT` Infantry／Archer body vertices最大1 influence，是可玩的rigid-piece prototype；Production remaster必須補extreme-pose測試與必要的multi-weight joint topology。
- Skin test至少包含arm raise、deep elbow bend、bow draw、shield guard、squat、run contact、death extreme。

## Weapon System

Weapon／Shield優先Separate Object／Separate Asset，不永久weld進character mesh。

沿用project實際命名：

```text
Socket_R_Hand
Socket_L_Hand
Socket_WeaponTip
Socket_Projectile
FX_Hit_Center
FX_Foot_L / FX_Foot_R
```

如另需`Shield_L`或`VFX_Weapon`，先在asset manifest與Prefab contract登記；不要平行建立同義socket。

## Animation Minimum

| Unit | REQUIRED clips／events |
| --- | --- |
| All standard units | Idle、Move、Attack_A、Hit、Death |
| Melee | `AttackImpact` |
| Ranged | `ProjectileRelease` |
| Optional | Attack_B/C、Block、Stun、Cast、Skill、Victory |

RTS動畫需Readable Anticipation、Clear Impact、Clear Recovery、Exaggerated Motion、Strong Pose。自然寫實但遠距看不出攻擊視為失敗。

## Timing／Authority

- 30 FPS authoring基準；事件誤差≤1 frame。
- Gameplay combat windup／cooldown是authoritative；`AttackRate`只將visual clip對齊。
- Animation Event不得直接扣HP、spawn authoritative projectile或改owner。
- Move命令可依既有Attack-Move contract取消未出手windup或visual backswing，但不能清除已成立cooldown。

## Acceptance

- [ ] Avatar valid／Generic mapping文件化。
- [ ] Root drift XZ≤0.01 m、Y≤0.02 m、rotation≤0.5°。
- [ ] `.blend`重開後actions仍存在；exported FBX與events manifest一致。
- [ ] 五個clips各循環／播放10次無跳格、腳滑、穿模。
- [ ] 64／32 px仍能讀anticipation／impact。
- [ ] Retarget到Skeleton Family reference animation不崩肩、肘、膝。

