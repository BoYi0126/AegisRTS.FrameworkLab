# Framework API 目標介面

目標使用體驗：CreateFaction、CreateSettlement、SpawnUnit、CreateArmy、IssueCommand、Recruit、Build、Research、StartSiege、CaptureSettlement、AddResource、StartScenario、Save、Load。

核心 API 必須同時可由 Player、AI、Scenario、Test 使用，不能只靠 Inspector 操作。

## Phase 01 Core API

`AegisRTS.Core` 是 Pure C# assembly，`noEngineReferences` 啟用且不引用 Gameplay 或 Presentation。所有時間步進與訊息派送均由呼叫端明確驅動，不依賴 MonoBehaviour lifecycle。

### Entity

```csharp
var ids = new EntityIdGenerator();
EntityId entityId = ids.Next();
```

- `EntityId`：可比較、可序列化為 `ulong` 的 runtime entity identity；`0` 為 `Invalid`。
- `EntityIdGenerator`：產生非零、單調遞增且可重設的 deterministic ID sequence。

### Time / Random

```csharp
var clock = new GameClock();
clock.SetSpeed(2d);
clock.Advance(unscaledDeltaSeconds);

IRandomSource random = new SeededRandom(seed);
int index = random.NextInt(maxExclusive);
```

- `GameClock`：追蹤 scaled／unscaled time，支援 `Pause`、`Resume`、正數 `Speed` 與 `Reset`。
- `SeededRandom`：固定 PCG 演算法；相同 seed 與呼叫順序必須得到相同結果，不依賴 `System.Random` runtime implementation。

### Command

```csharp
using var validator = commandBus.RegisterValidator<MyCommand>(Validate);
using var handler = commandBus.RegisterHandler<MyCommand>(Handle);
CommandDispatchResult result = commandBus.Dispatch(command);
```

- Command 實作 `ICommand`。
- 每個 command type 最多一個 handler，可有多個依註冊順序執行的 validator。
- 任一 validator 拒絕時不執行 handler，原因由 `CommandDispatchResult.Error` 回傳。
- Player、AI、Scenario 與 Test 應呼叫同一個 `Dispatch` flow。
- 註冊回傳 `IDisposable`，dispose 後取消該 handler 或 validator。

### Event

```csharp
using var subscription = eventBus.Subscribe<MyEvent>(OnEvent);
eventBus.Publish(eventData);
```

- Event 實作 `IEvent`，代表已經發生的事實。
- `EventBus` 依註冊順序同步發布給 exact event type 的 subscriber。
- 發布使用 subscriber snapshot，因此 callback 內 unsubscribe 不會破壞當次迭代。

### State Machine

```csharp
var machine = new StateMachine<MyContext>(context);
machine.Start(initialState);
machine.Tick(deltaSeconds);
machine.TransitionTo(nextState);
machine.Stop();
```

- State 實作 `IState<TContext>` 的 `Enter`、`Tick`、`Exit` lifecycle。
- Transition 保證 previous `Exit` 在 next `Enter` 之前發生。

### Diagnostics / Debug

- `IDiagnosticSink`：Core diagnostics adapter boundary。
- `DiagnosticBuffer`：thread-safe bounded history，滿載時移除最舊紀錄。
- `NullDiagnosticSink`：未配置 diagnostics 時的 no-op implementation。
- `GameClock`、`SeededRandom`、`CommandBus`、`EventBus`、`StateMachine` 提供 `GetDebugSummary()`；Bus 與 State Machine 可將生命週期事件送至 `IDiagnosticSink`。

Command Bus、Event Bus、State Machine 與 Entity ID Generator 預設由單一 simulation thread 擁有；`DiagnosticBuffer` 可安全接受多執行緒寫入。
