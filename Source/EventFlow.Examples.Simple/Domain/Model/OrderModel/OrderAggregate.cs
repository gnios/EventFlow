using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.Events;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;
using EventFlow.Exceptions;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel
{
    public class OrderAggregate : AggregateRoot<OrderAggregate, OrderId>
    {
        private readonly OrderState _state = new OrderState();

        public OrderAggregate(OrderId id) : base(id)
        {
            Register(_state);
        }

        public string CustomerId => _state.CustomerId;
        public string PaymentAccountId => _state.PaymentAccountId;
        public decimal TotalPrice => _state.TotalPrice;
        public IReadOnlyList<OrderItem> OrderItems => _state.OrderItems;
        public OrderStatus Status => _state.Status;
        public DateTime CreatedDate => _state.CreatedDate;

        public void Create(
            string customerId,
            string paymentAccountId,
            List<OrderItem> orderItems)
        {
            if (!IsNew)
                throw DomainError.With("Pedido já foi criado");

            if (string.IsNullOrWhiteSpace(customerId))
                throw DomainError.With("CustomerId não pode ser vazio");

            if (string.IsNullOrWhiteSpace(paymentAccountId))
                throw DomainError.With("PaymentAccountId não pode ser vazio");

            if (orderItems == null || orderItems.Count == 0)
                throw DomainError.With("Pedido deve conter pelo menos um item");

            var totalPrice = orderItems.Sum(item => item.TotalPrice);

            Emit(new OrderCreatedEvent(
                customerId,
                paymentAccountId,
                orderItems,
                totalPrice,
                DateTime.UtcNow));
        }

        public void MarkStockReserved()
        {
            if (IsNew)
                throw DomainError.With("Pedido não foi criado ainda");

            if (Status != OrderStatus.Created)
                throw DomainError.With($"Não é possível reservar estoque. Status atual: {Status}");

            Emit(new OrderStockReservedEvent(DateTime.UtcNow));
        }

        public void MarkStockReservationFailed(string errorMessage)
        {
            if (IsNew)
                throw DomainError.With("Pedido não foi criado ainda");

            if (Status != OrderStatus.Created)
                throw DomainError.With($"Não é possível falhar reserva de estoque. Status atual: {Status}");

            Emit(new OrderStockReservationFailedEvent(errorMessage, DateTime.UtcNow));
        }

        public void MarkPaymentCompleted()
        {
            if (IsNew)
                throw DomainError.With("Pedido não foi criado ainda");

            if (Status != OrderStatus.StockReserved)
                throw DomainError.With($"Não é possível completar pagamento. Status atual: {Status}");

            Emit(new OrderPaymentCompletedEvent(DateTime.UtcNow));
        }

        public void MarkPaymentFailed(string errorMessage)
        {
            if (IsNew)
                throw DomainError.With("Pedido não foi criado ainda");

            if (Status != OrderStatus.StockReserved)
                throw DomainError.With($"Não é possível falhar pagamento. Status atual: {Status}");

            Emit(new OrderPaymentFailedEvent(errorMessage, DateTime.UtcNow));
        }

        public void MarkCompleted()
        {
            if (Status != OrderStatus.PaymentCompleted)
                throw DomainError.With($"Não é possível completar pedido. Status atual: {Status}");

            Emit(new OrderCompletedEvent(DateTime.UtcNow));
        }

        public void MarkFailed(string errorMessage)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Failed)
                throw DomainError.With($"Pedido já está finalizado. Status atual: {Status}");

            Emit(new OrderFailedEvent(errorMessage, DateTime.UtcNow));
        }
    }
}

