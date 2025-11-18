using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("RoomReservationFailed", 1)]
    public class RoomReservationFailedEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string Reason { get; }

        public RoomReservationFailedEvent(string reason)
        {
            Reason = reason;
        }
    }
}

