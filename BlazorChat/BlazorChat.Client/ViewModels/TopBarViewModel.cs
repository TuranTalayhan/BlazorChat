using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.ViewModels;

public class TopBarViewModel(IFriendshipApiService apiService) : IAsyncDisposable
{
    private HubConnection? _hub;
    
    public List<PendingFriendshipDto> PendingRequests { get; private set; } = [];
    public bool IsInboxOpen { get; set; }
    
    // UI Events
    public event Action? OnChanged;
    // New: Notify the View when a friend request arrives so it can show a notification
    public event Action<string>? OnFriendRequestReceived;

    public async Task InitializeAsync()
    {
        PendingRequests = await apiService.GetPendingRequestsAsync();
        await SetupSignalR();
        OnChanged?.Invoke();
    }

    private async Task SetupSignalR()
    {
        _hub = new HubConnectionBuilder()

            .WithUrl("http://localhost:7138/hubs/chat", opt => 
            {
                opt.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler };
            })

            .Build(); 

        _hub.On<PendingFriendshipDto>("SendFriendRequest", dto => {
            PendingRequests.Insert(0, dto);
            OnFriendRequestReceived?.Invoke(dto.RequesterUsername); // Just emit the event
            OnChanged?.Invoke();
        });

        await _hub.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null) await _hub.DisposeAsync();
    }
}