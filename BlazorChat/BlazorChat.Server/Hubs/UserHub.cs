using System.Collections.Concurrent;
using System.Security.Claims;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class UserHub : Hub<IUserClient>
{
    private static readonly ConcurrentDictionary<int, int> UserConnections = new();

    private int GetUserId() =>
        int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        
        if (userId > 0)
        {
            var currentConnections = UserConnections.AddOrUpdate(userId, 1, (_, count) => count + 1);

            if (currentConnections == 1)
            {
                var statusUpdate = new ReceiveUserStatusDto 
                { 
                    Id = userId, 
                    Status = UserStatus.Online
                };
                
                await Clients.All.UserStatusChanged(statusUpdate);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        
        if (userId > 0)
        {
            if (UserConnections.TryGetValue(userId, out var count))
            {
                var newCount = count - 1;
                
                if (newCount <= 0)
                {
                    UserConnections.TryRemove(userId, out _);
                    
                    var statusUpdate = new ReceiveUserStatusDto 
                    { 
                        Id = userId, 
                        Status = UserStatus.Offline 
                    };
                    
                    await Clients.All.UserStatusChanged(statusUpdate);
                }
                else
                {
                    UserConnections.TryUpdate(userId, newCount, count);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }   
    
    /// <summary>Broadcasts a user's status change (Online, Idle, DND) to all clients.</summary>
    public async Task NotifyStatusChange(ReceiveUserStatusDto statusDto)
    {
        await Clients.All.UserStatusChanged(statusDto);
    }
}