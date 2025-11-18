using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Sagas;
using EventFlow.Examples.Simple.Domain.Model.BookingModel;

namespace EventFlow.Examples.Simple.Domain.Model.BookingModel.Sagas
{
    public class BookingSagaLocator : ISagaLocator
    {
        public Task<ISagaId> LocateSagaAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var bookingId = domainEvent.GetIdentity();
            var sagaId = new BookingSagaId($"booking-saga-{bookingId.Value}");
            return Task.FromResult<ISagaId>(sagaId);
        }
    }
}

