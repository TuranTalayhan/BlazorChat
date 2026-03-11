using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Data.Entities;

public class Channel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public int ServerId { get; set; }
    public ChatServer Server { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
