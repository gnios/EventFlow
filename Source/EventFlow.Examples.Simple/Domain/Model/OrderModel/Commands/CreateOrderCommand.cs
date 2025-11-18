using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    public class CreateOrderCommand : Command<OrderAggregate, OrderId>
    {
        public CreateOrderCommand(
            OrderId aggregateId,
            string customerId,
            string paymentAccountId,
            List<OrderItem> orderItems)
            : base(aggregateId)
        {
            CustomerId = customerId;
            PaymentAccountId = paymentAccountId;
            OrderItems = orderItems;
        }

        public string CustomerId { get; }
        public string PaymentAccountId { get; }
        public List<OrderItem> OrderItems { get; }
    }

    public class CreateOrderCommandHandler : CommandHandler<OrderAggregate, OrderId, CreateOrderCommand>
    {
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            CreateOrderCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] CreateOrderCommand - Executando | OrderId: {OrderId} | CustomerId: {CustomerId} | Items: {ItemCount}",
                command.AggregateId.Value, command.CustomerId, command.OrderItems.Count);

            aggregate.Create(
                command.CustomerId,
                command.PaymentAccountId,
                command.OrderItems);

            _logger.LogInformation(
                "[COMMAND] CreateOrderCommand - Concluído | OrderId: {OrderId} | Evento: OrderCreatedEvent",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

