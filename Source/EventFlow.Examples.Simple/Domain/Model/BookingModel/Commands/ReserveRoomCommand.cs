using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class ReserveRoomCommand : Command<BookingAggregate, BookingId>
    {
        public string HotelId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }

        public ReserveRoomCommand(BookingId aggregateId, string hotelId, DateTime checkInDate, DateTime checkOutDate)
            : base(aggregateId)
        {
            HotelId = hotelId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }
    }

    public class ReserveRoomCommandHandler : CommandHandler<BookingAggregate, BookingId, ReserveRoomCommand>
    {
        private readonly ILogger<ReserveRoomCommandHandler> _logger;

        public ReserveRoomCommandHandler(ILogger<ReserveRoomCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            ReserveRoomCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] ReserveRoomCommand - Executando | BookingId: {BookingId} | HotelId: {HotelId}",
                command.AggregateId.Value, command.HotelId);

            // Simula reserva de quarto - em produção, isso chamaria um serviço externo
            // Por simplicidade, sempre reserva com sucesso
            // Em um cenário real, poderia falhar baseado em regras de negócio
            var roomNumber = $"ROOM-{command.HotelId}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            var roomType = "Standard";

            aggregate.ReserveRoom(roomNumber, roomType);

            _logger.LogInformation(
                "[COMMAND] ReserveRoomCommand - Concluído | BookingId: {BookingId} | RoomNumber: {RoomNumber}",
                command.AggregateId.Value, roomNumber);

            return Task.CompletedTask;
        }
    }
}

