using System;
using System.Collections.Generic;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Events
{
    [EventVersion("OrderCreated", 1)]
    public class OrderCreatedEvent : AggregateEvent<OrderAggregate, OrderId>
    {
        public OrderCreatedEvent(
            string customerId,
            string paymentAccountId,
            List<OrderItem> orderItems,
            decimal totalPrice,
            DateTime createdDate)
        {
            CustomerId = customerId;
            PaymentAccountId = paymentAccountId;
            OrderItems = orderItems;
            TotalPrice = totalPrice;
            CreatedDate = createdDate;
        }

        public string CustomerId { get; }
        public string PaymentAccountId { get; }
        public List<OrderItem> OrderItems { get; }
        public decimal TotalPrice { get; }
        public DateTime CreatedDate { get; }
    }
}

