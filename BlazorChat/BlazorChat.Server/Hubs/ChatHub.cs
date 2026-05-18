using System.Collections.Concurrent;
using System.Security.Claims;
using BlazorChat.Server.Application.Features.Messages.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BlazorChat.Shared.Hubs;
using Mediator;

namespace BlazorChat.Server.Hubs;

[Authorize]
public class ChatHub(IMediator mediator) : Hub<IChatClient>
{
    private int GetUserId() =>
        int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    public static readonly ConcurrentDictionary<int, HashSet<int>> ActiveChannelUsers = new();
    
    public async Task MarkAsRead(int channelId, int lastMessageId)
    {
        var userId = GetUserId();
        if (userId <= 0 || channelId <= 0 || lastMessageId <= 0) return;

        await mediator.Send(new MarkChannelReadCommand(userId, channelId, lastMessageId));
        
        await Clients.User(userId.ToString()).ReadStateUpdated(channelId, lastMessageId);
    }
    
    public async Task JoinChannel(int channelId)
    {
        var userId = GetUserId();
        if (userId <= 0 || channelId <= 0) return;

        // Join the physical SignalR Group for live websocket streaming
        await Groups.AddToGroupAsync(Context.ConnectionId, $"channel:{channelId}");

        // Track that this User ID is actively viewing this specific channel
        ActiveChannelUsers.AddOrUpdate(channelId, 
            _ => [userId], 
            (_, set) => { lock(set) { set.Add(userId); } return set; });
    }

    public async Task LeaveChannel(int channelId)
    {
        var userId = GetUserId();
        if (userId <= 0 || channelId <= 0) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel:{channelId}");

        // Remove from memory tracking
        if (ActiveChannelUsers.TryGetValue(channelId, out var set))
        {
            lock (set) { set.Remove(userId); }
            if (set.Count == 0) ActiveChannelUsers.TryRemove(channelId, out _);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            foreach (var kp in ActiveChannelUsers)
            {
                lock (kp.Value) { kp.Value.Remove(userId); }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
