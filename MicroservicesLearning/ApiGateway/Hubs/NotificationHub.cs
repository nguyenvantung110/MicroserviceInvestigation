using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendOrderStatus(string orderId, string status)
        {
            await Clients.All.SendAsync("ReceiveOrderStatus", orderId, status);
        }
    }
}
