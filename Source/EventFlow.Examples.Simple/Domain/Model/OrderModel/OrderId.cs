using EventFlow.Core;
using EventFlow.ValueObjects;
using Newtonsoft.Json;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class OrderId : Identity<OrderId>
    {
        public OrderId(string value) : base(value)
        {
        }
    }
}

