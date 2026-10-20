using Apache.NMS;
using Apache.NMS.ActiveMQ;
using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Entities;
using MauricioAguilar.OrderService.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MauricioAguilar.OrderService.Messaging
{
    public sealed class InboxOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InboxOrderConsumer> _logger;

        private IConnection? _connection;

        public InboxOrderConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<InboxOrderConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory(
                _configuration["ActiveMq:BrokerUri"]);

            _connection = factory.CreateConnection(
                _configuration["ActiveMq:Username"],
                _configuration["ActiveMq:Password"]);

            _connection.Start();

            using var session =
                _connection.CreateSession(AcknowledgementMode.Transactional);

            using var consumer =
                session.CreateConsumer(session.GetQueue(_configuration["ActiveMq:Queue"]));

            _logger.LogInformation(
                "Order consumer is listening to ActiveMQ queue: {queue}", _configuration["ActiveMq:Queue"]);

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = consumer.Receive(TimeSpan.FromSeconds(1));

                if(message is not null && message.Properties["MessageType"].ToString() == "order")
                {
                    session.Commit();
                    continue;
                }

                if (message is not ITextMessage textMessage)
                {
                    continue;
                }

                try
                {
                    await ProcessAsync(textMessage, stoppingToken);

                    // Acknowledges the ActiveMQ message only after
                    // the database transaction succeeded.
                    session.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not process order message.");

                    // ActiveMQ will redeliver it according to its retry policy.
                    session.Rollback();
                }
            }
        }

        private async Task ProcessAsync(
            ITextMessage message,
            CancellationToken cancellationToken)
        {
            var command = JsonSerializer.Deserialize<CreateOrderCommand>(
                message.Text)
                ?? throw new InvalidOperationException(
                    "Invalid CreateOrderCommand message.");

            using var scope = _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Inbox check: protects against duplicate ActiveMQ delivery.
            var alreadyProcessed = await dbContext.InboxMessages.AnyAsync(
                x => x.MessageId == command.CommandId,
                cancellationToken);

            if (alreadyProcessed)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var order = OrderFactory.Create(
                customerId: command.CustomerId,
                items: command.Items.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Sku = x.Sku,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }),
                shippingAddress: command.ShippingAddress,
                billingAddress: command.BillingAddress,
                currency: command.Currency);

            dbContext.Orders.Add(order);

            // Publish only after the order is safely saved.
            var orderCreated = new OrderCreated(
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.Total);

            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = nameof(OrderCreated),
                Destination = _configuration["ActiveMq:Queue"] ?? "",
                Payload = JsonSerializer.Serialize(orderCreated),
                CorrelationId = command.CorrelationId,
                OccurredAtUtc = DateTime.UtcNow
            });

            dbContext.InboxMessages.Add(new InboxMessage
            {
                MessageId = command.CommandId,
                ProcessedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderNumber} created from message {MessageId}.",
                order.OrderNumber,
                command.CommandId);
        }

        public override void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}
