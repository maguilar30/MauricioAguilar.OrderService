namespace MauricioAguilar.OrderService.DTOs
{
    public sealed record OrderCreated(
        int OrderId,
        string OrderNumber,
        int CustomerId,
        decimal Total);
}
