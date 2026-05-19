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
}