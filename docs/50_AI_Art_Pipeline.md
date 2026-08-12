# AI Art Pipeline

可以使用 AI 生成美術，而且 Framework 前期建議 placeholder，Gameplay 穩定後再換。

適合：Hero portrait、UI icon、concept art、building/environment concept、loading illustration。

Sprite animation / 3D model 仍需要一致性、game-ready topology/rig/animation 等處理，不能把單張生成圖直接等同完成資產。

流程：Primitive→AI Concept→Approved Design→Game-ready Asset→Animation/VFX→Import→Prefab→Validation。

大量生成前先建立 Art Bible；Hero 建 Character Sheet 保持臉、服裝、武器、比例一致。

正式發布前保存生成工具、來源、License、Font/Audio/第三方資產授權紀錄。

## 來源檔與 Unity 資產分流

AI／外包的完整原始交付先放在 Repository Root 的 `ArtSource/`，依 `Units|Buildings|UI|VFX/<AssetName>/v001/` 分版保存 Concept、Model、Texture、Preview、UV、Documentation 與 Tools。

只有通過格式、授權、尺寸、材質與 Unity Game View 驗收後，才把執行期真正需要的 Model、Texture、Material、Animation 與 Prefab 放進 `Assets/AegisRTS/Content/Shared/Art/`。概念圖、驗收截圖、UV 圖與生成報告不要一起匯入 Unity。

收件、分類、Unity 候選、驗收與整合是不同狀態；整理進資料夾不代表資產已能在遊戲顯示。
