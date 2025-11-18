using System.Collections.Generic;
using EventFlow.ValueObjects;

namespace EventFlow.Examples.Simple.Domain.Model.OrderModel.ValueObjects
{
    public class OrderItem : ValueObject
    {
        public OrderItem(string productId, string productName, int quantity, decimal price)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            Price = price;
        }

        public string ProductId { get; }
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal Price { get; }
        public decimal TotalPrice => Quantity * Price;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ProductId;
            yield return ProductName;
            yield return Quantity;
            yield return Price;
        }
    }
}

