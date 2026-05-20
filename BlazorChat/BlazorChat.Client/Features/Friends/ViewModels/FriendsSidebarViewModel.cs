using BlazorChat.Client.Core;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.DirectMessage;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Services; // Ensure your NavigationState namespace is imported
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
    private readonly IDirectMessageApiService _dmApiService;
    private readonly IGlobalNotificationService _notifications;
    private readonly NavigationManager _navigationManager;
    private readonly AppState _appState;
    private readonly IChatHubService _chatHubService;
    private readonly NavigationState _navState; // INJECTED

    public event Action? OnStateChanged;

    private Dictionary<int, SidebarFriendSummaryDto> Friends { get; } = new();
    public HashSet<int> UnreadFriends { get; } = new(); 

    public string SearchTerm { get; set; } = string.Empty;
    public FriendFilter StatusFilter { get; set; } = FriendFilter.All;
    public int ActiveFriendId { get; private set; }
    
    public FriendsSidebarViewModel(
        IFriendshipApiService friendshipApiService, 
        IDirectMessageApiService dmApiService,
        IGlobalNotificationService notifications,
        NavigationManager navigationManager, 
        AppState appState, 
        IChatHubService chatHubService,
        NavigationState navState)
    {
        _friendshipApiService = friendshipApiService;
        _dmApiService = dmApiService;
        _notifications = notifications;
        _navigationManager = navigationManager;
        _appState = appState;
        _chatHubService = chatHubService;
        _navState = navState;

        _navState.OnGlobalUserStatusChanged += HandleUserStatusChanged;
        
        _notifications.OnNewFriendAdded += HandleNewFriend;
        _notifications.OnMessageReceived += HandleIncomingMessage; 
    }
    
    public IEnumerable<SidebarFriendSummaryDto> FilteredFriends => Friends.Values.Where(f =>
        (string.IsNullOrWhiteSpace(SearchTerm) || f.Username.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
        (StatusFilter == FriendFilter.All || f.Status != UserStatus.Offline)
    );

    public async Task InitializeAsync()
    {
        var summaryList = await _friendshipApiService.GetFriendsSummaryAsync();
    
        foreach (var item in summaryList)
        {
            Friends[item.FriendId] = item;

            if (_appState.LastSelectedFriend?.FriendId == item.FriendId)
            {
                ActiveFriendId = item.FriendId;
            }

            if (!item.HasUnreadMessages) continue;
            if (item.FriendId == ActiveFriendId)
            {
                _ = _chatHubService.MarkAsReadAsync(item.ChannelId, 0);
            }
            else
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

        var targetChannelId = friend.ChannelId;

        if (targetChannelId == 0)
        {
            targetChannelId = await _dmApiService.OpenDirectMessageAsync(friendId);
        
            if (targetChannelId > 0)
            {
                Friends[friendId] = friend with { ChannelId = targetChannelId };
            }
            else
            {
                return;
            }
        }

        ActiveFriendId = friendId;
        UnreadFriends.Remove(friendId); 
        OnStateChanged?.Invoke();
    
        try 
        {
            await _chatHubService.MarkAsReadAsync(targetChannelId, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to persist read status on backend: {ex.Message}");
        }
    
        _appState.SetLastSelectedFriend(friendId, targetChannelId);
        _navigationManager.NavigateTo($"/chat/{targetChannelId}");
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
        _navState.OnGlobalUserStatusChanged -= HandleUserStatusChanged;
        
        _notifications.OnNewFriendAdded -= HandleNewFriend;
        _notifications.OnMessageReceived -= HandleIncomingMessage;
    }
}