using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Sagas;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas
{
    public class OrderSagaLocator : ISagaLocator
    {
        public Task<ISagaId> LocateSagaAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // A saga é identificada pelo ID do pedido
            var orderId = domainEvent.GetIdentity();
            var sagaId = new OrderSagaId($"order-saga-{orderId.Value}");
            return Task.FromResult<ISagaId>(sagaId);
        }
    }
}

