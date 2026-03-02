using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Data.Entities;

public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)] // Discord-style limit
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    // --- Relationships ---

    // The person who sent it
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    // The channel it belongs to (Assuming you have or will have a Channel entity)
    public int ChannelId { get; set; }
    // public Channel Channel { get; set; } = null!; 
}