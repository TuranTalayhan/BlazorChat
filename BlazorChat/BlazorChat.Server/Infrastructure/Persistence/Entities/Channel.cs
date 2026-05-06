using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public enum ChannelType
{
    ServerText = 0,
    DirectMessage = 1,
}

public class Channel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
    
    public ChannelType Type { get; set; } = ChannelType.ServerText;

    public int? ServerId { get; set; }
    public ChatServer? Server { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? CategoryId { get; set; }
    public ChannelCategory? Category { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<User> Members { get; set; } = new List<User>();
}
