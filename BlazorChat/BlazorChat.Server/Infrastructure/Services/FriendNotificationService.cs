using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Infrastructure.Services;

public class FriendNotificationService(IHubContext<FriendHub, IFriendClient> hubContext) : IFriendNotificationService
{
    public async Task SendFriendRequestAsync(string targetUserId, PendingFriendshipDto dto)
    {
        await hubContext.Clients.User(targetUserId).ReceiveFriendRequest(dto);
    }

    public async Task SendNewFriendAddedAsync(string userId, FriendshipDto dto)
    {
        await hubContext.Clients.User(userId).ReceiveNewFriend(dto);
    }
}