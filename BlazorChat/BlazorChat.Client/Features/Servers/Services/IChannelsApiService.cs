using System.Net;
using System.Net.Http.Json;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IChannelsApiService
{
    Task<bool> UpdateUserRoleInServerAsync(int serverId, int targetUserId, ServerRole newRole);
    Task<List<ChannelDto>> GetChannelsAsync(int serverId, CancellationToken ct = default);
    Task<List<CategoryDto>> GetCategoriesAsync(int serverId, CancellationToken ct = default);
    
    Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(int serverId, CreateCategoryDto dto);
    
    Task DeleteChannelAsync(int serverId, int channelId);
    Task DeleteCategoryAsync(int serverId, int categoryId);
    
    Task UpdateChannelAsync(int channelId, UpdateChannelDto dto);
    Task UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto);

    Task<ServerRole> GetUserRoleInServerAsync(int serverId, CancellationToken ct = default);
    Task<InviteResponseDto?> CreateServerInviteAsync(int serverId, CreateInviteDto dto, CancellationToken ct = default);
    Task<ApiResponse<ServerDto>> JoinServerWithCodeAsync(string code, CancellationToken ct = default);
    Task<List<UserDto>> GetServerMembersAsync(int serverId, CancellationToken ct = default);
    
    Task<ServerDto?> GetServerByChannelIdAsync(int channelId, CancellationToken ct = default);
}

public class ChannelsApiService(HttpClient http) : IChannelsApiService
{
    private const string BaseUrl = "api/servers";
    
    public async Task<bool> UpdateUserRoleInServerAsync(int serverId, int targetUserId, ServerRole newRole)
    {
        var response = await http.PutAsJsonAsync($"api/servers/{serverId}/members/{targetUserId}/role", newRole);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<ServerDto?> GetServerByChannelIdAsync(int channelId, CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetAsync($"{BaseUrl}/by-channel/{channelId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ServerDto>(cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<List<UserDto>> GetServerMembersAsync(int serverId, CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<UserDto>>($"api/servers/{serverId}/members", ct) ?? [];
    }
    
    public async Task<List<ChannelDto>> GetChannelsAsync(int serverId, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<ChannelDto>>($"{BaseUrl}/{serverId}/channels", ct) ?? [];
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int serverId, CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<CategoryDto>>($"{BaseUrl}/{serverId}/categories", ct) ?? [];
    }

    public async Task<ChannelDto?> CreateChannelAsync(int serverId, CreateServerChannelDto dto)
    {
        var response = await http.PostAsJsonAsync($"{BaseUrl}/{serverId}/channels", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChannelDto>();
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(int serverId, CreateCategoryDto dto)
    {
        var response = await http.PostAsJsonAsync($"{BaseUrl}/{serverId}/categories", dto);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<CategoryDto>();
            return new ApiResponse<CategoryDto> { IsSuccess = true, Data = data, StatusCode = response.StatusCode };
        }

        var apiResponse = new ApiResponse<CategoryDto> 
        { 
            IsSuccess = false, 
            StatusCode = response.StatusCode 
        };

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
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
    
    public async Task<ServerRole> GetUserRoleInServerAsync(int serverId, CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetAsync($"{BaseUrl}/{serverId}/role", ct);
            
            if (!response.IsSuccessStatusCode) 
                return ServerRole.Member;

            return await response.Content.ReadFromJsonAsync<ServerRole>(cancellationToken: ct);
        }
        catch
        {
            return ServerRole.Member;
        }
    }

    public async Task<InviteResponseDto?> CreateServerInviteAsync(int serverId, CreateInviteDto dto, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"{BaseUrl}/{serverId}/invites", dto, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<InviteResponseDto>(cancellationToken: ct);
    }
    
    public async Task<ApiResponse<ServerDto>> JoinServerWithCodeAsync(string code, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/servers/join/{code.Trim()}", null, ct);

        if (response.IsSuccessStatusCode)
        {
            var server = await response.Content.ReadFromJsonAsync<ServerDto>(cancellationToken: ct);
            return new ApiResponse<ServerDto> { IsSuccess = true, Data = server, StatusCode = response.StatusCode };
        }

        var apiResponse = new ApiResponse<ServerDto> { IsSuccess = false, StatusCode = response.StatusCode };
    
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            apiResponse.ErrorMessage = await response.Content.ReadAsStringAsync(ct);
        }
        else
        {
            apiResponse.ErrorMessage = "An unexpected error occurred while trying to join.";
        }

        return apiResponse;
    }
}