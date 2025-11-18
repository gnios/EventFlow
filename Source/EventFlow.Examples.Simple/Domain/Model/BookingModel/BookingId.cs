using EventFlow.Core;
using EventFlow.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel
{
    public class BookingId : Identity<BookingId>
    {
        public BookingId(string value) : base(value)
        {
        }
    }
}

