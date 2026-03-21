namespace BlazorChat.Shared.DTO;

public class PendingFriendshipDto
{
    public int RequesterId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public DateTime CreatedAt { get; set; }
}
