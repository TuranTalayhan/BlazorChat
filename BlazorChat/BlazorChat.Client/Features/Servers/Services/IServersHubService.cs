using BlazorChat.Client.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IServersHubService : IAsyncDisposable
{
    Task ConnectAsync();
}

public class ServersHubService(ISignalRConnectionFactory connectionFactory) : IServersHubService
{
    private HubConnection? _hubConnection;

    public async Task ConnectAsync()
    {
        if (_hubConnection is not null) return; 
        _hubConnection = connectionFactory.CreateConnection("hubs/server");

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