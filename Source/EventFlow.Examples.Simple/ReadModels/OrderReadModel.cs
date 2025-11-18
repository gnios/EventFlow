using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.OrderModel;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;
using EventFlow.ReadStores;

namespace EventFlow.Examples.Simple.ReadModels
{
    public class OrderReadModel : IReadModel,
        IAmReadModelFor<OrderAggregate, OrderId, OrderCreatedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderStockReservedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderStockReservationFailedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderPaymentCompletedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderPaymentFailedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderCompletedEvent>,
        IAmReadModelFor<OrderAggregate, OrderId, OrderFailedEvent>
    {
        public string Id { get; private set; } = string.Empty;
        public string CustomerId { get; private set; } = string.Empty;
        public string PaymentAccountId { get; private set; } = string.Empty;
        public decimal TotalPrice { get; private set; }
        public List<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        public OrderStatus Status { get; private set; } = OrderStatus.None;
        public DateTime CreatedDate { get; private set; }
        public string? ErrorMessage { get; private set; }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderCreatedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Id = domainEvent.AggregateIdentity.Value;
            CustomerId = domainEvent.AggregateEvent.CustomerId;
            PaymentAccountId = domainEvent.AggregateEvent.PaymentAccountId;
            TotalPrice = domainEvent.AggregateEvent.TotalPrice;
            OrderItems = domainEvent.AggregateEvent.OrderItems.ToList();
            Status = OrderStatus.Created;
            CreatedDate = domainEvent.AggregateEvent.CreatedDate;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.StockReserved;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderStockReservationFailedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.StockReservationFailed;
            ErrorMessage = domainEvent.AggregateEvent.ErrorMessage;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentCompletedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.PaymentCompleted;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderPaymentFailedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.PaymentFailed;
            ErrorMessage = domainEvent.AggregateEvent.ErrorMessage;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderCompletedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.Completed;
            return Task.CompletedTask;
        }

        public Task ApplyAsync(
            IReadModelContext context,
            IDomainEvent<OrderAggregate, OrderId, OrderFailedEvent> domainEvent,
            CancellationToken cancellationToken)
        {
            Status = OrderStatus.Failed;
            ErrorMessage = domainEvent.AggregateEvent.ErrorMessage;
            return Task.CompletedTask;
        }
    }
}

