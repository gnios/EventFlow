# OrderSaga - Diagrama Completo de Fluxo

## Diagrama Principal - Máquina de Estados

```mermaid
flowchart TD
    Start([Início]) --> OrderCreated[OrderCreated<br/>Estado: OrderCreated]
    
    OrderCreated -->|OrderCreatedEvent<br/>ISagaIsStartedBy| EmitStarted[Emit OrderSagaStartedEvent]
    EmitStarted --> PublishReserveStock[Publish ReserveStockCommand]
    
    PublishReserveStock --> WaitStock{Aguardando<br/>Resposta}
    
    WaitStock -->|OrderStockReservedEvent| StockReserved[StockReserved<br/>Estado: StockReserved]
    WaitStock -->|OrderStockReservationFailedEvent| StockFailed[StockReservationFailed<br/>Estado: StockReservationFailed]
    
    StockReserved --> EmitStockReserved[Emit OrderSagaStockReservedEvent]
    EmitStockReserved --> PublishPayment[Publish CompletePaymentCommand]
    PublishPayment --> WaitPayment{Aguardando<br/>Pagamento}
    
    WaitPayment -->|OrderPaymentCompletedEvent| PaymentCompleted[PaymentCompleted<br/>Estado: PaymentCompleted]
    WaitPayment -->|OrderPaymentFailedEvent| PaymentFailed[PaymentFailed<br/>Estado: PaymentFailed]
    
    PaymentCompleted --> EmitPaymentCompleted[Emit OrderSagaPaymentCompletedEvent]
    EmitPaymentCompleted --> PublishMarkCompleted[Publish MarkOrderCompletedCommand]
    PublishMarkCompleted --> EmitSagaCompleted[Emit OrderSagaCompletedEvent]
    EmitSagaCompleted --> Complete[Complete Saga]
    Complete --> EndSuccess([Saga Finalizada<br/>SUCESSO])
    
    StockFailed --> EmitStockFailed[Emit OrderSagaStockReservationFailedEvent]
    EmitStockFailed --> PublishMarkFailed1[Publish MarkOrderFailedCommand]
    PublishMarkFailed1 --> EmitSagaFailed1[Emit OrderSagaFailedEvent]
    EmitSagaFailed1 --> Complete1[Complete Saga]
    Complete1 --> EndFail1([Saga Finalizada<br/>FALHA])
    
    PaymentFailed --> EmitPaymentFailed[Emit OrderSagaPaymentFailedEvent]
    EmitPaymentFailed --> PublishRollback[Publish StockRollbackCommand]
    PublishRollback --> PublishMarkFailed2[Publish MarkOrderFailedCommand]
    PublishMarkFailed2 --> EmitSagaFailed2[Emit OrderSagaFailedEvent]
    EmitSagaFailed2 --> Complete2[Complete Saga]
    Complete2 --> EndFail2([Saga Finalizada<br/>FALHA com Rollback])
    
    style OrderCreated fill:#e3f2fd
    style StockReserved fill:#fff3e0
    style PaymentCompleted fill:#e8f5e9
    style StockFailed fill:#ffebee
    style PaymentFailed fill:#ffebee
    style EndSuccess fill:#c8e6c9
    style EndFail1 fill:#ffcdd2
    style EndFail2 fill:#ffcdd2
```

## Diagrama de Sequência - Fluxo Completo (Happy Path)

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant CommandBus
    participant OrderAggregate
    participant EventStore
    participant OrderSaga
    participant OrderSagaState
    
    Client->>CommandBus: CreateOrderCommand
    CommandBus->>OrderAggregate: Create()
    OrderAggregate->>OrderAggregate: Emit(OrderCreatedEvent)
    OrderAggregate->>EventStore: Save Event
    
    EventStore->>OrderSaga: OrderCreatedEvent<br/>(ISagaIsStartedBy)
    OrderSaga->>OrderSaga: Emit(OrderSagaStartedEvent)
    OrderSaga->>OrderSagaState: Apply(OrderSagaStartedEvent)
    OrderSagaState-->>OrderSaga: Status = OrderCreated
    OrderSaga->>CommandBus: Publish(ReserveStockCommand)
    
    CommandBus->>OrderAggregate: ReserveStock()
    OrderAggregate->>OrderAggregate: Emit(OrderStockReservedEvent)
    OrderAggregate->>EventStore: Save Event
    
    EventStore->>OrderSaga: OrderStockReservedEvent<br/>(ISagaHandles)
    OrderSaga->>OrderSaga: Emit(OrderSagaStockReservedEvent)
    OrderSaga->>OrderSagaState: Apply(OrderSagaStockReservedEvent)
    OrderSagaState-->>OrderSaga: Status = StockReserved
    OrderSaga->>CommandBus: Publish(CompletePaymentCommand)
    
    CommandBus->>OrderAggregate: CompletePayment()
    OrderAggregate->>OrderAggregate: Emit(OrderPaymentCompletedEvent)
    OrderAggregate->>EventStore: Save Event
    
    EventStore->>OrderSaga: OrderPaymentCompletedEvent<br/>(ISagaHandles)
    OrderSaga->>OrderSaga: Emit(OrderSagaPaymentCompletedEvent)
    OrderSaga->>OrderSagaState: Apply(OrderSagaPaymentCompletedEvent)
    OrderSagaState-->>OrderSaga: Status = PaymentCompleted
    OrderSaga->>CommandBus: Publish(MarkOrderCompletedCommand)
    
    CommandBus->>OrderAggregate: MarkCompleted()
    OrderAggregate->>OrderAggregate: Emit(OrderCompletedEvent)
    OrderAggregate->>EventStore: Save Event
    
    OrderSaga->>OrderSaga: Emit(OrderSagaCompletedEvent)
    OrderSaga->>OrderSagaState: Apply(OrderSagaCompletedEvent)
    OrderSagaState-->>OrderSaga: Status = Completed
    OrderSaga->>OrderSaga: Complete()
