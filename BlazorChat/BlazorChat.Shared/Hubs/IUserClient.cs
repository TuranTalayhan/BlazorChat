using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Hubs;

public interface IUserClient
{
    Task UserStatusChanged(ReceiveUserStatusDto updateStatusDtoDto);
}