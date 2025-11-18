# OrderSaga - Diagrama de Fluxo

```mermaid
stateDiagram-v2
    [*] --> OrderCreated: OrderCreatedEvent<br/>(ISagaIsStartedBy)
    
    state OrderCreated {
        [*] --> EmitOrderSagaStartedEvent
        EmitOrderSagaStartedEvent --> PublishReserveStockCommand
        PublishReserveStockCommand --> [*]
    }
    
    OrderCreated --> StockReserved: OrderStockReservedEvent<br/>(ISagaHandles)
    OrderCreated --> StockReservationFailed: OrderStockReservationFailedEvent<br/>(ISagaHandles)
    
    state StockReserved {
        [*] --> EmitOrderSagaStockReservedEvent
        EmitOrderSagaStockReservedEvent --> PublishCompletePaymentCommand
        PublishCompletePaymentCommand --> [*]
    }
    
    StockReserved --> PaymentCompleted: OrderPaymentCompletedEvent<br/>(ISagaHandles)
    StockReserved --> PaymentFailed: OrderPaymentFailedEvent<br/>(ISagaHandles)
    
    state PaymentCompleted {
        [*] --> EmitOrderSagaPaymentCompletedEvent
        EmitOrderSagaPaymentCompletedEvent --> PublishMarkOrderCompletedCommand
        PublishMarkOrderCompletedCommand --> EmitOrderSagaCompletedEvent
        EmitOrderSagaCompletedEvent --> Complete
        Complete --> [*]
    }
    
    state StockReservationFailed {
        [*] --> EmitOrderSagaStockReservationFailedEvent
        EmitOrderSagaStockReservationFailedEvent --> PublishMarkOrderFailedCommand
        PublishMarkOrderFailedCommand --> EmitOrderSagaFailedEvent
        EmitOrderSagaFailedEvent --> Complete
        Complete --> [*]
    }
    
    state PaymentFailed {
        [*] --> EmitOrderSagaPaymentFailedEvent
        EmitOrderSagaPaymentFailedEvent --> PublishStockRollbackCommand
        PublishStockRollbackCommand --> PublishMarkOrderFailedCommand
        PublishMarkOrderFailedCommand --> EmitOrderSagaFailedEvent
        EmitOrderSagaFailedEvent --> Complete
        Complete --> [*]
    }
    
    PaymentCompleted --> [*]: Saga Finalizada<br/>(Completed)
    StockReservationFailed --> [*]: Saga Finalizada<br/>(Failed)
    PaymentFailed --> [*]: Saga Finalizada<br/>(Failed)
```

## Fluxo Principal (Happy Path)

```mermaid
sequenceDiagram
    participant Client
    participant CommandBus
    participant OrderAggregate
    participant OrderSaga
    participant StockService
    participant PaymentService
    
    Client->>CommandBus: CreateOrderCommand
    CommandBus->>OrderAggregate: Create()
    OrderAggregate->>OrderAggregate: Emit(OrderCreatedEvent)
    
    OrderCreatedEvent->>OrderSaga: HandleAsync (ISagaIsStartedBy)
    OrderSaga->>OrderSaga: Emit(OrderSagaStartedEvent)
    OrderSaga->>CommandBus: Publish(ReserveStockCommand)
    
    CommandBus->>OrderAggregate: ReserveStock()
    OrderAggregate->>OrderAggregate: Emit(OrderStockReservedEvent)
    
    OrderStockReservedEvent->>OrderSaga: HandleAsync (ISagaHandles)
    OrderSaga->>OrderSaga: Emit(OrderSagaStockReservedEvent)
    OrderSaga->>CommandBus: Publish(CompletePaymentCommand)
    
    CommandBus->>OrderAggregate: CompletePayment()
    OrderAggregate->>OrderAggregate: Emit(OrderPaymentCompletedEvent)
    
    OrderPaymentCompletedEvent->>OrderSaga: HandleAsync (ISagaHandles)
    OrderSaga->>OrderSaga: Emit(OrderSagaPaymentCompletedEvent)
    OrderSaga->>CommandBus: Publish(MarkOrderCompletedCommand)
    
    CommandBus->>OrderAggregate: MarkCompleted()
    OrderAggregate->>OrderAggregate: Emit(OrderCompletedEvent)
    
    OrderSaga->>OrderSaga: Emit(OrderSagaCompletedEvent)
    OrderSaga->>OrderSaga: Complete()
```

## Fluxo de Erro - Falha na Reserva de Estoque

