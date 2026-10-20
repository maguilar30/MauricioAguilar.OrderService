using MauricioAguilar.OrderService.Entities;
using MauricioAguilar.OrderService.Data;
using Microsoft.EntityFrameworkCore;

namespace MauricioAguilar.OrderService.Messaging
{
    public sealed class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox publisher worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(stoppingToken);

                    await Task.Delay(
                        TimeSpan.FromSeconds(3),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    // Normal shutdown.
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error in the outbox publisher worker.");

                    await Task.Delay(
                        TimeSpan.FromSeconds(10),
                        stoppingToken);
                }
            }
        }

        private async Task PublishPendingMessagesAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var publisher =
                scope.ServiceProvider.GetRequiredService<IActiveMqPublisher>();

            var now = DateTime.UtcNow;

            var messages = await dbContext.OutboxMessages
                .Where(x =>
                    x.PublishedAtUtc == null &&
                    (x.LockedUntilUtc == null || x.LockedUntilUtc < now))
                .OrderBy(x => x.OccurredAtUtc)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                // Claim the message for 1 minute.
                // This matters when you have multiple API instances.
                var claimed = await dbContext.OutboxMessages
                    .Where(x =>
                        x.Id == message.Id &&
                        x.PublishedAtUtc == null &&
                        (x.LockedUntilUtc == null || x.LockedUntilUtc < now))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            x => x.LockedUntilUtc,
                            now.AddMinutes(1)),
                        cancellationToken);

                if (claimed == 0)
                {
                    continue;
                }

                try
                {
                    publisher.Publish(message);

                    message.PublishedAtUtc = DateTime.UtcNow;
                    message.LockedUntilUtc = null;
                    message.Error = null;

                    _logger.LogInformation(
                        "Published outbox message {MessageId} ({MessageType}) to {Destination}.",
                        message.Id,
                        message.MessageType,
                        message.Destination);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.Error = ex.Message;

                    // Unlock it so a later worker iteration retries it.
                    message.LockedUntilUtc = null;

                    _logger.LogError(
                        ex,
                        "Failed to publish outbox message {MessageId}. Retry: {RetryCount}",
                        message.Id,
                        message.RetryCount);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
