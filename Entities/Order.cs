using MauricioAguilar.OrderService.Entities.Enums;
using System.Net;
using System.Text.Json.Serialization;

namespace MauricioAguilar.OrderService.Entities
{
    public class Order
    {
        // Identity
        public int Id { get; set;  }
        public string OrderNumber { get; set; } = string.Empty;

        // Customer
        public int CustomerId { get; set; }

        // Status
        public OrderStatus Status { get; set; }

        // Money
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";

        // Shipping
        public Address ShippingAddress { get; set; } = null!;
        public Address? BillingAddress { get; set; }

        // Items
        [JsonIgnore]
        public ICollection<OrderItem> Items { get; set; } = [];

        // Dates / Audit
        [JsonIgnore]
        public Guid RowVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

    public static partial class OrderFactory
    {
        public static Order Create(
            int customerId,
            IEnumerable<OrderItem> items,
            Address shippingAddress,
            Address? billingAddress,
            string currency)
        {
            var order = new Order
            {
                CustomerId = customerId,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderNumber = Guid.NewGuid().ToString("N")
            };

            var itemList = items.Select(i =>
            {
                i.Order = order;
                return i;
            }).ToList();

            order.Items = itemList;

            order.Subtotal = itemList.Sum(i => i.UnitPrice * i.Quantity);
            order.Total = order.Subtotal; // adjust as needed for tax, shipping, discounts

            return order;
        }
    }
}
