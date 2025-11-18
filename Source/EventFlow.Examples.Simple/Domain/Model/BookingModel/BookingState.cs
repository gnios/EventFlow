using System;
using EventFlow.Aggregates;
using EventFlow.Examples.Simple.Domain.Model.BookingModel.Events;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel
{
    public class BookingState : AggregateState<BookingAggregate, BookingId, BookingState>,
        IApply<BookingCreatedEvent>,
        IApply<BookingCreatedWithPromotionEvent>,
        IApply<LastMinuteBookingCreatedEvent>,
        IApply<RoomReservedEvent>,
        IApply<RoomReservationFailedEvent>,
        IApply<PaymentCompletedEvent>,
        IApply<PaymentFailedEvent>,
        IApply<ConfirmationEmailSentEvent>,
        IApply<BookingCancelledEvent>
    {
        public string CustomerId { get; private set; } = string.Empty;
        public string HotelId { get; private set; } = string.Empty;
        public DateTime CheckInDate { get; private set; }
        public DateTime CheckOutDate { get; private set; }
        public decimal Amount { get; private set; }
        public string? PromotionCode { get; private set; }
        public decimal? DiscountAmount { get; private set; }
        public string? RoomNumber { get; private set; }
        public string? RoomType { get; private set; }
        public string? TransactionId { get; private set; }
        public string? EmailAddress { get; private set; }
        public BookingStatus Status { get; private set; } = BookingStatus.None;
        public string? ErrorMessage { get; private set; }
        public string? CancellationReason { get; private set; }

        public void Apply(BookingCreatedEvent aggregateEvent)
        {
            CustomerId = aggregateEvent.CustomerId;
            HotelId = aggregateEvent.HotelId;
            CheckInDate = aggregateEvent.CheckInDate;
            CheckOutDate = aggregateEvent.CheckOutDate;
            Amount = aggregateEvent.Amount;
            Status = BookingStatus.Created;
        }

        public void Apply(BookingCreatedWithPromotionEvent aggregateEvent)
        {
            CustomerId = aggregateEvent.CustomerId;
            HotelId = aggregateEvent.HotelId;
            CheckInDate = aggregateEvent.CheckInDate;
            CheckOutDate = aggregateEvent.CheckOutDate;
            Amount = aggregateEvent.Amount;
            PromotionCode = aggregateEvent.PromotionCode;
            DiscountAmount = aggregateEvent.DiscountAmount;
            Status = BookingStatus.Created;
        }

        public void Apply(LastMinuteBookingCreatedEvent aggregateEvent)
        {
            CustomerId = aggregateEvent.CustomerId;
            HotelId = aggregateEvent.HotelId;
            CheckInDate = aggregateEvent.CheckInDate;
            CheckOutDate = aggregateEvent.CheckOutDate;
            Amount = aggregateEvent.Amount;
            Status = BookingStatus.Created;
        }

        public void Apply(RoomReservedEvent aggregateEvent)
        {
            RoomNumber = aggregateEvent.RoomNumber;
            RoomType = aggregateEvent.RoomType;
            Status = BookingStatus.RoomReserved;
        }

        public void Apply(RoomReservationFailedEvent aggregateEvent)
        {
            Status = BookingStatus.RoomReservationFailed;
            ErrorMessage = aggregateEvent.Reason;
        }

        public void Apply(PaymentCompletedEvent aggregateEvent)
        {
            TransactionId = aggregateEvent.TransactionId;
            Status = BookingStatus.PaymentCompleted;
        }

        public void Apply(PaymentFailedEvent aggregateEvent)
        {
            Status = BookingStatus.PaymentFailed;
            ErrorMessage = aggregateEvent.Reason;
        }

        public void Apply(ConfirmationEmailSentEvent aggregateEvent)
        {
            EmailAddress = aggregateEvent.EmailAddress;
            Status = BookingStatus.Confirmed;
        }

        public void Apply(BookingCancelledEvent aggregateEvent)
        {
            Status = BookingStatus.Cancelled;
            CancellationReason = aggregateEvent.Reason;
        }
    }

    public enum BookingStatus
    {
        None,
        Created,
        RoomReserved,
        RoomReservationFailed,
        PaymentCompleted,
        PaymentFailed,
        Confirmed,
        Cancelled
    }
}

