using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Infrastructure.Services;

public class ChatNotificationService(IHubContext<ChatHub, IChatClient> hubContext) : IChatNotificationService
{
    public async Task SendMessageToChannelAsync(int channelId, int recipientUserId, MessageDto message)
    {
        await hubContext.Clients.Group($"channel:{channelId}").ReceiveMessage(message);

        if (recipientUserId > 0)
        {
            var isFriendActiveInRoom = false;

            if (ChatHub.ActiveChannelUsers.TryGetValue(channelId, out var activeUsers))
            {
                lock (activeUsers)
                {
                    isFriendActiveInRoom = activeUsers.Contains(recipientUserId);
                }
            }
            
            if (!isFriendActiveInRoom)
            {
                await hubContext.Clients.User(recipientUserId.ToString()).ReceiveMessage(message);
            }
        }
    }
}