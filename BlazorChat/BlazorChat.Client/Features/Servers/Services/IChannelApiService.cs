using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IChannelApiService
{
    Task<List<ChannelDto>> GetByServerAsync(int serverId, CancellationToken ct = default);
}

public class ChannelApiService(HttpClient http) : IChannelApiService
{
    public async Task<List<ChannelDto>> GetByServerAsync(int serverId, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<ChannelDto>>($"api/servers/{serverId}/channels", ct) ?? [];
    }
}