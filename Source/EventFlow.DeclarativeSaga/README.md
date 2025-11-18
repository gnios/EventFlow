# EventFlow.StateMachine

A declarative saga builder for EventFlow that provides a fluent, type-safe API for orchestrating complex business workflows. Build sagas with confidence using compiler-enforced type safety and intuitive builder patterns.

## What Problems Does This Solve?

When building distributed systems with EventFlow, you often need to coordinate multiple aggregates and services to complete a business transaction. Traditional saga implementations require:

- ❌ Manual event handling with complex state management
- ❌ Error-prone timeout and compensation logic
- ❌ Difficult-to-read workflow definitions
- ❌ No compile-time validation of saga flows

**EventFlow.StateMachine** solves these problems by providing:

- ✅ **Type-safe saga definitions** - The compiler enforces valid workflow configurations
- ✅ **Fluent API** - Readable, self-documenting saga code
- ✅ **Automatic timeout handling** - Built-in compensation support with automatic job cancellation
- ✅ **Interface Segregation** - Your IDE guides you through valid next steps
- ✅ **Multiple execution paths** - Easily define success, failure, and alternative flows

## Common Use Cases

- **Order Processing**: Coordinate stock reservation, payment processing, and shipping
- **Booking Systems**: Manage room reservations, payments, and confirmations
- **Workflow Orchestration**: Coordinate multi-step business processes across services
- **Payment Processing**: Handle payment flows with automatic rollback on timeout
- **Resource Management**: Coordinate resource allocation with automatic cleanup

## Prerequisites

- **.NET 6.0** or later
- **EventFlow** framework (this library extends EventFlow)
- Basic understanding of CQRS and Event Sourcing concepts

## Installation

### Via NuGet Package Manager

```bash
dotnet add package EventFlow.StateMachine
```

### Via Package Manager Console

```powershell
Install-Package EventFlow.StateMachine
```

### Via .csproj

```xml
<ItemGroup>
  <PackageReference Include="EventFlow.StateMachine" Version="1.0.0" />
</ItemGroup>
```

## Quick Start

Here's a complete, working example that you can copy and paste:

```csharp
using EventFlow.StateMachine;
using EventFlow.Sagas;
using EventFlow.Aggregates;
using System.Threading;
using System.Threading.Tasks;

// 1. Define your saga state
public class OrderSagaState : DeclarativeSagaState<OrderSaga, OrderSagaId, OrderSagaState>
{
    public bool IsStarted { get; private set; }
    
    public void Apply(OrderSagaStartedEvent e) => IsStarted = true;
}

// 2. Create your saga class
// IMPORTANT: You MUST implement ISagaIsStartedBy and ISagaHandles interfaces
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,      // Event that starts the saga
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,    // Events handled during execution
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>
{
    public OrderSaga(OrderSagaId id, IServiceProvider serviceProvider) 
        : base(id, serviceProvider)
    {
        // Register saga state
        var state = RegisterState(new OrderSagaState());
        
        // Define saga workflow using the builder
        Define(builder =>
        {
            // Start saga when order is created
            builder.Initially()
                .When<OrderAggregate, OrderId, OrderCreatedEvent>()
                .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(evt.AggregateIdentity))
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new ReserveStockCommand(evt.AggregateIdentity));

            // Success path: stock reserved
            builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new CompletePaymentCommand(evt.AggregateIdentity))
                .AndCompensateWith<OrderAggregate, OrderId>((evt, saga) => 
                    new StockRollbackCommand(evt.AggregateIdentity));

            // Success path: payment completed
            builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
                .ThenEmitSagaEvent((evt, saga) => new OrderSagaCompletedEvent(evt.AggregateIdentity))
                .AndComplete();
        });
    }

    // 3. REQUIRED: Implement event handlers - delegate to HandleEventAsync
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
}

// 4. Register in EventFlow configuration
services.AddEventFlow(o => o
    .AddSagas(typeof(OrderSaga))
    .AddSagaLocators(typeof(OrderSagaLocator))
    // ... register events, commands, etc.
);
```

