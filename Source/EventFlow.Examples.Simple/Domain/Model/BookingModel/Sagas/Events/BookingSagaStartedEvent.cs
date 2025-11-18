using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events
{
    [EventVersion("BookingSagaStarted", 1)]
    public class BookingSagaStartedEvent : AggregateEvent<BookingSaga, BookingSagaId>
    {
        public string BookingId { get; }
        public string CustomerId { get; }
        public string HotelId { get; }
        public DateTime StartedDate { get; }

        public BookingSagaStartedEvent(string bookingId, string customerId, string hotelId, DateTime startedDate)
        {
            BookingId = bookingId;
            CustomerId = customerId;
            HotelId = hotelId;
            StartedDate = startedDate;
        }
    }
}

