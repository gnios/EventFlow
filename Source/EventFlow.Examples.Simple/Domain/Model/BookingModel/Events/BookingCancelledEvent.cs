using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("BookingCancelled", 1)]
    public class BookingCancelledEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string Reason { get; }

        public BookingCancelledEvent(string reason)
        {
            Reason = reason;
        }
    }
}