**What this example does:**

1. When an `OrderCreatedEvent` occurs, the saga starts and publishes a `ReserveStockCommand`
2. When stock is reserved, it publishes a `CompletePaymentCommand` with a 10-minute timeout
3. If payment completes within 10 minutes, the saga completes successfully
4. If payment doesn't complete in time, the stock is automatically rolled back (compensation)
5. All state changes are tracked through saga events

### Important: Implementing Required Interfaces

**You MUST implement the EventFlow saga interfaces** for your saga to work correctly:

- **`ISagaIsStartedBy<TAggregate, TIdentity, TEvent>`**: Implement this for the event that initiates your saga
- **`ISagaHandles<TAggregate, TIdentity, TEvent>`**: Implement this for each event your saga handles during execution

Each interface requires a `HandleAsync` method that delegates to `HandleEventAsync`:

```csharp
public Task HandleAsync(IDomainEvent<TAggregate, TIdentity, TEvent> domainEvent, 
    ISagaContext sagaContext, CancellationToken cancellationToken) 
    => HandleEventAsync(domainEvent, sagaContext, cancellationToken);
```

**Why this is required**: EventFlow uses these interfaces to discover which sagas should handle which events. Without them, your saga won't receive any events.

## Key Features

### Type-Safe Builder API

The builder uses Interface Segregation to guide you through valid configurations. Your IDE will only show methods that are valid at each step:

```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenPublish(...)      // ✅ Available
    .ThenEmitSagaEvent(...) // ✅ Available
    .ThenComplete()        // ✅ Available
    .ThenPublish(...)      // ❌ Compiler error - already completed!
```

### Automatic Timeout Handling

Define timeouts with automatic compensation. If the expected event arrives before timeout, compensation is automatically cancelled:

```csharp
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .AndCompensateWith<OrderAggregate, OrderId>((evt, saga) => new StockRollbackCommand(...));
```

### Multiple Execution Paths

Easily define success and failure paths:

```csharp
// Success path
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...));

// Failure path
builder.When<OrderAggregate, OrderId, OrderStockReservationFailedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new MarkOrderFailedCommand(...))
    .ThenComplete();
```

### Scheduled Commands

Schedule commands to run in the future:

```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenSchedule<OrderAggregate, OrderId>(
        (evt, saga) => new SendReminderCommand(evt.AggregateIdentity),
        TimeSpan.FromDays(7));
```

## Documentation

For detailed documentation, see:

