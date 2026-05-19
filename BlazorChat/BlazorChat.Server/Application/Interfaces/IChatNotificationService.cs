using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces;

public interface IChatNotificationService
{
    public Task SendMessageToChannelAsync(int channelId, int recipientUserId, MessageDto message,
        string? excludedConnectionId = null);
}