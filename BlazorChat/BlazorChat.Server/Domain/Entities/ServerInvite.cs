namespace BlazorChat.Server.Domain.Entities;

public class ServerInvite
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int ServerId { get; init; }
    public ChatServer Server { get; init; } = null!;
    public int CreatedByUserId { get; init; }
    public User CreatedByUser { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
    public int Uses { get; set; }
    public int? MaxUses { get; init; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsLimitReached => Uses >= MaxUses;

    public static ServerInvite Create(int serverId, int userId, int expiresInHours, int? maxUses)
    {
        return new ServerInvite
        {
            Code = Guid.NewGuid().ToString()[..8].ToUpper(),
            ServerId = serverId,
            CreatedByUserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(expiresInHours),
            MaxUses = maxUses,
            Uses = 0
        };
    }
}