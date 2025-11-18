using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("BookingCreated", 1)]
    public class BookingCreatedEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string CustomerId { get; }
        public string HotelId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public decimal Amount { get; }

        public BookingCreatedEvent(string customerId, string hotelId, DateTime checkInDate, DateTime checkOutDate, decimal amount)
        {
            CustomerId = customerId;
            HotelId = hotelId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Amount = amount;
        }
    }
}

