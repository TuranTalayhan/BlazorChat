using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public class CreateChannelDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } =  string.Empty;
    
    [Required]
    public ChannelType Type { get; set; } = ChannelType.Server;
    
    public string? CategoryName { get; set; } 
}