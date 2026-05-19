using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Infrastructure.Services;

public class ChatNotificationService(
    IHubContext<ChatHub, IChatClient> hubContext,
    IChatPresenceTracker presenceTracker)
    : IChatNotificationService
{
    public async Task SendMessageToChannelAsync(
        int channelId, 
        int recipientUserId, 
        MessageDto message, 
        string? excludedConnectionId = null)
    {
        var groupName = $"channel:{channelId}";

        if (!string.IsNullOrEmpty(excludedConnectionId))
        {
            // Broadcasts to the entire channel room, excluding the person who sent it
            var excludedList = new List<string> { excludedConnectionId }.AsReadOnly();
            await hubContext.Clients.GroupExcept(groupName, excludedList).ReceiveMessage(message);
        }
        else
        {
            // Fallback: if no connection id is passed, broadcast to everyone
            await hubContext.Clients.Group(groupName).ReceiveMessage(message);
        }

        if (recipientUserId > 0)
        {
            var isFriendActive = presenceTracker.IsUserActiveInChannel(channelId, recipientUserId);
        
            if (!isFriendActive)
            {
                await hubContext.Clients.User(recipientUserId.ToString()).ReceiveMessage(message);
            }
        }
    }
}