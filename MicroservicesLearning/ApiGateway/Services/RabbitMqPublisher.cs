using RabbitMQ.Client;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;

namespace ApiGateway.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection(new List<string> { "localhost" });
            _channel = _connection.CreateModel();

            QueueConfiguration.ConfigureQueues(_channel);
        }

        public void PublishOrder(object order)
        {
            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "order_queue",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[x] Published: {message}");
        }


        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}