# 選取驅動指揮面板規範

## 目的

玩家不應先選取物件，再手動尋找對應指揮頁面。每當 selection 實際改變，HUD 必須依選取類型自動切換到最相關的 command panel；玩家仍可在同一 selection 下手動切換其他頁籤，自動規則不得每 frame 強制覆蓋。

## 自動切頁規則

| 選取內容 | 自動頁籤 | 說明 |
| --- | --- | --- |
| 我方 `Structure`／`Settlement` | `Domestic`／內政 | 顯示建設、研究、招募與生產佇列。 |
| 任一 `Unit`／`Hero` | `UnitSettings`／兵種設定 | 顯示堅守陣地、普通、攻擊、反擊等兵種設定。 |
| 敵方 `Structure`／`Settlement` | `Siege`／攻城行動 | 顯示破門、進入內院與壓制主堡流程。 |
| 只有 Neutral 建築或空選取 | 保持目前頁籤 | 不猜測玩家意圖。 |
| 建築與兵種混合框選 | `UnitSettings`／兵種設定 | 可操控兵種優先，建築不會收到移動或戰鬥快捷命令。 |

## 觸發時機

- `SelectionService.Revision` 只在 selected ID set 實際改變時遞增。
- `PlayablePrototypeBootstrap.LateUpdate` 偵測 revision 改變後呼叫 `SelectionCommandContextResolver`。
- 玩家在 selection 不變時手動切換其他頁籤，頁籤會維持選擇。
- 重複點選同一 selection 不增加 revision，也不重設頁籤。

## 可選取建築

Playable Prototype 必須將下列 graybox world objects 註冊成正式 `UnitySelectableView`：

| 世界物件 | EntityId | Kind | 初始 affiliation |
| --- | --- | --- | --- |
| Player City | `PlayerCityId` | Settlement | Friendly |
| Neutral Village | `VillageId` | Settlement | Neutral |
| Fortress Gate | `FortressGateId` | Structure | Enemy |
| Fortress Stronghold | `EnemyFortressId` | Settlement | Enemy |

敵方主堡完成佔領後，Stronghold selectable affiliation 必須更新為 Friendly；下一次選取時改走內政情境。

## 指令安全規則

Selection 可以包含建築與兵種，但 RTS world command actor list 只允許：

- `Affiliation == Friendly`
- `Kind == Unit || Kind == Hero`

因此 Move、Stop、Hold 與 context Attack 不會把 Settlement／Structure ID 送入 Movement／Combat handler。HUD 的內政與攻城按鈕仍走各自既有 CommandBus command。

## 架構

```text
UnitySelectableView
→ SelectionService selected ID set + Revision
→ SelectionCommandContextResolver（pure presentation policy）
→ PrototypeCommandTab
→ HUD command panel
```

- `SelectionService` 只保存 selection truth，不引用 Prototype。
- `SelectionCommandContextResolver` 只讀 `ISelectionQuery` 與 descriptor，不讀 GameObject 名稱。
- `PrototypeCommandTab` 只代表產品 HUD 頁籤，不進入 Gameplay authoritative state 或 Save payload。
- `PrototypeHudAdapter` 可讀同一 `ISelectionQuery`，讓建築也能顯示 definition、owner、defense 或 descriptor。

## 驗收條件

1. 點選我方主堡後自動切到內政。
2. 點選或框選兵種／英雄後自動切到兵種設定。
3. 點選敵方城門／主堡後自動切到攻城行動。
4. 主堡加兵種混合框選時切到兵種設定。
5. selection 不變時，玩家手動切頁不會被強制切回。
6. 建築不接受 Move／Stop／Hold／Attack actor command。
7. HUD click 不穿透到世界 selection。
8. 選取建築時 Selected panel 能顯示名稱與據點資料。
