using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IServersHubService : IAsyncDisposable
{
    Task ConnectAsync();
}

public class ServersHubService : IServersHubService
{
    private HubConnection? _hubConnection;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7138/hubs/servers", options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => 
                    new CookieHandler { InnerHandler = innerHandler }; 
            })
            .Build();

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