using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MauricioAguilar.OrderService.Messaging
{
    public class ActiveMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly Apache.NMS.ISession _session;
        private readonly Apache.NMS.IMessageProducer _producer;
        private readonly ILogger<ActiveMqPublisher> _logger;

        public ActiveMqPublisher(IConfiguration configuration, ILogger<ActiveMqPublisher> logger)
        {
            _logger = logger;

            var broker = configuration.GetValue<string>("ActiveMq:BrokerUri") ?? "tcp://localhost:61616";
            var queue = configuration.GetValue<string>("ActiveMq:QueueName") ?? "order.created";

            var nmsAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("Apache.NMS"))
                .Select(a => a.FullName)
                .ToArray();
            _logger.LogInformation("Diagnostic: loaded Apache.NMS assemblies: {Assemblies}", string.Join(" | ", nmsAssemblies));

            var factory = new ConnectionFactory(broker);
            _connection = factory.CreateConnection();
            _connection.Start();

            _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            var destination = SessionUtil.GetQueue(_session, queue);
            _producer = _session.CreateProducer(destination);
            _producer.DeliveryMode = MsgDeliveryMode.Persistent;
        }

        public System.Threading.Tasks.Task PublishOrderCreatedAsync(Entities.Order order)
        {
            try
            {
                var payload = JsonSerializer.Serialize(order);
                var message = _session.CreateTextMessage(payload);
                _producer.Send(message);
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Failed to publish order created message");
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                _producer?.Close();
                _session?.Close();
                _connection?.Close();
            }
            catch { }

            _producer?.Dispose();
            _session?.Dispose();
            _connection?.Dispose();
        }
    }
}
