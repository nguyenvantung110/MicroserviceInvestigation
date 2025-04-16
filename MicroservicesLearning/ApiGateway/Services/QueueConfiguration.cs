using RabbitMQ.Client;

namespace ApiGateway.Services
{
    public class QueueConfiguration
    {
        public static void ConfigureQueues(IModel channel)
        {
            // Dead Letter Queue
            channel.QueueDeclare(
                queue: "order_dlq",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Order Queue with Dead Letter Exchange
            channel.QueueDeclare(
                queue: "order_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                { "x-dead-letter-exchange", "" }, // Send to DLQ
                { "x-dead-letter-routing-key", "order_dlq" }
                }
            );
        }
    }
}