- **[Complete Usage Guide](#complete-usage-guide)** - Comprehensive reference below
- **[EventFlow Documentation](https://docs.geteventflow.net/)** - Core EventFlow concepts
- **[Examples](Source/EventFlow.Examples.Simple)** - Working examples in the repository

---

## Complete Usage Guide

### Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Implementing Required Interfaces](#implementing-required-interfaces)
4. [Command Reference](#command-reference)
5. [Usage Patterns](#usage-patterns)
6. [Complete Examples](#complete-examples)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

---

## Introduction

The `DeclarativeSagaBuilder` provides a fluent API that guides developers to build valid sagas through **Interface Segregation**. Each step in the builder chain returns a specific interface that only exposes valid next actions, preventing invalid saga configurations.

### Key Concepts

- **Interface Segregation**: Each builder stage returns an interface that only allows valid next actions
- **Type Safety**: The compiler enforces the correct order of operations
- **IntelliSense Guidance**: Your IDE will only show valid methods at each stage

---

## Getting Started

### Step 1: Create Your Saga State

First, create a state class that inherits from `DeclarativeSagaState`:

```csharp
public class OrderSagaState : DeclarativeSagaState<OrderSaga, OrderSagaId, OrderSagaState>
{
    public bool IsStarted { get; private set; }
    public bool IsCompleted { get; private set; }
    
    public void Apply(OrderSagaStartedEvent e) => IsStarted = true;
    public void Apply(OrderSagaCompletedEvent e) => IsCompleted = true;
}
```

### Step 2: Create Your Saga Class

Create your saga class inheriting from `DeclarativeSaga`:

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>
{
    public OrderSaga(OrderSagaId id, IServiceProvider serviceProvider) 
        : base(id, serviceProvider)
    {
        var state = RegisterState(new OrderSagaState());
        
        Define(builder =>
        {
            // Configure your saga here
        });
    }
}
```

### Step 3: Register in EventFlow

Register your saga in EventFlow configuration:

```csharp
services.AddEventFlow(o => o
    .AddSagas(typeof(OrderSaga))
    .AddSagaLocators(typeof(OrderSagaLocator))
    // ... register events, commands, etc.
);
```

---

## Implementing Required Interfaces

**This is a critical step that is often missed!** Your saga class must implement the EventFlow saga interfaces for events to be routed to your saga.

### ISagaIsStartedBy

Implement `ISagaIsStartedBy<TAggregate, TIdentity, TEvent>` for the event that initiates your saga:

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>  // ← This interface
{
    // Required: Implement HandleAsync for the starting event
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> domainEvent, 
        ISagaContext sagaContext, CancellationToken cancellationToken) 
        => HandleEventAsync(domainEvent, sagaContext, cancellationToken);
}
```

### ISagaHandles

Implement `ISagaHandles<TAggregate, TIdentity, TEvent>` for each event your saga handles during execution:

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,      // ← Each event needs this
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>   // ← Each event needs this
{
    // Required: Implement HandleAsync for each event
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> domainEvent, 
        ISagaContext sagaContext, CancellationToken cancellationToken) 
        => HandleEventAsync(domainEvent, sagaContext, cancellationToken);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> domainEvent, 
        ISagaContext sagaContext, CancellationToken cancellationToken) 
        => HandleEventAsync(domainEvent, sagaContext, cancellationToken);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> domainEvent, 
        ISagaContext sagaContext, CancellationToken cancellationToken) 
        => HandleEventAsync(domainEvent, sagaContext, cancellationToken);
}
```

### Complete Example with Interfaces

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>
{
    public OrderSaga(OrderSagaId id, IServiceProvider serviceProvider) 
        : base(id, serviceProvider)
    {
        var state = RegisterState(new OrderSagaState());
        
        Define(builder =>
        {
            builder.Initially()
                .When<OrderAggregate, OrderId, OrderCreatedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new ReserveStockCommand(evt.AggregateIdentity));
            
            builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new CompletePaymentCommand(evt.AggregateIdentity));
            
            builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
                .ThenComplete();
        });
    }

    // REQUIRED: Implement all interface methods
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
}
```

**Important Notes:**

- ✅ **Always delegate to `HandleEventAsync`**: This is the base method that processes events using your builder configuration
- ✅ **One interface per event**: Each event your saga handles needs its own `ISagaHandles` interface
- ✅ **Match builder configuration**: The events in your interfaces must match the events you use in `builder.When<>()`
- ❌ **Don't skip this step**: Without these interfaces, EventFlow won't route events to your saga

---

## Command Reference

### 1. Initially() - Starting the Saga

**Purpose**: Configure what happens when the saga is first started by an event.

**Returns**: `InitiallyBuilder<TSaga, TIdentity, TLocator>`

**Usage**:
```csharp
builder.Initially()
    .When<OrderAggregate, OrderId, OrderCreatedEvent>()
    // ... continue with actions
```

**When to use**: Always use this for the event that initiates your saga.

---

### 2. When<TEvent>() - Handling Events

**Purpose**: Configure what happens when a specific event occurs during saga execution.

**Returns**: `WhenBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>`

**Usage**:
```csharp
// After Initially()
builder.Initially()
    .When<OrderAggregate, OrderId, OrderCreatedEvent>()
    // ... actions

// During execution
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    // ... actions
```

**When to use**: 
- After `Initially()` to specify the starting event
- Directly on `builder` to handle events during saga execution

**Important**: All `When()` calls belong to the **SAME saga**. Each call represents a different possible path (success, failure, etc.).

---

### 3. When<TEvent>(timeoutMinutes) - Events with Timeout

**Purpose**: Handle events with a timeout. If the next event doesn't arrive within the timeout, compensation will be executed.

**Returns**: `WhenWithTimeoutBuilder<TSaga, TIdentity, TLocator, TAggregate, TAggregateIdentity, TAggregateEvent>`

**Parameters**:
- `timeoutMinutes` (int): Minutes to wait for the next event before executing compensation

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .AndCompensateWith<OrderAggregate, OrderId>((evt, saga) => new StockRollbackCommand(...));
```

**When to use**: When you need to wait for a response and have a fallback if it doesn't arrive in time.

**Note**: This overload **REQUIRES** you to define compensation using `AndCompensateWith()`.

---

## Action Commands

After calling `When()`, you can chain various actions. The builder uses Interface Segregation to guide you through valid options.

### 4. ThenPublish<TCommand>() - Publish a Command

**Purpose**: Publish a command to an aggregate when the event occurs.

**Returns**: `IAfterPublishBuilder<...>` (allows: emit events, schedule commands, timeout, or complete)

**Signature**:
```csharp
ThenPublish<TCommandAggregate, TCommandAggregateIdentity>(
    Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
        new ReserveStockCommand(evt.AggregateIdentity, evt.AggregateEvent.Quantity))
```

**Parameters**:
- `commandFactory`: A function that receives the domain event and saga instance, and returns a command

**What you can do next**:
- `ThenEmitSagaEvent()` - Emit a saga event
- `AndEmitSagaEvent()` - Emit a saga event (Gherkin style)
- `ThenSchedule()` - Schedule a command for future execution
- `AndSchedule()` - Schedule a command (Gherkin style)
- `ThenTimeoutAfter()` - Configure a timeout
- `ThenComplete()` - Complete the saga
- `AndComplete()` - Complete the saga (Gherkin style)

---

### 5. ThenPublishMany<TCommand>() - Publish Multiple Commands

**Purpose**: Publish multiple commands to an aggregate when the event occurs.

**Returns**: `IAfterPublishBuilder<...>` (same as `ThenPublish`)

**Signature**:
```csharp
ThenPublishMany<TCommandAggregate, TCommandAggregateIdentity>(
    Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, IEnumerable<ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>>> commandsFactory)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenPublishMany<OrderAggregate, OrderId>((evt, saga) => 
        new[] 
        { 
            new ReserveStockCommand(evt.AggregateIdentity, evt.AggregateEvent.Quantity),
            new NotifyWarehouseCommand(evt.AggregateIdentity)
        })
```

**When to use**: When you need to publish multiple commands in response to a single event.

---

### 6. ThenEmitSagaEvent<TEvent>() - Emit a Saga Event

**Purpose**: Emit an internal event to record facts about the saga state.

**Returns**: `IAfterEmitBuilder<...>` (allows: publish commands, schedule commands, timeout, or complete)

**Signature**:
```csharp
ThenEmitSagaEvent<TSagaEvent>(
    Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, TSagaEvent> eventFactory)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenEmitSagaEvent((evt, saga) => 
        new OrderSagaStartedEvent(
            evt.AggregateIdentity,
            evt.AggregateEvent.CustomerId,
            DateTime.UtcNow))
```

**What you can do next**:
- `ThenPublish()` - Publish a command
- `AndPublish()` - Publish a command (Gherkin style)
- `ThenSchedule()` - Schedule a command
- `AndSchedule()` - Schedule a command (Gherkin style)
- `ThenTimeoutAfter()` - Configure a timeout
- `ThenComplete()` - Complete the saga
- `AndComplete()` - Complete the saga (Gherkin style)

---

### 7. ThenSchedule<TCommand>() - Schedule a Command

**Purpose**: Schedule a command to be executed after a delay.

**Returns**: `IAfterScheduleBuilder<...>` (allows: emit events, publish commands, or complete)

**Signature**:
```csharp
ThenSchedule<TCommandAggregate, TCommandAggregateIdentity>(
    Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCommandAggregate, TCommandAggregateIdentity, IExecutionResult>> commandFactory,
    TimeSpan delay)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenSchedule<OrderAggregate, OrderId>(
        (evt, saga) => new SendReminderCommand(evt.AggregateIdentity),
        TimeSpan.FromDays(7))
```

**Parameters**:
- `commandFactory`: Function that creates the command
- `delay`: Time to wait before executing the command

**What you can do next**:
- `ThenEmitSagaEvent()` - Emit a saga event
- `AndEmitSagaEvent()` - Emit a saga event (Gherkin style)
- `ThenPublish()` - Publish a command
- `AndPublish()` - Publish a command (Gherkin style)
- `ThenComplete()` - Complete the saga
- `AndComplete()` - Complete the saga (Gherkin style)

---

### 8. ThenTimeoutAfter() - Configure Timeout

**Purpose**: Configure a timeout to wait for the next event. **REQUIRES** compensation definition.

**Returns**: `ITimeoutBuilder<...>` (only allows compensation methods)

**Signature**:
```csharp
ThenTimeoutAfter(TimeSpan timeout)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .ThenTimeoutAfter(TimeSpan.FromMinutes(10))
    .ThenCompensateWith<OrderAggregate, OrderId>((evt, saga) => 
        new ReleaseStockCommand(evt.AggregateIdentity));
```

**Important**: After `ThenTimeoutAfter()`, you **MUST** call either:
- `ThenCompensateWith()` - Define compensation
- `AndCompensateWith()` - Define compensation (Gherkin style)

**What happens**:
- If the expected event arrives before timeout: compensation job is automatically cancelled
- If timeout occurs: compensation command is executed

---

### 9. ThenCompensateWith<TCommand>() - Define Compensation

**Purpose**: Define the compensation command to execute if timeout occurs.

**Returns**: `IAfterCompensateBuilder<...>` (allows: emit events, publish commands, or complete)

**Signature**:
```csharp
ThenCompensateWith<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity>(
    Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, ICommand<TCompensationCommandAggregate, TCompensationCommandAggregateIdentity, IExecutionResult>> compensationCommandFactory)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .ThenTimeoutAfter(TimeSpan.FromMinutes(10))
    .ThenCompensateWith<OrderAggregate, OrderId>((evt, saga) => 
        new StockRollbackCommand(evt.AggregateIdentity))
```

**When to use**: Always after `ThenTimeoutAfter()` or when using `When(timeoutMinutes)`.

---

### 10. ThenComplete() - Complete the Saga

**Purpose**: Mark the saga as completed. No further events will be processed.

**Returns**: `void` (ends the chain)

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderFailedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new MarkOrderFailedCommand(...))
    .ThenComplete()
```

**When to use**: 
- At the end of a failure path
- When the saga has reached its final state
- After all necessary actions have been completed

---

### 11. Then() - Execute Synchronous Action

**Purpose**: Execute a synchronous action when the event occurs.

**Returns**: `WhenBuilder<...>` (allows chaining more actions)

**Signature**:
```csharp
Then(Action<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga> action)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .Then((evt, saga) => 
    {
        saga._logger?.LogInformation("Order created: {OrderId}", evt.AggregateIdentity);
        // Perform synchronous operations
    })
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...))
```

**When to use**: For simple synchronous operations like logging, validation, or state updates.

---

### 12. ThenAsync() - Execute Asynchronous Action

**Purpose**: Execute an asynchronous action when the event occurs.

**Returns**: `WhenBuilder<...>` (allows chaining more actions)

**Signature**:
```csharp
ThenAsync(Func<IDomainEvent<TAggregate, TAggregateIdentity, TAggregateEvent>, TSaga, CancellationToken, Task> action)
```

**Usage**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenAsync(async (evt, saga, ct) => 
    {
        await saga._externalService.ProcessOrderAsync(evt.AggregateIdentity, ct);
        await saga._notificationService.SendEmailAsync(evt.AggregateEvent.CustomerId, ct);
    })
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...))
```

**When to use**: For I/O operations, API calls, or any asynchronous work.

---

## Gherkin-Style Methods

The builder supports both "Then" and "And" prefixes for better readability, following Gherkin/BDD conventions:

- `AndPublish()` - Same as `ThenPublish()`
- `AndEmitSagaEvent()` - Same as `ThenEmitSagaEvent()`
- `AndSchedule()` - Same as `ThenSchedule()`
- `AndCompensateWith()` - Same as `ThenCompensateWith()`
- `AndComplete()` - Same as `ThenComplete()`

**Example**:
```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
    .AndPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...))
    .AndSchedule<OrderAggregate, OrderId>(
        (evt, saga) => new SendReminderCommand(...),
        TimeSpan.FromDays(7))
