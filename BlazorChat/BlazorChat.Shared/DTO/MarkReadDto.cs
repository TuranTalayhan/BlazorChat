namespace BlazorChat.Shared.DTO;

public class MarkReadDto
{
    int ChannelId { get; set; }
    int LastMessageId { get; set; }
}