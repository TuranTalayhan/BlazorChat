using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces;

public interface IFriendNotificationService
{
    Task SendFriendRequestAsync(string targetUserId, PendingFriendshipDto dto);
    Task SendNewFriendAddedAsync(string userId, FriendshipDto dto);
}