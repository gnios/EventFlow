using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Commands
{
    public class RefundPaymentCommand : Command<BookingAggregate, BookingId, IExecutionResult>
    {
        public string TransactionId { get; }
        public decimal Amount { get; }

        public RefundPaymentCommand(BookingId aggregateId, string transactionId, decimal amount)
            : base(aggregateId)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}

