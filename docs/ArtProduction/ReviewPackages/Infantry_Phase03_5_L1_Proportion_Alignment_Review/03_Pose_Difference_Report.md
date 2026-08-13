# Pose Difference Report

## 1. 有多少差距主要來自 A-Pose？

手臂外張、盾牌離 torso、劍臂向外延伸與「肩部總寬」的大部分視覺差距屬 `POSE_ONLY`。未修改 P03R1 幾何的 L1 Compare Pose 已使手臂回到 torso 旁，證明不能以 A-Pose front silhouette 直接判定 arm length。

## 2. Compare Pose 下仍存在什麼？

Hip／knee 偏高、torso 偏短、腿部 segment 節奏與 L1 不一致仍清楚存在，分類為 `PROPORTION_MISMATCH`。Chest 比 L1 估測寬；shoulder armor 只有小幅超出，分類為 `ARMOR_MISMATCH`，沒有縮 skeleton。

## 3. 真正 Body Proportion mismatch

Hip、knee、upper-leg、lower-leg、torso ratio，以及 head mesh／chest／hand／boot 局部尺寸。這些已用 controlled remap／scale修正，沒有新增細節或 bones。

## 4. 只是 Armor silhouette 差異

Outer shoulder armor 使 silhouette 比 anatomical shoulder 更寬。Skeleton shoulder width差異為6.4%，在8% width gate內；因此只做2% armor refit。Shield position在Compare Pose中主要是 pose／attachment差異，尺寸本身完全在明示範圍。

## 5. 已在 tolerance 內、不應再改

Overall height、Helmet width／top、anatomical shoulder width、Chest Center／Belt／Shoulder Y、Shield size與Sword length。UpperArm與Forearm concept joints有遮擋及估測誤差；pose diagnosis沒有支持改骨長，故保持原 contract並列為 `UNCERTAIN`／reviewer decision，而不是機械套 threshold。

