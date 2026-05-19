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
    public async Task SendMessageToChannelAsync(int channelId, int recipientUserId, MessageDto message)
    {
        await hubContext.Clients.Group($"channel:{channelId}").ReceiveMessage(message);

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