```

---

## Usage Patterns

### Pattern 1: Simple Success Path

```csharp
builder.Initially()
    .When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...));

builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...));

builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
    .ThenEmitSagaEvent((evt, saga) => new OrderSagaCompletedEvent(...))
    .ThenComplete();
```

### Pattern 2: Success and Failure Paths

```csharp
// Success path
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...));

// Failure path
builder.When<OrderAggregate, OrderId, OrderStockReservationFailedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new MarkOrderFailedCommand(...))
    .ThenComplete();
```

### Pattern 3: Timeout with Compensation

```csharp
// Using When(timeoutMinutes) - Recommended
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .AndCompensateWith<OrderAggregate, OrderId>((evt, saga) => new StockRollbackCommand(...));

// Using ThenTimeoutAfter - Alternative
builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
    .ThenTimeoutAfter(TimeSpan.FromMinutes(10))
    .ThenCompensateWith<OrderAggregate, OrderId>((evt, saga) => new StockRollbackCommand(...));
```

### Pattern 4: Multiple Actions

```csharp
builder.When<OrderAggregate, OrderId, OrderCreatedEvent>()
    .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(...))
    .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...))
    .ThenSchedule<OrderAggregate, OrderId>(
        (evt, saga) => new SendReminderCommand(...),
        TimeSpan.FromDays(7))
    .ThenComplete();
