namespace BlazorChat.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task NotifyStatusChange(int userId, string newStatus)
    {
        await Clients.All.SendAsync("UserStatusChanged", userId, newStatus);
    }
}