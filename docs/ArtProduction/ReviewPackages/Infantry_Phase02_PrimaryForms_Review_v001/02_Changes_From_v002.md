# Changes From v002

## PRESERVED

- `CHR_Infantry_A_v002` files and their hashes.
- `unit.infantry`, `PF_Unit_Infantry`, Animator/event names and Runtime Prefab references.
- One 23-bone armature and existing empty/socket/anchor names as the contract baseline.
- Overall approved role: left shield, right one-handed short sword, heavy infantry stance.

## REBUILT

- Torso, pelvis, head and limbs use shaped curved volumes instead of the v002 prototype block language.
- Face receives nose, brow, cheek and chin masses suitable for later sculpt/topology decisions.
- Helmet now has dome, readable rim, top mount and short plume.
- Each shoulder has three descending armor layers with a wide upper silhouette.
- Chest uses a shaped shell, center mass and four raised lamellar bands.
- Waist adds belt, front/rear/side panels and cloth/scarf masses.
- Legs expose thigh／knee／calf rhythm; boots use rounded sole, heel, instep and toe forms.
- Shield has body thickness, rim, boss and cross reinforcement.
- Sword has a tapered blade, guard, grip and pommel.

## MODIFIED

- Meshes are object-parented to the preserved armature in a static A-pose for form review.
- Shield and sword store planned `LeftHand`／`RightHand` attachment metadata; final rigid binding and skinning are deferred.
- Six flat ID materials separate skin, cloth, armor, leather, wood and steel. They are not final materials.

## NOT YET ADDRESSED

- Final UV／Texture／Team Color mask／production shader。
- Final topology、skin weights、deformation and attachment binding。
- Animation source durability and Animation Polish。
- Formal LOD chain／impostor and Unity acceptance captures。
- Runtime Prefab replacement and Golden Sample lock。

## NOT CHANGED

No existing Blender／FBX／Texture／Material／Prefab／Animation／Shader／Scene／C# Production Asset was edited. No Unity import or runtime switch was made.
