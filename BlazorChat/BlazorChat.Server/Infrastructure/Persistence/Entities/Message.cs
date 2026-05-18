using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.Constants;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

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
    [MaxLength(ChatConstants.MaxMessageLength)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    // Author
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
}