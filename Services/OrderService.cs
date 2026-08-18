using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Entities;
using MauricioAguilar.OrderService.Entities.Enums;
using MauricioAguilar.OrderService.Repositories;

namespace MauricioAguilar.OrderService.Services
{
    public class OrderService : IOrderService
    {
        public IOrderRepository _repository;
        private readonly Messaging.IMessagePublisher _publisher;

        public OrderService(IOrderRepository repository, Messaging.IMessagePublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<Order> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
        {

            var order = new Order
            {
                OrderNumber = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                Status = OrderStatus.Processing,
                Tax = request.Tax,
                ShippingCost = request.ShippingCost,
                Discount = request.Items.Sum(i => i.Discount),
                Currency = request.Currency,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                RowVersion = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
            };

            foreach (var itemRequest in request.Items)
            {
                var item = new OrderItem
                {
                    ProductId = itemRequest.ProductId,
                    ProductName = itemRequest.ProductName,
                    Sku = itemRequest.Sku,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    Discount = itemRequest.Discount,
                    Total =
                        (itemRequest.UnitPrice * itemRequest.Quantity)
                        - itemRequest.Discount
                };

                order.Items.Add(item);
            }

            order.Subtotal = order.Items.Sum(x => x.Total);

            order.Total =
                order.Subtotal
                + order.Tax
                + order.ShippingCost
                - order.Discount;


            await _repository.AddAsync(order);

            // publish an event to ActiveMQ
            try
            {
                await _publisher.PublishOrderCreatedAsync(order);
            }
            catch
            {
                // swallow to avoid breaking order creation on messaging failures
            }

            return order;
        }

        public async Task<List<Order>> GetOrders()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Order?> GetById(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}
