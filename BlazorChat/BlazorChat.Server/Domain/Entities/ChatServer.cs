using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Server.Domain.Entities;

public class ChatServer
{
    public int Id { get; init; }

    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(512)]
    public string? IconUrl { get; init; }

    public int OwnerId { get; init; }
    public User Owner { get; init; } = null!;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Channel> Channels { get; init; } = new List<Channel>();
    
    public ICollection<ChannelCategory> Categories { get; init; } = new List<ChannelCategory>();
    public ICollection<ServerMembership> Members { get; init; } = new List<ServerMembership>();
    
    public static ChatServer CreateWithDefaults(string name, int ownerId)
    {
        var server = new ChatServer
        {
            Name = name.Trim(),
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        server.Channels.Add(Channel.CreateServerChannel("general", server.Id, categoryId: null));

        server.Members.Add(ServerMembership.CreateOwner(ownerId));

        return server;
    }
}
