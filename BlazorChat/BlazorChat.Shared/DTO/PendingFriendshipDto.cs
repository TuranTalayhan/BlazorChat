namespace BlazorChat.Shared.DTO;

public class PendingFriendshipDto
{
    public int RequesterId { get; set; }
    public string RequesterUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
