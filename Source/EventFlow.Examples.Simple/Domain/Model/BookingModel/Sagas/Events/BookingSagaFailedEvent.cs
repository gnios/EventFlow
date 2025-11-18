using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events
{
    [EventVersion("BookingSagaFailed", 1)]
    public class BookingSagaFailedEvent : AggregateEvent<BookingSaga, BookingSagaId>
    {
        public string BookingId { get; }
        public string Reason { get; }
        public DateTime FailedDate { get; }

        public BookingSagaFailedEvent(string bookingId, string reason, DateTime failedDate)
        {
            BookingId = bookingId;
            Reason = reason;
            FailedDate = failedDate;
        }
    }
}

