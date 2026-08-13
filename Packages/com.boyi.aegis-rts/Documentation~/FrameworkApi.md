# Framework API

AegisRTS exposes small subsystem APIs instead of a framework-wide manager. A composition root owns the systems and adapters, while Player, AI, Scenario, and tests send the same command types through `CommandBus`.

Selection-driven products can observe `SelectionService.Revision` and map the current `ISelectionQuery` through `SelectionCommandContextResolver`. The resolver returns `Domestic`, `UnitSettings`, `Siege`, or `None`; a mixed unit/building selection prioritizes unit settings. `UnityRtsInputAdapter` filters world-command actors to friendly units and heroes so selectable structures never receive movement or combat intents.

## Operation map

| Goal | Public entry point |
| --- | --- |
| Create a faction | `FactionSystem.Register` |
| Create a settlement | `SettlementSystem.Register` |
| Spawn a unit | Implement `IUnitSpawnSink.SpawnUnit` and register the new entity with the required gameplay systems |
| Create an army | Dispatch `CreateArmyCommand` through `ArmyCommandRouter` |
| Issue an intent | `CommandBus.Dispatch<TCommand>` |
| Set unit engagement mode | Dispatch `SetUnitEngagementModeCommand`; `CombatSystem.SetEngagementMode` owns mode and leash state |
| Recruit a unit | Dispatch `RecruitUnitCommand` through `RecruitmentCommandRouter` |
| Build a structure | Dispatch `ConstructBuildingCommand` through `BuildingCommandRouter` |
| Research technology | Dispatch `ResearchTechnologyCommand` through `TechnologyCommandRouter` |
| Start a siege | Dispatch `StartSiegeCommand` through `SiegeCommandRouter` |
| Repair a defense structure | Dispatch `RepairDefenseStructureCommand` through `SiegeCommandRouter`; the repairer must belong to the defender and the profile must be repairable |
| Capture a settlement | Dispatch `CaptureSettlementCommand` through `SettlementCommandRouter`; siege capture uses `CaptureSiegeCommand` |
| Add resources | `EconomySystem.AddResource` for the authoritative economy account |
| Start a scenario | Dispatch `StartScenarioCommand` through `ScenarioCommandRouter` |
| Save or load | `GameStateCoordinator.Save` and `GameStateCoordinator.Load` |
| Snapshot active production | `BuildingSystem.SnapshotQueue`, `TechnologySystem.SnapshotQueue`, and `RecruitmentSystem.SnapshotQueue` |
| Restore active production | Restore economy first, then call each system's `RestoreQueuedJob`; restored jobs are already paid/reserved |
| Snapshot or restore movement | `MovementSystem.SnapshotOrders` and `MovementSystem.RestoreOrders` |
| Restore transient runtime state | `CombatSystem.RestoreRuntimeState`, `ArmySystem.RestoreRuntimeState`, and `AiSystem.RestoreRuntimeState` |
| Read or adjust camera zoom sensitivity | `RtsCameraController.ZoomSensitivity`, `IncreaseZoomSensitivity`, and `DecreaseZoomSensitivity` |

## Composition rule

Setup operations such as faction, settlement, and unit creation belong to the application's composition layer. Runtime intents use commands, mutations stay inside their owning subsystem, and consumers read state through query interfaces and immutable snapshots.

Do not wrap these systems in a manager that owns combat, economy, AI, persistence, and presentation state. A thin application-specific composition root may expose convenience methods, but it must delegate to these public contracts.

`RtsCameraController` applies `ZoomSensitivity` as a multiplier to its serialized base zoom speed. Products can call `IncreaseZoomSensitivity` and `DecreaseZoomSensitivity`; the controller clamps the multiplier to the supported 1x-6x range while `RtsCameraRigModel` continues to own the zoom-distance bounds.

## Minimal command flow

```csharp
var commands = new CommandBus();
var armies = new ArmySystem(heroes, rules, orderExecutor, membershipSink, events);

using var armyCommands = new ArmyCommandRouter(commands, armies);
CommandDispatchResult result = commands.Dispatch(
    new CreateArmyCommand(armyId, factionId, memberIds, commanderId));
```

Use the corresponding router for recruitment, construction, technology, settlement capture, siege, and scenario commands. Retain each router for the lifetime of its registrations and dispose it during teardown.

For the complete per-phase contract, see the repository document `docs/26_Framework_API_目標介面.md`.

`GameRuleSet` can identify a world-neutral settlement archetype and expose gate repair, stronghold recruitment, and capture-instead-of-destruction switches. Content and product composition decide how those switches assemble systems; package Runtime does not contain story-specific faction or setting names.

## Persistence restore contract

Queue snapshots expose identifiers and remaining time only. The aggregate save must also preserve the authoritative `EconomyAccountSnapshot`, including balances, production, population used, and population capacity. On load, restore accounts and already completed progression before calling `RestoreQueuedJob`; these methods deliberately do not spend resources or reserve population a second time.

Register every referenced entity or agent before restoring Movement, Combat, Army, or AI runtime state. Restore navigation and siege blockers before movement orders so path acceptance is evaluated against the loaded world.

## Unit engagement modes

`HoldGround`, `Normal`, and `Aggressive` proactively acquire valid hostile targets within 0.5x, 1.0x, and 1.5x attack range. `Retaliate` never proactively acquires a target and locks the hostile damage source only after receiving damage. Explicit `AttackTargetCommand` always remains valid regardless of stance.

`CombatantSnapshot` exposes `EngagementMode`, `TargetReason`, `EngagementOrigin`, `DefenseRange`, and `ShouldReturnToOrigin`. Tick `CombatMovementCoordinator` after `CombatSystem.Tick` to translate chase and return intent into `MovementSystem` orders without duplicating authoritative state.
