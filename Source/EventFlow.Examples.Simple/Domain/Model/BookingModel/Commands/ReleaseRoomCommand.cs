using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class ReleaseRoomCommand : Command<BookingAggregate, BookingId>
    {
        public string RoomNumber { get; }

        public ReleaseRoomCommand(BookingId aggregateId, string roomNumber)
            : base(aggregateId)
        {
            RoomNumber = roomNumber;
        }
    }

    public class ReleaseRoomCommandHandler : CommandHandler<BookingAggregate, BookingId, ReleaseRoomCommand>
    {
        private readonly ILogger<ReleaseRoomCommandHandler> _logger;

        public ReleaseRoomCommandHandler(ILogger<ReleaseRoomCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            ReleaseRoomCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] ReleaseRoomCommand - Executando | BookingId: {BookingId} | RoomNumber: {RoomNumber}",
                command.AggregateId.Value, command.RoomNumber);

            // Simula liberação de quarto - em produção, isso chamaria um serviço externo
            // Por simplicidade, sempre libera com sucesso
            aggregate.ReleaseRoom();

            _logger.LogInformation(
                "[COMMAND] ReleaseRoomCommand - Concluído | BookingId: {BookingId} | RoomNumber: {RoomNumber}",
                command.AggregateId.Value, command.RoomNumber);

            return Task.CompletedTask;
        }
    }
}

