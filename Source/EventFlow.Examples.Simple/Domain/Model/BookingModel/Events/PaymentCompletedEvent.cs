using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("PaymentCompleted", 1)]
    public class PaymentCompletedEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string TransactionId { get; }
        public decimal Amount { get; }

        public PaymentCompletedEvent(string transactionId, decimal amount)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}

