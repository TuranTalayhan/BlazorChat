using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Components;

namespace BlazorChat.Client.Features.Friends.ViewModels;

public enum FriendFilter
{
    All,
    Online
}

public class FriendsSidebarViewModel : IDisposable
{
    private readonly IFriendshipApiService _friendshipApiService;
    private readonly IGlobalNotificationService _notifications;
    private readonly NavigationManager _navigationManager;
    private readonly AppState _appState;
    
    public event Action? OnStateChanged;

    private Dictionary<int, SidebarFriendSummaryDto> Friends { get; } = new();
    public HashSet<int> UnreadFriends { get; } = new(); 
    
    public string SearchTerm { get; set; } = string.Empty;
    public FriendFilter StatusFilter { get; set; } = FriendFilter.All;
    public int ActiveFriendId { get; private set; }

    public IEnumerable<SidebarFriendSummaryDto> FilteredFriends => Friends.Values.Where(f =>
        (string.IsNullOrWhiteSpace(SearchTerm) || f.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
        (StatusFilter == FriendFilter.All || f.Status != UserStatus.Offline)
    );

    public FriendsSidebarViewModel(
        IFriendshipApiService friendshipApiService, 
        IGlobalNotificationService notifications,
        NavigationManager navigationManager, 
        AppState appState)
    {
        _friendshipApiService = friendshipApiService;
        _notifications = notifications;
        _navigationManager = navigationManager;
        _appState = appState;

        _notifications.OnUserStatusChanged += HandleUserStatusChanged;
        _notifications.OnNewFriendAdded += HandleNewFriend;
        _notifications.OnMessageReceived += HandleIncomingMessage; 
    }

    public async Task InitializeAsync()
    {
        // One clean API call fetches friends, status, DM channels, and unread metrics completely
        var summaryList = await _friendshipApiService.GetFriendsSummaryAsync();
        
        foreach (var item in summaryList)
        {
            Friends[item.FriendId] = item;

            if (_appState.LastSelectedFriend?.FriendId == item.FriendId)
            {
                ActiveFriendId = item.FriendId;
            }

            if (item.HasUnreadMessages && item.FriendId != ActiveFriendId)
            {
                UnreadFriends.Add(item.FriendId);
            }
        }

        await _notifications.EnsureConnectedAsync();
        OnStateChanged?.Invoke();
    }

    private void HandleIncomingMessage(MessageDto message)
    {
        if (!Friends.ContainsKey(message.AuthorId) || ActiveFriendId == message.AuthorId) return;
        
        UnreadFriends.Add(message.AuthorId);
        OnStateChanged?.Invoke();
    }

    public async Task OpenChatWithFriend(int friendId)
    {
        if (!Friends.TryGetValue(friendId, out var friend)) return;

        ActiveFriendId = friendId;
        UnreadFriends.Remove(friendId); 
        OnStateChanged?.Invoke();
        
        _appState.SetLastSelectedFriend(friendId, friend.ChannelId);
        _navigationManager.NavigateTo($"/chat/{friend.ChannelId}");
    }

    private void HandleUserStatusChanged(ReceiveUserStatusDto statusDto)
    {
        if (!Friends.TryGetValue(statusDto.Id, out var oldRecord)) return;
        
        Friends[statusDto.Id] = oldRecord with { Status = statusDto.Status };
        OnStateChanged?.Invoke();
    }

    private void HandleNewFriend(FriendshipDto friend)
    {
        _ = InitializeAsync();
    }
    
    public void SetActiveFilter(FriendFilter filter)
    {
        StatusFilter = filter;
        OnStateChanged?.Invoke();
    }

    public static string GetStatusClass(UserStatus status) => status switch
    {
        UserStatus.Online => "online",
        UserStatus.Idle => "idle",
        UserStatus.DoNotDisturb => "dnd",
        _ => "offline"
    };

    public void Dispose()
    {
        _notifications.OnUserStatusChanged -= HandleUserStatusChanged;
        _notifications.OnNewFriendAdded -= HandleNewFriend;
        _notifications.OnMessageReceived -= HandleIncomingMessage;
    }
}