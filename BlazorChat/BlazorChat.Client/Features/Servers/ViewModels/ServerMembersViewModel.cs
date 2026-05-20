using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Client.Features.Servers.ViewModels;

public class ServerMembersViewModel : IDisposable
{
    private readonly IChannelsApiService _apiService;
    private readonly NavigationState _navState;
    private int _lastServerId;

    public List<UserDto> Members { get; private set; } = [];
    public bool IsLoading { get; private set; }
    public ServerRole CurrentUserRole { get; private set; } = ServerRole.Member;
    public bool IsCurrentUserOwner => CurrentUserRole == ServerRole.Owner;
    
    public event Action? StateChanged;

    public ServerMembersViewModel(IChannelsApiService apiService, NavigationState navState)
    {
        _apiService = apiService;
        _navState = navState;
        
        _navState.OnChanged += HandleNavigationChanged;
        _navState.OnGlobalUserStatusChanged += HandleLivePresenceUpdate;
        
        _navState.OnGlobalUserRoleChanged += HandleLiveRoleUpdate;
    }

    public async Task InitializeAsync()
    {
        await CheckAndReloadMembersAsync();
    }

    private async void HandleNavigationChanged()
    {
        await CheckAndReloadMembersAsync();
    }

    private void HandleLivePresenceUpdate(ReceiveUserStatusDto statusDto)
    {
        var existingMember = Members.FirstOrDefault(m => m.Id == statusDto.Id);
        if (existingMember != null)
        {
            existingMember.Status = statusDto.Status;
            StateChanged?.Invoke();
        }
    }

    private void HandleLiveRoleUpdate(int serverId, int userId, ServerRole newRole)
    {
        var currentServerId = _navState.SelectedServer?.Id ?? 0;
        if (currentServerId != serverId) return;
        
        var targetMember = Members.FirstOrDefault(m => m.Id == userId);
        if (targetMember == null) return;
        targetMember.Role = newRole;
        StateChanged?.Invoke();
    }

    private async Task CheckAndReloadMembersAsync()
    {
        var currentServerId = _navState.SelectedServer?.Id ?? 0;

        if (currentServerId > 0 && currentServerId != _lastServerId)
        {
            _lastServerId = currentServerId;
            await LoadMembersAsync(currentServerId);
        }
        else if (currentServerId == 0)
        {
            _lastServerId = 0;
            Members = [];
            CurrentUserRole = ServerRole.Member;
            StateChanged?.Invoke();
        }
    }

    private async Task LoadMembersAsync(int serverId)
    {
        IsLoading = true;
        StateChanged?.Invoke();

        CurrentUserRole = await _apiService.GetUserRoleInServerAsync(serverId);
        Members = await _apiService.GetServerMembersAsync(serverId);

        IsLoading = false;
        StateChanged?.Invoke();
    }

    public async Task ChangeUserRoleAsync(int targetUserId, ServerRole newRole)
    {
        var currentServerId = _navState.SelectedServer?.Id ?? 0;
        if (currentServerId == 0 || !IsCurrentUserOwner) return;
        
        await _apiService.UpdateUserRoleInServerAsync(currentServerId, targetUserId, newRole);
    }

    public string GetStatusClass(UserStatus status) => status switch
    {
        UserStatus.Online => "online",
        UserStatus.Idle => "away",
        UserStatus.DoNotDisturb => "dnd",
        _ => "offline"
    };
    
    public string GetRoleCssClass(ServerRole role) => role switch
    {
        ServerRole.Owner => "role-owner",
        ServerRole.Admin => "role-admin",
        _ => "role-member"
    };

    public void Dispose()
    {
        _navState.OnChanged -= HandleNavigationChanged;
        _navState.OnGlobalUserStatusChanged -= HandleLivePresenceUpdate;
        
        _navState.OnGlobalUserRoleChanged -= HandleLiveRoleUpdate;
    }
}