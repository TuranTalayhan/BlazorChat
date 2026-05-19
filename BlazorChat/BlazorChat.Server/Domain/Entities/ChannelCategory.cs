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
    
    public static ChannelCategory Create(string name, int serverId, int sortOrder)
    {
        return new ChannelCategory
        {
            Name = name.Trim(),
            ServerId = serverId,
            SortOrder = sortOrder
        };
    }
    
    public void PrepareForDeletion()
    {
        foreach (var channel in Channels)
        {
            channel.MoveToCategory(null); 
        }
    }
    
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return;

        Name = newName.Trim();
    }
}