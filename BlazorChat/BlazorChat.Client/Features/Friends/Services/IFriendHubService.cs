using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Friends.Services;

public interface IFriendHubService : IAsyncDisposable
{
    event Action<FriendshipDto>? OnNewFriendAdded;
    event Action<PendingFriendshipDto>? OnFriendRequestReceived;
    Task ConnectAsync();
}

public class FriendHubService(ISignalRConnectionFactory connectionFactory) : IFriendHubService
{
    private HubConnection? _hubConnection;
    public event Action<FriendshipDto>? OnNewFriendAdded;
    public event Action<PendingFriendshipDto>? OnFriendRequestReceived;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 
        _hubConnection = connectionFactory.CreateConnection("hubs/friend");
        
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