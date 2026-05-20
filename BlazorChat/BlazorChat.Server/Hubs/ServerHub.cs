using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class ServerHub : Hub<IServerHubClient>
{
    public async Task JoinServerGroup(int serverId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"server_{serverId}");
    }

    public async Task LeaveServerGroup(int serverId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"server_{serverId}");
    }
}