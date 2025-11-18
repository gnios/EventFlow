using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("ConfirmationEmailSent", 1)]
    public class ConfirmationEmailSentEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string EmailAddress { get; }

        public ConfirmationEmailSentEvent(string emailAddress)
        {
            EmailAddress = emailAddress;
        }
    }
}

