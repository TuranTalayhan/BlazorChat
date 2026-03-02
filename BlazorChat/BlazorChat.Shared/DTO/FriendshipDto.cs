namespace BlazorChat.Shared.DTO;

public class FriendshipDto
{
    public int FriendId { get; set; }
    public required string Username { get; set; }
    public UserStatus Status { get; set; }
    public bool IsOnline => Status != UserStatus.Offline;
    
    // You can add properties like 'SinceDate' or 'SharedServersCount'
}