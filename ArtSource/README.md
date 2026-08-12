# AegisRTS Art Source

`ArtSource/` 保存 AI、外包與人工美術的原始交付、概念圖、製作報告、UV、預覽與尚未通過 Unity 驗收的候選模型。

此資料夾位於 Unity `Assets/` 外，目的如下：

- 避免 Unity 匯入概念圖、尺寸圖、驗收截圖與大型來源檔。
- 保留每次交付的原始內容、版本、報告與授權紀錄。
- 讓「來源交付」與「正式遊戲資產」分開審查。
- 避免尚未支援的格式或未驗證 Editor script 影響專案編譯。

## 目錄規則

```text
ArtSource/
└─ Units/
   └─ <UnitType>/
      └─ <AssetName>/
         └─ v001/
            ├─ Concepts/
            ├─ Models/
            ├─ Textures/
            ├─ Previews/
            │  ├─ Camera/
            │  └─ Dimensions/
            ├─ UV/
            ├─ Documentation/
            ├─ Tools/
            └─ ASSET_MANIFEST.md
```

建築使用 `ArtSource/Buildings/`，UI 使用 `ArtSource/UI/`，VFX 使用 `ArtSource/VFX/`，並遵守相同的 Asset／Version 分層。

## Unity 正式資產位置

通過格式、授權、尺寸、材質與 Unity 實機驗收後，只把遊戲執行時需要的檔案放進：

```text
Assets/AegisRTS/Content/Shared/Art/
├─ Units/
├─ Buildings/
├─ Materials/
├─ Textures/
└─ VFX/
```

不把 `Concepts`、`Previews`、`UV`、生成報告或原始 AI 任務包複製到 `Assets`。

## 狀態詞彙

- `Received`：已收件並保存，尚未驗證。
- `Source Organized`：已分類且檔案完整，但未代表可在 Unity 使用。
- `Unity Candidate`：格式可由目前專案匯入，等待場景驗收。
- `Accepted`：尺寸、材質、授權與 Unity 驗收通過。
- `Integrated`：已建立 Prefab 並接上遊戲系統。
- `Blocked`：存在格式、授權、品質或技術阻塞。

任何資產只有到 `Accepted` 後才能視為正式美術；`Integrated` 才代表玩家能在遊戲中看到。

