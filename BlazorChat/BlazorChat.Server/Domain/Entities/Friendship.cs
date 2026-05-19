namespace BlazorChat.Server.Domain.Entities;

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Blocked
}

public class Friendship
{
    public int RequesterId { get; init; } 
    public User Requester { get; init; } = null!;

    public int ReceiverId { get; init; }
    public User Receiver { get; init; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
            return;

        Status = FriendshipStatus.Accepted;
    }
    
    public static Friendship CreatePending(int requesterId, int receiverId)
    {
        return new Friendship
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}