using EventFlow.Sagas;
using EventFlow.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.Sagas
{
    public class OrderSagaId : SingleValueObject<string>, ISagaId
    {
        public OrderSagaId(string value) : base(value)
        {
        }
    }
}

