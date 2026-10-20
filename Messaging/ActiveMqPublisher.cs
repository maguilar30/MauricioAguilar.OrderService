using Apache.NMS;
using MauricioAguilar.OrderService.Entities;

namespace MauricioAguilar.OrderService.Messaging
{
    public sealed class ActiveMqPublisher : IActiveMqPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;

        private readonly ILogger<ActiveMqPublisher> _logger;

        public ActiveMqPublisher(IConfiguration configuration, ILogger<ActiveMqPublisher> logger, IConnection connection)
        {
            _configuration = configuration;
            _logger = logger;
            _connection = connection;
        }

        public void Publish(OutboxMessage outboxMessage)
        {
            using var session =
                _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            using var producer =
                session.CreateProducer(session.GetTopic(_configuration["ActiveMq:Topic"]));

            producer.DeliveryMode = MsgDeliveryMode.Persistent;

            var message = session.CreateTextMessage(outboxMessage.Payload);

            // These headers let consumers handle duplicates and correlate logs.
            message.Properties.SetString("MessageId", outboxMessage.Id.ToString());
            message.Properties.SetString("MessageType", outboxMessage.MessageType);
            message.Properties.SetString("MessageSubType", outboxMessage.MessageSubType);
            message.NMSCorrelationID = outboxMessage.CorrelationId.ToString();

            producer.Send(message);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
