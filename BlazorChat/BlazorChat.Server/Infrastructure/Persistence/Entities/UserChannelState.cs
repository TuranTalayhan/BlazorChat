using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public class UserChannelState
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    [Required]
    public int LastReadMessageId { get; set; }
}