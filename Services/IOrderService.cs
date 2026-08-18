using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrders();

        Task<Order?> GetById(int id);

        Task<Order> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken);
    }
}
