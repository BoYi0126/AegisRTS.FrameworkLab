# Getting Started

## Requirements

- Unity 6000.0 or newer.
- Input System 1.20.0 or newer.

## Install and import a sample

1. Open Package Manager.
2. Add this package from disk or by its Git URL.
3. Select AegisRTS Framework and import one of the three samples.
4. Open the imported scene and enter Play Mode.

## Create a content pack

Content is JSON authored against stable lowercase IDs. Load it with `ContentPackJsonLoader`, validate it with `ContentPackValidator`, then activate it through the composition root. Display names are never references.

```csharp
ContentPack pack = new ContentPackJsonLoader().Load(json);
var assets = new ContentAssetCatalog(prefabIds);
ContentValidationResult result = new ContentPackValidator().Validate(pack, assets);
```

Keep background-specific data and assets outside `Runtime/`. The runtime package contains no Three Kingdoms or Fantasy content.

## Assembly boundaries

- `AegisRTS.Core`: deterministic IDs, time, random, commands, events, diagnostics, performance.
- `AegisRTS.Gameplay`: authoritative pure C# RTS state and rules.
- `AegisRTS.Presentation`: Unity input, camera, movement, combat, and HUD adapters.
- `AegisRTS.Persistence`: save, load, replay, and debug command boundaries.

Continue with [Framework API](FrameworkApi.md) for the setup, command, and persistence entry points.
