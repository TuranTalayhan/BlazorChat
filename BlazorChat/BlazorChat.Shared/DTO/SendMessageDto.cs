using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public class SendMessageDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    public int ChannelId { get; set; }
}
