namespace BlazorChat.Server.Data.Entities;

/// <summary>
/// Represents a 1-to-1 DM conversation between two users.
/// Messages in this conversation reference DirectMessage.Id via Message.DirectMessageId.
/// </summary>
public class DirectMessage
{
    public int Id { get; set; }

    public int User1Id { get; set; }
    public User User1 { get; set; } = null!;

    public int User2Id { get; set; }
    public User User2 { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
