using System.Net.Http.Json;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Features.Friends.Services;

public interface IFriendshipApiService
{
    Task<List<PendingFriendshipDto>> GetPendingRequestsAsync(CancellationToken ct = default);
    Task<bool> RespondToRequestAsync(int requesterId, bool accept, CancellationToken ct = default);
    
    Task<List<FriendshipDto>> GetFriendsAsync(CancellationToken ct = default);
    
    Task<List<SidebarFriendSummaryDto>> GetFriendsSummaryAsync(CancellationToken ct = default);
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
    
    public async Task<List<SidebarFriendSummaryDto>> GetFriendsSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await http.GetFromJsonAsync<List<SidebarFriendSummaryDto>>("api/friendships/sidebar-summary", ct);
            return response ?? [];
        }
        catch
        {
            return [];
        }
    }
}