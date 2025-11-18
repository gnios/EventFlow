using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderCompleted", 1)]
    public class OrderCompletedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderCompletedEvent(DateTime completedDate)
        {
            CompletedDate = completedDate;
        }

        public DateTime CompletedDate { get; }
    }
}

