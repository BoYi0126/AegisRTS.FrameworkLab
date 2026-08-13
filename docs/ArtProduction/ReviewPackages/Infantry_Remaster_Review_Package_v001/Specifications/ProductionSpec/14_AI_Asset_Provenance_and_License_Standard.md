# 14 — AI Asset Provenance and License Standard

- Specification Version：1.0
- Gate：`REQUIRED FOR PRODUCTION_READY`

## 必填紀錄

每個AI生成或AI協作資產建立`PROVENANCE.md`或等價machine-readable record：

```text
Asset ID:
Asset Version:
Tool:
Tool Version:
Model / Service:
Prompt:
Negative Prompt:
Seed:
Job ID:
Date:
Human Modification:
Input Reference:
Third-party Asset:
Source URL / Receipt:
License:
Commercial Use:
Modification Allowed:
Redistribution in Game Build Allowed:
Training / Reference Restrictions:
Reviewer / Review Date:
Known Limitations:
Release Status:
```

資訊不存在一律寫`UNKNOWN`，不以`N/A`掩蓋本來應有但找不到的Seed／Job ID。只有流程確定不使用random generation（例如deterministic Blender script）才可寫`N/A - deterministic`並附證據。

## Required Attachments

- 完整原始Prompt／Negative Prompt，不只摘要。
- Tool／model terms或license snapshot／link與取得日期。
- 所有input references與第三方asset清單；包含mocap、brush、font、texture、kitbash。
- Human modification逐步摘要與修改工具版本。
- Source／export hashes與manifest。
- 商用發布decision由project owner或指定reviewer簽核；Agent不可自行推定法律權利。

## Current Provenance Audit

| Asset | Known | Unknown／Gate |
| --- | --- | --- |
| Infantry v001 | L1/L2 files、metrics、received package | 原生成Tool／Model／Prompt／Negative Prompt／Seed／Job ID／第三方來源／商用權利 `UNKNOWN` |
| Infantry v002 | Blender 5.2、deterministic script、Prompt、修改步驟、無新增第三方 | derivative source rights承接v001，commercial approval未完成 |
| Archer v001 | Blender 5.2 deterministic derivative、Prompt、無新增第三方 | derivative rights承接v001；正式animation/art review未完成 |

因此兩個Golden Sample皆為`Release Blocked`。技術整合通過不會自動提升license狀態。

## AI／人工混合流程

1. AI輸出先進`ArtSource`隔離區。
2. Human review L1/L2一致性與IP／logo／文字風險。
3. DCC人工或scripted修正，完整保留變更紀錄。
4. License reviewer確認commercial use／modification／build redistribution。
5. 通過technical／visual／license gates後，才複製runtime exports到`Assets`。

## Reject／Block

- 來源不明、條款無法重現、現有IP／商標、浮水印、第三方動作無license：`REJECTED`或`Release Blocked`。
- 缺Seed／Job ID但工具本應提供：`UNKNOWN`，不可編造。
- 只保存最終圖／FBX而沒有input／prompt／terms：不可`PRODUCTION_READY`。

