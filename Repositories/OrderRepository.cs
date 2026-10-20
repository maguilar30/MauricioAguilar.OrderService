using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Entities;
using MauricioAguilar.OrderService.Entities.Enums;
using MauricioAguilar.OrderService.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MauricioAguilar.OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order, List<CreateOrderItemRequest> orderItems, CancellationToken cancellationToken)
        {
            await using var transaction = 
                await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync(cancellationToken);

            var payload = new OrderCreatedMessage(
                order.Id,
                order.CustomerId,
                order.Subtotal, order.Total,
                orderItems,
                order.ShippingAddress,
                order.BillingAddress,
                order.Currency);

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = nameof(order),
                MessageSubType = OrderStatus.Pending.ToString(),
                Destination = "inventory",
                Payload = JsonSerializer.Serialize(payload),
                CorrelationId = Guid.NewGuid(),
                OccurredAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
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
