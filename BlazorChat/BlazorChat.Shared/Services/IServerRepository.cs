using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Services;

public interface IServerRepository
{
    event Action? OnChanged;
    Task<List<ServerDto>> GetAllAsync();
    Task<ServerDto?> CreateAsync(CreateServerDto dto);
}
