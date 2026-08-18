using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.DTOs
{
    public sealed record OrderResponse(
    int CustomerId,
    List<CreateOrderItemRequest> Items,
    Address ShippingAddress,
    Address BillingAddress,
    string Currency);
}
