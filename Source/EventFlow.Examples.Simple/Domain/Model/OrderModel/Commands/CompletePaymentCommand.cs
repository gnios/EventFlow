using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Commands
{
    /// <summary>
    /// Comando para processar pagamento de um pedido.
    /// Este comando é publicado pela saga quando o estoque é reservado.
    /// </summary>
    public class CompletePaymentCommand : Command<OrderAggregate, OrderId>
    {
        public CompletePaymentCommand(OrderId aggregateId)
            : base(aggregateId)
        {
        }
    }

    public class CompletePaymentCommandHandler : CommandHandler<OrderAggregate, OrderId, CompletePaymentCommand>
    {
        private readonly ILogger<CompletePaymentCommandHandler> _logger;

        public CompletePaymentCommandHandler(ILogger<CompletePaymentCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            OrderAggregate aggregate,
            CompletePaymentCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] CompletePaymentCommand - Executando | OrderId: {OrderId}",
                command.AggregateId.Value);

            // Simula processamento de pagamento - em produção, isso chamaria um serviço de pagamento
            // Por simplicidade, sempre processa com sucesso
            // Em um cenário real, poderia falhar baseado em regras de negócio
            aggregate.MarkPaymentCompleted();

            _logger.LogInformation(
                "[COMMAND] CompletePaymentCommand - Concluído | OrderId: {OrderId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

