using System.Collections.Concurrent;
using BlazorChat.Server.Application.Interfaces;

namespace BlazorChat.Server.Infrastructure.Services;

public class ChatPresenceTracker : IChatPresenceTracker
{
    private readonly ConcurrentDictionary<int, HashSet<int>> _activeChannelUsers = new();

    public bool IsUserActiveInChannel(int channelId, int userId)
    {
        if (!_activeChannelUsers.TryGetValue(channelId, out var activeUsers))
            return false;

        lock (activeUsers) 
        {
            return activeUsers.Contains(userId);
        }
    }
}