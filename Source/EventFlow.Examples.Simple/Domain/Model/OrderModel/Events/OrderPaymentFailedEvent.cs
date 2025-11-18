using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderPaymentFailed", 1)]
    public class OrderPaymentFailedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderPaymentFailedEvent(string errorMessage, DateTime failedDate)
        {
            ErrorMessage = errorMessage;
            FailedDate = failedDate;
        }

        public string ErrorMessage { get; }
        public DateTime FailedDate { get; }
    }
}

