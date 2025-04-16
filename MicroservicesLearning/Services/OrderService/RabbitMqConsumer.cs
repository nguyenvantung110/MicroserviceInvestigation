using ApiGateway.Hubs;
using ApiGateway.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqConsumer
{
    private readonly IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;
    private readonly IHubContext<NotificationHub> _hubContext;

    public RabbitMqConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        ApiGateway.Services.QueueConfiguration.ConfigureQueues(_channel);

        // Declare queue "order_queue"
        //_channel.QueueDeclare(queue: "order_queue",
        //                      durable: false,
        //                      exclusive: false,
        //                      autoDelete: false,
        //                      arguments: null);
    }

    public void StartListening()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<Order>(message);

            try
            {
                Console.WriteLine($"[x] Processing Order: {order.OrderId}");

                // Update database
                await ProcessOrder(order);

                // Send noti when successfully!
                await _hubContext.Clients.All.SendAsync("ReceiveOrderStatus", order.OrderId, "Processed");

                // Send ACK to confirm process successfully
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");

                // Retry or Dead Letter
                RetryOrDeadLetter(ea, order);
            }
        };

        _channel.BasicConsume(queue: "order_queue",
                              autoAck: true,
                              consumer: consumer);

        Console.WriteLine(" [*] Waiting for messages.");
    }

    private async Task ProcessOrder(Order order)
    {
        // Update database
        if (order.Product == "ErrorProduct")
        {
            throw new Exception("Database error!");
        }

        Console.WriteLine($"[v] Order {order.OrderId} processed successfully.");

        // Update status
        order.Status = "Processed";
    }

    private void RetryOrDeadLetter(BasicDeliverEventArgs ea, Order order)
    {
        // Count of retry from header
        int retryCount = 0;
        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryCountObj))
        {
            retryCount = Convert.ToInt32(retryCountObj);
        }

        if (retryCount >= 3)
        {
            // Send to Dead Letter Queue
            Console.WriteLine("[!] Sending to Dead Letter Queue...");
            _channel.BasicNack(ea.DeliveryTag, false, false);

            // Noti when failure
            _hubContext.Clients.All.SendAsync("ReceiveOrderStatus", order.OrderId, "Failed").Wait();
        }
        else
        {
            // Retry message
            Console.WriteLine($"[!] Retrying... Attempt {retryCount + 1}");
            ea.BasicProperties.Headers ??= new Dictionary<string, object>();
            ea.BasicProperties.Headers["x-retry-count"] = retryCount + 1;

            // Requeue message
            _channel.BasicNack(ea.DeliveryTag, false, true);
        }
    }


    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}