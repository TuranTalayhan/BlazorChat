using System.Security.Claims;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorChat.Client.Services;

public class NavigationState : IDisposable
{
    private readonly IChannelsApiService _channelsApiService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private int? _cachedUserId;

    public ServerDto? SelectedServer { get; private set; }
    public int? SelectedChannelId { get; private set; }
    public ServerRole CurrentUserRole { get; private set; } = ServerRole.Member;

    public event Action? OnChanged;
    public event Action<ReceiveUserStatusDto>? OnGlobalUserStatusChanged;
    public event Action<int, int, ServerRole>? OnGlobalUserRoleChanged;
    public event Action<StructureActionEvent>? OnChannelStructureChanged;

    public NavigationState(IChannelsApiService channelsApiService, AuthenticationStateProvider authStateProvider)
    {
        _channelsApiService = channelsApiService;
        _authStateProvider = authStateProvider;
        
        _authStateProvider.AuthenticationStateChanged += HandleAuthenticationChanged;
    }

    public async Task<int> GetCurrentUserIdAsync()
    {
        if (_cachedUserId.HasValue) return _cachedUserId.Value;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var idClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(idClaim, out var parsedId)) return 0;
        _cachedUserId = parsedId;
        return parsedId;

    }

    public async void HandleUserRoleChanged(int serverId, int userId, ServerRole newRole)
    {
        if (SelectedServer?.Id == serverId) 
        {
            var myUserId = await GetCurrentUserIdAsync();

            if (userId == myUserId)
            {
                CurrentUserRole = newRole;
                OnChanged?.Invoke();
            }
        }

        OnGlobalUserRoleChanged?.Invoke(serverId, userId, newRole);
    }

    public async void SelectServer(ServerDto? server)
    {
        SelectedServer = server;
        SelectedChannelId = null; 
        CurrentUserRole = ServerRole.Member;

        if (server != null)
        {
            CurrentUserRole = await _channelsApiService.GetUserRoleInServerAsync(server.Id);
        }

        OnChanged?.Invoke();
    }

    public void SetActiveChannel(int channelId)
    {
        if (SelectedChannelId == channelId) return;
        SelectedChannelId = channelId;
        OnChanged?.Invoke();
    }
    
    public async Task EnsureServerIsLoadedForChannelAsync(int channelId)
    {
        if (SelectedServer != null) return;

        try
        {
            var server = await _channelsApiService.GetServerByChannelIdAsync(channelId);
            
            if (server != null)
            {
                SelectedServer = server;
                
                CurrentUserRole = await _channelsApiService.GetUserRoleInServerAsync(server.Id);
                
                OnChanged?.Invoke();
            }
        }
        catch
        {
            SelectedServer = null;
            CurrentUserRole = ServerRole.Member;
            OnChanged?.Invoke();
        }
    }

    public void HandleUserStatusChanged(ReceiveUserStatusDto statusDto) => OnGlobalUserStatusChanged?.Invoke(statusDto);
    public void HandleChannelCreated(ChannelDto newChannel) => OnChannelStructureChanged?.Invoke(new StructureActionEvent { Action = ResourceAction.Created, Channel = newChannel });
    
    public void HandleChannelDeleted(int channelId)
    {
        if (SelectedChannelId == channelId) SelectedChannelId = null;
        OnChannelStructureChanged?.Invoke(new StructureActionEvent { Action = ResourceAction.Deleted, TargetId = channelId });
    }

    public void HandleServerUpdated(ServerDto updatedServer)
    {
        if (SelectedServer?.Id != updatedServer.Id) return;
        SelectedServer = updatedServer;
        OnChanged?.Invoke();
    }

    private void HandleAuthenticationChanged(Task<AuthenticationState> newAuthState)
    {
        _cachedUserId = null;
        CurrentUserRole = ServerRole.Member;
    }

    public void Dispose()
    {
        _authStateProvider.AuthenticationStateChanged -= HandleAuthenticationChanged;
    }
}

public class StructureActionEvent
{
    public ResourceAction Action { get; set; }
    public ChannelDto? Channel { get; set; }
    public int? TargetId { get; set; }
}

public enum ResourceAction { Created, Deleted }