using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Services;

public interface IFriendshipApiService
{
    Task<List<PendingFriendshipDto>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<bool> RespondToRequestAsync(int requesterId, bool accept, CancellationToken ct = default);
    
    Task<List<FriendshipDto>> GetFriendsAsync(CancellationToken ct = default);
    
    Task<int> GetOrCreateDmAsync(int friendIdm, CancellationToken ct = default);
}

public class FriendshipApiService(HttpClient http) : IFriendshipApiService
{
    public async Task<List<PendingFriendshipDto>> GetPendingRequestsAsync(CancellationToken ct) =>
        await http.GetFromJsonAsync<List<PendingFriendshipDto>>("api/friendships/pending", ct) ?? [];

    public async Task<bool> RespondToRequestAsync(int requesterId, bool accept, CancellationToken ct)
    {
        var res = await http.PatchAsJsonAsync($"api/friendships/{requesterId}", accept, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<FriendshipDto>> GetFriendsAsync(CancellationToken ct)
    {
        var res = await http.GetAsync($"api/friendships", ct);
        if (!res.IsSuccessStatusCode) return [];
        return await res.Content.ReadFromJsonAsync<List<FriendshipDto>>(ct) ?? [];  
    }
    
    public async Task<int> GetOrCreateDmAsync(int friendId,  CancellationToken ct)
    {
        var response = await http.PostAsync($"api/channels/dm/{friendId}", null, ct);
        return await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
    }
}