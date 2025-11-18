using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    /// <summary>
    /// Comando para fazer rollback da reserva de estoque.
    /// Este comando é publicado pela saga quando o pagamento falha.
    /// </summary>
    public class StockRollbackCommand : Command<OrderAggregate, OrderId>
    {
        public StockRollbackCommand(OrderId aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class StockRollbackCommandHandler : CommandHandler<OrderAggregate, OrderId, StockRollbackCommand>
    {
        private readonly ILogger<StockRollbackCommandHandler> _logger;

        public StockRollbackCommandHandler(ILogger<StockRollbackCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            StockRollbackCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] StockRollbackCommand - Executando | OrderId: {OrderId}",
                command.AggregateId.Value);

            // Simula rollback de estoque - em produção, isso chamaria um serviço externo
            // Por simplicidade, apenas loga a ação

            _logger.LogInformation(
                "[COMMAND] StockRollbackCommand - Concluído | OrderId: {OrderId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

