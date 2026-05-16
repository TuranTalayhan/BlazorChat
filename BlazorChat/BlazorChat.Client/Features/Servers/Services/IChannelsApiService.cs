using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IChannelsApiService
{
    Task<List<ChannelDto>> GetChannelsAsync(int serverId, CancellationToken ct = default);
    Task<List<CategoryDto>> GetCategoriesAsync(int serverId, CancellationToken ct = default);
    
    Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto);
    
    Task<CategoryDto?> CreateCategoryAsync(int serverId, CreateCategoryDto dto);
    
    Task DeleteChannelAsync(int serverId, int channelId);
    
    Task DeleteCategoryAsync(int serverId, int categoryId);
    
    Task UpdateChannelAsync(int channelId, UpdateChannelDto dto);
    
    Task UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto);
}

public class ChannelsApiService(HttpClient http) : IChannelsApiService
{
    private const string BaseUrl = "api/servers";
    
    public async Task<List<ChannelDto>> GetChannelsAsync(int serverId, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<ChannelDto>>($"api/servers/{serverId}/channels", ct) ?? [];
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int serverId, CancellationToken ct = default)
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

    public async Task DeleteChannelAsync(int serverId, int channelId)
    {
        await http.DeleteAsync($"api/channels/{channelId}");
    }

    public async Task DeleteCategoryAsync(int serverId, int categoryId)
    {
        await http.DeleteAsync($"api/categories/{categoryId}");
    }

    public async Task UpdateChannelAsync(int channelId, UpdateChannelDto dto)
    {
        await http.PutAsJsonAsync($"api/channels/{channelId}", dto);
    }

    public async Task UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto)
    {
        await http.PutAsJsonAsync($"api/categories/{categoryId}", dto);
    }
}