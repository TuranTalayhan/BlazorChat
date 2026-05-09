using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.DirectMessage;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;

namespace BlazorChat.Client.Features.Friends.ViewModels;

public class FriendsSidebarViewModel : IDisposable
{
    private readonly IFriendshipApiService _friendshipApiService;
    private readonly IDirectMessageApiService _dmApiService;
    private readonly IChatHubService _hubService;
    private readonly NavigationManager _navigationManager;
    
    public event Action? OnStateChanged;

    private Dictionary<int, FriendshipDto> Friends { get; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = string.Empty;
    public int ActiveFriendId { get; private set; }

    public IEnumerable<FriendshipDto> FilteredFriends => Friends.Values.Where(f =>
        (string.IsNullOrWhiteSpace(SearchTerm) || f.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(StatusFilter) || f.Status != UserStatus.Offline)
    );

    public FriendsSidebarViewModel(IFriendshipApiService friendshipApiService, IChatHubService hubService,  NavigationManager navigationManager, IDirectMessageApiService dmApiService)
    {
        _friendshipApiService = friendshipApiService;
        _hubService = hubService;
        _navigationManager = navigationManager;
        _dmApiService = dmApiService;


        _hubService.OnUserStatusChanged += HandleUserStatusChanged;
        _hubService.OnNewFriendAdded += HandleNewFriend;
    }

    public async Task InitializeAsync()
    {
        var friendsList = await _friendshipApiService.GetFriendsAsync();
        foreach (var friend in friendsList)
        {
            Friends[friend.FriendId] = friend;
        }

        await _hubService.ConnectAsync();
        OnStateChanged?.Invoke();
    }

    public void SetActiveFilter(string filter)
    {
        StatusFilter = filter;
        OnStateChanged?.Invoke();
    }

    public async Task OpenChatWithFriend(int friendId)
    {
        ActiveFriendId = friendId;
    
        OnStateChanged?.Invoke();

        var channelId =  await _dmApiService.OpenDirectMessageAsync(friendId);
        
        _navigationManager.NavigateTo($"/chat/{channelId}");
    }

    private void HandleUserStatusChanged(ReceiveUserStatus status)
    {
        if (!Friends.TryGetValue(status.Id, out var friend)) return;
        friend.Status = status.Status;
        OnStateChanged?.Invoke();
    }
    
    public string GetStatusClass(UserStatus status) => status switch
    {
        UserStatus.Online => "online",
        UserStatus.Idle => "idle",
        UserStatus.DoNotDisturb => "dnd",
        _ => "offline"
    };

    private void HandleNewFriend(FriendshipDto friend)
    {
        Friends[friend.FriendId] = friend;
        OnStateChanged?.Invoke();
    }

    public void Dispose()
    {
        _hubService.OnUserStatusChanged -= HandleUserStatusChanged;
        _hubService.OnNewFriendAdded -= HandleNewFriend;
    }
}