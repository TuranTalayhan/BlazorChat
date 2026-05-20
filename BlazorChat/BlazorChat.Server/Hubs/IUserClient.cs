using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

public interface IUserClient
{
    Task UserStatusChanged(ReceiveUserStatusDto updateStatusDtoDto);
}