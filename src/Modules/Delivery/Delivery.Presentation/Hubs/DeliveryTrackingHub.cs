namespace Delivery.Presentation.Hubs;

using Microsoft.AspNetCore.SignalR;

public class DeliveryTrackingHub : Hub
{
    public async Task JoinOrderTracking(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public async Task LeaveOrderTracking(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public async Task BroadcastRiderLocation(string orderId, double latitude, double longitude, int etaMinutes)
    {
        await Clients.Group($"order-{orderId}").SendAsync("ReceiveRiderLocationUpdate", orderId, latitude, longitude, etaMinutes);
    }

    public async Task BroadcastOrderStatus(string orderId, string status)
    {
        await Clients.Group($"order-{orderId}").SendAsync("ReceiveOrderStatusUpdate", orderId, status);
    }
}

