using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    public class MarkOrderFailedCommand : Command<OrderAggregate, OrderId>
    {
        public MarkOrderFailedCommand(OrderId aggregateId, string errorMessage)
            : base(aggregateId)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }

    public class MarkOrderFailedCommandHandler : CommandHandler<OrderAggregate, OrderId, MarkOrderFailedCommand>
    {
        private readonly ILogger<MarkOrderFailedCommandHandler> _logger;

        public MarkOrderFailedCommandHandler(ILogger<MarkOrderFailedCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            MarkOrderFailedCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "[COMMAND] MarkOrderFailedCommand - Executando | OrderId: {OrderId} | Erro: {ErrorMessage}",
                command.AggregateId.Value, command.ErrorMessage);

            aggregate.MarkFailed(command.ErrorMessage);

            _logger.LogWarning(
                "[COMMAND] MarkOrderFailedCommand - Concluído | OrderId: {OrderId} | Erro: {ErrorMessage}",
                command.AggregateId.Value, command.ErrorMessage);

            return Task.CompletedTask;
        }
    }
}

