using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Data.Entities;

public enum MessageType
{
    Text,
    System
}

public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Channel message FK (null for DMs)
    public int? ChannelId { get; set; }
    public Channel? Channel { get; set; }

    // DM conversation FK (null for channel messages)
    public int? DirectMessageId { get; set; }
    public DirectMessage? DirectMessage { get; set; }

    // Author
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
}