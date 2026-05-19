namespace BlazorChat.Server.Application.Interfaces;

public interface IChatPresenceTracker
{
    bool IsUserActiveInChannel(int channelId, int userId);
}