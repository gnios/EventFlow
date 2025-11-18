using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("PaymentFailed", 1)]
    public class PaymentFailedEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string Reason { get; }

        public PaymentFailedEvent(string reason)
        {
            Reason = reason;
        }
    }
}