```mermaid
sequenceDiagram
    participant Client
    participant CommandBus
    participant OrderAggregate
    participant OrderSaga
    
    Client->>CommandBus: CreateOrderCommand
    CommandBus->>OrderAggregate: Create()
    OrderAggregate->>OrderAggregate: Emit(OrderCreatedEvent)
    
    OrderCreatedEvent->>OrderSaga: HandleAsync
    OrderSaga->>CommandBus: Publish(ReserveStockCommand)
    
    CommandBus->>OrderAggregate: ReserveStock() [FALHA]
    OrderAggregate->>OrderAggregate: Emit(OrderStockReservationFailedEvent)
    
    OrderStockReservationFailedEvent->>OrderSaga: HandleAsync
    OrderSaga->>OrderSaga: Emit(OrderSagaStockReservationFailedEvent)
    OrderSaga->>CommandBus: Publish(MarkOrderFailedCommand)
    
    CommandBus->>OrderAggregate: MarkFailed()
    OrderAggregate->>OrderAggregate: Emit(OrderFailedEvent)
    
    OrderSaga->>OrderSaga: Emit(OrderSagaFailedEvent)
    OrderSaga->>OrderSaga: Complete()
```

## Fluxo de Erro - Falha no Pagamento (com Rollback)

```mermaid
sequenceDiagram
    participant Client
    participant CommandBus
    participant OrderAggregate
    participant OrderSaga
    
    Note over OrderSaga: Estado: StockReserved
    
    CommandBus->>OrderAggregate: CompletePayment() [FALHA]
    OrderAggregate->>OrderAggregate: Emit(OrderPaymentFailedEvent)
    
    OrderPaymentFailedEvent->>OrderSaga: HandleAsync
    OrderSaga->>OrderSaga: Emit(OrderSagaPaymentFailedEvent)
    OrderSaga->>CommandBus: Publish(StockRollbackCommand)
    OrderSaga->>CommandBus: Publish(MarkOrderFailedCommand)
    
    CommandBus->>OrderAggregate: MarkFailed()
    OrderAggregate->>OrderAggregate: Emit(OrderFailedEvent)
    
    OrderSaga->>OrderSaga: Emit(OrderSagaFailedEvent)
    OrderSaga->>OrderSaga: Complete()
```

## Estados da Saga

```mermaid
graph TD
    A[None] -->|OrderCreatedEvent| B[OrderCreated]
    B -->|OrderStockReservedEvent| C[StockReserved]
    B -->|OrderStockReservationFailedEvent| D[StockReservationFailed]
    C -->|OrderPaymentCompletedEvent| E[PaymentCompleted]
    C -->|OrderPaymentFailedEvent| F[PaymentFailed]
    E -->|OrderSagaCompletedEvent| G[Completed]
    D -->|OrderSagaFailedEvent| H[Failed]
    F -->|OrderSagaFailedEvent| H
    G --> I[Fim]
    H --> I
    
    style B fill:#e1f5ff
    style C fill:#fff4e1
    style E fill:#e8f5e9
    style D fill:#ffebee
    style F fill:#ffebee
    style G fill:#c8e6c9
    style H fill:#ffcdd2
```

## Comandos Publicados pela Saga

| Estado Atual | Evento Recebido | Comando Publicado | Próximo Estado |
|--------------|-----------------|-------------------|----------------|
| New | OrderCreatedEvent | ReserveStockCommand | OrderCreated |
| OrderCreated | OrderStockReservedEvent | CompletePaymentCommand | StockReserved |
| OrderCreated | OrderStockReservationFailedEvent | MarkOrderFailedCommand | StockReservationFailed |
| StockReserved | OrderPaymentCompletedEvent | MarkOrderCompletedCommand | PaymentCompleted |
| StockReserved | OrderPaymentFailedEvent | StockRollbackCommand<br/>MarkOrderFailedCommand | PaymentFailed |

## Eventos Emitidos pela Saga

| Evento da Saga | Quando é Emitido | Dados |
|----------------|-----------------|-------|
| OrderSagaStartedEvent | Quando OrderCreatedEvent é recebido | OrderId, CustomerId, PaymentAccountId, OrderItems, TotalPrice, CreatedDate |
| OrderSagaStockReservedEvent | Quando OrderStockReservedEvent é recebido | ReservedDate |
| OrderSagaStockReservationFailedEvent | Quando OrderStockReservationFailedEvent é recebido | ErrorMessage, FailedDate |
| OrderSagaPaymentCompletedEvent | Quando OrderPaymentCompletedEvent é recebido | CompletedDate |
| OrderSagaPaymentFailedEvent | Quando OrderPaymentFailedEvent é recebido | ErrorMessage, FailedDate |
| OrderSagaCompletedEvent | Após publicar MarkOrderCompletedCommand | CompletedDate |
| OrderSagaFailedEvent | Após publicar MarkOrderFailedCommand | ErrorMessage, FailedDate |

