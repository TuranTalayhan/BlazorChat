namespace BlazorChat.Server.Domain.Entities;

public enum ServerRole
{
    Member,
    Admin,
    Owner
}

public class ServerMembership
{
    public int ServerId { get; init; }
    public ChatServer Server { get; init; } = null!;

    public int UserId { get; init; }
    public User User { get; init; } = null!;

    public ServerRole Role { get; init; } = ServerRole.Member;
    public DateTime JoinedAt { get; init; } = DateTime.UtcNow;
}
