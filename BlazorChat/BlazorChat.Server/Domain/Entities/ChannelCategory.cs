using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Domain.Entities;

public class ChannelCategory
{
    public int Id { get; init; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; init; }
    
    public int ServerId { get; init; }
    public ChatServer Server { get; init; } = null!;
    
    public ICollection<Channel> Channels { get; init; } = new List<Channel>();
}