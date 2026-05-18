using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Infrastructure.Services;

public class UserNotificationService(IHubContext<UserHub, IUserClient> hubContext) : IUserNotificationService
{
    public async Task SendUserStatusChangedAsync(IReadOnlyList<string> friendIds, ReceiveUserStatusDto statusDto)
    {
        await hubContext.Clients.Users(friendIds).UserStatusChanged(statusDto);
    }
}