```

---

## Complete Examples

### Example 1: Order Processing Saga

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservationFailedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentFailedEvent>
{
    public OrderSaga(OrderSagaId id, IServiceProvider serviceProvider) : base(id, serviceProvider)
    {
        var state = RegisterState(new OrderSagaState());
        
        Define(builder =>
        {
            // Step 1: Saga starts when order is created
            builder.Initially()
                .When<OrderAggregate, OrderId, OrderCreatedEvent>()
                .ThenEmitSagaEvent((evt, saga) => new OrderSagaStartedEvent(
                    evt.AggregateIdentity,
                    evt.AggregateEvent.CustomerId,
                    DateTime.UtcNow))
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new ReserveStockCommand(evt.AggregateIdentity, evt.AggregateEvent.Items));

            // Step 2: Success path - stock reserved
            builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new CompletePaymentCommand(evt.AggregateIdentity, evt.AggregateEvent.TotalAmount))
                .ThenTimeoutAfter(TimeSpan.FromMinutes(15))
                .ThenCompensateWith<OrderAggregate, OrderId>((evt, saga) => 
                    new ReleaseStockCommand(evt.AggregateIdentity));

            // Step 3: Payment completed
            builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
                .ThenEmitSagaEvent((evt, saga) => new OrderSagaCompletedEvent(
                    evt.AggregateIdentity,
                    DateTime.UtcNow))
                .ThenComplete();

            // Step 4: Failure path - stock reservation failed
            builder.When<OrderAggregate, OrderId, OrderStockReservationFailedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new MarkOrderFailedCommand(evt.AggregateIdentity, "Stock reservation failed"))
                .ThenComplete();

            // Step 5: Failure path - payment failed
            builder.When<OrderAggregate, OrderId, OrderPaymentFailedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new ReleaseStockCommand(evt.AggregateIdentity))
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => 
                    new MarkOrderFailedCommand(evt.AggregateIdentity, "Payment failed"))
                .ThenComplete();
        });
    }

    // REQUIRED: Implement all interface methods
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservationFailedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentFailedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
}
```

