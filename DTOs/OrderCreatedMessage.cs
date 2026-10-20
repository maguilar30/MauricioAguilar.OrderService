using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.DTOs
{
    public sealed record OrderCreatedMessage(
    int OrderId,
    int CustomerId,
    decimal Subtotal,
    decimal Total,
    List<CreateOrderItemRequest> Items,
    Address? ShippingAddress,
    Address? BillingAddress,
    string Currency);
}
