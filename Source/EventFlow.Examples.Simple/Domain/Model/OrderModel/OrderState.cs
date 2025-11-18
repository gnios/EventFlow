using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel
{
    public class OrderState : AggregateState<OrderAggregate, OrderId, OrderState>,
        IApply<OrderCreatedEvent>,
        IApply<OrderStockReservedEvent>,
        IApply<OrderStockReservationFailedEvent>,
        IApply<OrderPaymentCompletedEvent>,
        IApply<OrderPaymentFailedEvent>,
        IApply<OrderCompletedEvent>,
        IApply<OrderFailedEvent>
    {
        public string CustomerId { get; private set; } = string.Empty;
        public string PaymentAccountId { get; private set; } = string.Empty;
        public decimal TotalPrice { get; private set; }
        public List<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        public OrderStatus Status { get; private set; } = OrderStatus.None;
        public DateTime CreatedDate { get; private set; }
        public string? ErrorMessage { get; private set; }

        public void Apply(OrderCreatedEvent aggregateEvent)
        {
            CustomerId = aggregateEvent.CustomerId;
            PaymentAccountId = aggregateEvent.PaymentAccountId;
            TotalPrice = aggregateEvent.TotalPrice;
            OrderItems = aggregateEvent.OrderItems.ToList();
            Status = OrderStatus.Created;
            CreatedDate = aggregateEvent.CreatedDate;
        }

        public void Apply(OrderStockReservedEvent aggregateEvent)
        {
            Status = OrderStatus.StockReserved;
        }

        public void Apply(OrderStockReservationFailedEvent aggregateEvent)
        {
            Status = OrderStatus.StockReservationFailed;
            ErrorMessage = aggregateEvent.ErrorMessage;
        }

        public void Apply(OrderPaymentCompletedEvent aggregateEvent)
        {
            Status = OrderStatus.PaymentCompleted;
        }

        public void Apply(OrderPaymentFailedEvent aggregateEvent)
        {
            Status = OrderStatus.PaymentFailed;
            ErrorMessage = aggregateEvent.ErrorMessage;
        }

        public void Apply(OrderCompletedEvent aggregateEvent)
        {
            Status = OrderStatus.Completed;
        }

        public void Apply(OrderFailedEvent aggregateEvent)
        {
            Status = OrderStatus.Failed;
            ErrorMessage = aggregateEvent.ErrorMessage;
        }
    }

    public enum OrderStatus
    {
        None,
        Created,
        StockReserved,
        StockReservationFailed,
        PaymentCompleted,
        PaymentFailed,
        Completed,
        Failed
    }
}

