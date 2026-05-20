using System.Net;
using System.Net.Http.Json;
using BlazorChat.Client.Core;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Chat.Services;

public interface IChatApiService
{
    Task<ApiResponse<List<MessageDto>>> GetMessagesAsync(int channelId, int count, DateTime? before = null,
        int? cursorId = null, CancellationToken ct = default);
    Task<bool> SendMessageAsync(string content, int channelId);
    Task<List<ChannelUnreadStatusDto>> GetUnreadStatusesAsync(CancellationToken ct = default);
}

public class ChatApiService(HttpClient http) : IChatApiService
{
    public async Task<ApiResponse<List<MessageDto>>> GetMessagesAsync(int channelId, int count, DateTime? before = null, int? cursorId = null, CancellationToken ct = default)
    {
        var queryParams = new List<string> { $"count={count}" };
        
        if (before.HasValue)
        {
            queryParams.Add($"before={Uri.EscapeDataString(before.Value.ToString("o"))}");
        }

        if (cursorId.HasValue)
        {
            queryParams.Add($"messageId={cursorId.Value}");
        }

        var url = $"api/messages/{channelId}?{string.Join("&", queryParams)}";

        try
        {
            var response = await http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>(cancellationToken: ct) ?? [];
                return new ApiResponse<List<MessageDto>>
                {
                    IsSuccess = true,
                    Data = messages,
                    StatusCode = response.StatusCode
                };
            }

            var apiResponse = new ApiResponse<List<MessageDto>>
            {
                IsSuccess = false,
                Data = [],
                StatusCode = response.StatusCode
            };

            apiResponse.ErrorMessage = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Your active session has expired. Please log back in.",
                HttpStatusCode.Forbidden => "Access denied. You do not belong to the server hosting this channel.",
                HttpStatusCode.NotFound => "The selected channel or direct message thread could not be found.",
                _ => $"Failed to retrieve chat messages (Error: {(int)response.StatusCode})."
            };

            return apiResponse;
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MessageDto>>
            {
                IsSuccess = false,
                Data = [],
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "A network error occurred. Please check your internet connection."
            };
        }
    }

    public async Task<bool> SendMessageAsync(string content, int channelId)
    {
        var dto = new { Content = content, ChannelId = channelId };
        var response = await http.PostAsJsonAsync("api/messages", dto);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<List<ChannelUnreadStatusDto>> GetUnreadStatusesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetFromJsonAsync<List<ChannelUnreadStatusDto>>("api/messages/unread-states", ct);
            return response ?? [];
        }
        catch
        {
            return [];
        }
    }
}