# 04 — RTS Silhouette and Readability Standard

- Specification Version：1.0
- Priority：`REQUIRED / HIGHEST VISUAL GATE`

## 通用驗收

角色關閉 Texture、改成純黑、縮小到128／64／32 px screen height後，仍需辨識：Unit Class、Weapon Type、Armor Weight、Character Identity。每個判讀由至少3名未看名稱的測試者在2秒內回答；32 px至少2/3正確，64／128 px要求3/3。

測試矩陣：Front、Back、Left/Right、3/4、normal RTS pitch 55°；Idle、Move primary pose、Attack anticipation／impact各一張。Team Color與UI label在blind test中關閉。

## Infantry Golden Silhouette

`REQUIRED` 可見：Shield、Sword、Heavy Armor、Helmet、Strong Shoulder、Strong Lower Body。

- Shield應形成角色寬度至少25%的主要輪廓，不能貼在胸前變成同一塊黑形。
- Sword離身體保留negative space；劍尖方向不可與腿重合。
- 肩甲／胸甲厚度表達中重甲；靴／腿部不能細到像Archer。
- Idle時盾與頭盔都可見；Attack impact仍保持近戰重心。

## Archer Golden Silhouette

`REQUIRED` 可見：Bow、Quiver、Lighter Armor、Slimmer Silhouette、Different Shoulder Shape。

- Bow曲線在64 px仍是開放弧形，不像短棍。
- Quiver與背部形成第二辨識點；不得被身體完全遮住。
- 肩部較窄、甲片較少，手臂活動空間清楚。
- Attack anticipation需形成拉弓三角形negative space。

> Infantry換Bow不得當作Archer完成。不同兵種必須有不同body emphasis、armor mass、shoulder、pose與equipment silhouette。

## Hero／Special／Building

- Hero：至少三個只屬於該角色的Primary silhouette tokens；不能只靠顏色或頭像。
- Cavalry／Vehicle：移動方向由大形狀可見；不可把普通人型放大冒充。
- Building：關閉texture後仍以roof、tower、entrance、weapon platform與footprint辨識功能；Barracks、Archery Range、Stable、Mage Tower、Town Center不得只用貼圖區分。

## 失敗處理

1. 32 px失敗先改Primary Forms、pose或negative space。
2. 不先加rivet、noise、logo或更亮Team Color。
3. 修改後重跑黑剪影、灰階與blue/red色盲安全比較。
4. 記錄版本、受測圖、答對數與主要混淆對象。

