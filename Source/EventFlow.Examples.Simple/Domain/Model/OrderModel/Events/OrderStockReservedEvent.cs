using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderStockReserved", 1)]
    public class OrderStockReservedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderStockReservedEvent(DateTime reservedDate)
        {
            ReservedDate = reservedDate;
        }

        public DateTime ReservedDate { get; }
    }
}

