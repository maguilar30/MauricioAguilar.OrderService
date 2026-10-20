using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();

        Task<Order?> GetByIdAsync(int id);

        Task AddAsync(Order order, List<CreateOrderItemRequest> orderItems, CancellationToken cancellationToken);
    }
}
