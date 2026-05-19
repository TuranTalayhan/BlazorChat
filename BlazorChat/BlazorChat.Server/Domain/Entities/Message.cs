using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.Constants;

namespace BlazorChat.Server.Domain.Entities;

public enum MessageType
{
    Text,
    System
}

public class Message
{
    public int Id { get; init; }

    [Required]
    [MaxLength(ChatConstants.MaxMessageLength)]
    public string Content { get; init; } = string.Empty;

    public MessageType Type { get; init; } = MessageType.Text;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; init; }
    
    public int ChannelId { get; init; }
    public Channel Channel { get; init; } = null!;

    public int AuthorId { get; init; }
    public User Author { get; init; } = null!;
    
    public static Message Create(string content, int channelId, int authorId)
    {
        return new Message
        {
            Content = content.Trim(),
            ChannelId = channelId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };
    }
}