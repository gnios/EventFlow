using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events
{
    [EventVersion("OrderDeclarativeSagaCompleted", 1)]
    public class OrderDeclarativeSagaCompletedEvent : AggregateEvent<OrderDeclarativeSaga, OrderSagaId>
    {
        public OrderId OrderId { get; }
        public DateTime CompletedDate { get; }

        public OrderDeclarativeSagaCompletedEvent(OrderId orderId, DateTime completedDate)
        {
            OrderId = orderId;
            CompletedDate = completedDate;
        }
    }
}

