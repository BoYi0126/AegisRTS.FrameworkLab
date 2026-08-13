# Geometry Stats

| Metric | Result |
|---|---:|
| Height | 1.824010849 m |
| Vertices | 16,858 |
| Triangles | 33,248 |
| Mesh Count | 98 |
| Material IDs | 6 |
| Armatures | 1 |
| Bones | 23 |
| Empties | 10 |
| Actions | 0 |
| Non-manifold edges | 0 |
| Boundary edges | 0 |
| Loose edges | 0 |
| Zero-area faces | 0 |

Material IDs：`MATID_Cloth`、`MATID_Leather`、`MATID_Metal`、`MATID_Skin`、`MATID_Team`、`MATID_Wood`。這些是 review-only 分區材質，並非 final materials。

## Modified-area accounting

| Area | Geometry treatment |
|---|---|
| WaistCloth | 局部替換為封閉、有薄厚度的 fold profile |
| Scarf | 局部改成寬扁且貼胸的 cloth drape |
| UpperArm | 既有形體壓平、降低完美圓柱讀形 |
| Shield | Back brace／strap／grip hierarchy 局部重整 |
| Boot | 刪除附貼 panels，直接調整 boot primary mesh |

建議的 32K–36K triangle budget 內，最終為 33,248 triangles。完整逐物件資料見 `Manifests/Object_Manifest.csv`。

