using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Domain.Entities;

public class UserChannelState
{
    public int UserId { get; init; }
    public User User { get; init; } = null!;

    public int ChannelId { get; init; }
    public Channel Channel { get; init; } = null!;

    [Required]
    public int LastReadMessageId { get; set; }
    
    public static UserChannelState Create(int userId, int channelId, int lastMessageId)
    {
        return new UserChannelState
        {
            UserId = userId,
            ChannelId = channelId,
            LastReadMessageId = lastMessageId
        };
    }
    
    public void TrackProgress(int lastMessageId)
    {
        if (lastMessageId > LastReadMessageId)
        {
            LastReadMessageId = lastMessageId;
        }
    }
}