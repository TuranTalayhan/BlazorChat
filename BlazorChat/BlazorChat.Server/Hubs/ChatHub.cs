using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BlazorChat.Shared.Hubs;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<int, int> UserConnections = new();

    private int GetUserId() =>
        int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        
        if (userId > 0)
        {
            // Add user to the dictionary or increment their connection count
            var currentConnections = UserConnections.AddOrUpdate(userId, 1, (_, count) => count + 1);

            // If this is their very first connection, broadcast them as Online!
            if (currentConnections == 1)
            {
                var statusUpdate = new ReceiveUserStatusDto 
                { 
                    Id = userId, 
                    Status = UserStatus.Online // Assuming you have an enum or string for this
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
            // Safely decrement their connection count
            if (UserConnections.TryGetValue(userId, out var count))
            {
                var newCount = count - 1;
                
                if (newCount <= 0)
                {
                    // No more active connections, remove them and broadcast Offline
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
                    // Update with the decremented count (they still have other tabs open)
                    UserConnections.TryUpdate(userId, newCount, count);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
    
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
    
    /// <summary>Broadcasts a user's status change (Online, Idle, DND) to all clients.</summary>
    public async Task NotifyStatusChange(ReceiveUserStatusDto statusDto)
    {
        await Clients.All.UserStatusChanged(statusDto);
    }
}
