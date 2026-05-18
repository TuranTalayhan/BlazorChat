namespace BlazorChat.Client.Core;

public class AppState
{
    public FriendChannelSelection? LastSelectedFriend { get; private set; }
    
    private readonly Dictionary<int, int> _lastServerChannels = new();

    public void SetLastSelectedFriend(int friendId, int channelId)
    {
        LastSelectedFriend = new FriendChannelSelection(friendId, channelId);
    }

    public void SaveServerChannel(int serverId, int channelId)
    {
        _lastServerChannels[serverId] = channelId;
    }

    public int? GetLastChannelForServer(int serverId)
    {
        return _lastServerChannels.TryGetValue(serverId, out var channelId) 
            ? channelId 
            : null;
    }
    
    public FriendChannelSelection? GetLastSelectedFriend()
    {
        return LastSelectedFriend;
    }
}

public record FriendChannelSelection(int FriendId, int ChannelId);