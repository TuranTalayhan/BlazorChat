namespace BlazorChat.Shared.DTO;

public class ChannelUnreadStatusDto
{
    public int ChannelId { get; set; }
    public bool HasUnreadMessages { get; set; }
}