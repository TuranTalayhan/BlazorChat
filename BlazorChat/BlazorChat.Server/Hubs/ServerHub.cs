using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class ServerHub : Hub<IChatClient>
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
}