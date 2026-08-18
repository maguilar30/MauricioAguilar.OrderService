using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();

        Task<Order?> GetByIdAsync(int id);

        Task AddAsync(Order order);
    }
}
