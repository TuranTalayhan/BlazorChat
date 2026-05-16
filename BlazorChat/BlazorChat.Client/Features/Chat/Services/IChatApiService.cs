using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Chat.Services;

public interface IChatApiService
{
    Task<List<MessageDto>> GetMessagesAsync(int channelId, CancellationToken ct);
    Task<bool> SendMessageAsync(string content, int channelId);
}

public class ChatApiService(HttpClient http) : IChatApiService
{
    public async Task<List<MessageDto>> GetMessagesAsync(int channelId, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<MessageDto>>($"api/messages/{channelId}", ct) ?? [];
    }

    public async Task<bool> SendMessageAsync(string content, int channelId)
    {
        var dto = new { Content = content, ChannelId = channelId };
        var response = await http.PostAsJsonAsync("api/messages", dto);
        return response.IsSuccessStatusCode;
    }
}