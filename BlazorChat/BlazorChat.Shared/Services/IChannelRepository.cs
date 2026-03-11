using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Services;

public interface IChannelRepository
{
    Task<List<ChannelDto>> GetByServerAsync(int serverId);
}
