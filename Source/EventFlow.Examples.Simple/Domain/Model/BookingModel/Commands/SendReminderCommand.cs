using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class SendReminderCommand : Command<BookingAggregate, BookingId>
    {
        public string CustomerId { get; }
        public DateTime CheckInDate { get; }

        public SendReminderCommand(BookingId aggregateId, string customerId, DateTime checkInDate)
            : base(aggregateId)
        {
            CustomerId = customerId;
            CheckInDate = checkInDate;
        }
    }

    public class SendReminderCommandHandler : CommandHandler<BookingAggregate, BookingId, SendReminderCommand>
    {
        private readonly ILogger<SendReminderCommandHandler> _logger;

        public SendReminderCommandHandler(ILogger<SendReminderCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            SendReminderCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] SendReminderCommand - Executando | BookingId: {BookingId} | CustomerId: {CustomerId} | CheckInDate: {CheckInDate}",
                command.AggregateId.Value, command.CustomerId, command.CheckInDate);

            // Simula envio de lembrete - em produção, isso chamaria um serviço de notificação/email
            // Por simplicidade, apenas registra a ação
            // Este comando não altera o estado do agregado, apenas envia uma notificação

            _logger.LogInformation(
                "[COMMAND] SendReminderCommand - Concluído | BookingId: {BookingId} | Lembrete enviado para cliente {CustomerId} sobre check-in em {CheckInDate}",
                command.AggregateId.Value, command.CustomerId, command.CheckInDate);

            return Task.CompletedTask;
        }
    }
}

