namespace BlazorChat.Shared.DTO;

public enum MessageType
{
    Text,
    System
}

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int ChannelId { get; set; }
}
