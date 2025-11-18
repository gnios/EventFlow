using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    public class MarkOrderCompletedCommand : Command<OrderAggregate, OrderId>
    {
        public MarkOrderCompletedCommand(OrderId aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class MarkOrderCompletedCommandHandler : CommandHandler<OrderAggregate, OrderId, MarkOrderCompletedCommand>
    {
        private readonly ILogger<MarkOrderCompletedCommandHandler> _logger;

        public MarkOrderCompletedCommandHandler(ILogger<MarkOrderCompletedCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            MarkOrderCompletedCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] MarkOrderCompletedCommand - Executando | OrderId: {OrderId}",
                command.AggregateId.Value);

            aggregate.MarkCompleted();

            _logger.LogInformation(
                "[COMMAND] MarkOrderCompletedCommand - Concluído | OrderId: {OrderId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

