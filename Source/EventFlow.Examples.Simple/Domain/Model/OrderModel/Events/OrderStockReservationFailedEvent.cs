using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderStockReservationFailed", 1)]
    public class OrderStockReservationFailedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderStockReservationFailedEvent(string errorMessage, DateTime failedDate)
        {
            ErrorMessage = errorMessage;
            FailedDate = failedDate;
        }

        public string ErrorMessage { get; }
        public DateTime FailedDate { get; }
    }
}

