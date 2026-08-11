# Framework API

AegisRTS exposes small subsystem APIs instead of a framework-wide manager. A composition root owns the systems and adapters, while Player, AI, Scenario, and tests send the same command types through `CommandBus`.

## Operation map

| Goal | Public entry point |
| --- | --- |
| Create a faction | `FactionSystem.Register` |
| Create a settlement | `SettlementSystem.Register` |
| Spawn a unit | Implement `IUnitSpawnSink.SpawnUnit` and register the new entity with the required gameplay systems |
| Create an army | Dispatch `CreateArmyCommand` through `ArmyCommandRouter` |
| Issue an intent | `CommandBus.Dispatch<TCommand>` |
| Recruit a unit | Dispatch `RecruitUnitCommand` through `RecruitmentCommandRouter` |
| Build a structure | Dispatch `ConstructBuildingCommand` through `BuildingCommandRouter` |
| Research technology | Dispatch `ResearchTechnologyCommand` through `TechnologyCommandRouter` |
| Start a siege | Dispatch `StartSiegeCommand` through `SiegeCommandRouter` |
| Capture a settlement | Dispatch `CaptureSettlementCommand` through `SettlementCommandRouter`; siege capture uses `CaptureSiegeCommand` |
| Add resources | `EconomySystem.AddResource` for the authoritative economy account |
| Start a scenario | Dispatch `StartScenarioCommand` through `ScenarioCommandRouter` |
| Save or load | `GameStateCoordinator.Save` and `GameStateCoordinator.Load` |

## Composition rule

Setup operations such as faction, settlement, and unit creation belong to the application's composition layer. Runtime intents use commands, mutations stay inside their owning subsystem, and consumers read state through query interfaces and immutable snapshots.

Do not wrap these systems in a manager that owns combat, economy, AI, persistence, and presentation state. A thin application-specific composition root may expose convenience methods, but it must delegate to these public contracts.

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
