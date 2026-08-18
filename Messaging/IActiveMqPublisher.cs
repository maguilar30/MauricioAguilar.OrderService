using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.Messaging
{
    public interface IActiveMqPublisher
    {
        //Task PublishOrderCreatedAsync(Entities.Order order);

        void Publish(OutboxMessage outboxMessage);
    }
}
