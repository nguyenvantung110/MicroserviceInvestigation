using ApiGateway.Hubs;
using ApiGateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private static ConcurrentDictionary<string, Order> _orders = new();

    private readonly IHubContext<NotificationHub> _hubContext;

    public OrderController(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Get list order
    [HttpGet]
    public IActionResult GetOrders()
    {
        return Ok(_orders.Values);
    }

    // Create new order
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync([FromBody] Order order)
    {
        if (order == null || string.IsNullOrEmpty(order.OrderId) || string.IsNullOrEmpty(order.Product))
        {
            return BadRequest(new { Message = "Invalid order data." });
        }

        // Set org status is "Pending"
        order.Status = "Pending";
        _orders[order.OrderId] = order;

        // Send org status for client
        await _hubContext.Clients.All.SendAsync("OrderStatusUpdated", order.OrderId, order.Status);

        // Ex process time
        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);

            // Error
            if (order.Product == "ErrorProduct")
            {
                order.Status = "Failed";
                order.Message = "An error occurred while processing the order.";
            }
            else
            {
                order.Status = "Processed";
                order.Message = "Order processed successfully.";
            }

            // Update status of order
            _orders[order.OrderId] = order;

            // Send new status to client
            await _hubContext.Clients.All.SendAsync("OrderStatusUpdated", order.OrderId, order.Status);
        });

        // return origin status for user
        return Ok(new { OrderId = order.OrderId, Status = order.Status, Message = "Order is being processed." });
    }

    //[HttpGet]
    //public IActionResult GetTime()
    //{
    //    return Ok(new { Time = DateTime.UtcNow.ToString("HH:mm:ss"), Date = DateTime.UtcNow.ToString("yyyy-MM-dd") });
    //}
}