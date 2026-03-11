using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BlazorChat.Shared.Hubs;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    /// <summary>Join a channel group to receive its messages.</summary>
    public async Task JoinChannel(int channelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"channel:{channelId}");
    }

    /// <summary>Leave a channel group.</summary>
    public async Task LeaveChannel(int channelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel:{channelId}");
    }

    /// <summary>Join a DM conversation group.</summary>
    public async Task JoinDm(int dmId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"dm:{dmId}");
    }

    /// <summary>Leave a DM conversation group.</summary>
    public async Task LeaveDm(int dmId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dm:{dmId}");
    }

    public async Task NotifyStatusChange(int userId, UserStatus newStatus)
    {
        await Clients.All.UserStatusChanged(userId, newStatus);
    }
}
