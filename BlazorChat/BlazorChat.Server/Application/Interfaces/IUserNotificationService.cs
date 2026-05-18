using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces;

public interface IUserNotificationService
{
    Task SendUserStatusChangedAsync(IReadOnlyList<string> friendIds, ReceiveUserStatusDto statusDto);
}