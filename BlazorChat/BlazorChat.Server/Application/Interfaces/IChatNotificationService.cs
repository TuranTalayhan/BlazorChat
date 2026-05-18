using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces;

public interface IChatNotificationService
{
    Task SendMessageToChannelAsync(int channelId, int recipientUserId, MessageDto message);
}