using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Domain.Entities;

public class Channel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    
    public ChannelType Type { get; set; } = ChannelType.Server;

    public int? ServerId { get; set; }
    public ChatServer? Server { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public int? CategoryId { get; set; }
    public ChannelCategory? Category { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<User> Members { get; set; } = new List<User>();
    
    public static Channel CreateDirectMessage(int user1Id, int user2Id)
    {
        return new Channel
        {
            Type = ChannelType.DirectMessage,
            CreatedAt = DateTime.UtcNow,
            Members = new List<User> 
            { 
                new() { Id = user1Id }, 
                new() { Id = user2Id } 
            }
        };
    }
    
    public static Channel CreateServerChannel(string name, int serverId, int? categoryId)
    {
        return new Channel
        {
            Name = name.Trim().ToLower(),
            Type = ChannelType.Server,
            ServerId = serverId,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateSettings(string? newName, int? newCategoryId, int? newSortOrder)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            Name = newName.Trim().ToLower();
        }

        if (newCategoryId.HasValue)
        {
            CategoryId = newCategoryId.Value;
        }

        if (newSortOrder.HasValue)
        {
            SortOrder = newSortOrder.Value;
        }
    }
}
