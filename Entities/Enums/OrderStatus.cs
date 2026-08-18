namespace MauricioAguilar.OrderService.Entities.Enums
{
    public enum OrderStatus
    {
        Pending,
        InventoryReserved,
        Confirmed,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        PaymentFailed,
        RefundPending,
        Refunded
    }
}
