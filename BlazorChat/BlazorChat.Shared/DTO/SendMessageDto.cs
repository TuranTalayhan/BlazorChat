namespace BlazorChat.Shared.DTO;

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
    public int ChannelId { get; set; } = 1;
}
