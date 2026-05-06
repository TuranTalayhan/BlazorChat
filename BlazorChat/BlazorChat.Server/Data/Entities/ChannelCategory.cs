using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Data.Entities;

public class ChannelCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
    
    public int ServerId { get; set; }
    public ChatServer Server { get; set; } = null!;
    
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
}