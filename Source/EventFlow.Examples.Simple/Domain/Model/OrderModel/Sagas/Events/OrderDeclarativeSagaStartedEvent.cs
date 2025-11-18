using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas.Events
{
    [EventVersion("OrderDeclarativeSagaStarted", 1)]
    public class OrderDeclarativeSagaStartedEvent : AggregateEvent<OrderDeclarativeSaga, OrderSagaId>
    {
        public OrderId OrderId { get; }
        public string CustomerId { get; }
        public string PaymentAccountId { get; }
        public DateTime StartedDate { get; }

        public OrderDeclarativeSagaStartedEvent(OrderId orderId, string customerId, string paymentAccountId, DateTime startedDate)
        {
            OrderId = orderId;
            CustomerId = customerId;
            PaymentAccountId = paymentAccountId;
            StartedDate = startedDate;
        }
    }
}

