using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events
{
    [EventVersion("BookingSagaCompleted", 1)]
    public class BookingSagaCompletedEvent : AggregateEvent<BookingSaga, BookingSagaId>
    {
        public string BookingId { get; }
        public string ConfirmationNumber { get; }
        public DateTime CompletedDate { get; }

        public BookingSagaCompletedEvent(string bookingId, string confirmationNumber, DateTime completedDate)
        {
            BookingId = bookingId;
            ConfirmationNumber = confirmationNumber;
            CompletedDate = completedDate;
        }
    }
}

