namespace MauricioAguilar.OrderService.Entities.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        InventoryReserved = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        PaymentFailed = 7,
        RefundPending = 8,
        Refunded = 9
    }
}
