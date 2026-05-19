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
    
    public event Action? StateChanged;

    public ServerMembersViewModel(IChannelsApiService apiService, NavigationState navState)
    {
        _apiService = apiService;
        _navState = navState;
        
        _navState.OnChanged += HandleNavigationChanged;
    }

    public async Task InitializeAsync()
    {
        await CheckAndReloadMembersAsync();
    }

    private async void HandleNavigationChanged()
    {
        await CheckAndReloadMembersAsync();
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
            StateChanged?.Invoke();
        }
    }

    private async Task LoadMembersAsync(int serverId)
    {
        IsLoading = true;
        StateChanged?.Invoke();

        Members = await _apiService.GetServerMembersAsync(serverId);

        IsLoading = false;
        StateChanged?.Invoke();
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
    }
}