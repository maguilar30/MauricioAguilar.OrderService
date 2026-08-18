using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.DTOs
{
    public class CreateOrderRequest()
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; }
        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public string Currency { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
    }
}
