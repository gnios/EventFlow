using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    /// <summary>
    /// Comando para reservar estoque de um pedido.
    /// Este comando é publicado pela saga quando um pedido é criado.
    /// </summary>
    public class ReserveStockCommand : Command<OrderAggregate, OrderId>
    {
        public ReserveStockCommand(OrderId aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class ReserveStockCommandHandler : CommandHandler<OrderAggregate, OrderId, ReserveStockCommand>
    {
        private readonly ILogger<ReserveStockCommandHandler> _logger;

        public ReserveStockCommandHandler(ILogger<ReserveStockCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            ReserveStockCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] ReserveStockCommand - Executando | OrderId: {OrderId}",
                command.AggregateId.Value);

            // Simula reserva de estoque - em produção, isso chamaria um serviço externo
            // Por simplicidade, sempre reserva com sucesso
            // Em um cenário real, poderia falhar baseado em regras de negócio
            aggregate.MarkStockReserved();

            _logger.LogInformation(
                "[COMMAND] ReserveStockCommand - Concluído | OrderId: {OrderId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

