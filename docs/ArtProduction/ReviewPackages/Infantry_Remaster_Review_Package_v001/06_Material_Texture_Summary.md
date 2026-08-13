# 06 — Material and Texture Summary

## 1. Runtime materials

| Material | Shader | Instancing | Texture references | Metallic／Smoothness | Current role |
|---|---|---:|---|---|---|
| `MAT_Infantry_Base` | URP Lit, GUID `933532a4...` | Enabled | BaseColor + Normal | 0／0.20 | body/equipment base surfaces |
| `MAT_Infantry_TeamColor` | URP Lit, same GUID | Enabled | None | 0／0.18 | solid runtime faction tint through property block |

Both are opaque, back-face culled and receive shadows. No Infantry custom shader was found; `Materials/Shaders/` remains intentionally empty.

## 2. Current textures

| Texture | Resolution | PNG channels | File size | Intended usage | Current material usage |
|---|---:|---|---:|---|---|
| `T_Infantry_A_BaseColor_1K.png` | 1024×1024 | 8-bit RGB | 5,451 bytes | palette/base colour | Base `_BaseMap`: YES |
| `T_Infantry_A_Normal_1K.png` | 1024×1024 | 8-bit RGB | 5,334 bytes | tangent-space normal | Base `_BumpMap`: YES |
| `T_Infantry_A_ORM_1K.png` | 1024×1024 | 8-bit RGB | 5,334 bytes | intended packed surface data | NO material reference found |
| `T_Infantry_A_TeamColorMask_1K.png` | 1024×1024 | 8-bit grayscale | 2,760 bytes | intended team mask | NO material reference found |

Small compressed source byte sizes and visual flatness indicate highly uniform/procedural prototype maps; file resolution alone is not production detail.

## 3. Unity texture importer

| Texture | sRGB | Texture type | Mipmaps | Read/Write | Streaming | Alpha transparency | Green flip |
|---|---:|---|---:|---:|---:|---:|---:|
| BaseColor | Yes | Default | Yes | Off | Off | Off | Off |
| Normal | No | Normal Map | Yes | Off | Off | Off | Off |
| ORM | No | Default | Yes | Off | Off | Off | Off |
| TeamColorMask | No | Default | Yes | Off | Off | Off | Off |

Metadata serializes a 1024 DefaultTexturePlatform setting and 2048 Standalone/WebGL rows with overrides off; the physical images are only 1024, so no platform can recover additional detail. Default compression row is uncompressed; Standalone/WebGL rows serialize normal compression but are not explicitly overridden.

## 4. Source/runtime identity

The four v002 ArtSource textures and four Unity runtime textures match byte-for-byte. The source master FBX and Unity master FBX also match. This package carries only one current texture binary set plus Unity `.meta`, avoiding redundant source copies; provenance paths remain in `Source_Copy_Map.csv` and source manifests.

## 5. Team color classification

The current implementation is neither blue/red duplicate meshes nor a mask-driven custom shader:

```text
One geometry family
+ split Base / TeamColor material slots
+ material-name selection
+ MaterialPropertyBlock _BaseColor / _Color
= current team-color result
```

The shield specifically keeps wood/metal on its Base slot and tints only its TeamColor panel/emblem slot. Body Team meshes supply other faction-colour regions. Existing historical images show blue player and red enemy instances, but standardized paired captures are missing.

## 6. ORM/mask ambiguity

- Source production spec proposes a future packed map such as Metallic/AO/Roughness/TeamMask, but that is not implemented here.
- Current file `ORM` channel semantics are documented as intended data, yet no current material consumes it.
- TeamColorMask is grayscale but does not drive current runtime tint.
- Do not repack, rename or connect these maps during review collection.

## 7. Review implications

Preserve candidates：two-value team readability, shield panel isolation, MaterialPropertyBlock/no instance-per-unit behavior.

Modify/partial rebuild candidates：material separation strategy, production texel density, actual surface response, readable metal/leather/cloth grouping, normal detail, mip-safe team mask and packed-map contract.

Rebuild trigger：approved shader contract or L2 modular/material regions cannot be achieved with the existing UV/layout, or provenance blocks texture reuse.

## 8. Missing evidence

- Channel-by-channel diagnostic renders。
- UV checker/distortion and mip review。
- Production lighting comparison in Unity。
- GPU/batch/material-instance profile at representative unit count。
- Approved shader/team-mask contract and target-platform texture budget。
- License/provenance evidence sufficient for release。
