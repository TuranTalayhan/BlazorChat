using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Features.Servers;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.ViewModels;

public class TopBarViewModel(IServerApiService serverApiService, IFriendshipApiService friendshipApiService) : IAsyncDisposable
{
    private HubConnection? _hub;
    
    public List<PendingFriendshipDto> PendingRequests { get; private set; } = [];
    public bool IsInboxOpen { get; set; }
    public event Action? OnChanged;
    public event Action<string>? OnFriendRequestReceived;

    public async Task InitializeAsync()
    {
        PendingRequests = await friendshipApiService.GetPendingRequestsAsync();
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