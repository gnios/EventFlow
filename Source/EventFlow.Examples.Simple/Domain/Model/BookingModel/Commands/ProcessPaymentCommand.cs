using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class ProcessPaymentCommand : Command<BookingAggregate, BookingId>
    {
        public decimal Amount { get; }
        public string PaymentMethod { get; }

        public ProcessPaymentCommand(BookingId aggregateId, decimal amount, string paymentMethod)
            : base(aggregateId)
        {
            Amount = amount;
            PaymentMethod = paymentMethod;
        }
    }

    public class ProcessPaymentCommandHandler : CommandHandler<BookingAggregate, BookingId, ProcessPaymentCommand>
    {
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(ILogger<ProcessPaymentCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            ProcessPaymentCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] ProcessPaymentCommand - Executando | BookingId: {BookingId} | Amount: {Amount} | PaymentMethod: {PaymentMethod}",
                command.AggregateId.Value, command.Amount, command.PaymentMethod);

            // Simula processamento de pagamento - em produção, isso chamaria um serviço de pagamento
            // Por simplicidade, sempre processa com sucesso
            // Em um cenário real, poderia falhar baseado em regras de negócio
            var transactionId = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

            aggregate.ProcessPayment(transactionId, command.Amount);

            _logger.LogInformation(
                "[COMMAND] ProcessPaymentCommand - Concluído | BookingId: {BookingId} | TransactionId: {TransactionId}",
                command.AggregateId.Value, transactionId);

            return Task.CompletedTask;
        }
    }
}

