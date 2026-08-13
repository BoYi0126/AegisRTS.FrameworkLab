# Open Issues

Status: `READY FOR PHASE03 REVISION REVIEW`

## Reviewer decisions required

1. Front Waist Cloth 的中央 broad fold 與兩側主平面是否已足以消除硬板感。
2. Scarf 的寬扁截面、胸前貼合與終止是否符合預期的 stylized cloth language。
3. Upper Arm 的 planar form 是否在 Normal／64 px 距離仍足以取代圓柱感。
4. Shield back 的 strap／grip／brace hierarchy 已可讀，但 grip 尺寸與接觸位置仍需 Reviewer 確認美術意圖。
5. Boot 已消除 glued-on panel，但 final leather breakup、roughness 與接縫須留到 Phase 04。

## Known limitations

- Unity 使用 neutral clay／Material-ID review materials；shader 表現不代表 final material。
- FBX 不含 animation clips，僅供 geometry／scale／ground／RTS readability 檢查。
- A-pose 的盾牌手部接觸用於靜態讀形，不代表 final skin deformation。
- Unity 第一次 Null Graphics Device capture 為空白，已排除；成果圖由 graphics-enabled batch run 重拍，相關兩份 log 保留供 audit。

沒有已知 non-manifold、loose edge、zero-area face、錯誤尺度、離地或 Runtime Prefab replacement 問題。Phase 03 是否通過仍由 Reviewer 決定。

