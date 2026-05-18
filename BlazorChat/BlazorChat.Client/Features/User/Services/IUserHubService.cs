using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.User.Services;

public interface IUserHubService : IAsyncDisposable
{
    event Action<ReceiveUserStatusDto>? OnUserStatusChanged;
    Task ConnectAsync();
}

public class UserHubService : IUserHubService
{
    private HubConnection? _hubConnection;
    public event Action<ReceiveUserStatusDto>? OnUserStatusChanged;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/user", options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler }; 
            })
            .Build();
        
        _hubConnection.On<ReceiveUserStatusDto>("UserStatusChanged", userStatus =>
        {
            OnUserStatusChanged?.Invoke(userStatus);
        });

        await _hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) await _hubConnection.DisposeAsync();
    }
}