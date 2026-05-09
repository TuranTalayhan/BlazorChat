using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IChannelApiService
{
    Task<List<ChannelDto>> GetChannelsGetByServerAsync(int serverId, CancellationToken ct = default);
    Task<List<CategoryDto>> GetCategoriesByServerAsync(int serverId, CancellationToken ct = default);
    
    Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto);
    
    Task<CategoryDto?> CreateCategoryAsync(int serverId, CreateCategoryDto dto);
}

public class ChannelApiService(HttpClient http) : IChannelApiService
{
    public async Task<List<ChannelDto>> GetChannelsGetByServerAsync(int serverId, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<ChannelDto>>($"api/servers/{serverId}/channels", ct) ?? [];
    }

    public async Task<List<CategoryDto>> GetCategoriesByServerAsync(int serverId, CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<CategoryDto>>($"api/servers/{serverId}/categories", ct) ?? [];
    }

    public async Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto)
    {
        var response = await http.PostAsJsonAsync($"api/servers/{serverId}/channels", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChannelDto>();
    }

    public async Task<CategoryDto?> CreateCategoryAsync(int serverId, CreateCategoryDto dto)
    {
        var response = await http.PostAsJsonAsync($"api/servers/{serverId}/categories", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>();
    }
}