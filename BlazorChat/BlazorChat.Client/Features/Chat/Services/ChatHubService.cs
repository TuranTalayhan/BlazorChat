using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Chat.Services;

public interface IChatHubService : IAsyncDisposable
{
    event Action<ReceiveUserStatus>? OnUserStatusChanged;
    event Action<FriendshipDto>? OnNewFriendAdded;
    
    Task ConnectAsync();
}

public class ChatHubService : IChatHubService
{
    private HubConnection? _hubConnection;

    public event Action<ReceiveUserStatus>? OnUserStatusChanged;
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
        
        _hubConnection.On<ReceiveUserStatus>("UserStatusChanged", userStatus =>
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