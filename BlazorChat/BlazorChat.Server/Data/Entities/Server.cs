using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Data.Entities;

public class ChatServer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? IconUrl { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    public ICollection<ServerMembership> Members { get; set; } = new List<ServerMembership>();
}
