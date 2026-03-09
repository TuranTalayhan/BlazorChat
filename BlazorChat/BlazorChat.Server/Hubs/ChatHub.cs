using Microsoft.AspNetCore.SignalR;
using BlazorChat.Shared.Hubs;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public async Task NotifyStatusChange(int userId, UserStatus newStatus)
    {
        await Clients.All.UserStatusChanged(userId, newStatus);
    }
}