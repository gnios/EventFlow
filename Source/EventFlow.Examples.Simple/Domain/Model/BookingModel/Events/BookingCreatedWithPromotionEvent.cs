using System;
using EventFlow.Aggregates;
using EventFlow.EventStores;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Events
{
    [EventVersion("BookingCreatedWithPromotion", 1)]
    public class BookingCreatedWithPromotionEvent : AggregateEvent<BookingAggregate, BookingId>
    {
        public string CustomerId { get; }
        public string HotelId { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public decimal Amount { get; }
        public string PromotionCode { get; }
        public decimal DiscountAmount { get; }

        public BookingCreatedWithPromotionEvent(string customerId, string hotelId, DateTime checkInDate, DateTime checkOutDate, decimal amount, string promotionCode, decimal discountAmount)
        {
            CustomerId = customerId;
            HotelId = hotelId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Amount = amount;
            PromotionCode = promotionCode;
            DiscountAmount = discountAmount;
        }
    }
}

