using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.DirectMessage;

public interface IDirectMessageApiService
{
    Task<List<ChannelDto>> GetMyDirectMessageChannelsAsync(CancellationToken ct = default);
    
    Task<int> OpenDirectMessageAsync(int friendId, CancellationToken ct = default);
}

public class DirectMessageApiService(HttpClient http) : IDirectMessageApiService
{
    public async Task<List<ChannelDto>> GetMyDirectMessageChannelsAsync(CancellationToken ct = default)
    {
        var response = await http.GetFromJsonAsync<List<ChannelDto>>("api/dms", ct);
        return response ?? [];
    }

    public async Task<int> OpenDirectMessageAsync(int friendId, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/dms", friendId, ct);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        }
        return 0;
    }
}