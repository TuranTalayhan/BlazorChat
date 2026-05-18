using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.ViewModels;

public class TopBarViewModel(IFriendshipApiService apiService, IFriendHubService hubService)
    : IDisposable
{
    public List<PendingFriendshipDto> PendingRequests { get; private set; } = [];
    public bool IsInboxOpen { get; set; }

    public event Action? OnChanged;
    public event Action<string>? OnFriendRequestNotification;

    public async Task InitializeAsync()
    {
        PendingRequests = await apiService.GetPendingRequestsAsync();

        hubService.OnFriendRequestReceived += HandleNewRequest;
        await hubService.ConnectAsync();
        
        OnChanged?.Invoke();
    }

    private void HandleNewRequest(PendingFriendshipDto dto)
    {
        if (PendingRequests.Any(r => r.RequesterId == dto.RequesterId)) return;
        PendingRequests.Insert(0, dto);
        OnFriendRequestNotification?.Invoke(dto.RequesterUsername);
        OnChanged?.Invoke();
    }

    public async Task RespondToRequestAsync(PendingFriendshipDto request, bool accept)
    {
        var success = await apiService.RespondToRequestAsync(request.RequesterId, accept);
        
        if (success) 
        {
            PendingRequests.RemoveAll(r => r.RequesterId == request.RequesterId);
            OnChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        hubService.OnFriendRequestReceived -= HandleNewRequest;
    }
}