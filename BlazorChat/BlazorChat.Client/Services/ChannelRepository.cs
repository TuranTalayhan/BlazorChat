using System.Net.Http.Json;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Services;

namespace BlazorChat.Client.Services;

public class ChannelRepository(HttpClient http) : IChannelRepository
{
    public async Task<List<ChannelDto>> GetByServerAsync(int serverId)
    {
        try
        {
            return await http.GetFromJsonAsync<List<ChannelDto>>($"api/servers/{serverId}/channels") ?? new();
        }
        catch (HttpRequestException)
        {
            return new();
        }
    }
}
