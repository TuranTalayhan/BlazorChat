using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.Friends.Services;
using BlazorChat.Client.Features.User.Services;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Client.Core;

public interface IGlobalNotificationService
{
    event Action<MessageDto>? OnMessageReceived;
    event Action<ReceiveUserStatusDto>? OnUserStatusChanged;
    event Action<FriendshipDto>? OnNewFriendAdded;

    Task EnsureConnectedAsync();
}

public class GlobalNotificationService(
    IChatHubService chatHub, 
    IUserHubService userHub, 
    IFriendHubService friendHub) : IGlobalNotificationService
{
    public event Action<MessageDto>? OnMessageReceived;
    public event Action<ReceiveUserStatusDto>? OnUserStatusChanged;
    public event Action<FriendshipDto>? OnNewFriendAdded;

    private bool _isConnecting;

    public async Task EnsureConnectedAsync()
    {
        if (_isConnecting) return;
        _isConnecting = true;

        chatHub.OnMessageReceived += msg => OnMessageReceived?.Invoke(msg);
        userHub.OnUserStatusChanged += status => OnUserStatusChanged?.Invoke(status);
        friendHub.OnNewFriendAdded += friend => OnNewFriendAdded?.Invoke(friend);

        await Task.WhenAll(
            chatHub.ConnectAsync(),
            userHub.ConnectAsync(),
            friendHub.ConnectAsync()
        );

        _isConnecting = false;
    }
}