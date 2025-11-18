using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderPaymentCompleted", 1)]
    public class OrderPaymentCompletedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderPaymentCompletedEvent(DateTime completedDate)
        {
            CompletedDate = completedDate;
        }

        public DateTime CompletedDate { get; }
    }
}

