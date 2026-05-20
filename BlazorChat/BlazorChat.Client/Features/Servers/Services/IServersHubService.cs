using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Features.Servers.Services;

public interface IServerHubService
{
    Task WatchServerAsync(int serverId);
}

public class ServerHubService : IServerHubService, IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly NavigationState _navState;
    private int _currentWatchedServerId;

    public ServerHubService(ISignalRConnectionFactory factory, NavigationState navState)
    {
        _navState = navState;
        _connection = factory.CreateConnection("hubs/server");

        _connection.On<ServerDto>("ServerUpdated", server => _navState.HandleServerUpdated(server));
        _connection.On<int, ChannelDto>("ChannelCreated", (sId, ch) => _navState.HandleChannelCreated(ch));
        _connection.On<int, int>("ChannelDeleted", (sId, chId) => _navState.HandleChannelDeleted(chId));
        _connection.On<int, int, ServerRole>("UserRoleUpdated", (serverId, userId, newRole) => 
            _navState.HandleUserRoleChanged(serverId, userId, newRole));
        _connection.On<int, UserDto>("UserJoinedServer", (serverId, user) => 
            _navState.HandleUserJoinedServer(serverId, user));
    }

    public async Task WatchServerAsync(int serverId)
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
        }

        if (_currentWatchedServerId == serverId) return;

        if (_currentWatchedServerId > 0)
        {
            await _connection.InvokeAsync("LeaveServerGroup", _currentWatchedServerId);
        }

        _currentWatchedServerId = serverId;
        await _connection.InvokeAsync("JoinServerGroup", serverId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.State != HubConnectionState.Disconnected)
        {
            await _connection.StopAsync();
        }
        await _connection.DisposeAsync();
    }
}