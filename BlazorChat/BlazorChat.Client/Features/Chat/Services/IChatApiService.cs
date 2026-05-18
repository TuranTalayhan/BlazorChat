using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Chat.Services;

public interface IChatApiService
{
    public Task<List<MessageDto>> GetMessagesAsync(int channelId, int count, DateTime? before = null,
        int? cursorId = null, CancellationToken ct = default);
    Task<bool> SendMessageAsync(string content, int channelId);
    Task<List<ChannelUnreadStatusDto>> GetUnreadStatusesAsync(CancellationToken ct = default);
}

public class ChatApiService(HttpClient http) : IChatApiService
{
    public async Task<List<MessageDto>> GetMessagesAsync(int channelId, int count, DateTime? before = null, int? cursorId = null, CancellationToken ct = default)
    {
        var url = $"api/messages/{channelId}?count={count}";
        
        if (before.HasValue)
        {
            url += $"&before={Uri.EscapeDataString(before.Value.ToString("o"))}";
        }

        if (cursorId.HasValue)
        {
            url += $"&messageId={cursorId.Value}";
        }

        return await http.GetFromJsonAsync<List<MessageDto>>(url, ct) ?? [];
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