using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Friends.Services;

public interface IFriendHubService : IAsyncDisposable
{
    event Action<ReceiveUserStatusDto>? OnUserStatusChanged;
    event Action<FriendshipDto>? OnNewFriendAdded;
    
    Task ConnectAsync();
}

public class FriendHubService : IFriendHubService
{
    private HubConnection? _hubConnection;

    public event Action<ReceiveUserStatusDto>? OnUserStatusChanged;
    public event Action<FriendshipDto>? OnNewFriendAdded;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/chat", options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => 
                    new CookieHandler { InnerHandler = innerHandler }; 
            })
            .Build();
        
        _hubConnection.On<ReceiveUserStatusDto>("UserStatusChanged", userStatus =>
        {
            OnUserStatusChanged?.Invoke(userStatus);
        });
        
        _hubConnection.On<FriendshipDto>("NewFriendAdded", friend =>
        {
            OnNewFriendAdded?.Invoke(friend);
        });
        

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}