using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.DTOs
{
    public sealed record CreateOrderCommand(
        Guid CommandId,
        Guid CorrelationId,
        int CustomerId,
        string Currency,
        Address ShippingAddress,
        Address BillingAddress,
        IReadOnlyCollection<CreateOrderItemCommand> Items);
}
