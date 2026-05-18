using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Friends.Services;

public interface IFriendHubService : IAsyncDisposable
{
    event Action<FriendshipDto>? OnNewFriendAdded;
    event Action<PendingFriendshipDto>? OnFriendRequestReceived;
    Task ConnectAsync();
}

public class FriendHubService : IFriendHubService
{
    private HubConnection? _hubConnection;
    public event Action<FriendshipDto>? OnNewFriendAdded;
    public event Action<PendingFriendshipDto>? OnFriendRequestReceived;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/friend", options => 
            {
                options.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler }; 
            })
            .Build();
        
        _hubConnection.On<FriendshipDto>("ReceiveNewFriend", friend =>
        {
            OnNewFriendAdded?.Invoke(friend);
        });

        _hubConnection.On<PendingFriendshipDto>("ReceiveFriendRequest", friend =>
        {
            OnFriendRequestReceived?.Invoke(friend);
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) await _hubConnection.DisposeAsync();
    }
}