### Example 2: Saga with Timeout (Recommended Pattern)

```csharp
public class OrderSaga : DeclarativeSaga<OrderSaga, OrderSagaId, OrderSagaLocator>,
    ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,
    ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>
{
    public OrderSaga(OrderSagaId id, IServiceProvider serviceProvider) : base(id, serviceProvider)
    {
        var state = RegisterState(new OrderSagaState());
        
        Define(builder =>
        {
            builder.Initially()
                .When<OrderAggregate, OrderId, OrderCreatedEvent>()
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new ReserveStockCommand(...));

            // Using When(timeoutMinutes) - cleaner syntax
            builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
                .ThenPublish<OrderAggregate, OrderId>((evt, saga) => new CompletePaymentCommand(...))
                .AndCompensateWith<OrderAggregate, OrderId>((evt, saga) => new StockRollbackCommand(...));

            builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
                .ThenComplete();
        });
    }

    // REQUIRED: Implement all interface methods
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
    
    public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> e, 
        ISagaContext ctx, CancellationToken ct) => HandleEventAsync(e, ctx, ct);
}
```

---

## Best Practices

1. **Always implement required interfaces**: `ISagaIsStartedBy` and `ISagaHandles` are required for EventFlow to route events to your saga.

2. **Always define compensation for timeouts**: The builder enforces this, but be thoughtful about what compensation means for your domain.

