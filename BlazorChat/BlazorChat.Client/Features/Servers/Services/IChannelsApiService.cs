using System.Net;
using System.Net.Http.Json;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IChannelsApiService
{
    Task<List<ChannelDto>> GetChannelsAsync(int serverId, CancellationToken ct = default);
    Task<List<CategoryDto>> GetCategoriesAsync(int serverId, CancellationToken ct = default);
    
    Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto);
    
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(int serverId, CreateCategoryDto dto);
    
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

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(int serverId, CreateCategoryDto dto)
    {
        var response = await http.PostAsJsonAsync($"api/servers/{serverId}/categories", dto);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<CategoryDto>();
            return new ApiResponse<CategoryDto> { IsSuccess = true, Data = data, StatusCode = response.StatusCode };
        }

        // Handle the specific error states
        var apiResponse = new ApiResponse<CategoryDto> 
        { 
            IsSuccess = false, 
            StatusCode = response.StatusCode 
        };

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                // Extract the { message = result.ErrorMessage } payload from the server
                var errorContent = await response.Content.ReadFromJsonAsync<ErrorPayload>();
                apiResponse.ErrorMessage = errorContent?.Message ?? "Invalid request data.";
                break;

            case HttpStatusCode.Unauthorized:
                apiResponse.ErrorMessage = "You must be logged in to do this.";
                break;

            case HttpStatusCode.Forbidden:
                apiResponse.ErrorMessage = "You do not have permission to create a category here.";
                break;

            case HttpStatusCode.NotFound:
                apiResponse.ErrorMessage = "The specified server was not found.";
                break;

            case HttpStatusCode.InternalServerError:
            default:
                apiResponse.ErrorMessage = "An unexpected error occurred. Please try again later.";
                break;
        }

        return apiResponse;
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
        await http.PatchAsJsonAsync($"api/channels/{channelId}", dto);
    }

    public async Task UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto)
    {
        await http.PatchAsJsonAsync($"api/categories/{categoryId}", dto);
    }
}