using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.Constants;

namespace BlazorChat.Shared.DTO;

public class SendMessageDto
{
    [Required, MaxLength(ChatConstants.MaxMessageLength)]
    public string Content { get; set; } = string.Empty;
    public int ChannelId { get; set; }
}
