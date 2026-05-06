namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public enum ServerRole
{
    Member,
    Admin,
    Owner
}

public class ServerMembership
{
    public int ServerId { get; set; }
    public ChatServer Server { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ServerRole Role { get; set; } = ServerRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
