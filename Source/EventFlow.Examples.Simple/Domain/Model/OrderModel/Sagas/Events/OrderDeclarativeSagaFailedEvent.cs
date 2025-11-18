using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events
{
    [EventVersion("OrderDeclarativeSagaFailed", 1)]
    public class OrderDeclarativeSagaFailedEvent : AggregateEvent<OrderDeclarativeSaga, OrderSagaId>
    {
        public OrderId OrderId { get; }
        public string ErrorMessage { get; }
        public DateTime FailedDate { get; }

        public OrderDeclarativeSagaFailedEvent(OrderId orderId, string errorMessage, DateTime failedDate)
        {
            OrderId = orderId;
            ErrorMessage = errorMessage;
            FailedDate = failedDate;
        }
    }
}

