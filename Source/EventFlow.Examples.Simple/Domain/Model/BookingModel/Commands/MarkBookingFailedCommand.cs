using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class MarkBookingFailedCommand : Command<BookingAggregate, BookingId>
    {
        public string Reason { get; }

        public MarkBookingFailedCommand(BookingId aggregateId, string reason)
            : base(aggregateId)
        {
            Reason = reason;
        }
    }

    public class MarkBookingFailedCommandHandler : CommandHandler<BookingAggregate, BookingId, MarkBookingFailedCommand>
    {
        private readonly ILogger<MarkBookingFailedCommandHandler> _logger;

        public MarkBookingFailedCommandHandler(ILogger<MarkBookingFailedCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            MarkBookingFailedCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] MarkBookingFailedCommand - Executando | BookingId: {BookingId} | Reason: {Reason}",
                command.AggregateId.Value, command.Reason);

            // Marca a reserva como falhada
            aggregate.MarkFailed(command.Reason);

            _logger.LogInformation(
                "[COMMAND] MarkBookingFailedCommand - Concluído | BookingId: {BookingId}",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

