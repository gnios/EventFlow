using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderFailed", 1)]
    public class OrderFailedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderFailedEvent(string errorMessage, DateTime failedDate)
        {
            ErrorMessage = errorMessage;
            FailedDate = failedDate;
        }

        public string ErrorMessage { get; }
        public DateTime FailedDate { get; }
    }
}