3. **Use meaningful saga events**: Emit saga events to record important state changes for debugging and auditing.

4. **Group related paths**: Use comments to group success and failure paths for better readability.

5. **Prefer `When(timeoutMinutes)`**: The overloaded `When()` method with timeout is cleaner than `ThenTimeoutAfter()`.

6. **Use Gherkin style for readability**: `And` methods can make your saga definitions read more naturally.

7. **Complete sagas explicitly**: Always call `ThenComplete()` on terminal paths to make the saga lifecycle clear.

8. **Match interfaces with builder configuration**: Ensure every event in your `ISagaHandles` interfaces has a corresponding `builder.When<>()` call.

---

## Troubleshooting

### "Saga is not receiving events"

**Check these common issues:**

1. **Missing interfaces**: Did you implement `ISagaIsStartedBy` and `ISagaHandles`?
2. **Missing HandleAsync methods**: Each interface requires a `HandleAsync` method that delegates to `HandleEventAsync`
3. **Saga not registered**: Did you call `.AddSagas(typeof(YourSaga))` in EventFlow configuration?
4. **Events not registered**: Did you register all saga events in EventFlow configuration?

### "Cannot find method X after Y"

This is **by design**! The Interface Segregation pattern prevents invalid builder chains. Check the command reference to see what methods are available at each stage.

### "TimeoutBuilder requires compensation"

You **must** call `ThenCompensateWith()` or `AndCompensateWith()` after `ThenTimeoutAfter()`. This is enforced by the compiler to prevent sagas without timeout handling.

### "Multiple When() calls - are they separate sagas?"

No! All `When()` calls belong to the **same saga**. Each represents a different possible path (success, failure, etc.) within that saga.

---

## Summary

The `DeclarativeSagaBuilder` provides a type-safe, guided way to configure sagas:

- ✅ **Type-safe**: Compiler enforces valid chains
- ✅ **Self-documenting**: Interface Segregation shows available options
- ✅ **Flexible**: Supports multiple paths, timeouts, and compensation
- ✅ **Readable**: Gherkin-style methods improve readability

**Remember**: Always implement `ISagaIsStartedBy` and `ISagaHandles` interfaces, and delegate their `HandleAsync` methods to `HandleEventAsync`!

Follow the command reference above to build your sagas step by step, and let the compiler guide you to valid configurations!

---

## License

This library is part of the EventFlow project and is licensed under the [MIT License](../LICENSE).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please make sure to:
- Follow the existing code style
- Add tests for new features
- Update documentation as needed
- Ensure all tests pass
