namespace MauricioAguilar.OrderService.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishOrderCreatedAsync(Entities.Order order);
    }
}
