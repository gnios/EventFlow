using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class MarkBookingConfirmedCommand : Command<BookingAggregate, BookingId>
    {
        public string ConfirmationNumber { get; }

        public MarkBookingConfirmedCommand(BookingId aggregateId, string confirmationNumber)
            : base(aggregateId)
        {
            ConfirmationNumber = confirmationNumber;
        }
    }

    public class MarkBookingConfirmedCommandHandler : CommandHandler<BookingAggregate, BookingId, MarkBookingConfirmedCommand>
    {
        private readonly ILogger<MarkBookingConfirmedCommandHandler> _logger;

        public MarkBookingConfirmedCommandHandler(ILogger<MarkBookingConfirmedCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            MarkBookingConfirmedCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] MarkBookingConfirmedCommand - Executando | BookingId: {BookingId} | ConfirmationNumber: {ConfirmationNumber}",
                command.AggregateId.Value, command.ConfirmationNumber);

            // Marca a reserva como confirmada
            aggregate.MarkConfirmed(command.ConfirmationNumber);

            _logger.LogInformation(
                "[COMMAND] MarkBookingConfirmedCommand - Concluído | BookingId: {BookingId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

