using EventFlow.Sagas;
using EventFlow.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas
{
    public class BookingSagaId : SingleValueObject<string>, ISagaId
    {
        public BookingSagaId(string value) : base(value)
        {
        }
    }
}

