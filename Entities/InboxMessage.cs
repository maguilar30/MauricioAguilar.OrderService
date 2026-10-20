namespace MauricioAguilar.OrderService.Entities
{
    public sealed class InboxMessage
    {
        public Guid MessageId { get; set; }

        public string MessageType { get; set; } = string.Empty;

        public DateTime ReceivedAtUtc { get; set; }

        public DateTime ProcessedAtUtc { get; set; }

        public string? CorrelationId { get; set; }
    }
}
