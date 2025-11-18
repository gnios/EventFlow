using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events
{
    [EventVersion("BookingSagaPaymentTimeout", 1)]
    public class BookingSagaPaymentTimeoutEvent : AggregateEvent<BookingSaga, BookingSagaId>
    {
        public string BookingId { get; }
        public DateTime TimeoutDate { get; }

        public BookingSagaPaymentTimeoutEvent(string bookingId, DateTime timeoutDate)
        {
            BookingId = bookingId;
            TimeoutDate = timeoutDate;
        }
    }
}

