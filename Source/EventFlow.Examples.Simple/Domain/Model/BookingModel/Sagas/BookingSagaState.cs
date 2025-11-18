using System;
using EventFlow.Aggregates;
using EventFlow.DeclarativeSaga.StateMachine;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas.Events;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas
{
    /// <summary>
    /// State for BookingSaga.
    /// Inherits from DeclarativeSagaState which automatically handles CompensationJobId management.
    /// Developers only need to apply their own domain events - no need to worry about compensation job IDs.
    /// </summary>
    public class BookingSagaState : DeclarativeSagaState<BookingSaga, BookingSagaId, BookingSagaState>,
        IApply<BookingSagaStartedEvent>,
        IApply<BookingSagaCompletedEvent>,
        IApply<BookingSagaFailedEvent>,
        IApply<BookingSagaPaymentTimeoutEvent>
    {
        public string? BookingId { get; private set; }
        public string? CustomerId { get; private set; }
        public string? HotelId { get; private set; }
        public DateTime? StartedDate { get; private set; }
        public DateTime? CompletedDate { get; private set; }
        public string? FailureReason { get; private set; }

        public void Apply(BookingSagaStartedEvent aggregateEvent)
        {
            BookingId = aggregateEvent.BookingId;
            CustomerId = aggregateEvent.CustomerId;
            HotelId = aggregateEvent.HotelId;
            StartedDate = aggregateEvent.StartedDate;
        }

        public void Apply(BookingSagaCompletedEvent aggregateEvent)
        {
            CompletedDate = aggregateEvent.CompletedDate;
        }

        public void Apply(BookingSagaFailedEvent aggregateEvent)
        {
            FailureReason = aggregateEvent.Reason;
        }

        public void Apply(BookingSagaPaymentTimeoutEvent aggregateEvent)
        {
            // Timeout event applied
        }
    }
}

