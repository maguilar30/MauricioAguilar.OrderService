namespace MauricioAguilar.OrderService.DTOs
{
    public sealed record CreateOrderItemCommand(
    int ProductId,
    string ProductName,
    string? Sku,
    int Quantity,
    decimal UnitPrice);
}
