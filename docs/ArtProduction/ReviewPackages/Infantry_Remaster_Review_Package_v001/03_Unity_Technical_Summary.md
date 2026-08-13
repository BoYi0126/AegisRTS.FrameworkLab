# 03 — Unity Technical Summary

> Method：read-only parsing of copied `ContentPack.json`, Prefab YAML, Controller YAML, Material YAML and importer `.meta`. Unity Editor was not launched; historical Builder/Test PASS is context, not a new validation result.

## 1. Current content and resource binding

```text
Definition: unit.infantry
Display: Infantry
Prefab ID: PF_Unit_Infantry
Resource path: AegisRTS/Units/Infantry/PF_Unit_Infantry
Prefab: Unity/Prefabs/PF_Unit_Infantry.prefab
```

`ContentPack.json` and `PrototypeUnitArtCatalog.cs` agree on the ID/path. The Prefab points only to current master GUID `15fa862142be95a44b46245b976ab639`; legacy GLB GUIDs are not referenced by the current Prefab.

## 2. Master FBX import settings

| Setting | Serialized value | Interpretation |
|---|---:|---|
| Global Scale | `1` | 1 Unity unit per source metre contract |
| Material Import Mode | `0` | Disabled; project materials assigned by builder/prefab |
| Mesh Compression | `0` | Off |
| Read/Write | `isReadable: 0` | Disabled |
| Import BlendShapes | `1` | Enabled, although copied source reports 0 shape keys |
| Import Visibility | `1` | Enabled |
| Import Cameras／Lights | `0`／`0` | Disabled |
| Bake Axis Conversion | `0` | Disabled |
| Normals | `normalImportMode: 0` | Import from file |
| Tangents | `tangentImportMode: 3` | Unity serialized mode 3; exact UI label depends on Unity version |
| Import Animation | `1` | Enabled on master |
| Animation Type | `3` | Humanoid |
| Avatar Setup | `1` | Create From This Model |
| Optimize Game Objects | builder explicitly `false` | Bone/socket transforms preserved |
| Animation Compression | `1` | Keyframe reduction serialized |

Historical builder/report says `isHuman=True` and `isValid=True`; this task verifies the importer/Pefab references but **does not rerun Avatar validation**.

## 3. Prefab component inventory

| Component type | Count | Notes |
|---|---:|---|
| GameObject／Transform | 51／51 | Imported rig, render objects, sockets and gameplay anchors |
| SkinnedMeshRenderer | 6 | Base + Team body for LOD0/1/2 |
| MeshRenderer／MeshFilter | 6／6 | Sword + shield for LOD0/1/2 |
| Animator | 1 | Avatar/master + `AC_Infantry`; Apply Root Motion off |
| LODGroup | 1 | Three LOD levels then culled |
| CapsuleCollider | 1 | Presentation prefab collider component exists; authoritative selection/movement remains runtime contract |
| MonoBehaviour | 2 | `PrototypeUnitAnimatorView`, `PrototypeUnitArtView` |

### Presentation components

| Component | Status | Reference／notes |
|---|---|---|
| `PrototypeUnitArtView` | CURRENT | selection、health anchors; six team-color renderers; animator view; no projectile socket |
| `PrototypeUnitAnimatorView` | CURRENT | `deathDurationSeconds=1.4777777`, movement reference 4.5 m/s, clip rate 1.8, attack event source time 0.43333334 s, cancel blend 0.07 s |
| Animator | CURRENT | controller GUID `013fcea...`; avatar GUID `15fa862...`; `m_ApplyRootMotion: 0` |
| LODGroup | CURRENT | thresholds 0.040／0.012／0.003 |
| SelectionAnchor | CURRENT | root-level anchor at Y=0.02; a duplicate imported anchor also exists below VisualRoot |
| HealthBarAnchor | CURRENT | root-level anchor at Y=2.1; imported duplicate also exists |
| GroundContact | CURRENT | imported plus root-level representations visible in YAML |
| Weapon sockets | CURRENT | `Socket_R_Hand`, `Socket_L_Hand`, `Socket_WeaponTip`, `Socket_Head` |
| Projectile socket | N/A | Prefab field is null; Infantry is melee |

No gameplay HP、damage、faction or command truth is owned by this art Prefab. Player/AI use shared gameplay commands; presentation reacts through existing adapters.

## 4. LOD and render composition

| LOD | Screen-relative transition | Triangles | Render composition |
|---|---:|---:|---|
| LOD0 | 0.040 | 4,376 | 2 skinned body renderers + rigid shield + rigid sword |
| LOD1 | 0.012 | 1,512 | same four-object split |
| LOD2 | 0.003 | 542 | same four-object split |
| Below LOD2 | culled | 0 | no LOD3/impostor |

The current thresholds are prototype values. Pixel-height and target-hardware calibration are missing.

## 5. Animator Controller

States found：`Idle`, `Move`, `Attack`, `Hit`, `Death`。

| Parameter | Serialized type | Default |
|---|---|---:|
| `Speed` | Float (`1`) | 0 |
| `MoveRate` | Float (`1`) | 0 |
| `AttackRate` | Float (`1`) | 1 |
| `Attack` | Trigger (`9`) | false |
| `Hit` | Trigger (`9`) | false |
| `Die` | Trigger (`9`) | false |
| `IsDead` | Bool (`4`) | false |

The controller references the five current animation FBXs. The presentation adapter converts movement/combat snapshots to these parameters and counts visual-only animation events.

## 6. Team color implementation

Current implementation is **D — separate material slots on one geometry set**:

1. Each LOD body has Base and Team skinned objects; shield has Base + TeamColor sub-material slots.
2. `PrototypeUnitArtView.ApplyTeamColor` scans shared material names for `TeamColor`.
3. It writes `_BaseColor` and `_Color` through a per-slot `MaterialPropertyBlock`.
4. Friendly color is `#4AA3D8`; enemy color is `#D94A45` in current bootstrap.
5. No blue/red duplicate current FBX is used.

This is not option A's mask shader. `T_Infantry_A_TeamColorMask_1K.png` exists but is not referenced by either current `.mat`.

## 7. Materials and shader

- Both materials use shader GUID `933532a4fcc9baf4fa0491de14d08ed7`, known in this Unity/URP baseline as `Universal Render Pipeline/Lit`.
- Both enable instancing variants.
- Base material references BaseColor and Normal; metallic 0, smoothness 0.2.
- TeamColor material has no texture; metallic 0, smoothness 0.18.
- No project Infantry `.shader`, `.shadergraph` or `.hlsl` was found. Package shader source is intentionally not copied.

## 8. Model/animation integration concerns for remaster review

- Imported visual uses an orientation/grounding correction under `VisualRoot`; preserve Prefab-root identity and test axes before swapping a new FBX.
- `optimizeGameObjects=false` is required for exposed sockets/anchors unless an explicit exposed-transform list replaces it.
- Six team-color renderers are serialized; a remaster that changes mesh/material split must migrate `PrototypeUnitArtView` references.
- Root motion must remain off unless gameplay movement architecture is separately redesigned.
- Legacy GLBs and `.asset` meshes are not current, but should remain available until replacement validation succeeds.

## 9. Not run

- Unity clean reimport／Builder validation
- EditMode／PlayMode tests
- standalone build／smoke test
- runtime Avatar API query
- renderer-bounds／LOD visual transition capture
- Profiler measurements

These remain `NOT RUN`, not inferred from historical reports.
