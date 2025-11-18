using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.DeclarativeSaga.StateMachine;
using EventFlow.DeclarativeSaga.StateMachine.Builders;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events;
using EventFlow.Jobs;
using EventFlow.Sagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas
{
    /// <summary>
    /// Exemplo de saga usando a API declarativa simplificada.
    /// Esta saga usa apenas a API declarativa, mantendo o padrão padrão do EventFlow para gestão de estado.
    /// </summary>
    public class OrderDeclarativeSaga : DeclarativeSaga<OrderDeclarativeSaga, OrderSagaId, OrderSagaLocator>,
        ISagaIsStartedBy<OrderAggregate, OrderId, OrderCreatedEvent>,
        ISagaHandles<OrderAggregate, OrderId, OrderStockReservedEvent>,
        ISagaHandles<OrderAggregate, OrderId, OrderStockReservationFailedEvent>,
        ISagaHandles<OrderAggregate, OrderId, OrderPaymentCompletedEvent>,
        ISagaHandles<OrderAggregate, OrderId, OrderPaymentFailedEvent>
    {
        private readonly ILogger<OrderDeclarativeSaga>? _logger;

        private readonly OrderDeclarativeSagaState _sagaState;

        // O EventFlow requer apenas um construtor por saga.
        // Usamos IServiceProvider para obter todas as dependências necessárias.
        public OrderDeclarativeSaga(OrderSagaId id, IServiceProvider serviceProvider)
            : base(id, serviceProvider)
        {
            // Obtém o logger através do service provider
            _logger = serviceProvider?.GetService<ILogger<OrderDeclarativeSaga>>();

            // Registra o estado agregado e habilita gerenciamento automático de compensation job ID
            _sagaState = RegisterState(new OrderDeclarativeSagaState());

            Define(builder => { ConfigureSaga(builder); });
        }

        private void ConfigureSaga(DeclarativeSagaBuilder<OrderDeclarativeSaga, OrderSagaId, OrderSagaLocator> builder)
        {
            builder.Initially()
                .When<OrderAggregate, OrderId, OrderCreatedEvent>()
                .ThenEmitSagaEvent(CreateSagaStartedEvent)
                .AndPublish<OrderAggregate, OrderId>(PublishReserveStockCommand);

            builder.When<OrderAggregate, OrderId, OrderStockReservedEvent>(timeoutMinutes: 10)
                .ThenPublish<OrderAggregate, OrderId>(PublishCompletePaymentCommand)
                .AndCompensateWith<OrderAggregate, OrderId>(CreateStockRollbackCommand);

            builder.When<OrderAggregate, OrderId, OrderPaymentCompletedEvent>()
                .ThenPublish<OrderAggregate, OrderId>(PublishMarkOrderCompletedCommand)
                .AndEmitSagaEvent(CreateSagaCompletedEvent)
                .AndComplete();

            builder.When<OrderAggregate, OrderId, OrderStockReservationFailedEvent>()
                .ThenPublish<OrderAggregate, OrderId>(PublishMarkOrderFailedForStockReservation)
                .AndEmitSagaEvent(CreateSagaFailedEventForStockReservation)
                .AndComplete();

            builder.When<OrderAggregate, OrderId, OrderPaymentFailedEvent>()
                .ThenPublishMany<OrderAggregate, OrderId>(PublishPaymentFailedCommands)
                .AndEmitSagaEvent(CreateSagaFailedEventForPayment)
                .AndComplete();
        }

        // Métodos auxiliares para criação de eventos da saga
        private OrderDeclarativeSagaStartedEvent CreateSagaStartedEvent(
            IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            return new OrderDeclarativeSagaStartedEvent(
                evt.AggregateIdentity,
                evt.AggregateEvent.CustomerId,
                evt.AggregateEvent.PaymentAccountId,
                DateTime.UtcNow);
        }

        private OrderDeclarativeSagaCompletedEvent CreateSagaCompletedEvent(
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            return new OrderDeclarativeSagaCompletedEvent(evt.AggregateIdentity, DateTime.UtcNow);
        }

        private OrderDeclarativeSagaFailedEvent CreateSagaFailedEventForStockReservation(
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservationFailedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            return new OrderDeclarativeSagaFailedEvent(evt.AggregateIdentity, "Stock reservation failed", DateTime.UtcNow);
        }

        private OrderDeclarativeSagaFailedEvent CreateSagaFailedEventForPayment(
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentFailedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            return new OrderDeclarativeSagaFailedEvent(evt.AggregateIdentity, "Payment failed", DateTime.UtcNow);
        }

        // Métodos auxiliares para publicação de comandos
        private ReserveStockCommand PublishReserveStockCommand(
            IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            saga.LogEvent("OrderCreatedEvent", evt.AggregateIdentity);
            saga.LogCommand("ReserveStockCommand", evt.AggregateIdentity);
            return new ReserveStockCommand(evt.AggregateIdentity);
        }

        private CompletePaymentCommand PublishCompletePaymentCommand(
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            saga.LogEvent("OrderStockReservedEvent", evt.AggregateIdentity);
            saga.LogCommand("CompletePaymentCommand", evt.AggregateIdentity);
            return new CompletePaymentCommand(evt.AggregateIdentity);
        }

        private MarkOrderCompletedCommand PublishMarkOrderCompletedCommand(
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            saga.LogEvent("OrderPaymentCompletedEvent", evt.AggregateIdentity);
            saga.LogCommand("MarkOrderCompletedCommand", evt.AggregateIdentity);
            return new MarkOrderCompletedCommand(evt.AggregateIdentity);
        }

        private MarkOrderFailedCommand PublishMarkOrderFailedForStockReservation(
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservationFailedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            saga.LogEvent("OrderStockReservationFailedEvent", evt.AggregateIdentity);
            saga.LogCommand("MarkOrderFailedCommand", evt.AggregateIdentity);
            return new MarkOrderFailedCommand(evt.AggregateIdentity, "Stock reservation failed");
        }

        private StockRollbackCommand CreateStockRollbackCommand(
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            return new StockRollbackCommand(evt.AggregateIdentity);
        }

        private IEnumerable<ICommand<OrderAggregate, OrderId, IExecutionResult>> PublishPaymentFailedCommands(
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentFailedEvent> evt,
            OrderDeclarativeSaga saga)
        {
            saga.LogEvent("OrderPaymentFailedEvent", evt.AggregateIdentity);
            return new ICommand<OrderAggregate, OrderId, IExecutionResult>[]
            {
                new StockRollbackCommand(evt.AggregateIdentity),
                new MarkOrderFailedCommand(evt.AggregateIdentity, "Payment failed")
            };
        }

        // Implementação dos handlers das interfaces - todos delegam para HandleEventAsync
        public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderStockReservationFailedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        public Task HandleAsync(IDomainEvent<OrderAggregate, OrderId, OrderPaymentFailedEvent> domainEvent, ISagaContext sagaContext, CancellationToken cancellationToken)
            => HandleEventAsync(domainEvent, sagaContext, cancellationToken);

        // Helpers para melhorar legibilidade
        private void LogEvent(string eventName, OrderId orderId) =>
            _logger?.LogInformation("[SAGA] OrderDeclarativeSaga - Evento recebido: {EventName} | OrderId: {OrderId} | SagaId: {SagaId}",
                eventName, orderId.Value, Id.Value);

        private void LogCommand(string commandName, OrderId orderId) =>
            _logger?.LogInformation("[SAGA] OrderDeclarativeSaga - Publicando comando: {CommandName} | OrderId: {OrderId} | SagaId: {SagaId}",
                commandName, orderId.Value, Id.Value);
    }
}