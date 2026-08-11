# Unity Asset / Scene / Prefab 規範

## Bootstrap

只負責全域服務與 scene flow，不放完整戰場。

## Sandbox

每個大型系統獨立：RTS、Combat、Siege、AI。

## Unit Prefab

主要是 View：

```text
Root
├─ Visual
├─ Animator
├─ SelectionIndicator
├─ WorldUIAnchor
├─ Collider
└─ View Components
```

## ScriptableObject

適合 Definition，不適合保存 runtime HP、當前資源、當前 owner。

## Scene Reference

跨 scene 使用 ID / Service，不到處用 SerializeField 綁場景物件。

## Placeholder

前期使用 Cube/Capsule/Plane 優先；Gameplay 穩定後才換正式美術。
