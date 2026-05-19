namespace BlazorChat.Server.Domain.Entities;

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Blocked
}

public class Friendship
{
    // Ensure these match the 'int' type in your User class
    public int RequesterId { get; set; } 
    public User Requester { get; set; } = null!;

    public int ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
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