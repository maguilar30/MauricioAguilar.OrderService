namespace MauricioAguilar.OrderService.Entities
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }

        public string MessageType { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;

        public Guid CorrelationId { get; set; }
        public DateTime OccurredAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }
        public int RetryCount { get; set; }
        public string? Error { get; set; }

        // Helps safely run multiple Order Service replicas.
        public DateTime? LockedUntilUtc { get; set; }
    }
}