```

## Diagrama de Estados - Transições

```mermaid
stateDiagram-v2
    [*] --> New: Saga Criada
    
    New --> OrderCreated: OrderCreatedEvent<br/>Emit: OrderSagaStartedEvent<br/>Publish: ReserveStockCommand
    
    state OrderCreated {
        [*] --> WaitingStock
        WaitingStock --> [*]
    }
    
    OrderCreated --> StockReserved: OrderStockReservedEvent<br/>Emit: OrderSagaStockReservedEvent<br/>Publish: CompletePaymentCommand
    OrderCreated --> StockReservationFailed: OrderStockReservationFailedEvent<br/>Emit: OrderSagaStockReservationFailedEvent<br/>Publish: MarkOrderFailedCommand<br/>Emit: OrderSagaFailedEvent<br/>Complete()
    
    state StockReserved {
        [*] --> WaitingPayment
        WaitingPayment --> [*]
    }
    
    StockReserved --> PaymentCompleted: OrderPaymentCompletedEvent<br/>Emit: OrderSagaPaymentCompletedEvent<br/>Publish: MarkOrderCompletedCommand<br/>Emit: OrderSagaCompletedEvent<br/>Complete()
    StockReserved --> PaymentFailed: OrderPaymentFailedEvent<br/>Emit: OrderSagaPaymentFailedEvent<br/>Publish: StockRollbackCommand<br/>Publish: MarkOrderFailedCommand<br/>Emit: OrderSagaFailedEvent<br/>Complete()
    
    PaymentCompleted --> [*]: Saga Finalizada
    StockReservationFailed --> [*]: Saga Finalizada
    PaymentFailed --> [*]: Saga Finalizada
```

## Tabela de Transições e Ações

| Estado Atual | Evento Recebido | Interface | Ação 1 | Ação 2 | Ação 3 | Próximo Estado |
|--------------|----------------|-----------|--------|--------|--------|----------------|
| New | OrderCreatedEvent | ISagaIsStartedBy | Emit OrderSagaStartedEvent | Publish ReserveStockCommand | - | OrderCreated |
| OrderCreated | OrderStockReservedEvent | ISagaHandles | Emit OrderSagaStockReservedEvent | Publish CompletePaymentCommand | - | StockReserved |
| OrderCreated | OrderStockReservationFailedEvent | ISagaHandles | Emit OrderSagaStockReservationFailedEvent | Publish MarkOrderFailedCommand | Emit OrderSagaFailedEvent + Complete() | StockReservationFailed |
| StockReserved | OrderPaymentCompletedEvent | ISagaHandles | Emit OrderSagaPaymentCompletedEvent | Publish MarkOrderCompletedCommand | Emit OrderSagaCompletedEvent + Complete() | PaymentCompleted |
| StockReserved | OrderPaymentFailedEvent | ISagaHandles | Emit OrderSagaPaymentFailedEvent | Publish StockRollbackCommand | Publish MarkOrderFailedCommand + Emit OrderSagaFailedEvent + Complete() | PaymentFailed |

## Validações de Estado

```mermaid
flowchart TD
    A[Evento Recebido] --> B{Estado da Saga}
    B -->|New| C{ISagaIsStartedBy?}
    B -->|Running| D{ISagaHandles?}
    B -->|Completed| E[Ignorar Evento]
    
    C -->|Sim| F[Processar]
    C -->|Não| G[Erro: Saga não iniciada]
    
    D -->|Sim| H{Estado Válido?}
    D -->|Não| I[Erro: Evento não tratado]
    
    H -->|Sim| J[Processar]
    H -->|Não| K[Erro: Transição inválida]
    
    F --> L[Executar HandleAsync]
    J --> L
    L --> M[Emitir Eventos da Saga]
    M --> N[Publicar Comandos]
    N --> O[Atualizar Estado]
    
    style C fill:#e3f2fd
    style D fill:#fff3e0
    style H fill:#e8f5e9
    style G fill:#ffebee
    style I fill:#ffebee
    style K fill:#ffebee
```

## Resumo da OrderSaga

### Interfaces Implementadas
- `ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>` - Inicia a saga
- `ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>` - Processa reserva de estoque
- `ISagaHandles<OrderAggregate, OrderId, OrderStockReservationFailedEvent>` - Processa falha na reserva
- `ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>` - Processa pagamento completo
- `ISagaHandles<OrderAggregate, OrderId, OrderPaymentFailedEvent>` - Processa falha no pagamento

### Eventos Emitidos pela Saga
1. **OrderSagaStartedEvent** - Saga iniciada
2. **OrderSagaStockReservedEvent** - Estoque reservado
3. **OrderSagaStockReservationFailedEvent** - Falha na reserva
4. **OrderSagaPaymentCompletedEvent** - Pagamento completo
5. **OrderSagaPaymentFailedEvent** - Falha no pagamento
6. **OrderSagaCompletedEvent** - Saga completada com sucesso
7. **OrderSagaFailedEvent** - Saga finalizada com falha

### Comandos Publicados pela Saga
1. **ReserveStockCommand** - Reservar estoque
2. **CompletePaymentCommand** - Processar pagamento
3. **StockRollbackCommand** - Rollback de estoque (compensação)
4. **MarkOrderCompletedCommand** - Marcar pedido como completo
5. **MarkOrderFailedCommand** - Marcar pedido como falho

