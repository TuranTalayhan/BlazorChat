using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public class ChannelCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    
    public int ServerId { get; set; }
    public ChatServer Server { get; set; } = null!;
    
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
}