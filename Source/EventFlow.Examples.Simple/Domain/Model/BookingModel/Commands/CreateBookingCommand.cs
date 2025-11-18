using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Commands;
using Microsoft.Extensions.Logging;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class CreateBookingCommand : Command<BookingAggregate, BookingId>
    {
        public CreateBookingCommand(
            BookingId aggregateId,
            string customerId,
            string hotelId,
            DateTime checkInDate,
            DateTime checkOutDate,
            decimal amount)
            : base(aggregateId)
        {
            CustomerId = customerId;
            HotelId = hotelId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Amount = amount;
        }

        public string CustomerId { get; }
        public string HotelId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public decimal Amount { get; }
    }

    public class CreateBookingCommandHandler : CommandHandler<BookingAggregate, BookingId, CreateBookingCommand>
    {
        private readonly ILogger<CreateBookingCommandHandler> _logger;

        public CreateBookingCommandHandler(ILogger<CreateBookingCommandHandler> logger)
        {
            _logger = logger;
        }

        public override Task ExecuteAsync(
            BookingAggregate aggregate,
            CreateBookingCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[COMMAND] CreateBookingCommand - Executando | BookingId: {BookingId} | CustomerId: {CustomerId} | HotelId: {HotelId}",
                command.AggregateId.Value, command.CustomerId, command.HotelId);

            aggregate.Create(
                command.CustomerId,
                command.HotelId,
                command.CheckInDate,
                command.CheckOutDate,
                command.Amount);

            _logger.LogInformation(
                "[COMMAND] CreateBookingCommand - Concluído | BookingId: {BookingId} | Evento: BookingCreatedEvent",
                command.AggregateId.Value);

            return Task.CompletedTask;
        }
    }
}

