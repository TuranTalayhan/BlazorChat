namespace BlazorChat.Shared.DTO;

/// <summary>
/// Represents a 1-to-1 DM conversation between the current user and another user.
/// </summary>
public class DirectMessageDto
{
    public int Id { get; set; }
    public int OtherUserId { get; set; }
    public string OtherUsername { get; set; } = string.Empty;
    public string? OtherAvatarUrl { get; set; }
    public UserStatus OtherStatus { get; set; }
}
