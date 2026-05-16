using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Infrastructure.Services;

public class ChatNotificationService(IHubContext<ChatHub, IChatClient> hubContext) : IChatNotificationService
{
    public async Task SendFriendRequestAsync(string targetUserId, PendingFriendshipDto dto)
    {
        await hubContext.Clients.User(targetUserId).SendFriendRequest(dto);
    }

    public async Task SendNewFriendAddedAsync(string userId, FriendshipDto dto)
    {
        await hubContext.Clients.User(userId).NewFriendAdded(dto);
    }

    public async Task SendMessageToChannelAsync(int channelId, MessageDto message)
    {
        await hubContext.Clients.Group($"channel:{channelId}").ReceiveMessage(message);
    }

    public async Task SendUserStatusChangedAsync(IReadOnlyList<string> friendIds, ReceiveUserStatusDto statusDto)
    {
        await hubContext.Clients.Users(friendIds).UserStatusChanged(statusDto);
    }
}