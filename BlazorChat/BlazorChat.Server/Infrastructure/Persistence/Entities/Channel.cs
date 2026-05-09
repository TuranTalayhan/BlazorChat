using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public class Channel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
    
    public ChannelType Type { get; set; } = ChannelType.Server;

    public int? ServerId { get; set; }
    public ChatServer? Server { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public int? CategoryId { get; set; }
    public ChannelCategory? Category { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<User> Members { get; set; } = new List<User>();
}
