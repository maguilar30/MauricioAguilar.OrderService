using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using MauricioAguilar.OrderService.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MauricioAguilar.OrderService.Messaging
{
    public sealed class ActiveMqPublisher : IActiveMqPublisher, IDisposable
    {
        private readonly IConnection _connection;

        private readonly ILogger<ActiveMqPublisher> _logger;

        public ActiveMqPublisher(IConfiguration configuration, ILogger<ActiveMqPublisher> logger)
        {
            _logger = logger;

            var broker = configuration.GetValue<string>("ActiveMq:BrokerUri");

            var factory = new ConnectionFactory(broker);

            _connection = factory.CreateConnection(
                configuration["ActiveMq:Username"],
                configuration["ActiveMq:Password"]);

            _connection.Start();
        }

        public void Publish(OutboxMessage outboxMessage)
        {
            using var session =
                _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            using var producer =
                session.CreateProducer(session.GetQueue(outboxMessage.Destination));

            producer.DeliveryMode = MsgDeliveryMode.Persistent;

            var message = session.CreateTextMessage(outboxMessage.Payload);

            // These headers let consumers handle duplicates and correlate logs.
            message.Properties.SetString("MessageId", outboxMessage.Id.ToString());
            message.Properties.SetString("MessageType", outboxMessage.MessageType);
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
