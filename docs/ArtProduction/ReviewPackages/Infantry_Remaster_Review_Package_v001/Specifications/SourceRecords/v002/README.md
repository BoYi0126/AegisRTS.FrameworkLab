# Unit_03 Infantry L3 v002 corrected build package

This package supersedes the original blocked L3 attempt. It has now been built with Blender 5.2.0 LTS and integrated into Unity 6000.5.7f1.

## Delivered outputs

- Editable source: `Source/CHR_Infantry_A_v002.blend`
- Humanoid master: `Models/SK_Infantry_A_v002.fbx`
- Separate clips: Idle, Move, Attack_A, Hit, Death under `Animations/`
- Original v001 inputs retained under `Input_v001/`
- Single grayscale team-color mask under `Textures/`
- Deterministic Blender build source and build result under `Source/`
- Generation, import, validation, event, license, and checksum records under `Documentation/`

The character remains based on `CHR_Infantry_A_v001`; it is not a redesign. Bind pose is A-Pose. Shield and sword are rigid objects attached to `LeftHand` and `RightHand`. All clips are authored In Place and the Root bone is not keyed.

## Rebuild

Windows:

```bat
Source\BUILD_WINDOWS.bat "C:\Program Files\Blender Foundation\Blender 5.2\blender.exe"
```

Direct command:

```text
blender --background --factory-startup --python Source/build_unit03_l3_blender.py -- --package-root <this folder>
```

The script rebuilds the BLEND/FBX outputs, records triangle counts, and refreshes `Documentation/MANIFEST.json`.

## Unity integration

Runtime assets are copied to:

```text
Assets/AegisRTS/Content/Shared/Art/Units/Infantry/
```

Run `AegisRTS.Editor.InfantryL3PrefabBuilder.BuildAndValidate` to reapply the model import settings, Humanoid Avatar sharing, animation clips/events, Animator Controller, materials, LODGroup, anchors, and prefab references.

## Team color rule

Only `T_Infantry_A_TeamColorMask_1K.png` is used for team color. Do not generate blue/red duplicate geometry.

## Release note

Technical L3 integration is validated. Commercial release still requires the project owner to retain or confirm provenance and commercial rights for the original v001 source asset.
