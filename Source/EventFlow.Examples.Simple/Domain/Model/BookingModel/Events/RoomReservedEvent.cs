using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("RoomReserved", 1)]
    public class RoomReservedEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string RoomNumber { get; }
        public string RoomType { get; }

        public RoomReservedEvent(string roomNumber, string roomType)
        {
            RoomNumber = roomNumber;
            RoomType = roomType;
        }
    }
}

