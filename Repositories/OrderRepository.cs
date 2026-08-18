using MauricioAguilar.OrderService.Entities;
using MauricioAguilar.Product.Service.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MauricioAguilar.OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        private readonly Messaging.IActiveMqPublisher _publisher;

        public OrderRepository(OrderDbContext context, Messaging.IActiveMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            _context.Orders.Add(order);

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = nameof(order),
                Destination = "inventory.reserve",
                Payload = JsonSerializer.Serialize(order),
                CorrelationId = Guid.NewGuid(),
                OccurredAtUtc = DateTime.UtcNow
            });


            //// publish an event to ActiveMQ
            //try
            //{
            //    await _publisher.PublishOrderCreatedAsync(order);
            //}
            //catch
            //{
            //    // swallow to avoid breaking order creation on messaging failures
            //}

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }
    }
}
