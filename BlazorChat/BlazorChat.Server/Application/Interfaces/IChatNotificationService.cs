using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces;

public interface IChatNotificationService
{
    Task SendFriendRequestAsync(string targetUserId, PendingFriendshipDto dto);
    Task SendNewFriendAddedAsync(string userId, FriendshipDto dto);
    Task SendMessageToChannelAsync(int channelId, MessageDto message);
    Task SendUserStatusChangedAsync(IReadOnlyList<string> friendIds, ReceiveUserStatusDto statusDto);
}